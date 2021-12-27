using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Un-sets any value for a given key for the given character collection
      /// </summary>
      public class UN_SET_LOCALLY_FOR : UN_SET {

        UN_SET_LOCALLY_FOR()
          : base(
              new("UN-SET-LOCALLY-FOR"),
              new[] {
                typeof(Collection<Character>),
                typeof(String)
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
            if(program._variablesByCharacter.TryGetValue(character.Id, out var characterVariables)) {
              characterVariables.Remove(((String)@params[1]).Value);
              if(!characterVariables.Any()) {
                program._variablesByCharacter.Remove(character.Id);
              }
            }
          }

          return null;
        };
      }
    }
  }
}
