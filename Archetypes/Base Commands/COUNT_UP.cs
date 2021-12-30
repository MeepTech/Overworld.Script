using Meep.Tech.Data;
using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An Loop that starts at 0, and invokes the command given with every number from 0- the provided stop point.
      /// The stop point is not inclusive.
      /// You can use the keyword LOOP-INDEX to substitute the current loop value in a Command that is called directly by the loop.
      /// </summary>
      public class COUNT_UP : Ows.Command.Type {

        COUNT_UP()
          : base(
              new("COUNT-UP"),
              new[] {
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Index index = new(context.Command.Program, 0);
          int end = context.GetUltimateParameterVariable<Number>(0).IntValue;
          while(index.IntValue < end) {
            IParameter result = (context.OrderedParameters[1] as Command)._executeWithExtraParams(
              context.Executor,
              indexReplacement: index
            );

            result.GetUltimateVariableFor(context);

            index.Increment();
          }

          return null;
        };
      }
    }
  }
}
