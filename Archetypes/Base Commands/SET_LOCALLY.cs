using Meep.Tech.Data.Utility;
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

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          SetLocalVariableForCharacter(context, context.Executor.Id, (String)context.OrderedParameters[0], context.OrderedParameters[1]);

          return null;
        };
      }

      /// <summary>
      /// Sets a value passed via the do:with scope
      /// </summary>
      public class SET_HERE : SET {

        SET_HERE()
          : base(
              new("SET-HERE"),
              new[] {
                typeof(String),
                typeof(IParameter)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          context._temporaryScopedVariables.Value[(context.OrderedParameters[0] as String).Value] 
            = context.GetUltimateParameterVariable(1);

          return null;
        };
      }
    }
  }
}
