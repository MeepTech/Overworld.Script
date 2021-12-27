using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Sets a value to a given key for the executing character
      /// </summary>
      public class SET_LOCALLY_FOR : SET_FOR {

        SET_LOCALLY_FOR()
          : base(
              new("SET-LOCALLY-FOR"),
              new[] {
                typeof(Collection<Character>),
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          foreach(string characterId in (@params[0] as Collection<Character>).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = program.GetCharacter(characterId);
            SetLocalVariableForCharacter(program, executor, character, (String)@params[0], @params[1]);
          }

          return null;
        };
      }
    }
  }
}
