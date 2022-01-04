using System;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          if(context.Command.Program._variablesByCharacter.TryGetValue(context.Executor.Id, out var characterVariables)) {
            characterVariables.Remove(context.GetUltimateParameterVariable<String>(0).Value);
            if(!characterVariables.Any()) {
              context.Command.Program._variablesByCharacter.Remove(context.Executor.Id);
            }
          }

          return null;
        };
      }
    }
  }
}
