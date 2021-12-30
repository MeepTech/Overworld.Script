namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Signifies a return from the GOTO command to the original code
    /// </summary>
    internal class GoToResult : Variable {
      internal GoToResult(Program program, object value, string name = null) 
        : base(program, value, name) {}
    }
  }
}
