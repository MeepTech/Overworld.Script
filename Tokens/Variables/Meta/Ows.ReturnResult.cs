namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Signifies a value that was signaled as a return by the RETURN command
    /// </summary>
    public class ReturnAllResult : Variable {

      public new Variable Value {
        get => base.Value as Variable;
      }

      internal ReturnAllResult(Program program, Variable finalReturnResult) 
        : base(program, finalReturnResult, null) {}
    }

    /// <summary>
    /// Signifies a value that was given back from a return.
    /// Just gets sent back to the caller
    /// </summary>
    public class ReturnResult : Variable {

      public new Variable Value {
        get => base.Value as Variable;
      }

      internal ReturnResult(Program program, Variable finalReturnResult) 
        : base(program, finalReturnResult, null) {}
    }
  }
}
