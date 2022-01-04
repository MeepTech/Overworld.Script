using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public static class StringConcatinator {

      /// <summary>
      /// Base archetype for conditions
      /// </summary>
      public class Type : Command.Type {

        protected Type(Identity id)
          : base(
              id ?? new Identity("String-Concatinator"),
              new[] {
                typeof(IParameter),
                typeof(IParameter)
              }
            ) {
        }

        /// <summary>
        /// Make function to make a new condition
        /// </summary>
        public Command Make(Program program, IEnumerable<IParameter> @params, Opperators? opperator = null)
          => Make(
            (nameof(Command.Parameters), @params.Count() == 1
                ? @params.Append(null).ToList()
                : @params.ToList()),
            (nameof(Command.Program), program),
            (nameof(MathOpperator.Opperator), opperator)
          );

        public override Func<Command.Context, Variable> Execute {
          get;
        } = context =>
          new String(context.Command.Program,
            context.GetUltimateParameterVariable<String>(0).Value
              + context.GetUltimateParameterVariable<String>(1).Value
          );
      }
    }
  }
}
