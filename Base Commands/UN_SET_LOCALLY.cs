using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Un-sets any value for a given key for the local character
      /// </summary>
      public class UN_SET_LOCALLY : UN_SET {

        UN_SET_LOCALLY()
          : base(
              new("UN-SET-LOCALLY"),
              new[] {
                typeof(String)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          if(program._variablesByCharacter.TryGetValue(executor.Id, out var characterVariables)) {
            characterVariables.Remove(((String)@params[0].GetUltimateVariableFor(executor)).Value);
            if(!characterVariables.Any()) {
              program._variablesByCharacter.Remove(executor.Id);
            }
          }

          return null;
        };
      }
    }
  }
}
