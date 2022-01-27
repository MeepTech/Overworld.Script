using System;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// A loop that executes on each item in a object collection.
      /// You can substitute in 'LOOP-OBJECT' for any of the params of the command that will be called for it to execute on the current loop object.
      /// </summary>
      public class FOR_EACH : Ows.Command.Type {

        FOR_EACH()
          : base(
              new("FOR-EACH"),
              new[] {
                typeof(Ows.Collection),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          throw new System.NotImplementedException($"FOR-EACH and LOOP-OBJECT not yet implimented");
        };
      }
    }
  }
}
