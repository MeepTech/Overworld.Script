using System;
using System.Collections.Generic;
using System.Linq;
using Overworld.Data;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Go To a Line of Code
      /// </summary>
      public class GO_TO : Ows.Command.Type {

        GO_TO()
          : base(
              new("GO-TO"),
              new[] {
                typeof(String),
                typeof(Number)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          string labelText = (@params.First().GetUltimateVariableFor(executor) as String).Value;
          if(program._labelsByLineNumber.ContainsKey(labelText)) {
            return program._executeAllStartingAtLine(
              program._labelsByLineNumber[labelText],
              executor,
              ((Number)@params[1].GetUltimateVariableFor(executor)).IntValue
            );
          } else
            return program._executeAllStartingAtLine(
              program._labelsByLineNumber[((String)program.TryToGetVariableByName(labelText)).Value],
              executor,
              ((Number)@params[1].GetUltimateVariableFor(executor)).IntValue
            );
        };
      }
    }
  }
}
