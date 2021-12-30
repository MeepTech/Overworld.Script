namespace Overworld.Script {

  public static partial class Ows {
    /// <summary>
    /// Represents a placeholder for an index for a For loop
    /// </summary>
    internal class PlaceholderIndex : Index {
      internal PlaceholderIndex(Program program) 
        : base(program, 0) {}
    }
  }
}
