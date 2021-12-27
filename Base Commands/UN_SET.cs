using System;
using System.Collections.Generic;
using System.Linq;
using Meep.Tech.Data;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Un-sets any value for a given key for the local character
      /// </summary>
      public class UN_SET : Ows.Command.Type {

        UN_SET()
          : base(
              new("UN-SET"),
              new[] {
                typeof(String)
              }
            ) {
        }

        /// <summary>
        /// For internal extension
        /// </summary>
        internal UN_SET(Identity id, IEnumerable<System.Type> paramTypes)
          : base(
              id,
              paramTypes
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          if(_globalVariablesByCharacter.TryGetValue(executor.Id, out var characterVariables)) {
            characterVariables.Remove(((String)@params[0].GetUltimateVariableFor(executor)).Value);
            if(!characterVariables.Any()) {
              _globalVariablesByCharacter.Remove(executor.Id);
            }
          }

          return null;
        };
      }
    }
  }
}
