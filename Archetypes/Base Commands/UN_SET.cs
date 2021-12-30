using System;
using System.Collections.Generic;
using System.Linq;
using Meep.Tech.Data;
using Meep.Tech.Data.Utility;

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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          if(_globalVariablesByCharacter.TryGetValue(context.Executor.Id, out var characterVariables)) {
            characterVariables.Remove(((String)context.OrderedParameters[0]).Value);
            if(!characterVariables.Any()) {
              _globalVariablesByCharacter.Remove(context.Executor.Id);
            }
          }

          return null;
        };
      }
    }
  }
}
