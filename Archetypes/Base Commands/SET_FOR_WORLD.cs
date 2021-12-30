using Meep.Tech.Data.Utility;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          Ows._globals[((String)context.OrderedParameters[0]).Value]
            = context.GetUltimateParameterVariable(1);

          return null;
        };
      }
    }
  }
}
