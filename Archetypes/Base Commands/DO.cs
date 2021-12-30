using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Overworld.Script {

  public partial class Ows {

    public partial class Command {
      public class DO : Ows.Command.Type {

        DO()
          : base(
              new("DO"),
              new[] {
                typeof(String),
                typeof(Ows.VariableMap),
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
              context.GetUltimateParameterVariable<Number>(2).IntValue
              // TODO: add temp scope params here:
            );
          }
          else
            return context.Command.Program._executeAllStartingAtLine(
              context.Command.Program._labelsByLineNumber[context.TryToGetVariableByName<String>(labelText).Value],
              context.Executor,
              context.GetUltimateParameterVariable<Number>(2).IntValue
            );
        };
      }
    }
  }
}
