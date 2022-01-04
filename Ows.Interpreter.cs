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
            .Split('\n').Select(s => s.Trim('\r'))
            .SelectMany(line => {
              if(line.Contains(';')) {
                return line.Split(LineEndAlternateSymbol).Where(x => x.Any(c => c != ' '));
              }

              return line.SingleItemAsEnumerable();
            })
          );

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
      public Program Build([NotNull] string rawLines, string preInitialLines = null) {
        var rawlinesSplitByNewline = rawLines
          .Split('\n')
          .Select(s => s.Trim('\r'))
          // remove empty lines from beginning and end.
          .SkipWhile(l => string.IsNullOrWhiteSpace(l))
          .Reverse()
          .SkipWhile(l => string.IsNullOrWhiteSpace(l))
          .Reverse();
        var rawLinesSplitByExtraLineEndChar = rawlinesSplitByNewline
          .SelectMany(line => {
            if(line.Contains(';')) {
              return line.Split(LineEndAlternateSymbol).Where(x => x.Any(c => c != ' '));
            }

            return line.SingleItemAsEnumerable();
          });

        return Build(
          rawLinesSplitByExtraLineEndChar,
          preInitialLines?
            .Split('\n').Select(s => s.Trim('\r'))
            .SelectMany(line => {
              if(line.Contains(';')) {
                return line.Split(LineEndAlternateSymbol).Where(x => x.Any(c => c != ' '));
              }

              return line.SingleItemAsEnumerable();
            })
        );
      }

      /// <summary>
      /// Build a new program from a single line of text
      /// </summary>
      /// <param name="rawLine">The single line of text to build from</param>
      public Command BuildLine([NotNull] string rawLine)
        => Build(rawLine)
            ._commands.First().Value;

      /// <summary>
      /// Build a new program from a single line of text
      /// </summary>
      /// <param name="rawLine">The single line of text to build from</param>
      /// <param name="label">The label of the line if there is one</param>
      public Command BuildLine([NotNull] string rawLine, out string label) {
        var program = Build(rawLine);
        
        var firstCommand = program._commands.First();
        var firstLabel = program._labelsWithLineNumber.FirstOrDefault();
        if(firstLabel.Key != null && firstLabel.Value != firstCommand.Key) {
          throw new System.InvalidOperationException($"Multi line command with label passed into BuildLine. Use Build instead");
        }

        label = firstLabel.Key;
        return firstCommand.Value;
      }

      /// <summary>
      /// Build a new program from a bunch of files
      /// </summary>
      Program Build(IEnumerable<string> rawLines, IEnumerable<string> preInitialLines = null) {
        _cleanProgram();

        /// save the raw text
        preInitialLines ??= Enumerable.Empty<string>();
        preInitialLines = preInitialLines.ToUpperExeptStringLiterals();
        Program.PreStartRawText =
          string.Join('\n', preInitialLines);

        rawLines = rawLines.ToUpperExeptStringLiterals();
        Program.PostStartRawText ??= string.Join('\n', rawLines);

        Program.RawText ??= string.Join(
          '\n',
          rawLines.Concat(preInitialLines)
        );

        int lineNumber = 0;
        Program.StartLine = preInitialLines?.Count() ?? Program.StartLine;
        Program._labelsWithLineNumber.Add(StartLabel, Program.StartLine);
        Program._labelsByLineNumber.Add(Program.StartLine, StartLabel);
        string[] preCompiledLines = preInitialLines.Concat(rawLines).ToArray();
        bool startCommandFound=  false;
        for(int preCompiledLineIndex = 0; preCompiledLineIndex < preCompiledLines.Length; preCompiledLineIndex++) {
          string currentLine = preCompiledLines[preCompiledLineIndex];
          string currentCommandText = string.Copy(currentLine);
          // remove comments:
          if(currentCommandText.Contains('#')) {
            bool inString = false;
            bool inComment = false;
            string corrected = "";
            foreach(char c in currentCommandText) {
              if(!inComment) {
                if(!inString) {
                  if(c == '#') {
                    inComment = true;
                    continue;
                  }
                  if(c == StringQuotesSymbol) {
                    inString = true;
                  }
                }
                else {
                  if(c == StringQuotesSymbol) {
                    inString = false;
                  }
                }

                corrected += c;
              }
              else {
                if(c == '#') {
                  inComment = false;
                }
              }
            }

            currentCommandText = corrected;
          }

          // if there's a label, store it
          if(currentCommandText.Trim().FirstOrDefault() == LabelStartSymbol) {
            /// START label is special
            string labelName = currentCommandText.Trim()[1..].Until(LabelEndSymbol).Trim();
            if(labelName.ToUpper() == StartLabel) {
              if(startCommandFound) {
                throw new ArgumentException($"Multiple START lables detected. Only one START label is allowed per compiled program.");
              }
              Program._labelsByLineNumber.Remove(Program._labelsWithLineNumber[StartLabel]);
              Program._labelsWithLineNumber[StartLabel] = lineNumber;
              Program._labelsByLineNumber[lineNumber] = StartLabel;
              Program.StartLine = lineNumber;
              startCommandFound = true;
            }
            else {
              Program._labelsWithLineNumber.Add(
                labelName,
                lineNumber
              );
              Program._labelsByLineNumber.Add(
                lineNumber,
                labelName
              );
            }
            currentCommandText = currentCommandText.After(FunctionSeperatorSymbol);
          }

          // if the next lines have ..., join them to the current line
          if(preCompiledLines.Length > preCompiledLineIndex + 1) {
            // TODO: make the ... a const
            while(preCompiledLines[preCompiledLineIndex + 1].Trim().StartsWith("...")) {
              currentCommandText += preCompiledLines[preCompiledLineIndex + 1].Trim()[3..] + " ";
              preCompiledLineIndex++;
              if(preCompiledLines.Length <= (preCompiledLineIndex + 1)) {
                break;
              }
            }
          }

          if(!string.IsNullOrWhiteSpace(currentCommandText)) {
            try {
              /// Process the command
              Program._commands.Add(lineNumber,
                _parseCommand(ref currentCommandText, lineNumber));
            } catch(System.Exception e) {
              throw new System.InvalidOperationException($"Error compiling OWS code on line {lineNumber}.", e);
            }
          } else {
            Program._commands.Add(lineNumber, null);
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
            parameters.Add(
              new String(Program, remainingCommandText.Trim())
            );
            parameters.Add(
              new Number(Program, lineNumber)
            );
            remainingCommandText = "";
          } else

          // set is special
          if(commandType is Command.SET setCommand) {
            specialCommandFound = true;
            if(commandType is Command.SET_FOR) {
              // if it's a special set_for:
              if(remainingCommandText.Trim().StartsWith(new string[] {
                WorldPhrase,
                ProgramPhrase,
                ProgramSymbol.ToString(),
                WorldSymbol.ToString()
              }, out string foundSpecialSelector)) {
                if(foundSpecialSelector[0] == WorldSymbol || foundSpecialSelector == WorldPhrase) {
                  commandType = Command.Types.Get<Command.SET_FOR_WORLD>();
                  remainingCommandText = remainingCommandText.After(FunctionSeperatorSymbol).Trim();
                }
                if(foundSpecialSelector[0] == ProgramSymbol || foundSpecialSelector == ProgramPhrase) {
                  commandType = Command.Types.Get<Command.SET_FOR_PROGRAM>();
                  remainingCommandText = remainingCommandText.After(FunctionSeperatorSymbol).Trim();
                }
              }
              else {
                Collection characters =
                  _parseCollection(
                    ref remainingCommandText,
                    typeof(Collection),
                    lineNumber
                  );
                parameters.Add(characters);
              }
            }

            string[] parts = null;
            string left = remainingCommandText.UntilAny(new string[] {
              SetToSymbol.ToString(),
              SetToPhrase,
              SetIsPhrase,
              SetsAsPhrase,
              Comparitors.EQUALS.ToString()
            }, out string foundSetText);
            if(foundSetText != null) {
              parts = remainingCommandText.Split(foundSetText);
              remainingCommandText = remainingCommandText[(parts.First().Length + foundSetText.Length)..].Trim();
            } else
              throw new ArgumentException($"Set command is missing 'to', 'as', 'is', '=', or 'equals' syntax.");

            parameters.Add(
              new String(Program, parts[0].Trim())
            );
            parameters.Add(
              _parseParam(ref remainingCommandText, typeof(Token), lineNumber)
            );
          } else

          // unset is special too
          if(commandType is Command.UN_SET unSetCommand) {
            specialCommandFound = true;
            if(commandType is Command.UN_SET_FOR) {
              // if it's a special set_for:
              if(remainingCommandText.Trim().StartsWith(new string[] {
                WorldPhrase,
                ProgramPhrase,
                ProgramSymbol.ToString(),
                WorldSymbol.ToString()
              }, out string foundSpecialSelector)) {
                if(foundSpecialSelector[0] == WorldSymbol || foundSpecialSelector == WorldPhrase) {
                  commandType = Command.Types.Get<Command.UN_SET_FOR_WORLD>();
                  remainingCommandText = remainingCommandText.After(FunctionSeperatorSymbol).Trim();
                }
                if(foundSpecialSelector[0] == ProgramSymbol || foundSpecialSelector == ProgramPhrase) {
                  commandType = Command.Types.Get<Command.UN_SET_FOR_WORLD>();
                  remainingCommandText = remainingCommandText.After(FunctionSeperatorSymbol).Trim();
                }
              }
              else {
                Collection characters =
                _parseCollection(
                  ref remainingCommandText,
                  typeof(Collection),
                  lineNumber
                );

                parameters.Add(characters);
              }
            }

            parameters.Add(
              new String(Program, remainingCommandText.Trim())
            );
            remainingCommandText = "";
          }
          else

          // DO is special too
          if(commandType is Command.DO doCommand) {
            specialCommandFound = true;
            string[] parts = null;
            string goToLabel = remainingCommandText.UntilNot(
              c => char.IsLetterOrDigit(c) || (c == '_') || (c == '-'));
            remainingCommandText = remainingCommandText[goToLabel.Length..];
            parameters.Add(
              new String(Program, goToLabel.Trim())
            );
            if(remainingCommandText.Contains(ConcatPhrase)) {
              parts = remainingCommandText.Split(ConcatPhrase);
              remainingCommandText = remainingCommandText[(parts.First().Length + ConcatPhrase.Length)..].Trim();
              if(remainingCommandText[0] == ' ' || remainingCommandText[0] == CollectionStartSymbol) {
                parameters.Add(
                  _parseVariableMap(ref remainingCommandText, lineNumber)
                );
              } else throw new ArgumentException($"Improper with syntax. Make sure you have a space or ( before the values passed in.");
            } // empty local scope:
            else {
              parameters.Add(null);
            }

            // add line number:
            parameters.Add(new Number(Program, lineNumber));
          }


          /// If command has special post-params
          if(commandType is Command.IF ifCommand || commandType is Command.IF_NOT endifCommand) {
            try {
              if(string.IsNullOrWhiteSpace(remainingCommandText)) {
                throw new ArgumentNullException($"Param #{1} : {commandType.ParameterTypes[0]}", $"Command : {commandType} is missing an expected parameter.");
              }

              // condition:
              parameters.Add(
              _parseParam(
                ref remainingCommandText,
                commandType.ParameterTypes[0],
                lineNumber));

              if(string.IsNullOrWhiteSpace(remainingCommandText)) {
                throw new ArgumentNullException($"Param #{2} : {commandType.ParameterTypes[1]}", $"Command : {commandType} is missing an expected parameter.");
              }

              // on success:
              parameters.Add(
              _parseParam(
                  ref remainingCommandText,
                  commandType.ParameterTypes[1],
                  lineNumber
                )
              );

              specialCommandFound = true;
              if(remainingCommandText.Length > 0 && remainingCommandText.StartsWith(ElsePhrase)) {
                remainingCommandText = remainingCommandText.Trim()[ElsePhrase.Length..].After(':');
                // Add the else param:
                parameters.Add(
                  _parseParam(
                    ref remainingCommandText,
                    commandType.ParameterTypes[2], 
                    lineNumber
                  )
                );
              }
              else
                parameters.Add(null);

            } catch(System.Exception e) {
              throw new System.ArgumentException($"Failed to parse params for Command: {currentFunctionName}", e);
            }
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
              throw new System.ArgumentException($"Failed to parse param for Command: {currentFunctionName}.", $"Prama #{paramIndex + 1}: {commandType.ParameterTypes[paramIndex].Name}", e);
            }

            remainingCommandText = remainingCommandText.Trim(FunctionSeperatorSymbol, ' ');
          }
        

          return commandType.Make(Program, parameters);
        } else
          throw new System.MissingMethodException(nameof(Ows), currentFunctionName);
      }

      VariableMap _parseVariableMap(ref string remainingCommandText, int lineNumber) {
        VariableMap variableMap = new(Program, new Dictionary<string, IParameter>());
        bool inClosure = false;
        if(remainingCommandText.StartsWith(CollectionStartSymbol)) {
          inClosure = true;
          remainingCommandText = remainingCommandText.Trim(CollectionStartSymbol, ' ');
        }
        else {
          remainingCommandText.Trim();
        }

        bool finished = false;
        do {
          string potentialScopedVariable = remainingCommandText.UntilAny(
            // TODO: cache this:
            new string[] {
              ((char)Comparitors.AND).ToString(),
              ",",
              $" {Comparitors.AND} ",
              CollectionEndSymbol.ToString()
            },
            out string foundSplitter
          );

          if(foundSplitter != null) {
            if(foundSplitter[0] == CollectionEndSymbol) {
              if(!inClosure) {
                throw new ArgumentException($"Improper with syntax. Make sure you close and start the list with [].");
              }

              finished = true;
            }
            remainingCommandText = remainingCommandText[(potentialScopedVariable.Length + foundSplitter.Length)..];
            string scopedVarName = potentialScopedVariable.UntilAny(
              // TODO: cache this:
              new string[] {
                ((char)Comparitors.EQUALS).ToString(),
                $" {Comparitors.EQUALS} ",
                SetToPhrase,
                SetIsPhrase,
                SetsAsPhrase
              },
              out string foundSetter
            );
            potentialScopedVariable = potentialScopedVariable[scopedVarName.Length..].Trim()[foundSetter.Trim().Length..].Trim();
            scopedVarName = scopedVarName.Trim();
            if(ReservedKeywords.Contains(scopedVarName)) {
              throw new ArgumentException($"Tried to use reserved keyword as variable name: {scopedVarName.Trim()}");
            }
            if(char.IsDigit(scopedVarName.FirstOrDefault())) {
              throw new System.Exception($"Invalid digit detected in where syntax. The variable name goes first in where/{foundSetter} Syntax");
            }
            if(scopedVarName[0] == StringQuotesSymbol) {
              throw new System.Exception($"Invalid string detected in where syntax. The variable name goes first in where/{foundSetter} Syntax");
            }
            //TODO: standardize variable name rules:
            // Can contain: a-z,A-Z,0-9,- and _.
            // must begin with a-z,A-Z, or _.
            if(!char.IsLetter(scopedVarName[0]) && scopedVarName[0] != '_') {
              throw new System.Exception($"Invalid variable name detected in where syntax. The variable name goes first in where/{foundSetter} Syntax.");
            }

            variableMap.Value.Add(
              scopedVarName, 
              _parseParam(
                ref potentialScopedVariable, 
                typeof(Token),
                lineNumber
            ));
          }
          else {
            if(inClosure) {
              throw new ArgumentException($"Improper with syntax. Make sure you close the list with a ].");
            }

            finished = true;
          }
        } while(!finished);

        return variableMap;
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
        if(expectedParamReturnType == typeof(Collection) || firstParamStub.StartsWith(CollectionStartSymbol)) {
          Collection collection = _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber);
          fullRemaininglineText = fullRemaininglineText.Trim();

          // check for concat
          return _parseAnyTrailingCollectionConcatinators(
            ref fullRemaininglineText,
            expectedParamReturnType,
            lineNumber, 
            collection
          );
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
        if(fullRemaininglineText.StartsWith((char)Comparitors.NOT) || fullRemaininglineText.StartsWith(Ows.NotComparitorPrefixPhrase)) {
          return _parseCondition(ref fullRemaininglineText, lineNumber);
        }

        /// if it's a string
        if(firstParamStub.FirstOrDefault() == StringQuotesSymbol) {
          String @string = _parseString(ref fullRemaininglineText);
          IParameter concatinator 
            = _parseAnyTrailingStringConcatinators(ref fullRemaininglineText, @string, lineNumber);

          return concatinator ?? @string;
        }

        /// numbers
        /*if(firstParamStub.FirstOrDefault().Equals('#')) {
          string textualNumberString = firstParamStub
            .UntilNot(c => char.IsLetterOrDigit(c) || c == '-' || c == DecimalSymbol)
            .Replace('-', ' ')
            .Replace(DecimalSymbol.ToString(), "point");
          throw new NotImplementedException($"Number text to number format not yet supported.");
        }*/

        // if it's a plain number
        if(char.IsNumber(firstParamStub.FirstOrDefault())) {
          Number number = _parseNumber(ref fullRemaininglineText);

          // check for opperators
          MathOpperator compiledOpperaton = _parseAnyTrailingMathOpperators(
            ref fullRemaininglineText, 
            number,
            lineNumber
          );

          return compiledOpperaton ?? (IParameter)number;
        }

        // if the whole thing is a closure, remove the closure and parse:
        if(fullRemaininglineText[0] == LogicStartSymbol) {
          string closure = fullRemaininglineText.UntilClosure('(', ')');
          fullRemaininglineText = fullRemaininglineText[(closure.Length + 2)..].Trim();
          IParameter value = _parseParam(ref closure, expectedParamReturnType, lineNumber);
          return _tryToParseAnyPotentialConcatinatorsOrOpperators(ref fullRemaininglineText, value, expectedParamReturnType, lineNumber);
        }


        // check if it's a command by chance that returns what we want
        if(Program.Context.Commands.ContainsKey(firstParamStub)) {
          IParameter parameter =  _parseCommand(ref fullRemaininglineText, lineNumber);
          return _tryToParseAnyPotentialConcatinatorsOrOpperators(ref fullRemaininglineText, parameter, expectedParamReturnType, lineNumber);
        }

        /// lastly, assume it's a plain old variable
        IParameter param = _parseExistingVariable(ref fullRemaininglineText);
        return _tryToParseAnyPotentialConcatinatorsOrOpperators(ref fullRemaininglineText, param, expectedParamReturnType, lineNumber);
      }

      IParameter _tryToParseAnyPotentialConcatinatorsOrOpperators(ref string fullRemainingLineText, IParameter current, Type expectedParamType, int lineNumber) {
        fullRemainingLineText = fullRemainingLineText.Trim();
        // nothing left to parse?:
        if(string.IsNullOrWhiteSpace(fullRemainingLineText) || fullRemainingLineText.FirstOrDefault() == FunctionSeperatorSymbol) {
          return current;
        }

        Type expectedCommandReturn = null;

        // if it's a command, check the expected return types:
        if(current is Command command) {
          expectedCommandReturn = command.Archetype.ExpectedReturnTypes.First();
          // if the current expected type is generic, and we have an expected return, replace the expected type:
          if((expectedParamType == typeof(Token)
              || expectedParamType == typeof(IParameter)
              || expectedParamType == typeof(Variable)
              || expectedParamType == typeof(Command))
            && (expectedCommandReturn is not null)
          ) {
            expectedParamType = expectedCommandReturn;
          }
        }

        if(current is Number || expectedParamType == typeof(Number)
          || (expectedCommandReturn is not null && expectedCommandReturn == typeof(Number))
        ) {
         return _parseAnyTrailingMathOpperators(ref fullRemainingLineText, current, lineNumber) ?? current;
        }
        else if(current is String || expectedParamType == typeof(String)
          || (expectedCommandReturn is not null && expectedCommandReturn == typeof(String))
        ) {
          return _parseAnyTrailingStringConcatinators(ref fullRemainingLineText, current, lineNumber) ?? current;
        }
        else if(current is Boolean || typeof(IConditional).IsAssignableFrom(expectedParamType)
          || (expectedCommandReturn is not null && typeof(IConditional).IsAssignableFrom(expectedCommandReturn))
        ) {
          return _parseCondition(ref fullRemainingLineText, lineNumber, current);
        }
        else if(current is Collection || typeof(Collection).IsAssignableFrom(expectedParamType)
          || (expectedCommandReturn is not null && typeof(Collection).IsAssignableFrom(expectedCommandReturn))
        ) {
          return _parseAnyTrailingCollectionConcatinators(ref fullRemainingLineText, expectedParamType, lineNumber, current);
        }
        // else we have to work with the symbols we have and make assumptions.
        else {
          if(fullRemainingLineText.Trim().StartsWith(new string[] {
            ConcatPhrase,
            CollectionExclusionPhrase
          }.Concat(ComparitorPhrases)
            .Concat(NumberOpperatorPhrases)
            .Concat(ComparitorSymbols.Select(c => c.ToString()))
            .Concat(NumberOpperatorSymbols.Select(c => c.ToString()))
          , out string foundOperator)
          ) {
            bool isVauge = false;
            // if it's a symbol:
            if(foundOperator.Length == 1) {
              // comparitor symbols assume boolean, except &
              if(ComparitorSymbols.Contains(foundOperator[0])) {
                if(foundOperator[0] == (char)Comparitors.AND) {
                  isVauge = true;
                } else
                  return _parseCondition(ref fullRemainingLineText, lineNumber, current);
              }// numeric is assumed for all but + and - 
              else if (NumberOpperatorSymbols.Contains(foundOperator[0])) {
                if(foundOperator[0] == (char)Opperators.PLUS
                  || foundOperator[0] == (char)Opperators.MINUS) {
                  isVauge = true;
                }
                else
                  return _parseAnyTrailingMathOpperators(ref fullRemainingLineText, current as Number, lineNumber);
              }
            } // if it's a phrase:
            else {
              // comparitor phrases assume boolean, except AND
              if(ComparitorPhrases.Contains(foundOperator)) {
                if(foundOperator == AndConcatPhrase) {
                  isVauge = true;
                }
                else
                  return _parseCondition(ref fullRemainingLineText, lineNumber, current);
              } // numeric is assumed for all but + and -
              else if(NumberOpperatorPhrases.Contains(foundOperator)) {
                if(foundOperator == Opperators.PLUS.ToString()
                  || foundOperator == Opperators.MINUS.ToString()) {
                  isVauge = true;
                }
                else
                  return _parseAnyTrailingMathOpperators(ref fullRemainingLineText, current as Number, lineNumber);
              } // collection phrases
              else if(foundOperator == ConcatPhrase || foundOperator == CollectionExclusionPhrase) {
                return _parseAnyTrailingCollectionConcatinators(ref fullRemainingLineText, expectedParamType, lineNumber, current as Collection);
              }
            }

            if(isVauge) {
              fullRemainingLineText = fullRemainingLineText[foundOperator.Length..].Trim();
              UnknownOperator @return 
                = Command.Types.Get<UnknownOperator.Type>()
                  .Make(Program, new IParameter[] {
                    current,
                    _parseParam(ref fullRemainingLineText, expectedParamType, lineNumber)
                  }, foundOperator);
              return @return;
            }
          }
        }

        throw new System.ArgumentException($"Unrecognized operation or parameter at\n: {fullRemainingLineText}");
      }

      MathOpperator _parseAnyTrailingMathOpperators(ref string fullRemaininglineText, IParameter left, int lineNumber) {
        MathOpperator compiledOpperaton = null;
        while(fullRemaininglineText.StartsWith(new string[] {
        // find add for collections
          AndConcatPhrase,
          ((char)Comparitors.AND).ToString()
        }.Concat(NumberOpperatorSymbols.Select(x => x.ToString()))
          // todo: make constants
          .Concat(Enum.GetValues(typeof(Opperators))
            .Cast<Opperators>()
            .Select(s => $"{s.ToString().Replace('_', '-')} "))
          // While we find an opperation prefix:
        , out string foundPrefix)) {

          // get the opperator
          Opperators opperation;
          if(foundPrefix.Length > 1) {
            if(foundPrefix.Equals(AndConcatPhrase)) {
              opperation = Opperators.PLUS;
            }
            else
              opperation = Enum.Parse<Opperators>(foundPrefix.Trim());
          }
          else {
            if(foundPrefix[0].Equals((char)Comparitors.AND)) {
              opperation = Opperators.PLUS;
            }
            else
              opperation = (Opperators)foundPrefix[0];
          }

          fullRemaininglineText = fullRemaininglineText[foundPrefix.Length..];
          IParameter right = null;
          // squared is special, and only has one param
          if(opperation != Opperators.SQUARED) {
            right = _parseParam(ref fullRemaininglineText, typeof(Number), lineNumber);
          }

          compiledOpperaton = Command.Types.Get<MathOpperator.Type>()
            .Make(
              Program,
              new IParameter[] {
                  compiledOpperaton ?? left,
                  right
              },
              opperation
            );
          fullRemaininglineText = fullRemaininglineText.Trim();
        }

        return compiledOpperaton;
      }

      IParameter _parseAnyTrailingStringConcatinators(ref string fullRemaininglineText, IParameter left, int lineNumber) {
        Command compiledConcatinator = null;

        // concat remaining strings
        while(fullRemaininglineText.StartsWith(new string[] {
            // find add for collections
            AndConcatPhrase,
            Opperators.PLUS.ToString(),
            ((char)Comparitors.AND).ToString(),
            ((char)Opperators.PLUS).ToString()
          }, out string foundPrefix)) {
          fullRemaininglineText = fullRemaininglineText[foundPrefix.Length..];
          IParameter right = _parseParam(ref fullRemaininglineText, typeof(String), lineNumber);
          compiledConcatinator = Command.Types.Get<StringConcatinator.Type>()
            .Make(
              Program,
              new IParameter[] {
                  compiledConcatinator ?? left,
                  right
              }
            );
          fullRemaininglineText = fullRemaininglineText.Trim();
        }

        return compiledConcatinator;
      }

      /// <summary>
      /// Parse any trailing concatinators after a collection and add them to the main collection object.
      /// This does nothing if there are no concatinators.
      /// </summary>
      IParameter _parseAnyTrailingCollectionConcatinators(ref string fullRemaininglineText, Type expectedParamReturnType, int lineNumber, IParameter left) {
        CollectionConcatinator compiledConcatinator = null;
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
          compiledConcatinator = Command.Types.Get<CollectionConcatinator.Type>()
            .Make(Program, new IParameter[] {
              compiledConcatinator == null
                ? left
                : compiledConcatinator,
              _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber)
            }, isAdd);
          /*if(isAdd) {
            collection.Value.AddRange(_parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber).Value);
          }
          else {
            foreach(var o in _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber).Value) {
              collection.Value.Remove(o);
            }
          }*/
          fullRemaininglineText = fullRemaininglineText.Trim();
        }

        return compiledConcatinator == null
          ? left
          : compiledConcatinator;
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
        Type collectionItemType = typeof(IParameter);

        if(expectedType is not null
          && typeof(Collection<>).IsAssignableFrom(expectedType)
        ) {
          collectionItemType 
            = expectedType.GetInheritedGenericTypes(typeof(Collection<>)).FirstOrDefault();
        }

        Collection collection = new(Program, list, collectionItemType);
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
          if(CollectAllPhrase.Length > 1 || (fullRemainingLineText[foundPhrase.Length..].FirstOrDefault() == ' ' || fullRemainingLineText[foundPhrase.Length..].FirstOrDefault() == ']')) {
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

      IParameter _parseCondition(ref string fullRemainingLineText, int lineNumber, IParameter left = null) {
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
            ComparitorSymbols.Select(ch => ch.ToString())
              // phrases
              .Concat(ComparitorPhrases)
              .Append(LogicStartSymbol.ToString())
              .Append(LogicEndSymbol.ToString())
              .Append(FunctionSeperatorSymbol.ToString()),
            out string foundConditionOrEnding
          );

          parsed += leftConditionText;
          fullRemainingLineText = fullRemainingLineText[leftConditionText.Length..];
          if(foundConditionOrEnding is null) {
            string foundNot = null;
            if(conditionLogicContainerDepth > 0) {
              throw new ArgumentException($"Condition closure not closed. Add a ')'");
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

            // If it's not, trim, and return a NOT of the actual comparitor:
            if(foundConditionOrEnding == NotComparitorPrefixPhrase || foundConditionOrEnding[0] == (char)Comparitors.NOT) {
              comparitor = Comparitors.NOT;
              fullRemainingLineText = fullRemainingLineText[foundConditionOrEnding.Length..];
              return Command.Types.Get<Condition.Type>().Make(Program, new[] {
                _parseCondition(ref fullRemainingLineText, lineNumber)
              }, comparitor);
            }

            if(foundConditionOrEnding.EndsWith(FunctionSeperatorSymbol)) {
              if(Program.Context.Commands.ContainsKey(leftConditionText.Trim())) {
                identityCondition = _parseCommand(ref fullRemainingLineText, lineNumber);
              } else
                identityCondition = _parseCondition(ref leftConditionText, lineNumber);
            }

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
            left ??=
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

      Variable _parseExistingVariable(ref string fullRemaininglineText) {
        string varName = fullRemaininglineText.UntilNot(c => 
          char.IsLetterOrDigit(c)
          || c == '_'
          || c == '-'
        );
        fullRemaininglineText = fullRemaininglineText[varName.Length..].Trim();
        return _makeExistingVariable(varName.Trim());
      }

      /// <summary>
      /// Make or find a symbol represeing an existing variable
      /// </summary>
      Variable _makeExistingVariable(string variableName) {
        return new ScopedVariable(Program, variableName);
      }

      /// <summary>
      /// Parse the next bit of the remaining line as a string
      /// </summary>
      String _parseString(ref string fullRemaininglineText) {
        string value = fullRemaininglineText.Skip(1).Until(StringQuotesSymbol).Trim();
        fullRemaininglineText = fullRemaininglineText[(value.Length + 2)..].Trim();
        return new String(Program, value);
      }

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
