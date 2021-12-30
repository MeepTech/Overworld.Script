using Meep.Tech.Data;
using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Like Countdown, but it decrements the provided variable by reference instead of making a new index.
      /// </summary>
      public class COUNTDOWN_WITH : Ows.Command.Type {

        public override string Description 
          => "A command taking 2 Params;(Param1:Number and Param2:Command), allowing you to execute another command;(Param2), X;(Param1) times while counting down from X(exclusive) to 0. " +
          "Unlike the standard Countdown Command, this command will decrement the provided variable as it counts down." +
          "You can pass the looping countdown Number value into your Command's Params using the LOOP-INDEX special variable as well as the provided one.";

        public override (string code, string summary)[] Examples => new[]{
          (
            @"
              SET VALUE = 100;
              COUNTDOWN-WITH : VALUE : SAY : VALUE",
            "This will say the numbers 99 to 0 in descending order."
          ),
          (
            @"
              SET VALUE = 100;
              COUNTDOWN : VALUE : GO-TO : OTHER_CODE_LOCATIOM",
            "This will GO-TO the code at OTHER_CODE_LOCATIOM 100 times, changing VALUE each time by minus one"
          )
        };

        public override IEnumerable<System.Type> ExpectedReturnTypes 
          => new System.Type[0];

        COUNTDOWN_WITH()
          : base(
              new("COUNTDOWN-WITH"),
              new[] {
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Number index = context.GetUltimateParameterVariable<Number>(0);
          while(index.IntValue >= 0) {
            context.GetUltimateParameterVariable(1);
            index.RawValue -= 1;
          }

          return null;
        };
      }
    }
  }
}
