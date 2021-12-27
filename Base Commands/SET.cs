using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Sets a value to a given key for the executing character
      /// </summary>
      public class SET : Ows.Command.Type {

        SET()
          : base(
              new("SET"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        /// <summary>
        /// For internal extension
        /// </summary>
        internal SET(Identity id, IEnumerable<System.Type> paramTypes)
          : base(
              id,
              paramTypes
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          SetGlobalVariableForCharacter(program, executor, executor, (String)@params[0], @params[1]);

          return null;
        };

        /// <summary>
        /// Helper function to get the variables for a character safely
        /// </summary>
        protected static void SetLocalVariableForCharacter(Program program, Data.Character executor, Data.Character character, String variableName, IParameter value) {
          var characterSpecificCollection =  program._variablesByCharacter.TryGetValue(character.Id, out var found)
            ? found
            : (program._variablesByCharacter[character.Id] = new Dictionary<string, Variable>());
          object valueToSet = value.GetUltimateValueFor(executor);

          if(characterSpecificCollection.TryGetValue(variableName.Value, out Variable current)) {
            current.Value = valueToSet;
          } else {
            characterSpecificCollection[variableName.Value] = Variable.Make(program, variableName.Value, valueToSet);
          }
        }

        /// <summary>
        /// Helper function to get the variables for a character safely
        /// </summary>
        protected static void SetGlobalVariableForCharacter(Program program, Data.Character executor, Data.Character character, String variableName, IParameter value) {
          var characterSpecificCollection =  _globalVariablesByCharacter.TryGetValue(character.Id, out var found)
            ? found
            : (_globalVariablesByCharacter[character.Id] = new Dictionary<string, Variable>());
          object valueToSet = value.GetUltimateValueFor(executor);

          if(characterSpecificCollection.TryGetValue(variableName.Value, out Variable current)) {
            current.Value = valueToSet;
          } else {
            characterSpecificCollection[variableName.Value] = Variable.Make(program, variableName.Value, valueToSet);
          }
        }
      }
    }
  }
}
