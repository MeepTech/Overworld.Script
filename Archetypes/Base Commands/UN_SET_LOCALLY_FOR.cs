using System;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          foreach(string characterId in (context.GetUltimateParameterVariable<Collection<Character>>(0)).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = context.Command.Program.GetCharacter(characterId);
            if(context.Command.Program._variablesByCharacter.TryGetValue(character.Id, out var characterVariables)) {
              characterVariables.Remove(((String)context.OrderedParameters[1]).Value);
              if(!characterVariables.Any()) {
                context.Command.Program._variablesByCharacter.Remove(character.Id);
              }
            }
          }

          return null;
        };
      }
    }
  }
}
