using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class CollectionConcatinator {
      /// <summary>
      /// Base archetype for conditions
      /// </summary>
      public new class Type : Command.Type {

        protected Type(Identity id)
          : base(
              id ?? new Identity("Collection-Concatinator"),
              new[] {
                typeof(IParameter),
                typeof(IParameter)
              }
            ) {
        }

        /// <summary>
        /// Make function to make a new condition
        /// </summary>
        public CollectionConcatinator Make(Program program, IEnumerable<IParameter> @params, bool isAdditive)
          => Make<CollectionConcatinator>(
            (nameof(Command.Parameters), @params.ToList()),
            (nameof(Command.Program), program),
            (nameof(isAdditive), isAdditive)
          );

        public override Func<Command.Context, Variable> Execute {
          get;
        } = context => {
          Ows.Collection collectionLeft = context.GetUltimateParameterVariable<Ows.Collection>(0);
          Ows.Collection collectionRight = context.GetUltimateParameterVariable<Ows.Collection>(1);
          List<IParameter> values = collectionLeft.Value;
          if((context.Command as CollectionConcatinator).IsAdditive) {
            values.AddRange(collectionRight.Value);
          }
          else {
            foreach(IParameter valueToRemove in collectionRight.Value) {
              values.Remove(valueToRemove);
            }
          }

          return new Ows.Collection(
            context.Command.Program,
            values,
            collectionRight.RestrictedToType == collectionLeft.RestrictedToType
              ? collectionLeft.RestrictedToType
              : null
          );
        };
      }
    }
  }
}
