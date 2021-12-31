using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An Reversed for loop that starts at the provided number and stops at 0
      /// </summary>
      public class COUNTDOWN : Ows.Command.Type {

        public override string Description 
          => "A command taking 2 Params;(Param1:Number and Param2:Command), allowing you to execute another command;(Param2), X;(Param1) times while counting down from X(exclusive) to 0. " +
          "You can pass the looping countdown Number value into your Command's Params using the LOOP-INDEX special variable.";

        public override (string code, string summary)[] Examples => new[]{
          (
            "COUNTDOWN : 100 : SAY : LOOP-INDEX",
            "This will say the numbers 99 to 0 in descending order."
          ),
          (
            "COUNTDOWN : 4 : GO-TO : OTHER_CODE_LOCATIOM",
            "This will GO-TO the code at OTHER_CODE_LOCATIOM 4 times."
          )
        };

        public override IEnumerable<System.Type> ExpectedReturnTypes 
          => new System.Type[0];

        COUNTDOWN()
          : base(
              new("COUNTDOWN"),
              new[] {
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Index index = new(context.Command.Program, context.GetUltimateParameterVariable<Number>(0).IntValue);
          while(index.IntValue >= 0) {
            IParameter result = (context.OrderedParameters[1] as Command)._executeWithExtraParams(
              context.Executor,
              indexReplacement: index
            );

            result.GetUltimateValueFor(context);

            index.Decrement();
          }

          return null;
        };
      }
    }
  }
}
