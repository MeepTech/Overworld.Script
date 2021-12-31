namespace Overworld.Script {

  public partial class Ows {
    public partial class Command {
      /// <summary>
      /// Go To a Line of Code (alias)
      /// </summary>
      public class GOTO : GO_TO {
        protected GOTO() 
          : base(new("GOTO")) {}
      }
    }
  }
}
