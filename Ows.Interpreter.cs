using Meep.Tech.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace Overworld.Script {

  public static partial class Ows {

    public class Interpreter {

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
        => rawFiles.OrderBy(raw => raw.filename).SelectMany(raw => raw.contents.Split(Environment.NewLine));

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
      public static Interpreter Build(Program.ContextData context, IEnumerable<(string filename, string contents)> rawFiles) {
        List<(string filename, string contents)> rawLines = new();
        List<(string filename, string contents)> rawPreInitLines = new();
        foreach(var (filename, contents) in rawFiles.OrderBy(file => file.filename)) {
          if(System.IO.Path.GetFileName(filename).StartsWith("_")) {
            rawPreInitLines.Add((filename, contents));
          } else
            rawLines.Add((filename, contents));
        }

        Interpreter @return = new(context);
        @return.Build(JoinOwsFiles(rawFiles), JoinOwsFiles(rawPreInitLines));
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
      public Program Build([NotNull] string rawLines, string preInitialLines = null)
        => Build(rawLines.Split(Environment.NewLine), preInitialLines?.Split(Environment.NewLine));

      /// <summary>
      /// Build a new program from a bunch of files
      /// </summary>
      public Program Build(IEnumerable<string> rawLines, IEnumerable<string> preInitialLines = null) {
        _cleanProgram();

        /// save the raw text
        preInitialLines ??= Enumerable.Empty<string>();
        Program.PreStartRawText =
          string.Join(Environment.NewLine, preInitialLines)
            .Trim()
            .ToUpper();

        Program.PostStartRawText ??= string.Join(Environment.NewLine, rawLines)
          .Trim()
          .ToUpper();

        Program.RawText ??= string.Join(
          Environment.NewLine,
          new[] { Program.PreStartRawText, Program.PostStartRawText }
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
              new string(currentCommandText.Trim().Substring(1).Until(LabelEndSymbol)).Trim().ToUpper(),
              lineNumber
            );
            currentCommandText = currentCommandText.After(FunctionSeperatorSymbol);
          }

          // if the next lines have ..., join them to the current line
          if(preCompiledLines.Length > preCompiledLineIndex + 1) {
            // TODO: make the ... a const
            while(preCompiledLines[preCompiledLineIndex + 1].Trim().StartsWith("...")) {
              currentCommandText += preCompiledLines[preCompiledLineIndex + 1].Substring(3);
              preCompiledLineIndex++;
              if(preCompiledLines.Length <= preCompiledLineIndex) {
                break;
              }
            }
          }

          if(!string.IsNullOrWhiteSpace(currentCommandText)) {
            currentCommandText = currentCommandText.ToUpper();
            try {
              /// Process the command
              Program._commands.Add(lineNumber,
                _parseCommand(ref currentCommandText, lineNumber));
            } catch(Exception e) {
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
        string currentFunctionName = remainingCommandText.Until(FunctionSeperatorSymbol).Trim();
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
          } else

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
              remainingCommandText = remainingCommandText.Substring(parts.First().Length)
                .Substring(1).Trim();
            } else if(remainingCommandText.Contains($" {SetToPhrase} ")) {
              parts = remainingCommandText.Split($" {SetToPhrase} ");
              remainingCommandText = remainingCommandText.Substring(parts.First().Length)
                .Substring($" {SetToPhrase} ".Length).Trim();
            }

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
            parameters.Add(
              _parseParam(
                ref remainingCommandText,
                commandType.ParameterTypes[paramIndex],
                lineNumber
              )
            );
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

        // if we expect a command, try to find it:
        if(expectedParamReturnType == typeof(Command)) {
          if(Program.Context.Commands.TryGetValue(firstParamStub.Until(FunctionSeperatorSymbol), out Command.Type found)) {
            // if (expectedType == null || (expectedType == found.ReturnType))
            return _parseCommand(ref fullRemaininglineText, lineNumber);
          } else
            throw new MissingMethodException(nameof(Ows), firstParamStub.Until(FunctionSeperatorSymbol));
        }

        // if we expect a collection of somekind, it could be a collection of strings, or entities.
        if(expectedParamReturnType.IsAssignableToGeneric(typeof(Collection<>))) {
          return _parseCollection(ref fullRemaininglineText, expectedParamReturnType, lineNumber);
        }

        /// if it's a plain old bool/conditional
        if(firstParamStub.StartsWith("TRUE", true, null) && firstParamStub.Length == 4) {
          fullRemaininglineText = fullRemaininglineText.Substring(firstParamStub.Length).Trim();
          return new Boolean(Program, true);
        }
        if(firstParamStub.StartsWith("FALSE", true, null) && firstParamStub.Length == 4) {
          fullRemaininglineText = fullRemaininglineText.Substring(firstParamStub.Length).Trim();
          return new Boolean(Program, false);
        }

        // check if it's a conditional that's expected.
        if(expectedParamReturnType.Equals(typeof(IConditional))) {
          return _parseCondition(ref fullRemaininglineText, lineNumber);
        }

        /// if it's a string
        if(firstParamStub.FirstOrDefault() == StringQuotesSymbol) {
          /// TODO: Check for string opperation here!
          Regex rx = new Regex(@""".\ (\+)|(\ AND\ )|(\ PLUS\ ).\ """);
          string text = "This is a string [Ref:1234/823/2]";
          MatchCollection matches = rx.Matches(text);
          if(firstParamStub.Contains("+")
            || firstParamStub.Contains(" AND ")
            || firstParamStub.Contains(" PLUS ")
          ) {
            return _parseStringOpperator(ref fullRemaininglineText);
          }

          return _parseString(ref fullRemaininglineText, firstParamStub);
        }

        if(firstParamStub.FirstOrDefault().Equals('#')) {
          if(firstParamStub.Intersect(NumberOpperatorChars).Any()) {
            return _parseStringOpperator(ref fullRemaininglineText);
          }
          throw new NotImplementedException($"# based string Number format (ex: #One Hundred Twenty + ...) is not yet supporrted");
        }

        /// if it's a plain number
        if(char.IsNumber(firstParamStub.FirstOrDefault())) {
          if(firstParamStub.Intersect(NumberOpperatorChars).Any()) {
            return _parseStringOpperator(ref fullRemaininglineText);
          }

          // get all characters until we have a non number/decimal point, or a space.
          int decimalCount = 0;
          string value = firstParamStub.Until(chararcter => {
            // allow one decimal
            if (chararcter.Equals('.') && decimalCount == 0) {
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
          fullRemaininglineText = fullRemaininglineText.After(FunctionSeperatorSymbol).Trim();
          return new Number(Program, double.Parse(value));
        }

        // IF we've exausted most options and it begins with a not, try to make this a condition
        if(fullRemaininglineText.StartsWith(Comparitors.NOT.ToString() + "-")) {
          return _parseCondition(ref fullRemaininglineText, lineNumber);
        }

        // check if it's a command by chance that returns what we want
        if(Program.Context.Commands.ContainsKey(firstParamStub)) {
          return _parseCommand(ref fullRemaininglineText, lineNumber);
        }

        /// lastly, assume it's a plain old variable
        return _parseExistingVariable(ref fullRemaininglineText, string.IsNullOrWhiteSpace(firstParamStub) ? fullRemaininglineText : firstParamStub);
      }

      Command _parseStringOpperator(ref string fullRemaininglineText) {
        throw new NotImplementedException();
      }

      IParameter _parseCondition(ref string fullRemainingLineText, int lineNumber) {
        string preparsed = fullRemainingLineText.Trim();
        //int skipUntilSubCollectionEndCount = 0;
        //int parsedCharacterCount = 0;
        //bool firstCharacter = true;
        IParameter identityCondition = null;
        //IParameter conditionRight = null;
        Comparitors? comparitor = null;
        int conditionLogicContainerDepth = 0;

        char endCharacter = FunctionSeperatorSymbol;
        if(fullRemainingLineText[0].Equals(LogicStartSymbol)) {
          fullRemainingLineText = fullRemainingLineText.Substring(1).Trim();
          endCharacter = LogicEndSymbol;
        }

        string leftConditionText = null;
        do {
          leftConditionText = (leftConditionText ?? "") + fullRemainingLineText.UntilAny(
            // TODO: cache this whole thing in a static
            // symbols
            ComparitorSymbols.Select(ch => ch.ToString()).ToList().Except(new []{"!" })
              // phrases
              .Concat(ComparitorPhrases.Except(new []{"NOT"}).Select(phrase => $" {phrase} "))
              .Append(LogicStartSymbol.ToString())
              .Append(LogicEndSymbol.ToString()),
            out string foundEnding
          );

          fullRemainingLineText = fullRemainingLineText.Substring(leftConditionText.Length);

          if(foundEnding is null) {
            if(endCharacter.Equals(LogicEndSymbol)) {
              throw new ArgumentException($"Condition closure not closed. Add a ')'");
            } else if(fullRemainingLineText.StartsWith("NOT-")) {
              comparitor = Comparitors.NOT;
            } else {
              comparitor = Comparitors.IDENTITY;
            }

            identityCondition = (IParameter)_parseParam(ref leftConditionText, typeof(Token), lineNumber);
            fullRemainingLineText = leftConditionText;
          } else {
            fullRemainingLineText = fullRemainingLineText.Substring(foundEnding.Length);
            // )
            if(endCharacter.Equals(LogicEndSymbol)) {
              // we found an existing container end:
              if(conditionLogicContainerDepth > 0) {
                conditionLogicContainerDepth--;
              } else if(leftConditionText.Length == fullRemainingLineText.Length) {
                if(fullRemainingLineText.StartsWith("NOT-")) {
                  comparitor = Comparitors.NOT;
                } else {
                  comparitor = Comparitors.IDENTITY;
                }
                // we found the end of our initial closure without anything, it's an identity.
                identityCondition = (IParameter)_parseParam(ref leftConditionText, typeof(Token), lineNumber);
              } else
                throw new ArgumentException($"Unexpected character {LogicEndSymbol} in condition.");

            } // (
            else if(foundEnding.Equals(LogicStartSymbol.ToString())) {
              conditionLogicContainerDepth++;
            }  // symbol
            else if(foundEnding.Length == 1) {
              comparitor = (Comparitors)foundEnding[0];
            } // word
            else {
              comparitor = Enum.Parse<Comparitors>(foundEnding.Trim());
            }
          }

          if(identityCondition is not null) {
            return identityCondition;
          } else {
            return Archetypes<Condition.Type>._.Make(Program, new IParameter[] {
              _parseParam(ref leftConditionText, typeof(IConditional), lineNumber),
              _parseParam(ref fullRemainingLineText, typeof(IConditional), lineNumber)
            }, comparitor);
          }
        } while(conditionLogicContainerDepth > 0);

        /// parse each char
       /* foreach(char character in fullRemainingLineText) {

          /// after checking for the all syntax:
          // add a sub collection count
          if(character.Equals(LogicStartSymbol)) {
            if(!firstCharacter) {
              skipUntilSubCollectionEndCount++;
            }
          }

          // skip sub collections
          if(skipUntilSubCollectionEndCount > 0) {
            if(character.Equals(LabelEndSymbol)) {
              skipUntilSubCollectionEndCount--;
            }
            parsedCharacterCount++;
            continue;
          }

          // AND or END found
          string remainingConditionText = fullRemainingLineText.Substring(parsedCharacterCount);
          bool usesSymbol;
          bool comparitorSymbol = false;
          string phrase = null;
          if((usesSymbol = character.Equals(LogicEndSymbol))
            || (usesSymbol = character.Equals(LogicStartSymbol))
            || (usesSymbol = character.Equals(FunctionSeperatorSymbol))
            || (usesSymbol = comparitorSymbol = CollectionItemSeperatorSymbols.Contains(character))
            || (character.Equals(' ')
              && (phrase = ComparitorPhrases.FirstOrDefault(
                comparitorPhrase => {
                  return  remainingConditionText.StartsWith($" {comparitorPhrase} ");
                })) != null)

          ) {
            if(usesSymbol) {
              if(comparitorSymbol) {
                comparitor = (Comparitors)character;
              }
              remainingConditionText = remainingConditionText.Substring(1).Trim();
              parsedCharacterCount++;
            } else {
              comparitor = (Comparitors)
                Enum.Parse(typeof(Comparitors), phrase.Replace('-', '_'));
              parsedCharacterCount += phrase.Length + 2;
            }

            if(conditionLeft is null) {
              /// if we parsed characters, the first half must be a plain token
              if(parsedCharacterCount > 0) {
                if ()
                var leftsideParse = fullRemainingLineText.Substring(0, parsedCharacterCount - (usesSymbol ? 1 : phrase.Length + 2));
                conditionLeft =
                    _parseParam(ref leftsideParse, typeof(Token)) as IParameter;
                fullRemainingLineText = leftsideParse.Trim();

                continue;
              } // if there are no characters but we found a NOT identity, what follows must be only the left half of a param behind a NOT
              else if(comparitor == Comparitors.NOT) {
                if(conditionLeft != null) {
                  throw new ArgumentException($"Having a Not- Syntax right after an identity indicates incorrect syntax. Use a Comparitor to compare them!");
                }
                fullRemainingLineText = fullRemainingLineText.Until('-');
                Boolean conditionLeftBool
                  = _parseParam(ref fullRemainingLineText, typeof(Boolean)) as Boolean;
                conditionLeft = conditionLeftBool.Not;

                continue;
              }
            } else {
              conditionRight = (IParameter)_parseParam(ref fullRemainingLineText, typeof(IConditional));

              return Archetypes<Condition.Type>._.Make(Program, new Token[] {
                  (Token)conditionLeft,
                  (Token)conditionRight
                }, comparitor);
            }

            // if this is the end
            if(character.Equals(CollectionEndSymbol) || character.Equals(FunctionSeperatorSymbol)) {
              parsedCharacterCount++;
              break;
            }

            continue;
          }

          // add char to parsed
          parsedCharacterCount++;
          firstCharacter = false;
        }*/

        throw new ArgumentException($"Could not parse condtion: \n{preparsed}");
      }

      Variable _parseExistingVariable(ref string fullRemaininglineText, string variableName) {
        fullRemaininglineText = fullRemaininglineText.Substring(variableName.Length).Trim();
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
      String _parseString(ref string fullRemaininglineText, string firstParamStub) {
        string value = firstParamStub.Skip(1).Until(StringQuotesSymbol).Trim();
        fullRemaininglineText = ((string)fullRemaininglineText.Skip(firstParamStub.Length + 2)).Trim();
        return new String(Program, value);
      }

      Collection _parseCollection(ref string fullRemaininglineText, Type expectedType, int lineNumber) {

        IList list;
        Collection collection;
        Type collectionItemType;
        // TODO: Impliment these as modular collection builders.
        if(expectedType.GenericTypeArguments.First().Equals(typeof(String))) {
          collectionItemType = typeof(String);
          list = new List<String>();
          collection = new Collection<String>(Program, list as IList<String>);
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
