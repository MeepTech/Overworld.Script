using System;

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
          String labelText = context.GetUltimateParameterVariable<String>(0);
          return context.Command.Program._executeAllStartingAtLine(
            context.Command.Program.GetLineNumberForLabel(labelText.Value, context),
            context.Executor,
            context.GetUltimateParameterVariable<Number>(2).IntValue,
            context.GetUltimateParameterVariable<VariableMap>(1),
            context._debugData
          );
        };
      }
    }
  }
}
