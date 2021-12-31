using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Sets a value to a given key for the current program
      /// </summary>
      public class SET_FOR_PROGRAM : SET {

        SET_FOR_PROGRAM()
          : base(
              new("SET-FOR-PROGRAM"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = Context => {
          Context.Command.Program._globals[((String)Context.OrderedParameters[0]).Value]
            = Context.GetUltimateParameterVariable(1);

          return null;
        };
      }
    }
  }
}
