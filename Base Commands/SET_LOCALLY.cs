using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Sets a value to a given key for the executing character
      /// </summary>
      public class SET_LOCALLY : SET {

        SET_LOCALLY()
          : base(
              new("SET-LOCALLY"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          SetLocalVariableForCharacter(program, executor, executor, (String)@params[0], @params[1]);

          return null;
        };
      }
    }
  }
}
