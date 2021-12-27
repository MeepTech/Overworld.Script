using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An IF conditional that can run a command
      /// </summary>
      public class IF : Ows.Command.Type {

        IF()
          : base(
              new("IF"),
              new[] {
                typeof(IConditional),
                typeof(Command)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          if(((IConditional)@params.First()).ComputeFor(executor).Value) {
            return (@params[1] as Command).ExecuteUltimateCommandFor(executor);
          }

          return null;
        };
      }
    }
  }
}
