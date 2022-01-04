using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Ends the program and returns a value to the compiler
      /// </summary>
      public class END_AND_RETURN : Ows.Command.Type {

        public override IEnumerable<System.Type> ExpectedReturnTypes
          => new System.Type[] { typeof(ReturnAllResult) };

        public override string Description 
          => $"This Command stops the program execution on the current line and ends all execution, providing a Return value back to the executor of the program";

        END_AND_RETURN()
          : base(
              new("END-AND-RETURN"),
              new [] {
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context =>
          new ReturnAllResult(context.Command.Program, context.GetUltimateParameterVariable(0));
      }
    }
  }
}
