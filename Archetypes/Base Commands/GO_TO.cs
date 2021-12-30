using System;
using System.Collections.Generic;
using System.Linq;
using Meep.Tech.Data.Utility;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          string labelText = context.GetUltimateParameterVariable<String>(0).Value;
          if(context.Command.Program._labelsByLineNumber.ContainsKey(labelText)) {
            return context.Command.Program._executeAllStartingAtLine(
              context.Command.Program._labelsByLineNumber[labelText],
              context.Executor,
              context.GetUltimateParameterVariable<Number>(1).IntValue
            );
          } else
            return context.Command.Program._executeAllStartingAtLine(
              context.Command.Program._labelsByLineNumber[context.TryToGetVariableByName<String>(labelText).Value],
              context.Executor,
              context.GetUltimateParameterVariable<Number>(1).IntValue
            );
        };
      }
    }
  }
}
