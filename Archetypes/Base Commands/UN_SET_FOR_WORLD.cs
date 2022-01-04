using System;
using System.Linq;

namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {

      /// <summary>
      /// Un-sets any value for a given key for the given global world level variable 
      /// </summary>
      public class UN_SET_FOR_WORLD : UN_SET {

        UN_SET_FOR_WORLD()
          : base(
              new("UN-SET-FOR-WORLD"),
              new[] {
                typeof(String)
              }
            ) {
        }

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          string variableName = ((String)context.OrderedParameters[1]).Value;
          if(_globals.ContainsKey(variableName)) {
            _globals.Remove(variableName);
          }

          return null;
        };
      }
    }
  }
}
