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

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          foreach(string characterId in (@params[0] as Collection<Character>).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = program.GetCharacter(characterId);
            SetGlobalVariableForCharacter(program, character, (String)@params[0], @params[1]);
          }

          return null;
        };
      }
    }
  }
}
