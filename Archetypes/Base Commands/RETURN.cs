using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Ends the program and returns a value to the compiler
      /// </summary>
      public class RETURN : Ows.Command.Type {

        public override IEnumerable<System.Type> ExpectedReturnTypes
          => new System.Type[] { typeof(ReturnResult) };

        public override string Description 
          => $"This Command ENDs the program, and provides a Return value back to the executor";

        public override (string code, string summary)[] Examples
          => new[] {
            (@"
              SET VALUE TO TRUE
              
              IF:VALUE EQUALS FALSE:RETURN 7
              SAY ""THIS WILL BE SAID""

              RETURN 5
              SAY: ""This Will Not Be Said!""",
            "This program will end before running the second SAY command, and will Return 5 to the runner"),
            (@"
              SET VALUE TO TRUE
              RETURN: VALUE",
            "This will return a boolean value of TRUE to the executor")
          };

        RETURN()
          : base(
              new("RETURN"),
              new [] {
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context =>
          new ReturnResult(context.Command.Program, context.GetUltimateParameterVariable(0));
      }
    }
  }
}
