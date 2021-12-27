using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Un-sets any value for a given key for the given character collection
      /// </summary>
      public class UN_SET_FOR : UN_SET {

        UN_SET_FOR()
          : base(
              new("UN-SET-FOR"),
              new[] {
                typeof(Collection<Character>),
                typeof(String)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          foreach(string characterId in (@params[0].GetUltimateVariableFor(executor) as Collection<Character>).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = program.GetCharacter(characterId);
            if(_globalVariablesByCharacter.TryGetValue(character.Id, out var characterVariables)) {
              characterVariables.Remove(((String)@params[1].GetUltimateVariableFor(executor)).Value);
              if(!characterVariables.Any()) {
                _globalVariablesByCharacter.Remove(character.Id);
              }
            }
          }

          return null;
        };
      }
    }
  }
}
