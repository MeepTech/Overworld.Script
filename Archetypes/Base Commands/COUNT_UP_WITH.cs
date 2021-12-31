using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Like Countdown, but it decrements the provided variable by reference instead of making a new index.
      /// </summary>
      public class COUNT_UP_WITH : Ows.Command.Type {

        public override string Description 
          => "A command taking 3 Params;(Param1:Number, Param2:Number, and Param2:Command), allowing you to execute another command;(Param2), multiple times while counting up from Param1 to Param2. " +
          "Unlike the standard Count up Command, this command will increment the first provided Number variable as it counts up to the second." +
          "You can pass the looping countdown Number value into your Command's Params using the LOOP-INDEX special variable as well as the provided one.";

        public override (string code, string summary)[] Examples => new[]{
          (
            @"
              SET VALUE = 0;
              COUNT-UP-UNTIL : VALUE : 100 : SAY : VALUE",
            "This will say the numbers 0 to 99 in descending order."
          ),
          (
            @"
              SET VALUE = 50;
              COUNT-UP-UNTIL : VALUE : 55 : GO-TO : OTHER_CODE_LOCATIOM",
            "This will GO-TO the code at OTHER_CODE_LOCATIOM 5 times, changing VALUE each time by plus one"
          )
        };

        public override IEnumerable<System.Type> ExpectedReturnTypes 
          => new System.Type[0];

        COUNT_UP_WITH()
          : base(
              new("COUNT-UP-WITH"),
              new[] {
                typeof(Number),
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Number index = context.GetUltimateParameterVariable<Number>(0);
          Number end = context.GetUltimateParameterVariable<Number>(1);
          while(index.IntValue < end.IntValue) {
            context.GetUltimateParameterVariable(2);
            index.IntValue += 1;
          }

          return null;
        };
      }
    }
  }
}
