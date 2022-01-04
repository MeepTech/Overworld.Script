using System;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Un-sets any value for a given key for the given global program level variable 
      /// </summary>
      public class UN_SET_FOR_PROGRAM : UN_SET {

        UN_SET_FOR_PROGRAM()
          : base(
              new("UN-SET-FOR-PROGRAM"),
              new[] {
                typeof(String)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          string variableName = ((String)context.OrderedParameters[1]).Value;
          if(context.Command.Program._globals.ContainsKey(variableName)) {
            context.Command.Program._globals.Remove(variableName);
          }

          return null;
        };
      }
    }
  }
}
