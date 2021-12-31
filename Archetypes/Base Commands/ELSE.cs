using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Placeholder for ELSE logic in If-Else
      /// </summary>
      public class ELSE : Ows.Command.Type {

        ELSE()
          : base(
              new("ELSE"),
              new System.Type[] {typeof(Command)}
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context =>
          throw new ArgumentException($"Else called on it's own");
      }
    }
  }
}
