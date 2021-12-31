using System;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          foreach(string characterId in context.GetUltimateParameterVariable<Collection<Character>>(0).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character =  context.Command.Program.GetCharacter(characterId);
            if(_globalVariablesByCharacter.TryGetValue(character.Id, out var characterVariables)) {
              characterVariables.Remove(((String)context.OrderedParameters[1]).Value);
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
