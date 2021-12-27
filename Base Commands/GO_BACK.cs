using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Go back to  the last line from go-to
      /// </summary>
      public class GO_BACK : Ows.Command.Type {

        GO_BACK()
          : base(
              new("GO-BACK"),
              new[] {
                typeof(Number)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params)
             => throw new NotSupportedException();
      }
    }
  }
}
