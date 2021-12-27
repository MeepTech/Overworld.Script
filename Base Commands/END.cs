using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An IF conditional that can run a command
      /// </summary>
      public class END : Ows.Command.Type {

        END()
          : base(
              new("END"),
              new System.Type[0]
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) =>
          throw new ArgumentException($"End");
      }
    }
  }
}
