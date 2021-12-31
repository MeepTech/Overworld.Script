using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Ends the program.
      /// </summary>
      public class END : Ows.Command.Type {

        public override (string code, string summary)[] Examples
          => new[] {
            (@"
              SET VALUE TO TRUE
              
              IF:VALUE EQUALS TRUE:END
              SAY: ""This Will Not Be Said!""
              
              RETURN 5",
            "This program will end before running SAY or RETURNing 5, and will instead return null/nothing")
          };

        public override string Description 
          => "Is used to immediately end the program, and return nothing/null";

        public override IEnumerable<System.Type> ExpectedReturnTypes
          => new System.Type[0];

        END()
          : base(
              new("END"),
              new System.Type[0]
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context =>
          throw new ArgumentException($"End");
      }
    }
  }
}
