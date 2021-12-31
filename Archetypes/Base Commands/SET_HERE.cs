using System;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Sets a value passed via the do:with scope
      /// </summary>
      public class SET_HERE : SET {

        SET_HERE()
          : base(
              new("SET-HERE"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          String variableName = (context.OrderedParameters[0] as String);
          if(ReservedKeywords.Contains(variableName.Value)) {
            throw new ArgumentException($"Tried to use reserved keyword as variable name: {variableName.Value}");
          }
          context._temporaryScopedVariables.Value[variableName.Value] 
            = context.GetUltimateParameterVariable(1);

          return null;
        };
      }
    }
  }
}
