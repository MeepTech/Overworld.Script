using Meep.Tech.Data;
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

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          MathOpperator opperator = (MathOpperator)@params.Last();
          switch(opperator?.Opperator) {
            case Opperators.SQUARED:
              return new Number(program, Math.Pow(@params[0].GetUltimateVariableAs<Number>(executor).RawValue, 2));
            case Opperators.DIVIDED_BY:
              return new Number(program, 
                @params[0].GetUltimateVariableAs<Number>(executor).RawValue
                / @params[1].GetUltimateVariableAs<Number>(executor).RawValue);
            case Opperators.PLUS:
              return new Number(program, 
                @params[0].GetUltimateVariableAs<Number>(executor).RawValue
                + @params[1].GetUltimateVariableAs<Number>(executor).RawValue);
            case Opperators.MINUS:
              return new Number(program, 
                @params[0].GetUltimateVariableAs<Number>(executor).RawValue
                - @params[1].GetUltimateVariableAs<Number>(executor).RawValue);
            case Opperators.MODULO:
              return new Number(program, 
                @params[0].GetUltimateVariableAs<Number>(executor).RawValue
                % @params[1].GetUltimateVariableAs<Number>(executor).RawValue);
            case Opperators.TIMES:
              return new Number(program, 
                @params[0].GetUltimateVariableAs<Number>(executor).RawValue
                * @params[1].GetUltimateVariableAs<Number>(executor).RawValue);
            case Opperators.TO_THE_POWER_OF:
              return new Number(program, 
                Math.Pow(@params[0].GetUltimateVariableAs<Number>(executor).RawValue,
                 @params[1].GetUltimateVariableAs<Number>(executor).RawValue));
            default:
              throw new ArgumentException($"No Conditional Type Provided.");
          };
        };
      }
    }
  }
}
