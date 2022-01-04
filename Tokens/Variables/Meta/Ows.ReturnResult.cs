namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Signifies a value that was signaled as a return by the RETURN command
    /// </summary>
    public class ReturnResult : Variable {

      public bool EndAll {
        get;
        internal set;
      } = false;

      public new Variable Value {
        get => base.Value as Variable;
      }

      internal ReturnResult(Program program, Variable value) 
        : base(program, value, null) {}
    }
  }
}
