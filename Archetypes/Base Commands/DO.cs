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
                typeof(Ows.VariableMap)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          String labelText = context.GetUltimateParameterVariable<String>(0);
          return new DoWithStartResult(
            context.Command.Program,
            labelText
          ) {
            _scopedParams = context.GetUltimateParameterVariable<VariableMap>(1),
            _targetLineNumber = context.Command.Program.GetLineNumberForLabel(labelText.Value, context)
          };
        };
      }
    }
  }
}
