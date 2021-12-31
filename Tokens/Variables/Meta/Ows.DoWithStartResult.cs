namespace Overworld.Script {

  public static partial class Ows {
    /// <summary>
    /// Signifies a value that was signaled as a return by the RETURN command
    /// </summary>
    public class DoWithStartResult : Variable {

      public new Variable Value {
        get => base.Value as Variable;
      }

      internal VariableMap _scopedParams;

      internal int _targetLineNumber;

      internal String _goToLabel {
        get;
      }

      internal DoWithStartResult(Program program, String label) 
        : base(program, null, null) {
        _goToLabel = label;
        _scopedParams = new VariableMap(Program);
      }
    }
  }
}
