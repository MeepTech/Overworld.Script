namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Signifies a return from the GOTO command to the original code
    /// </summary>
    internal class GoToResult : Variable {

      internal int _fromLine;

      internal GoToResult(Program program) 
        : base(program, null, null) {}
    }
  }
}
