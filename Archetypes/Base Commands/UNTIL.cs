using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>    
      /// A reversed while conditional that can loop a command
      /// </summary>
      public class UNTIL : Ows.Command.Type {

        UNTIL()
          : base(
              new("UNTIL"),
              new[] {
                typeof(IConditional),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          while(!context.GetUltimateParameterVariable<Boolean>(0).Value) {
            return (context.OrderedParameters[1] as Command).ExecuteUltimateCommandFor(context);
          }

          return null;
        };
      }
    }
  }
}
