using System;

namespace Overworld.Script {

  public partial class Ows {

    public partial class Command {

      public class ALWAYS : Ows.Command.Type {

        ALWAYS() 
          : base(new Identity("ALWAYS"), new System.Type[] {typeof(Command)}) {}

        public override Func<Context, Variable> Execute {
          get;
        } = context => {
          while(true) {
            context.GetUltimateParameterVariable(0);
          }
        };
      }
    }
  }
}
