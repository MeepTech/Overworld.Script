using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class Command {

      /// <summary>
      /// Context used for executing a command
      /// </summary>
      public struct Context : IEnumerable<IParameter> {

        /// <summary>
        /// Debug data passed along with the context.
        /// </summary>
        public class DebugData {

          /// <summary>
          /// Executes before each command line
          /// </summary>
          public Action<Context, int> BeforeLine;

          /// <summary>
          /// Executes after each command line
          /// </summary>
          public Action<Context, int> AfterLine;

          /// <summary>
          /// Executes before each command line
          /// </summary>
          public Action<Context> BeforeCommandExecution;
        }

        /// <summary>
        /// The executed command
        /// </summary>
        public Command Command {
          get;
          private set;
        }

        /// <summary>
        /// The character who executed the command
        /// </summary>
        public readonly Data.Character Executor;


        internal readonly VariableMap _temporaryScopedVariables;
        internal readonly Index _indexReplacement;
        internal IList<IParameter> _extraParameters;
        List<IParameter> _compiledParameters;
        internal DebugData _debugData;

        /// <summary>
        /// The ordered parameters passed to this command
        /// </summary>
        public readonly IReadOnlyList<IParameter> OrderedParameters
          => _compiledParameters;

        internal Context(
          Command command,
          Data.Character executor,
          IList<IParameter> extraParameters = null,
          VariableMap scopedParameters = null,
          Index indexReplacement = null,
          Program overrideProgram = null
        ) {
          Command = command;
          Executor = executor;
          _temporaryScopedVariables = scopedParameters ?? new VariableMap(overrideProgram ?? command?.Program);
          _indexReplacement = indexReplacement;
          _compiledParameters = null;
          _extraParameters = extraParameters ?? Enumerable.Empty<IParameter>().ToList();
          _debugData = null;
          _compileParams();
        }

        void _compileParams() {
          var indexReplacement = _indexReplacement;
          var executor = Executor;
          var scopedParameters = _temporaryScopedVariables;
          // TODO: is it faster to not copy this and remove the select?
          Context @this = this;

          _compiledParameters = Command?.Parameters?.Concat(_extraParameters)
            .Select(param => {
              if(param is PlaceholderIndex index) {
                if(indexReplacement is null) {
                  throw new ArgumentException($"Index provided as an argument but no replacement provided to the execute for function");
                }
                else
                  return indexReplacement;
              }
              else if(param is ScopedVariable characterSpecific) {
                return characterSpecific.GetFor(@this, executor.Id);
              }
              else
                return param;
            })?.ToList() ?? new List<IParameter>();
        }

        void _recompileParams(IList<IParameter> extraParameters = null) {
          _extraParameters = extraParameters ?? Enumerable.Empty<IParameter>().ToList();
          _compileParams();
        }

        internal Context _addExtraParameter(IParameter parameter) {
          _extraParameters.Add(parameter);
          _compiledParameters.Add(parameter);
          return this;
        }

        internal Context _swapTo(Command command, IList<IParameter> extraParameters = null) {
          Command = command;
          _recompileParams(extraParameters);

          return this;
        }

        #region Non-Generic

        /// <summary>
        /// Gets an item passed in to the current command as an ordered parameter by index
        /// </summary>
        public IParameter GetParameter(int index)
          => _compiledParameters[index];

        /// <summary>
        /// Gets an the ultimage variable value of a parameter passed in to the current command as an ordered parameter by index
        /// </summary>
        public Variable GetUltimateParameterVariable(int index)
          => _compiledParameters[index].GetUltimateVariableFor(this);

        /// <summary>
        /// Best method for getting variables by name.
        /// Gets the first variable that matches the name and charachter.
        /// First hits them scoped,
        /// then charachter specific
        /// then global
        /// </summary>
        public Variable GetFirstVariable(string name, string charachterId) {
          Variable found;
          if((found = TryToGetTempScopedVariable(name)) != null) {
            return found;
          }
          if(!string.IsNullOrWhiteSpace(charachterId)
            && (found = TryToGetCharacterSpecificVariable(charachterId, name)) != null
          ) {
            return found;
          }
          else
            return GetGlobalVariable(name);
        }

        /// <summary>
        /// Best method for getting variables by name.
        /// Gets the first variable that matches the name and charachter.
        /// First hits them scoped,
        /// then charachter specific
        /// then global
        /// </summary>
        public Variable TryToGetFirstVariable(string name, string charachterId) {
          Variable found;
          if((found = TryToGetTempScopedVariable(name)) != null) {
            return found;
          }
          if(!string.IsNullOrWhiteSpace(charachterId)
            && (found = TryToGetCharacterSpecificVariable(charachterId, name)) != null
          ) {
            return found;
          }
          else
            return TryToGetGlobalVariable(name);
        }

        /// <summary>
        /// Gets the first program or world level variable with the given name
        /// (not character specific)
        /// </summary>
        public Variable GetGlobalVariable(string name)
          => _temporaryScopedVariables.Value.TryGetValue(name, out var local)
            ? local.GetUltimateVariableFor(this)
            : Command.Program.GetVariableByName(name);

        /// <summary>
        /// Gets the first program or world level variable with the given name
        /// (not character specific)
        /// </summary>
        public Variable TryToGetGlobalVariable(string name)
          => ((Variable)(_temporaryScopedVariables.Value.TryGetValue(name, out var local)
            ? local
            : Command.Program.TryToGetVariableByName(name)));

        /// <summary>
        /// Gets the first character specific program or world level variable with the given name
        /// </summary>
        public Variable TryToGetCharacterSpecificVariable(string characterId, string name)
            => (Command.Program.TryToGetVariableByName(characterId, name, out var found)
              ? found
              : null);

        /// <summary>
        /// Gets the first temp scoped variable with the given name
        /// </summary>
        public Variable GetTempScopedVariable(string name)
          => _temporaryScopedVariables.Value[name]
            .GetUltimateVariableFor(this);

        /// <summary>
        /// Gets the first temp scoped variable with the given name
        /// </summary>
        public Variable TryToGetTempScopedVariable(string name)
          => _temporaryScopedVariables.Value.TryGetValue(name, out var found)
             ? found.GetUltimateVariableFor(this)
             : null;

        /// <summary>
        /// Gets the first program level variable with the given name
        /// </summary>
        public Variable GetGlobalProgramVariable(string name)
          => Command.Program._globals[name];

        /// <summary>
        /// Gets the first world level variable with the given name
        /// </summary>
        public Variable GetGlobalWorldVariable(string name)
          => Ows._globals[name];

        /// <summary>
        /// Gets the first charachter specific program or world level variable with the given name
        /// </summary>
        public Variable GetCharacterSpecificVariableFor(string characterId, string name)
          => Command.Program.GetVariableByName(characterId, name);

        /// <summary>
        /// Gets the first charachter specific world level variable with the given name
        /// </summary>
        public Variable GetWorldLevelCharacterSpecificVariableFor(string characterId, string name)
          => Ows._globalVariablesByCharacter[characterId][name];

        #endregion

        /// <summary>
        /// Gets an item passed in to the current command as an ordered parameter by index
        /// </summary>
        public TParameter GetParameter<TParameter>(int index)
          where TParameter : IParameter
            => (TParameter)_compiledParameters[index];

        /// <summary>
        /// Gets an the ultimage variable value of a parameter passed in to the current command as an ordered parameter by index
        /// </summary>
        public TVariable GetUltimateParameterVariable<TVariable>(int index)
          where TVariable : Variable
            => _compiledParameters[index].GetUltimateVariableAs<TVariable>(this);

        /// <summary>
        /// Best method for getting variables by name.
        /// Gets the first variable that matches the name and charachter.
        /// First hits them scoped,
        /// then charachter specific
        /// then global
        /// </summary>
        public TVariable GetFirstVariable<TVariable>(string name, string charachterId)
          where TVariable : Variable 
        {
          Variable found;
          if((found = TryToGetTempScopedVariable(name)) != null) {
            return (TVariable)found;
          }
          if(!string.IsNullOrWhiteSpace(charachterId)
            && (found = TryToGetCharacterSpecificVariable(charachterId, name)) != null
          ) {
            return (TVariable)found;
          }
          else
            return (TVariable)GetGlobalVariable(name);
        }

        /// <summary>
        /// Best method for getting variables by name.
        /// Gets the first variable that matches the name and charachter.
        /// First hits them scoped,
        /// then charachter specific
        /// then global
        /// </summary>
        public TVariable TryToGetFirstVariable<TVariable>(string name, string charachterId)
          where TVariable : Variable 
        {
          Variable found;
          if((found = TryToGetTempScopedVariable(name)) != null) {
            return (TVariable)found;
          }
          if(!string.IsNullOrWhiteSpace(charachterId)
            && (found = TryToGetCharacterSpecificVariable(charachterId, name)) != null
          ) {
            return (TVariable)found;
          }
          else
            return TryToGetGlobalVariable<TVariable>(name);
        }

        /// <summary>
        /// Gets the first program or world level variable with the given name
        /// (not character specific)
        /// </summary>
        public TVariable GetGlobalVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)(_temporaryScopedVariables.Value.TryGetValue(name, out var local)
              ? local
              : Command.Program.GetVariableByName(name));

        /// <summary>
        /// Gets the first program or world level variable with the given name
        /// (not character specific)
        /// </summary>
        public TVariable TryToGetGlobalVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)(_temporaryScopedVariables.Value.TryGetValue(name, out var local)
              ? local
              : Command.Program.TryToGetVariableByName(name));

        /// <summary>
        /// Gets the first character specific program or world level variable with the given name
        /// </summary>
        public TVariable TryToGetCharacterSpecificVariable<TVariable>(string characterId, string name)
          where TVariable : Variable
            => (TVariable)(Command.Program.TryToGetVariableByName(characterId, name, out var found)
              ? found
              : null);

        public TVariable GetTempScopedVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)_temporaryScopedVariables.Value[name];

        public TVariable GetGlobalProgramVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)Command.Program._globals[name];

        public TVariable GetGlobalWorldVariable<TVariable>(string name)
          where TVariable : Variable
            => (TVariable)Ows._globals[name];

        public TVariable GetLocalVariableFor<TVariable>(string characterId, string name)
          where TVariable : Variable
            => (TVariable)Command.Program.GetVariableByName(characterId, name);

        public TVariable GetGlobalVariableFor<TVariable>(string characterId, string name)
          where TVariable : Variable
            => (TVariable)Ows._globalVariablesByCharacter[characterId][name];

        public IEnumerator<IParameter> GetEnumerator()
          => (_compiledParameters ?? Enumerable.Empty<IParameter>()).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
          => GetEnumerator();
      }
    }
  }

  public static class ContextExtensions {

    /// <summary>
    /// Add an extra parameter to the context
    /// </summary>
    public static Ows.Command.Context AddExtraParameter(this Ows.Command.Context context, Ows.IParameter parameter)
      => context._addExtraParameter(parameter);
  }
}