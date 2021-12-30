using Meep.Tech.Data.Utility;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          SetGlobalVariableForCharacter(context, context.Executor.Id, (String)context.OrderedParameters[0], context.OrderedParameters[1]);

          return null;
        };

        /// <summary>
        /// Helper function to get the variables for a character safely
        /// </summary>
        protected static void SetLocalVariableForCharacter(Context context, string characterId, String variableName, IParameter value) {
          var characterSpecificCollection = context.Command.Program._variablesByCharacter.TryGetValue(characterId, out var found)
            ? found
            : (context.Command.Program._variablesByCharacter[characterId] = new Dictionary<string, Variable>());
          object valueToSet = value.GetUltimateValueFor(context);

          if(characterSpecificCollection.TryGetValue(variableName.Value, out Variable current)) {
            current.Value = valueToSet;
          } else {
            characterSpecificCollection[variableName.Value] = Variable.Make(context.Command.Program, variableName.Value, valueToSet);
          }
        }

        /// <summary>
        /// Helper function to get the variables for a character safely
        /// </summary>
        protected static void SetGlobalVariableForCharacter(Context context, string characterId, String variableName, IParameter value) {
          var characterSpecificCollection =  _globalVariablesByCharacter.TryGetValue(characterId, out var found)
            ? found
            : (_globalVariablesByCharacter[characterId] = new Dictionary<string, Variable>());
          object valueToSet = value.GetUltimateValueFor(context);

          if(characterSpecificCollection.TryGetValue(variableName.Value, out Variable current)) {
            current.Value = valueToSet;
          } else {
            characterSpecificCollection[variableName.Value] = Variable.Make(context.Command.Program, variableName.Value, valueToSet);
          }
        }
      }
    }
  }
}
