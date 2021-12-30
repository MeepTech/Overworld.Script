using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Sets a value to a given key for the executing character
      /// </summary>
      public class SET_FOR : SET {

        SET_FOR()
          : base(
              new("SET-FOR"),
              new[] {
                typeof(Collection<Character>),
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        /// <summary>
        /// For internal extension
        /// </summary>
        internal SET_FOR(Identity id, IEnumerable<System.Type> paramTypes)
          : base(
              id,
              paramTypes
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          foreach(string characterId in (context.GetUltimateParameterVariable<Collection<Character>>(0)).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = context.Command.Program.GetCharacter(characterId);
            SetGlobalVariableForCharacter(context, character.Id, (String)context.OrderedParameters[0], context.OrderedParameters[1]);
          }

          return null;
        };
      }
    }
  }
}
