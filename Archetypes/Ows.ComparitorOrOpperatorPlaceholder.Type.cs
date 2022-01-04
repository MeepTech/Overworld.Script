using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.Data.Configuration.Loader.Settings;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class UnknownOperator {

      /// <summary>
      /// Base archetype for conditions
      /// </summary>
      [Branch]
      public new class Type : Command.Type {

        protected override Dictionary<string, object> DefaultTestParams
          => base.DefaultTestParams.Append(nameof(Operator), "AND");

        protected Type(Identity id)
          : base(
              id ?? new Identity("Unknown-Operator"),
              new[] {
                typeof(IParameter),
                typeof(IParameter)
              }
            ) {
        }

        /// <summary>
        /// Make function to make a new condition
        /// </summary>
        public UnknownOperator Make(Program program, IEnumerable<IParameter> @params, string @operator)
          => Make<UnknownOperator>(
            (nameof(Command.Parameters), @params.Count() == 1
                ? @params.Append(null).ToList()
                : @params.ToList()),
            (nameof(Command.Program), program),
            (nameof(UnknownOperator.Operator), @operator)
          );

        public override Func<Context, Variable> Execute {
          get;
        } = context => /*((context.Command as UnknownOperator)
          .compiled ??=*/ _compile(context,
            context.GetUltimateParameterVariable(0),
            context.GetUltimateParameterVariable(1),
            (context.Command as UnknownOperator).Operator
          ).ExecuteUltimateCommandFor(context);

        /// <summary>
        /// Compile the command based on it's variables:
        /// </summary>
        static Command _compile(Command.Context context, IParameter left, IParameter right, string @operator) {
          bool isPositive = @operator[0] == (char)Opperators.PLUS
            || @operator[0] == (char)Comparitors.AND
            || @operator == Comparitors.AND.ToString()
            || @operator == Opperators.PLUS.ToString();

          return left is Number
            ? Command.Types.Get<MathOpperator.Type>().Make(context.Command.Program,
              new IParameter[] {
                left,
                right
              }, isPositive ? Opperators.PLUS : Opperators.MINUS)
            : left is String
              ? Command.Types.Get<MathOpperator.Type>().Make(context.Command.Program,
                new IParameter[] {
                  left,
                  right
                }, isPositive ? Opperators.PLUS : throw new NotSupportedException($"There is no negative concatination using - for strings"))
              : left is Collection
                ? Command.Types.Get<CollectionConcatinator.Type>().Make(context.Command.Program,
                  new IParameter[] {
                    left,
                    right
                  }, isPositive)
                : left is IConditional
                  ? Command.Types.Get<Condition.Type>().Make(context.Command.Program,
                    new IParameter[] {
                      left,
                      right
                    }, isPositive ? Comparitors.AND : throw new NotSupportedException($"There is no negative concatination using - for booleans"))
                  : throw new NotSupportedException($"Could not determine types for concatination. Try adding INumeric, IConditional, or ITextual to your command:\n {left} {@operator} {right}");
        }
      }
    }
  }
}
