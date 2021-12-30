using Meep.Tech.Data;
using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.Data.Configuration.Loader.Settings;

namespace Overworld.Script {
  public static partial class Ows {
    public partial class MathOpperator {

      /// <summary>
      /// Base archetype for conditions
      /// </summary>
      [Branch]
      public new class Type : Command.Type {

        protected override Dictionary<string, object> DefaultTestParams 
          => base.DefaultTestParams.Append(nameof(Opperator), Opperators.PLUS);

        protected Type(Identity id)
          : base(
              id ?? new Identity("Math-Operator"),
              new[] { typeof(IParameter), typeof(IParameter) }
            ) {
        }

        /// <summary>
        /// Make function to make a new condition
        /// </summary>
        public MathOpperator Make(Program program, IEnumerable<IParameter> @params, Opperators? opperator = null)
          => Make<MathOpperator>(
            (nameof(Command.Parameters), @params.Count() == 1
              && (opperator == Opperators.SQUARED)
                // by default, it adds null for squareds
                ? @params.Append(null).ToList()
                : @params.ToList()),
            (nameof(Command.Program), program),
            (nameof(MathOpperator.Opperator), opperator)
          );

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          MathOpperator opperator = context.GetParameter<MathOpperator>(2);
          switch(opperator?.Opperator) {
            case Opperators.SQUARED:
              return new Number(context.Command.Program, Math.Pow(
                context.GetUltimateParameterVariable<Number>(0).RawValue, 2));
            case Opperators.DIVIDED_BY:
              return new Number(context.Command.Program, 
                context.GetUltimateParameterVariable<Number>(0).RawValue
                / context.GetUltimateParameterVariable<Number>(1).RawValue);
            case Opperators.PLUS:
              return new Number(context.Command.Program, 
                context.GetUltimateParameterVariable<Number>(0).RawValue
                + context.GetUltimateParameterVariable<Number>(1).RawValue);
            case Opperators.MINUS:
              return new Number(context.Command.Program, 
                context.GetUltimateParameterVariable<Number>(0).RawValue
                - context.GetUltimateParameterVariable<Number>(1).RawValue);
            case Opperators.MODULO:
              return new Number(context.Command.Program, 
                context.GetUltimateParameterVariable<Number>(0).RawValue
                % context.GetUltimateParameterVariable<Number>(1).RawValue);
            case Opperators.TIMES:
              return new Number(context.Command.Program, 
                context.GetUltimateParameterVariable<Number>(0).RawValue
                * context.GetUltimateParameterVariable<Number>(1).RawValue);
            case Opperators.TO_THE_POWER_OF:
              return new Number(context.Command.Program, 
                Math.Pow(context.GetUltimateParameterVariable<Number>(0).RawValue,
                 context.GetUltimateParameterVariable<Number>(1).RawValue));
            default:
              throw new ArgumentException($"No Conditional Type Provided.");
          };
        };
      }
    }
  }
}
