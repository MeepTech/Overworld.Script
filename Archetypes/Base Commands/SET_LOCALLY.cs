using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Sets a value to a given key for the executing character
      /// </summary>
      public class SET_LOCALLY : SET {

        SET_LOCALLY()
          : base(
              new("SET-LOCALLY"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          SetLocalVariableForCharacter(context, context.Executor.Id, (String)context.OrderedParameters[0], context.OrderedParameters[1]);

          return null;
        };
      }
    }
  }
}
