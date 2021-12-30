using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

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
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          if(context.GetUltimateParameterVariable<Boolean>(0).Not.Value) {
            return (context.OrderedParameters[1] as Command).ExecuteUltimateCommandFor(context);
          }

          return null;
        };
      }
    }
  }
}
