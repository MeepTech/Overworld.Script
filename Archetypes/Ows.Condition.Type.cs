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

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          Condition condition = (Condition)@params.Last();
          switch(condition?.Comparitor) {
            case Comparitors.IDENTITY:
              return ((IConditional)@params.First()).ComputeFor(executor);
            case Comparitors.NOT:
              return ((IConditional)@params.First()).ComputeFor(executor).Not;
            case Comparitors.AND:
              return ((IConditional)@params.First()).ComputeFor(executor)
                .And(((IConditional)@params[1]).ComputeFor(executor));
            case Comparitors.OR:
              return ((IConditional)@params.First()).ComputeFor(executor)
                .Or(((IConditional)@params[1]).ComputeFor(executor));
            case Comparitors.EQUALS:
              return new Boolean(program, @params[0].Value.Equals(@params[1].Value));
            case Comparitors.LESS_THAN:
              if(@params[0] is Number && @params[1] is Number) {
                return new Boolean(
                  program,
                  ((Number)@params[0]).RawValue < ((Number)@params[1]).RawValue
                );
              } else
                throw new ArgumentException($"Condition of type {condition.Comparitor} requires two Number parameters");
            case Comparitors.GREATER_THAN:
              if(@params[0] is Number && @params[1] is Number) {
                return new Boolean(
                  program,
                  ((Number)@params[0]).RawValue > ((Number)@params[1]).RawValue
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
