using Meep.Tech.Collections.Generic;
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

        ///<summary><inheritdoc/></summary>
        protected override Dictionary<string, object> DefaultTestParams
          => base.DefaultTestParams.Append(nameof(Operator), "AND");

        ///<summary><inheritdoc/></summary>
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
            ? global::Overworld.Script.Ows.Command.Types.Get<global::Overworld.Script.Ows.MathOpperator.Type>().Make(context.Command.Program,
              new global::Overworld.Script.Ows.IParameter[] {
                left,
                right
              }, isPositive ? global::Overworld.Script.Ows.Opperators.PLUS : global::Overworld.Script.Ows.Opperators.MINUS)
            : left is global::Overworld.Script.Ows.String
              ? global::Overworld.Script.Ows.Command.Types.Get<global::Overworld.Script.Ows.MathOpperator.Type>().Make(context.Command.Program,
                new global::Overworld.Script.Ows.IParameter[] {
                  left,
                  right
                }, isPositive ? global::Overworld.Script.Ows.Opperators.PLUS : throw new global::System.NotSupportedException($"There is no negative concatination using - for strings"))
              : left is global::Meep.Tech.Data.Archetype.Collection
                ? global::Overworld.Script.Ows.Command.Types.Get<global::Overworld.Script.Ows.CollectionConcatinator.Type>().Make(context.Command.Program,
                  new global::Overworld.Script.Ows.IParameter[] {
                    left,
                    right
                  }, isPositive)
                : left is global::Overworld.Script.Ows.IConditional
                  ? global::Overworld.Script.Ows.Command.Types.Get<global::Overworld.Script.Ows.Condition.Type>().Make(context.Command.Program,
                    new global::Overworld.Script.Ows.IParameter[] {
                      left,
                      right
                    }, isPositive ? global::Overworld.Script.Ows.Comparitors.AND : throw new global::System.NotSupportedException($"There is no negative concatination using - for booleans"))
                  : throw new global::System.NotSupportedException($"Could not determine types for concatination. Try adding INumeric, IConditional, or ITextual to your command:\n {left} {@operator} {right}");
        }
      }
    }
  }
}
