namespace Overworld.Script {

  public static partial class Ows {
    /// <summary>
    /// Represents an index for a For loop
    /// </summary>
    public class Index : Number {
      internal Index(Program program, int value) 
        : base(program, value, null) {}

      public void Increment() {
        Value = IntValue + 1;
      }

      public void Decrement() {
        Value = IntValue - 1;
      }
    }
  }
}
