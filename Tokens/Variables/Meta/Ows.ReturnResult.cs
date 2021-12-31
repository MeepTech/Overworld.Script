namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Signifies a value that was signaled as a return by the RETURN command
    /// </summary>
    public class ReturnResult : Variable {

      public new Variable Value {
        get => base.Value as Variable;
      }

      internal ReturnResult(Program program, Variable value) 
        : base(program, value, null) {}
    }

    /// <summary>
    /// Signifies a kill/end value
    /// </summary>
    public class EndResult : Variable {

      public new Variable Value
        => throw new System.Exception($"Tried to get value of END result.");

      internal EndResult(Program program) 
        : base(program, null, null) {}
    }
  }
}
