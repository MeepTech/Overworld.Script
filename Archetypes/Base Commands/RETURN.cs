using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// returns a value to the kube that this came from.
      /// </summary>
      public class RETURN : Ows.Command.Type {

        public override IEnumerable<System.Type> ExpectedReturnTypes
          => new System.Type[] { typeof(ReturnResult) };

        public override string Description 
          => $"This Command stops the program execution on the current line, and GO-BACK's to the line it last came from, providing a Return value back to the executor";

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
              SET VALUE TO 1
              
              COUNTDOWN:3:SET VALUE TO GOTO:double_value
              RETURN VALUE

              [double_value]:RETURN:VALUE+VALUE",
            "This program double the value of Value 3 times and then return it from the program."),
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
