using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// An Loop that starts at 0, and invokes the command given with every number from 0- the provided stop point.
      /// The stop point is not inclusive.
      /// You can use the keyword LOOP-INDEX to substitute the current loop value in a Command that is called directly by the loop.
      /// </summary>
      public class FOR : Ows.Command.Type {

        FOR()
          : base(
              new("FOR"),
              new[] {
                typeof(Number),
                typeof(Command)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          Index index = new(program, 0);
          int end = ((Number)@params[0].GetUltimateVariableFor(executor)).IntValue;
          while(index.IntValue < end) {
            IParameter result = (@params[1] as Command)._executeFor(
              executor,
              Enumerable.Empty<IParameter>(),
              index
            );

            if(result is Command command) {
              command.ExecuteUltimateCommandFor(executor);
            }

            index.Increment();
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
