using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// A reversed IF conditional that can run a command if the condition is "not true"/false
      /// </summary>
      public class IF_NOT : Ows.Command.Type {

        IF_NOT()
          : base(
              new("IF-NOT"),
              new[] {
                typeof(IConditional),
                typeof(Command),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          if(context.GetUltimateParameterVariable<Boolean>(0).Not.Value) {
            return (context.OrderedParameters[1] as Command).ExecuteUltimateCommandFor(context);
          }// if there's an else:
          else if(context.OrderedParameters[2] is not null) {
            return (context.OrderedParameters[2] as Command).ExecuteUltimateCommandFor(context);
          }

          return null;
        };
      }
    }
  }
}
