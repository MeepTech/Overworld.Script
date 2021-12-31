using Meep.Tech.Data;
using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// An executable ows program
    /// </summary>
    public partial class Program {

      /// <summary>
      /// The context of this program relating to the world and controller that executed it
      /// </summary>
      public ContextData Context {
        get;
      }

      /// <summary>
      /// The variables universal to all parts of this program
      /// </summary>
      internal Dictionary<string, Variable> _globals
        = new();

      /// <summary>
      /// The variables unique to each character, by variable name
      /// </summary>
      internal Dictionary<string, Dictionary<string, Variable>> _variablesByCharacter
        = new();

      /// <summary>
      /// The names of local variables
      /// </summary>
      static HashSet<string> _localVariableNames
        = new();

      /// <summary>
      /// The line the program starts at
      /// </summary>
      public int StartLine {
        get;
        internal set;
      } = 0;

      /// <summary>
      /// The raw text of the program before it's initial/start point
      /// </summary>
      public string PreStartRawText {
        get;
        internal set;
      }

      /// <summary>
      /// The raw text of the program after it's initial/start point
      /// </summary>
      public string PostStartRawText {
        get;
        internal set;
      }

      /// <summary>
      /// The raw text of the entire program
      /// </summary>
      public string RawText {
        get;
        internal set;
      }

      /// <summary>
      /// Total line count
      /// </summary>
      public int LineCount {
        get;
        internal set;
      }

      /// <summary>
      /// Label name keys and what line they relate to
      /// </summary>
      internal Dictionary<string, int> _labelsByLineNumber
        = new();

      /// <summary>
      /// The commands by line
      /// </summary>
      internal OrderedDictionary<double, Command> _commands
        = new();

      /// <summary>
      /// Make and compile a new Ows program from a collection of lines
      /// </summary>
      public Program(ContextData context) {
        Context = context;
      }

      /// <summary>
      /// Execute this program as a specific character
      /// TODO: Runs should be wrapped in an object.
      /// </summary>
      public Variable ExecuteAs(Data.Character character)
        => _executeAllStartingAtLine(StartLine, character, null);

      /// <summary>
      /// Try to get the line number from some label text
      /// </summary>
      public int GetLineNumberForLabel(string labelName, Command.Context context) {
        if(_labelsByLineNumber.TryGetValue(labelName, out var found)) {
          return found;
        }
        else
          return _labelsByLineNumber[context.GetFirstVariable<String>(labelName, context.Executor.Id).Value];
      }

      /// <summary>
      /// Try to get a world or program level variable by name for this program
      /// </summary>
      public bool TryToGetVariableByName(string variableName, out Variable variable) {
        if(_globals.TryGetValue(variableName, out var foundGlobal)) {
          variable = foundGlobal;

          return true;
        } else if(Ows._globals.TryGetValue(variableName, out var foundProgramLevel)) {
          variable = foundProgramLevel;

          return true;
        }

        variable = null;
        return false;
      }

      /// <summary>
      /// Try to get a world or program level variable by name for this program
      /// </summary>
      public Variable TryToGetVariableByName(string variableName) {
        if(_globals.TryGetValue(variableName, out var foundGlobal)) {
         return foundGlobal;
        } else if(Ows._globals.TryGetValue(variableName, out var foundProgramLevel)) {
          return foundProgramLevel;
        }

        return null;
      }

      /// <summary>
      /// Try to get a world or program level variable by name for this program
      /// </summary>
      public Variable GetVariableByName(string variableName) {
        try {
          if(_globals.TryGetValue(variableName, out var foundGlobal)) {
            return foundGlobal;
          } else
            return Ows._globals[variableName];
        } catch(Exception e) {
          throw new KeyNotFoundException($"Global Variable with name {variableName}, not found", e);
        }
      }

      /// <summary>
      /// Try to get a world or program level variable by name for this program
      /// </summary>
      public Variable GetVariableByName(Data.Character @for, string variableName) {
        try {
          if(_globals.TryGetValue(variableName, out var foundGlobal)) {
            return foundGlobal;
          } else
            return Ows._globalVariablesByCharacter[@for.Id][variableName];
        } catch (Exception e) {
          throw new KeyNotFoundException($"Variable with name {variableName}, not found for character: {@for.Name}", e);
        }
      }

      /// <summary>
      /// Try to get a world or program level variable by name for this program
      /// </summary>
      public Variable GetVariableByName(string characterId, string variableName) {
        try {
          if(_globals.TryGetValue(variableName, out var foundGlobal)) {
            return foundGlobal;
          } else
            return Ows._globalVariablesByCharacter[characterId][variableName];
        } catch (Exception e) {
          throw new KeyNotFoundException($"Variable with name {variableName}, not found for character: ID: {characterId}", e);
        }
      }

      /// <summary>
      /// Try to get a character level varaible by name from the world or program 
      /// </summary>
      public bool TryToGetVariableByName(Data.Character @for, string variableName, out Variable variable) {
        // shadowing Program level locals first
        if(_variablesByCharacter[@for.Id].TryGetValue(variableName, out var foundGlobal)) {
          variable = foundGlobal;

          return true;
        } else if(Ows._globalVariablesByCharacter[@for.Id].TryGetValue(variableName, out var foundProgramLevel)) {
          variable = foundProgramLevel;

          return true;
        }

        variable = null;
        return false;
      }

      /// <summary>
      /// Try to get a character level varaible by name from the world or program 
      /// </summary>
      public bool TryToGetVariableByName(string characterId, string variableName, out Variable variable) {
        // shadowing Program level locals first
        if(_variablesByCharacter.TryGetValue(characterId, out var characterVars) && characterVars.TryGetValue(variableName, out var foundGlobal)) {
          variable = foundGlobal;

          return true;
        } else if(Ows._globalVariablesByCharacter.TryGetValue(characterId, out var characterVars3) && characterVars3.TryGetValue(variableName, out var foundProgramLevel)) {
          variable = foundProgramLevel;

          return true;
        }

        variable = null;
        return false;
      }

      /// <summary>
      /// Get a matching character by id or unique name
      /// TODO: these should be turned into modular object fetcher/builder plugins
      /// </summary>
      public Data.Character GetCharacter(string characterNameOrId) {
        if (Context.Characters.TryGetValue(characterNameOrId, out Data.Character found)) {
          return found;
        }

        return Context.Characters
          .FirstOrDefault(character => character.Value.UniqueName.Equals(characterNameOrId)).Value;
      }

      /// <summary>
      /// Get a matching entity by id or  name
      /// TODO: these should be turned into modular object fetcher/builder plugins
      /// </summary>
      public Data.Entity GetEntity(string entityNameOrId) {
        if (Context.Entities.TryGetValue(entityNameOrId, out Data.Entity found)) {
          return found;
        }

        return Context.Entities
          .FirstOrDefault(character => character.Value.Name.Equals(entityNameOrId)).Value;
      }

      /// <summary>
      /// Execute the whole program, starting at the given line
      /// </summary>
      internal Variable _executeAllStartingAtLine(int line, Data.Character character, int? fromLine = null, VariableMap scopedVariables = null) {
        Variable result = null;
        while(line <= LineCount) {
          while(!_commands.Contains(line)) {
            line++;
            if(line > LineCount) {
              break;
            }
          }
          if(line > LineCount) {
            break;
          }

          Command command = _commands[(double)line];

          /// END
          if(command.Archetype is Command.END) {
            return new EndResult(this);
          }

          // GO-TO
          if(command.Archetype is Command.GO_TO) {
            try {
              command._executeWithExtraParams(character, new List<IParameter> { new Number(this, line) }, scopedVariables);
            } catch (InvalidOperationException e) {
              throw new InvalidOperationException($"Failed to execute GOTO on line {line}.", e);
            }
            // if it was a goto command, it is expected to have finished the rest itself.
            break;
          } // GO-BACK is implimented here:
          else if(command.Archetype is Command.GO_BACK) {
            if(fromLine.HasValue) {
              fromLine = line;
              line = fromLine.Value + 1;
              result = null;
              continue;
            } else
              throw new ArgumentNullException($"No available From Line to go back to. GO-BACK may only work once.");
          }

          try {
            if(scopedVariables != null) {
              result
                 = command._executeWithExtraParams(character, scopedParameters: scopedVariables);
            }
            else {
              result
                = command.ExecuteFor(character);
            }
          }
          catch(InvalidOperationException e) {
            throw new InvalidOperationException($"Failed to execute command: {command}, on line: {line}.", e);
          }

          // DOWITH Call:
          if(result is DoWithStartResult doWithStart) {
            result = _executeAllStartingAtLine(
             doWithStart._targetLineNumber,
             character,
             line,
             doWithStart._scopedParams
            );
          }

          if(result is EndResult) {
            break;
          }

          /// RETURN
          if(result is ReturnResult returnResult) {
            break;
          }

          // break if we came back from a goto
          // TODO: this should return goto-result with a value.
          if(result is GoToResult) {
            break;
          }

          line++;
        }

        if(result is EndResult) {
          return null;
        }

        if(fromLine != null) {
          if(result is ReturnResult) {
            return result;
          }
          return result is GoToResult existing
             ? existing
             : new GoToResult(this) { _fromLine = line};
        }

        return result is ReturnResult
          ? result
          : null;
      }

      /// <summary>
      /// Get all the variables of a given object type
      /// </summary>
      internal Collection _getAllObjectsOfType(Type collectionItemType) {
        if(!typeof(Object).IsAssignableFrom(collectionItemType)) {
          throw new ArgumentException($"ALL(*) Syntax can only be used with collections of Objects");
        }

        // TODO: Cache these and make these more modular
        if(collectionItemType.Equals(typeof(Character))) {
          return new Collection<Character>(this, Context.Characters.Values.ToList());
        } else if(collectionItemType.Equals(typeof(Entity))) {
          return new Collection<Character>(this, Context.Characters.Values.ToList());
        } else
          throw new NotSupportedException($"The ALL Syntax (*), can only be used with entities and characters atm");
      }

      /// <summary>
      /// Set the variable for the given characters
      /// </summary>
      internal void _setVariableForCharacters(IEnumerable<string> characterIds, string name, object value, Variable.Scopes scope) {
        _onSetNammedVariable(name);
        characterIds.ForEach(characterId => {
          if(_variablesByCharacter.TryGetValue(characterId, out var characterVariables)) {
            characterVariables[name] = Variable.Make(this, name, value);
          } else
            characterVariables = new Dictionary<string, Variable> {
              { name, Variable.Make(this, name, value) }
            };
        });
      }

      /// <summary>
      /// Set a global(not character specific) variable in a scope
      /// </summary>
      internal void _setGlobalVariable(Variable.Scopes scope, string name, object value) {
        if(scope == Variable.Scopes.Program) {
          _setGlobalProgramVariable(name, value);
        } else {
          _setGlobalWorldVariable(name, value);
        }
      }

      /// <summary>
      /// Add a variable to the global "program" context
      /// </summary>
      void _setGlobalProgramVariable(string name, object value) {
        _onSetNammedVariable(name, isProgramLevel: true);
        _globals.Add(name, Variable.Make(this, name, value));
      }

      /// <summary>
      /// add a variable to the global "world" context
      /// </summary>
      void _setGlobalWorldVariable(string name, object value) {
        _onSetNammedVariable(name, isGlobal: true);
        Ows._globals.Add(name, Variable.Make(this, name, value));
      }

      /// <summary>
      /// logic executed on adding a variable to the runtime.
      /// </summary>
      void _onSetNammedVariable(string name, bool isGlobal = false, bool isProgramLevel = false) {
        if(Context.Commands.ContainsKey(name)) {
          throw new ArgumentException(name, $"Variable name conflict, variable shares a name with the Command: {name}");
        }
        if(isGlobal || isProgramLevel) {
          if(_localVariableNames.Contains(name)) {
            throw new ArgumentException(name, $"Variable name conflict, local and global or program level variable share a name: {name}");
          }
        }
        else {
          _localVariableNames.Add(name);
        }

        if(!isGlobal && Ows._globals.ContainsKey(name)) {
          throw new ArgumentException(name, $"Cannot have another variable with a name matching existing global variable: {name}");
        }

        if(!isProgramLevel && _globals.ContainsKey(name)) {
          throw new ArgumentException(name, $"Cannot have another variable with a name matching existing top level Program variable: {name}");
        }
      }
    }
  }
}
