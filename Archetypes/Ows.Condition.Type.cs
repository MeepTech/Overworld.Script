using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static Meep.Tech.Data.Configuration.Loader.Settings;

namespace Overworld.Script {
  public static partial class Ows {
    public partial class Condition {

      /// <summary>
      /// Base archetype for conditions
      /// </summary>
      [Branch]
      public new class Type : Command.Type {

        protected Type(Identity id)
          : base(
              id ?? new Command.Type.Identity("Condition"),
              new[] { typeof(IParameter), typeof(IParameter) }
            ) {
        }

        /// <summary>
        /// Make function to make a new condition
        /// </summary>
        public Condition Make(Program program, IEnumerable<IParameter> @params, Comparitors? comparitor = null)
          => Make<Condition>(
            (nameof(Command.Parameters), @params.Count() == 1
              && (comparitor == Comparitors.IDENTITY
                || comparitor == Comparitors.NOT)
                // by default, it adds null for identities and nots
                ? @params.Append(null).ToList()
                : @params.ToList()),
            (nameof(Command.Program), program),
            (nameof(Condition.Comparitor), comparitor)
          );

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Condition condition = (Condition)context.OrderedParameters[2];
          switch(condition?.Comparitor) {
            case Comparitors.IDENTITY:
              return context.GetUltimateParameterVariable<Boolean>(0);
            case Comparitors.NOT:
              return context.GetUltimateParameterVariable<Boolean>(0).Not;
            case Comparitors.AND:
              return context.GetUltimateParameterVariable<Boolean>(0).And(
                context.GetUltimateParameterVariable<Boolean>(1));
            case Comparitors.OR:
              return context.GetUltimateParameterVariable<Boolean>(0).Or(
                context.GetUltimateParameterVariable<Boolean>(1));
            case Comparitors.EQUALS:
              var left2 = context.GetUltimateParameterVariable(0).Value;
              var right2 = context.GetUltimateParameterVariable(1).Value;
              bool equality = left2 == right2;
              bool equality1 = left2.Equals(right2);

              return new Boolean(context.Command.Program, equality || equality1);
            case Comparitors.LESS_THAN:
              if(context.OrderedParameters[0] is Number left && context.OrderedParameters[1] is Number right) {
                return new Boolean(
                  context.Command.Program,
                  left.DoubleValue < right.DoubleValue
                );
              } else
                throw new ArgumentException($"Condition of type {condition.Comparitor} requires two Number parameters");
            case Comparitors.GREATER_THAN:
              if(context.OrderedParameters[0] is Number left1 && context.OrderedParameters[1] is Number right1) {
                return new Boolean(
                  context.Command.Program,
                  left1.DoubleValue > right1.DoubleValue
                );
              } else
                throw new ArgumentException($"Condition of type {condition.Comparitor} requires two Number parameters");
            default:
              throw new ArgumentException($"No Conditional Type Provided.");
          };
        };
      }
    }
  }
}
