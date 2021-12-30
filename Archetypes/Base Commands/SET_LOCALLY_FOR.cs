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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          foreach(string characterId in (context.GetUltimateParameterVariable<Collection<Character>>(0)).Value.Select(
            character => character.Value.Id
          )) {
            Data.Character character = context.Command.Program.GetCharacter(characterId);
            SetLocalVariableForCharacter(context, characterId, (String)context.OrderedParameters[1], context.OrderedParameters[2]);
          }

          return null;
        };
      }
    }
  }
}
