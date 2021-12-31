using System;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// A For Loop
      /// </summary>
      public class FOR : Ows.Command.Type {

        FOR()
          : base(
              new("FOR"),
              new[] {
                typeof(IConditional),
                typeof(MathOpperator),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Condition endCondition = context.OrderedParameters[0] as Condition;
          MathOpperator loopOpperator =  context.OrderedParameters[1] as MathOpperator;
          Number index = loopOpperator.Parameters.First() as Number;
          while(endCondition.ComputeFor(context.Executor).Value) {
            IParameter result = ( context.OrderedParameters[1] as Command)
              .ExecuteUltimateCommandFor(context);

            index.Value = ((Number)loopOpperator
              .ExecuteUltimateCommandFor(context)).Value;
          }

          return null;
        };
      }
    }
  }
}
