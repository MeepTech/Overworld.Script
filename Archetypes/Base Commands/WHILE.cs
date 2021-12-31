using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An while conditional that can loop a command
      /// </summary>
      public class WHILE : Ows.Command.Type {

        WHILE()
          : base(
              new("WHILE"),
              new[] {
                typeof(IConditional),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          while(context.GetUltimateParameterVariable<Boolean>(0).Value) {
            return (context.OrderedParameters[1] as Command).ExecuteUltimateCommandFor(context);
          }

          return null;
        };
      }
    }
  }
}
