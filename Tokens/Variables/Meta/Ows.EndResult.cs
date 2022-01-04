namespace Overworld.Script {

  public static partial class Ows {
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
