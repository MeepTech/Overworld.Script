using System;

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
              new System.Type[0]
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
