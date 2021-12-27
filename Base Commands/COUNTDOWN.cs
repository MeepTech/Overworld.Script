using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An Reversed for loop that starts at the provided number and stops at 0
      /// </summary>
      public class COUNTDOWN : Ows.Command.Type {

        COUNTDOWN()
          : base(
              new("COUNTDOWN"),
              new[] {
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          Index index = new(program, ((Number)@params[0].GetUltimateVariableFor(executor)).IntValue);
          while(index.IntValue >= 0) {
            IParameter result = (@params[1] as Command)._executeFor(
              executor,
              Enumerable.Empty<IParameter>(),
              index
            );

            if(result is Command command) {
              command.ExecuteUltimateCommandFor(executor);
            }

            index.Decrement();
          }

          return null;
        };

        /*protected override Command ConfigureModel(IBuilder<Command> builder, Command model) {
          model = base.ConfigureModel(builder, model);
          if(model.Parameters.Skip(1).First() is Command command) {
          }

          return model;
        }*/
      }
    }
  }
}
