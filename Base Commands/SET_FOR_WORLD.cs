using System;
using System.Collections.Generic;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Sets a value to a given key for the whole world
      /// </summary>
      public class SET_FOR_WORLD : SET {

        SET_FOR_WORLD()
          : base(
              new("SET-FOR-WORLD"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        } = (program, executor, @params) => {
          Ows._globals[((String)@params[0]).Value]
            = @params[1].GetUltimateVariableFor(executor);

          return null;
        };
      }
    }
  }
}
