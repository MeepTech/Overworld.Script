using Meep.Tech.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A factory for building programs from a provided context and lines of text.
    /// NOT-THREAD-SAFE
    /// </summary>
    public partial class Interpreter {

      /// <summary>
      /// The the program that was/is being built
      /// </summary>
      public Program Program {
        get;
        private set;
      }

      /// <summary>
      /// Join together several .ows files.
      /// </summary>
      public static IEnumerable<string> JoinOwsFiles(IEnumerable<(string filename, string contents)> rawFiles)
        => rawFiles.OrderBy(raw => raw.filename)
          .SelectMany(raw => raw.contents
          // TODO: make this an extension method for here and below uses
            .Split(Environment.NewLine)
              .SelectMany(line => line
                .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries)));

      /// <summary>
      /// Join together several .ows files into a program
      /// </summary>
      public static IEnumerable<string> JoinOwsFiles(IEnumerable<string> rawFileNames)
        => JoinOwsFiles(rawFileNames.Select(rawFileName =>
          (rawFileName, System.IO.File.ReadAllText(rawFileName))
        ));

      /// <summary>
      /// Join together several .ows files.
      /// </summary>
      public static IEnumerable<string> JoinOwsFiles(params (string filename, string contents)[] raws)
        => JoinOwsFiles((IEnumerable<(string filename, string contents)>)raws);

      /// <summary>
      /// Build a new program from a bunch of files and return it's interpreter 
      /// </summary>
      /// <param name="context">The context for the interpreter to use when building a Program</param>
      /// <param name="unorderedRawFiles">File contents and file names for the files you want compiled into one program. Files will be sorted alphabeticaly.
      ///   Lines of code in files with names that begin with '_' will be set as "pre initial lines" and will be placed before the default entry point of the program</param>
      public static Interpreter Build(Program.ContextData context, IEnumerable<(string filename, string contents)> unorderedRawFiles) {
        List<(string filename, string contents)> rawLines = new();
        List<(string filename, string contents)> rawPreInitLines = new();
        foreach(var (filename, contents) in unorderedRawFiles.OrderBy(file => file.filename)) {
          if(System.IO.Path.GetFileName(filename).StartsWith("_")) {
            rawPreInitLines.Add((filename, contents));
          } else
            rawLines.Add((filename, contents));
        }

        Interpreter @return = new(context);
        @return.Build(JoinOwsFiles(unorderedRawFiles), JoinOwsFiles(rawPreInitLines));
        return @return;
      }

      /// <summary>
      /// Make a new program interpreter. They built programs~
      /// </summary>
      /// <param name="context"></param>
      public Interpreter(Program.ContextData context) {
        Program = new Program(context);
      }

      /// <summary>
      /// Build a new program from a bunch of files
      /// </summary>
      /// <param name="rawLines">The raw lines of program text, starting from the initial entry point</param>
      /// <param name="preInitialLines">Lines that are placed before the default start point of the compiled program. These are useful for adding dependencies and such to a Program that you don't want run by default.</param>
      public Program Build([NotNull] string rawLines, string preInitialLines = null)
        => Build(rawLines
            .Split(Environment.NewLine)
              .SelectMany(line => line
                .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries)),
        preInitialLines?
            .Split(Environment.NewLine)
              .SelectMany(line => line
                .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries)));

      /// <summary>
      /// Build a new program from a single line of text
      /// </summary>
      /// <param name="rawLine">The single line of text to build from</param>
      public Command BuildLine([NotNull] string rawLine)
        => Build(rawLine
            .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries))
              ._commands.First().Value;

      /// <summary>
      /// Build a new program from a single line of text
      /// </summary>
      /// <param name="rawLine">The single line of text to build from</param>
      /// <param name="label">The label of the line if there is one</param>
      public Command BuildLine([NotNull] string rawLine, out string label) {
        var program = Build(rawLine
          .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries));
        
        var firstCommand = program._commands.First();
        var firstLabel = program._labelsByLineNumber.FirstOrDefault();
        if(firstLabel.Key != null && firstLabel.Value != firstCommand.Key) {
          throw new System.InvalidOperationException($"Multi line command with label passed into BuildLine. Use Build instead");
        }

        label = firstLabel.Key;
        return firstCommand.Value;
      }

      /// <summary>
      /// Build a new program from a bunch of files
      /// </summary>
      public Program Build(IEnumerable<string> rawLines, IEnumerable<string> preInitialLines = null) {
        _cleanProgram();

        /// save the raw text
        preInitialLines ??= Enumerable.Empty<string>();
        preInitialLines = preInitialLines.ToUpperExeptStringLiterals();
        Program.PreStartRawText =
          string.Join(Environment.NewLine, preInitialLines.SelectMany(
            line => line.Split(
              LineEndAlternateSymbol,
              StringSplitOptions.RemoveEmptyEntries
            )));

        rawLines = rawLines.ToUpperExeptStringLiterals();
        Program.PostStartRawText ??= string.Join(Environment.NewLine, rawLines
          .SelectMany(line => line
            .Split(LineEndAlternateSymbol, StringSplitOptions.RemoveEmptyEntries))
          ).Trim();

        Program.RawText ??= string.Join(
          Environment.NewLine,
          rawLines.Concat(preInitialLines)
        );

        int lineNumber = 0;
        Program.StartLine = preInitialLines?.Count() ?? Program.StartLine;
        string[] preCompiledLines = preInitialLines.Concat(rawLines).ToArray();
        for(int preCompiledLineIndex = 0; preCompiledLineIndex < preCompiledLines.Length; preCompiledLineIndex++) {
          string currentLine = preCompiledLines[preCompiledLineIndex];
          string currentCommandText = string.Copy(currentLine);

          // if there's a label, store it
          if(currentCommandText.Trim().FirstOrDefault() == LabelStartSymbol) {
            Program._labelsByLineNumber.Add(
              new string(currentCommandText.Trim()[1..].Until(LabelEndSymbol)).Trim(),
              lineNumber
            );
            currentCommandText = currentCommandText.After(FunctionSeperatorSymbol);
          }

          // if the next lines have ..., join them to the current line
          if(preCompiledLines.Length > preCompiledLineIndex + 1) {
            // TODO: make the ... a const
            while(preCompiledLines[preCompiledLineIndex + 1].Trim().StartsWith("...")) {
              currentCommandText += preCompiledLines[preCompiledLineIndex + 1].Trim()[3..] + " ";
              preCompiledLineIndex++;
              if(preCompiledLines.Length <= preCompiledLineIndex) {
                break;
              }
            }
          }

          if(!string.IsNullOrWhiteSpace(currentCommandText)) {
            try {
              if(currentCommandText.Trim().StartsWith("ELSE")) {
                if(Program._commands.TryGetValue(lineNumber - 1, out var prevCommand)) {
                  if(prevCommand.Archetype is Command.IF || prevCommand.Archetype is Command.IF_NOT) {
                    currentCommandText = currentCommandText.Trim()[4..].After(':');
                  } else throw new ArgumentException($"Unexpected ELSE:");
                } else throw new ArgumentException($"Unexpected ELSE:");
              }
              /// Process the command
              Program._commands.Add(lineNumber,
                _parseCommand(ref currentCommandText, lineNumber));
            } catch(System.Exception e) {
              throw new System.InvalidOperationException($"Error compiling OWS code on line {lineNumber}.", e);
            }
          }

          lineNumber++;
        }

        Program.LineCount = lineNumber;
        return Program;
      }

      /// <summary>
      /// Clean the program
      /// </summary>
      void _cleanProgram() {
        Program = new Program(Program.Context);
      }

      /// <summary>
      /// Process the given command
      Command _parseCommand(ref string remainingCommandText, int lineNumber) {
        string currentFunctionName = remainingCommandText.Trim(FunctionSeperatorSymbol, ' ')
          .Until(FunctionSeperatorSymbol).Trim();
        remainingCommandText = remainingCommandText.After(FunctionSeperatorSymbol).Trim();

        // make sure it's a recognized command
        if(Program.Context.Commands.TryGetValue(currentFunctionName, out Command.Type commandType)) {
          /// Set command has some specialish syntax:
          List<IParameter> parameters = new();
          bool specialCommandFound = false;

          // goto is special:
          if(commandType is Command.GO_TO gotoCommand) {
            specialCommandFound = true;
            // try to check if it's a string name of an existing label, and then if it's a variable with a string value
            parameters.Add(
              new String(Program, remainingCommandText.Trim())
            );
            parameters.Add(
              new Number(Program, lineNumber)
            );
            remainingCommandText = "";
          }
          else

          // set is special
          if(commandType is Command.SET setCommand) {
            specialCommandFound = true;
            if(commandType is Command.SET_FOR) {
              Collection characters = (Collection)
                _parseParam(
                  ref remainingCommandText,
                  typeof(Collection<Character>),
                  lineNumber
              );
              parameters.Add(characters);
            }
            string[] parts = null;
            if(remainingCommandText.Contains(SetToSymbol)) {
              parts = remainingCommandText.Split(SetToSymbol);
              remainingCommandText = remainingCommandText[(parts.First().Length + 1)..].Trim();
            }
            else if(remainingCommandText.Contains(SetToPhrase)) {
              parts = remainingCommandText.Split(SetToPhrase);
              remainingCommandText = remainingCommandText[(parts.First().Length + SetToPhrase.Length)..].Trim();
            }

            parameters.Add(
              new String(Program, parts[0].Trim())
            );
            parameters.Add(
              _parseParam(ref remainingCommandText, typeof(Token), lineNumber)
            );
          }
          else

          // unset is special too
          if(commandType is Command.UN_SET unSetCommand) {
            specialCommandFound = true;
            if(commandType is Command.UN_SET_FOR) {
              Collection characters =
                (Collection)_parseParam(
                  ref remainingCommandText,
                  typeof(Collection<Character>),
                  lineNumber
                );
              parameters.Add(characters);
            }
            parameters.Add(
              new String(Program, remainingCommandText.Trim())
            );
            remainingCommandText = "";
          }

          if(specialCommandFound) {
            return commandType.Make(
              Program,
              parameters
            );
          }

          /// normal commands:
          int commandParams = commandType.ParameterTypes.Count();
          for(int paramIndex = 0; paramIndex < commandParams; paramIndex++) {
            try {
              if(string.IsNullOrWhiteSpace(remainingCommandText)) {
                throw new ArgumentNullException($"Param #{paramIndex + 1} : {commandType.ParameterTypes[paramIndex]}", $"Command : {commandType} is missing an expected parameter.");
              }

              parameters.Add(
                _parseParam(
                  ref remainingCommandText,
                  commandType.ParameterTypes[paramIndex],
                  lineNumber
                )
              );
            } catch(System.Exception e) {
              throw new System.ArgumentException($"Failed to parse param for Command: {currentFunctionName}.", $"Prama #{paramIndex + 1}: {commandType.ParameterTypes[paramIndex].Name}");
            }

            remainingCommandText = remainingCommandText.Trim(FunctionSeperatorSymbol, ' ');
          }
          return commandType.Make(Program, parameters);
        } else
          throw new System.MissingMethodException(nameof(Ows), currentFunctionName);
      }

      /// <summary>
      /// Create a token as a param for a command
      /// </summary>
      IParameter _parseParam(ref string fullRemaininglineText, Type expectedParamReturnType, int lineNumber) {
        string firstParamStub = fullRemaininglineText
          .Until(FunctionSeperatorSymbol)
          .Trim(FunctionSeperatorSymbol)
          .Trim();

        // if we expect a command, try to find it first:
        if(expectedParamReturnType == typeof(Command)) {
          if(Program.Context.Commands.TryGetValue(firstParamStub.Until(FunctionSeperatorSymbol), out Command.Type found)) {
            // if (expectedType == null || (expectedType == found.ReturnType))
            return _parseCommand(ref fullRemaininglineText, lineNumber);
          } else
            throw new MissingMethodException(nameof(Ows), firstParamStub.Until(FunctionSeperatorSymbol));
        }

        // if we expect a collection of somekind, it could be a collection of strings, or entities.
        if(expectedParamReturnType.IsAssignableToGeneric(typeof(Collection<>)) || firstParamStub.StartsWith(CollectionStartSymbol)) {
          Collection collection = _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber);
          fullRemaininglineText = fullRemaininglineText.Trim();
          // check for concat
          bool isAdd = true;
          while(isAdd = fullRemaininglineText.StartsWith(new string[] {
            // find add for collections
            AndConcatPhrase,
            Opperators.PLUS.ToString(),
            ConcatPhrase,
            ((char)Comparitors.AND).ToString(),
            ((char)Opperators.PLUS).ToString()
          }, out string foundPrefix)
            // find minus for collections
          || fullRemaininglineText.StartsWith(new string[] {
              Opperators.MINUS.ToString(),
              CollectionExclusionPhrase,
              ((char)Opperators.MINUS).ToString()
            }, out foundPrefix)
          ) {
            fullRemaininglineText = fullRemaininglineText[foundPrefix.Length..];
            if(isAdd) {
              collection.Value.AddRange(_parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber).Value);
            } else {
              foreach(var o in _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber).Value) {
                collection.Value.Remove(o);
              }
            }
            fullRemaininglineText = fullRemaininglineText.Trim();
          }

          return collection;
        }

        /// if it's a plain old bool/conditional 
        // check if it's a conditional that's expected.
        if(expectedParamReturnType.Equals(typeof(IConditional))
          // check for true false text
          || (firstParamStub.StartsWith(new string[] { "TRUE", "FALSE" }, out string initialBoolVal)
            && ((initialBoolVal.Length == firstParamStub.Length)
            || (!char.IsLetterOrDigit(firstParamStub[initialBoolVal.Length])
              && firstParamStub[initialBoolVal.Length] != '-'
              && firstParamStub[initialBoolVal.Length] != '_')))
        ) {
          return _parseCondition(ref fullRemaininglineText, lineNumber);
        }

        /// RESERVED Loop index Keyword
        if(firstParamStub.StartsWith("LOOP-INDEX", true, null) && firstParamStub.Length == "LOOP-INDEX".Length) {
          fullRemaininglineText = fullRemaininglineText[firstParamStub.Length..].Trim();
          return new PlaceholderIndex(Program);
        }

        // IF we've exausted most options and it begins with a not, try to make this a condition
        if(fullRemaininglineText.StartsWith((char)Comparitors.NOT) || fullRemaininglineText.StartsWith(Ows.NotPrefixPhrase)) {
          return _parseCondition(ref fullRemaininglineText, lineNumber);
        }

        /// if it's a string
        if(firstParamStub.FirstOrDefault() == StringQuotesSymbol) {
          String @string = _parseString(ref fullRemaininglineText);

          // concat remaining strings
          while(fullRemaininglineText.StartsWith(new string[] {
            // find add for collections
            AndConcatPhrase,
            Opperators.PLUS.ToString(),
            ConcatPhrase,
            ((char)Comparitors.AND).ToString(),
            ((char)Opperators.PLUS).ToString()
          }, out string foundPrefix)) {
            fullRemaininglineText = fullRemaininglineText[foundPrefix.Length..];
            @string.Value += _parseString(ref fullRemaininglineText);
            fullRemaininglineText = fullRemaininglineText.Trim();
          }

          return @string;
        }

        /// numbers
        if(firstParamStub.FirstOrDefault().Equals('#')) {
          string textualNumberString = firstParamStub
            .UntilNot(c => char.IsLetterOrDigit(c) || c == '-' || c == DecimalSymbol)
            .Replace('-', ' ')
            .Replace(DecimalSymbol.ToString(), "point");
          throw new NotImplementedException($"Number text to number format not yet supported.");
        }

        // if it's a plain number
        if(char.IsNumber(firstParamStub.FirstOrDefault())) {
          Number number = _parseNumber(ref fullRemaininglineText);
          MathOpperator compiledOpperaton = null;

          // check for opperators
          while(fullRemaininglineText.StartsWith(new string[] {
            // find add for collections
              AndConcatPhrase,
              ((char)Comparitors.AND).ToString()
            }.Concat(NumberOpperatorChars.Select(x => x.ToString()))
            // todo: make constants
            .Concat(Enum.GetValues(typeof(Opperators)).Cast<Opperators>().Select(s => $"{s.ToString().Replace('_', '-')} "))
            , out string foundPrefix)
          ) {
            // get the opperator
            Opperators opperation;
            if(foundPrefix.Length > 1) {
              if(foundPrefix.Equals(AndConcatPhrase)) {
                opperation = Opperators.PLUS;
              } else
                opperation = Enum.Parse<Opperators>(foundPrefix.Trim());
            } else {
              if(foundPrefix[0].Equals((char)Comparitors.AND) || foundPrefix[0] == 'X') {
                opperation = Opperators.PLUS;
              } else
                opperation = (Opperators)foundPrefix[0];
            }
            fullRemaininglineText = fullRemaininglineText[foundPrefix.Length..];
            Number next = null;
            // squared is special, and only has one param
            if(opperation != Opperators.SQUARED) {
              next =_parseNumber(ref fullRemaininglineText);
            }
            compiledOpperaton = Command.Types.Get<MathOpperator.Type>()
              .Make(
                Program,
                new IParameter[] {
                  compiledOpperaton ?? (IParameter)number,
                  next
                },
                opperation
              );
            fullRemaininglineText = fullRemaininglineText.Trim();
          }

          return compiledOpperaton ?? (IParameter)number;
        }

        /// TODO: check if there's any spaces, or special characters in the potential variable or function name.
        /// If there are, then we should check which and send it to either conditional or opperation
        /*firstParamStub.UntilAny(
            // TODO: cache this whole thing in a static
            // symbols
            ComparitorSymbols.Select(ch => ch.ToString()).ToList().Except(new[] { "!" })
              // phrases
              .Concat(ComparitorPhrases
                .Except(new[] { Comparitors.NOT.ToString() })
                .Select(phrase => $" {phrase} ")
                ).Append(LogicStartSymbol.ToString())
                .Append(" "),
            out string foundSeperator
        );
        if(foundSeperator != null) {
          if(foundSeperator != FunctionSeperatorSymbol.ToString()) {
            // TODO: also check for opperators when we impliment those
            return _parseCondition(ref fullRemaininglineText, lineNumber);
          }
          if(foundSeperator == " ") {
            throw new ArgumentException($"Unexpected space at:\n>{firstParamStub}");
          }
        }*/


        // check if it's a command by chance that returns what we want
        if(Program.Context.Commands.ContainsKey(firstParamStub)) {
          return _parseCommand(ref fullRemaininglineText, lineNumber);
        }

        /// lastly, assume it's a plain old variable
        return _parseExistingVariable(ref fullRemaininglineText, string.IsNullOrWhiteSpace(firstParamStub) ? fullRemaininglineText : firstParamStub);
      }

      Number _parseNumber(ref string fullRemaininglineText) {
        // get all characters until we have a non number/decimal point, or a space.
        int decimalCount = 0;
        string value = fullRemaininglineText.UntilNot(chararcter => {
          // allow one decimal
          if (chararcter.Equals(DecimalSymbol) && decimalCount == 0) {
            decimalCount++;
            return true;
          } else if (decimalCount == 1) {
            decimalCount++;
            return false;
          }

          // if it's not a number, return
          return char.IsNumber(chararcter);
        });
        if(decimalCount == 2) {
          throw new ArgumentException($"Unexpected second decimal character in float value starting: {fullRemaininglineText}");
        }
        fullRemaininglineText = fullRemaininglineText[value.Length..].Trim(' ', ':');
        return new Number(Program, double.Parse(value));
      }

      Collection _parseCollection(ref string fullRemainingLineText, Type expectedType, int lineNumber) {
        IList list = new ArrayList();
        Collection collection;
        Type collectionItemType;
        // TODO: Impliment these as modular collection builders.
        if(expectedType.GenericTypeArguments.First().Equals(typeof(String))) {
          collectionItemType = typeof(String);
          collection = new Collection<String>(Program, list);
        } else if(expectedType.GenericTypeArguments.First().Equals(typeof(Character))) {
          collectionItemType = typeof(Character);
          collection = new Collection<Character>(Program, list);
        } else if(expectedType.GenericTypeArguments.First().Equals(typeof(Entity))) {
          collectionItemType = typeof(Entity);
          collection = new Collection<Entity>(Program, list);
        } else
          throw new NotSupportedException($"Collections of type {expectedType} are not yet supported");

        string preparsed = fullRemainingLineText.Trim();
        string parsed = "";
        int collectionDepth = 0;

        if(fullRemainingLineText[0].Equals(CollectionStartSymbol)) {
          collectionDepth++;
          parsed += fullRemainingLineText[0];
          fullRemainingLineText = fullRemainingLineText[1..].Trim();
        } else {
          list.Add(
            _parseParam(ref fullRemainingLineText, collectionItemType, lineNumber)
          );

          return collection;
        }

        // All * syntax
        if(fullRemainingLineText.Trim().StartsWith(new string[] { CollectAllSymbol.ToString(), CollectAllPhrase }, out string foundPhrase)) {
          if(CollectAllPhrase.Length > 1 && !char.IsLetterOrDigit(fullRemainingLineText[foundPhrase.Length..].FirstOrDefault())) {
            collection = Program._getAllObjectsOfType(collectionItemType);
            if(collectionDepth > 0 && fullRemainingLineText[foundPhrase.Length..].Trim().FirstOrDefault() != CollectionEndSymbol) {
              throw new ArgumentException($"No closure found for [ in an All/* collection syntax statement");
            }

            return collection;
          }
        }

        do {
          string potentialCondition = fullRemainingLineText.UntilAny(
            // TODO: cache this:
            new string[] {
              ((char)Comparitors.AND).ToString(),
              ",",
              $" {Comparitors.AND} ",
              CollectionEndSymbol.ToString()
            },
            out string foundSplitter
          );

          parsed += potentialCondition;
          fullRemainingLineText = fullRemainingLineText[potentialCondition.Length..];
          if(foundSplitter is null) {
            if(collectionDepth > 0) {
              throw new ArgumentException($"Condition closure not closed. Add a ']'");
            }
          } else {
            if(foundSplitter[0] == CollectionEndSymbol) {
              return collection;
            } else if (foundSplitter[0] == CollectionStartSymbol) {
              parsed += fullRemainingLineText;
              list.Add(_parseCollection(ref fullRemainingLineText, collectionItemType, lineNumber));
              parsed = parsed[..fullRemainingLineText.Length];
            } else {
              parsed += fullRemainingLineText;
              list.Add(_parseParam(ref fullRemainingLineText, collectionItemType, lineNumber));
              parsed = parsed[..fullRemainingLineText.Length];
            }
          }
        } while(collectionDepth < 0);

        throw new ArgumentException($"Failed to parse collection.");
      }

      IParameter _parseCondition(ref string fullRemainingLineText, int lineNumber) {
        string preparsed = fullRemainingLineText.Trim();
        string parsed = "";
        IParameter identityCondition = null;
        Comparitors? comparitor = null;
        int conditionLogicContainerDepth = 0;

        if(fullRemainingLineText[0].Equals(LogicStartSymbol)) {
          conditionLogicContainerDepth++;
          parsed += fullRemainingLineText[0];
          fullRemainingLineText = fullRemainingLineText[1..].Trim();
        }

        string leftConditionText = null;
        bool finishedClosure;
        do {
          finishedClosure = false;
          leftConditionText = fullRemainingLineText.UntilAny(
            // TODO: cache this whole thing in a static
            // symbols
            ComparitorSymbols.Select(ch => ch.ToString()).ToList().Except(new []{"!" })
              // phrases
              .Concat(ComparitorPhrases.Except(new []{"NOT"}).Select(phrase => $" {phrase} "))
              .Append(LogicStartSymbol.ToString())
              .Append(LogicEndSymbol.ToString()),
            out string foundConditionOrEnding
          );

          parsed += leftConditionText;
          fullRemainingLineText = fullRemainingLineText[leftConditionText.Length..];
          if(foundConditionOrEnding is null) {
            string foundNot = null;
            if(conditionLogicContainerDepth > 0) {
              throw new ArgumentException($"Condition closure not closed. Add a ')'");
            } else if(fullRemainingLineText.StartsWith(new string[] { "NOT-", "!" }, out foundNot)) {
              comparitor = Comparitors.NOT;
            } else {
              comparitor = Comparitors.IDENTITY;
            }

            fullRemainingLineText = preparsed.Trim('(', ' ');
            if(foundNot != null) {
              fullRemainingLineText = fullRemainingLineText[foundNot.Length..];
            }

            string remainingParamText = fullRemainingLineText.Until(':').Trim();
            if(remainingParamText == "TRUE") {
              identityCondition = new Boolean(Program, true);
              fullRemainingLineText= fullRemainingLineText[remainingParamText.Length..];
            }
            else if(remainingParamText == "FALSE") {
              identityCondition = new Boolean(Program, false);
              fullRemainingLineText = fullRemainingLineText[remainingParamText.Length..];
            } else
              identityCondition = _parseParam(ref fullRemainingLineText, typeof(Token), lineNumber);

            return Archetypes<Condition.Type>._.Make(Program, new IParameter[] {
              identityCondition
            }, comparitor);
          } else {
            leftConditionText = string.IsNullOrWhiteSpace(leftConditionText) 
              ? parsed
              : leftConditionText;
            parsed += fullRemainingLineText[..foundConditionOrEnding.Length];
            fullRemainingLineText = fullRemainingLineText[foundConditionOrEnding.Length..];

            // )
            if(foundConditionOrEnding.Equals(LogicEndSymbol.ToString())) {
              // we found an existing container end:
              if(conditionLogicContainerDepth > 0) {
                conditionLogicContainerDepth--;
                // if there's nothing else to parse, lets see if we have an identity
                if(!string.IsNullOrWhiteSpace(fullRemainingLineText) && !fullRemainingLineText.StartsWith(FunctionSeperatorSymbol)) {
                  finishedClosure = true;
                  continue;
                }
              }


              if(parsed.Trim().Length == preparsed.Trim().Length) {
                if(fullRemainingLineText.StartsWith("NOT-")) {
                  comparitor = Comparitors.NOT;
                } else {
                  comparitor = Comparitors.IDENTITY;
                }

                // we found the end of our initial closure without anything, it's an identity.
                fullRemainingLineText = preparsed.Trim(' ', LogicStartSymbol, LogicEndSymbol);
                identityCondition = _parseParam(ref fullRemainingLineText, typeof(IConditional), lineNumber);
              } // this means we closed a bracket on the left, and should pass the whole thing without brackets as the left condition. 
              else if (parsed.Trim().Length == preparsed.Until(FunctionSeperatorSymbol).Trim().Length) {
                parsed = parsed.Trim(' ', LogicStartSymbol, LogicEndSymbol);
                identityCondition = _parseCondition(ref parsed, lineNumber);
              }
            }

            if(conditionLogicContainerDepth > 0) {
              continue;
            }
            
            
            // (
            if(foundConditionOrEnding.Equals(LogicStartSymbol.ToString())) {
              conditionLogicContainerDepth++;
            }  // symbol
            else if(foundConditionOrEnding.Length == 1) {
              comparitor = (Comparitors)foundConditionOrEnding[0];
            } // word
            else {
              comparitor = Enum.Parse<Comparitors>(foundConditionOrEnding.Trim());
            }
          }

          if(identityCondition is not null) {
            return identityCondition;
          } else {
            IParameter left =
              _parseParam(ref leftConditionText, typeof(IConditional), lineNumber);
            IParameter right =
              _parseParam(ref fullRemainingLineText, typeof(IConditional), lineNumber);

            Condition colusre = Archetypes<Condition.Type>._.Make(Program, new IParameter[] {
              left,
              right
            }, comparitor);

            return colusre;
          }
        } while(conditionLogicContainerDepth > 0 || finishedClosure);

        throw new ArgumentException($"Could not parse condtion: \n{preparsed}");
      }

      Variable _parseExistingVariable(ref string fullRemaininglineText, string variableName) {
        fullRemaininglineText = fullRemaininglineText[variableName.Length..].Trim();
        return _makeExistingVariable(variableName);
      }

      /// <summary>
      /// Make or find a symbol represeing an existing variable
      /// </summary>
      Variable _makeExistingVariable(string variableName) {
        // check if it's a global, then program, then local character variable
        if(Program.TryToGetVariableByName(variableName, out Variable variable)) {
          return variable;
        } // as a last resort, assume it's a char specific variable:
        else {
          return new CharacterSpecificVariable(Program, variableName);
        }
      }

      /// <summary>
      /// Parse the next bit of the remaining line as a string
      /// </summary>
      String _parseString(ref string fullRemaininglineText) {
        string value = fullRemaininglineText.Skip(1).Until(StringQuotesSymbol).Trim();
        fullRemaininglineText = fullRemaininglineText[(value.Length + 2)..].Trim();
        return new String(Program, value);
      }

      /*Collection _parseCollection(ref string fullRemaininglineText, Type expectedType, int lineNumber) {

        IList list;
        Collection collection;
        Type collectionItemType;
        // TODO: Impliment these as modular collection builders.
        if(expectedType.GenericTypeArguments.First().Equals(typeof(String))) {
          collectionItemType = typeof(String);
          list = new List<String>();
          collection = new Collection<String>(Program, list);
        } else if(expectedType.GenericTypeArguments.First().Equals(typeof(Character))) {
          collectionItemType = typeof(Character);
          list = new List<Character>();
          collection = new Collection<Character>(Program, list as IList<Character>);
        } else if(expectedType.GenericTypeArguments.First().Equals(typeof(Entity))) {
          collectionItemType = typeof(Entity);
          list = new List<Entity>();
          collection = new Collection<Entity>(Program, list as IList<Entity>);
        } else
          throw new NotSupportedException($"Collections of type {expectedType} are not yet supported");

        // if it doesn't start with a collection symbol, it's a lone item
        if(!fullRemaininglineText.StartsWith(CollectionStartSymbol)) {
          list.Add(
            _parseParam(ref fullRemaininglineText, collectionItemType, lineNumber)
          );
          return collection;
        } /// tokenize each collection element
        else {
          // trim off the start symbol
          fullRemaininglineText = fullRemaininglineText.Substring(1).Trim();

          /*string potentialInitialCommand = fullRemaininglineText.UntilAny(new string[] {
            FunctionSeperatorSymbol.ToString(),
            ((char)Comparitors.AND).ToString(),
            $" {Comparitors.AND} ",
          }, out string seperator);

          if(seperator != null) {
            if(Program.Context.Commands.ContainsKey(potentialInitialCommand.Trim())) {

            } else if (seperator = ) {

            }
          }*//*

          string potentialAnd = string.Empty;
          string parsedSubLine = string.Empty;
          int skipUntilSubCollectionEndCount = 0;
          int parsedCharacterCount = 0;
          bool isFirst = true;
          int skipForAll = 0;
          bool notSyntax = false;

          /// parse each char
          foreach(char character in fullRemaininglineText) {
            /// if it starts with the all syntax
            // if the first char is "A" or "*"
            if(isFirst) {
              if(character.Equals(CollectAllSymbol)) {
              } else if(character.Equals("A") && fullRemaininglineText.StartsWith($"{CollectAllPhrase} ")) {
                skipForAll = 3;
              }

              collection = Program._getAllObjectsOfType(collectionItemType);
              notSyntax = true;
              isFirst = false;
              parsedCharacterCount++;
              continue;
            }

            // skip the 'LL ' in 'ALL '
            if(skipForAll-- > 0) {
              parsedCharacterCount++;
              continue;
            }

            /// after checking for the all syntax:
            // add a sub collection count
            if(character.Equals(CollectionStartSymbol)) {
              skipUntilSubCollectionEndCount++;
            }

            // skip sub collections
            if(skipUntilSubCollectionEndCount > 0) {
              if(character.Equals(CollectionEndSymbol)) {
                skipUntilSubCollectionEndCount--;
              }
              parsedCharacterCount++;
              continue;
            }

            // nonwhitespace chars
            if(!char.IsWhiteSpace(character)) {
              // AND or END found
              if(character.Equals(CollectionEndSymbol)
                || CollectionItemSeperatorSymbols.Contains(character)
                || potentialAnd.Count() == 5
              ) {
                if(potentialAnd.Count() == 5) {
                  parsedSubLine.Substring(0, parsedSubLine.Length - 5);
                } else {
                  parsedSubLine.Trim(CollectionItemSeperatorSymbols);
                }
                parsedSubLine = parsedSubLine.Trim();

                if(notSyntax) {
                  bool usingSymbol;
                  if((usingSymbol = parsedSubLine.StartsWith((char)Comparitors.NOT)) || parsedSubLine.StartsWith(Comparitors.NOT.ToString().ToUpper() + "-")) {
                    list.Remove(_getCollectionItemTokenForType(parsedSubLine, collectionItemType));
                  } else
                    throw new NotSupportedException($"The ALL Syntax (*), can only be used Alone or with AND + NOT Syntax. EX: (*&!VAR)");
                } else {
                  list.Add(_parseParam(ref parsedSubLine, collectionItemType, lineNumber));
                }

                potentialAnd = string.Empty;
                parsedSubLine = string.Empty;

                // if this is the end
                if(character.Equals(CollectionEndSymbol)) {
                  break;
                }
              } // part of an AND being built found 
              else if(
                potentialAnd.Count() == 1 && character.Equals("A")
                || potentialAnd.Count() == 2 && character.Equals("N")
                || potentialAnd.Count() == 3 && character.Equals("D")
              ) {
                potentialAnd += character;
              } // if the AND broke:
              else if(potentialAnd.Any()) {
                potentialAnd = string.Empty;
              }
            } // for text based & it needs to be built starting with and ending with a space
            else {
              if(potentialAnd.Count() == 0 || potentialAnd.Count() == 4) {
                potentialAnd += character;
              }
            }

            // add char to parsed
            parsedCharacterCount++;
            parsedSubLine += character;
          }

          fullRemaininglineText = fullRemaininglineText
            .Substring(parsedCharacterCount, fullRemaininglineText.Length).Trim();

          return collection;
        }
      }*/

      /// <summary>
      /// Gets a collection token for an object, based on a string
      /// </summary>
      /// <param name="tokenText"></param>
      /// <param name="expectedType"></param>
      /// <returns></returns>
      Token _getCollectionItemTokenForType(string tokenText, Type expectedType) {
        // TODO: make this modular as well with the other 2 places. Probably replace them all with a settings virtual obj
        if(tokenText[0].Equals(StringQuotesSymbol)) {
          if(expectedType.Equals(typeof(String))) {
            return new String(Program, tokenText.Trim(StringQuotesSymbol));
          } else if(expectedType.Equals(typeof(Character))) {
            return new Character(Program, Program.GetCharacter(tokenText.Trim(StringQuotesSymbol)));
          } else if(expectedType.Equals(typeof(Character))) {
            return new Entity(Program, Program.GetEntity(tokenText.Trim(StringQuotesSymbol)));
          } else
            throw new NotSupportedException($"Collections of type {expectedType} are not yet supported.");
        } else
          return _makeExistingVariable(tokenText);
      }

    }
  }
}
