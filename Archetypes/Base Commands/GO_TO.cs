using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Go To a Line of Code
      /// </summary>
      public class GO_TO : Ows.Command.Type {

        protected GO_TO(Identity id)
          : base(
              id ?? new("GO-TO"),
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
          return context.Command.Program._executeAllStartingAtLine(
            context.Command.Program.GetLineNumberForLabel(labelText, context),
            context.Executor,
            context.GetUltimateParameterVariable<Number>(1).IntValue
          );
        };
      }
    }
  }
}
