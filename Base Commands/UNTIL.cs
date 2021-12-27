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

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          while(!((IConditional)@params.First()).ComputeFor(executor).Value) {
            return (@params[1] as Command).ExecuteUltimateCommandFor(executor);
          }

          return null;
        };
      }
    }
  }
}
