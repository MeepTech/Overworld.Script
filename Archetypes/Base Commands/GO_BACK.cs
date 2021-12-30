using Meep.Tech.Data.Utility;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context
             => throw new NotSupportedException();
      }
    }
  }
}
