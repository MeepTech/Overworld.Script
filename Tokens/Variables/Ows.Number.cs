namespace Overworld.Script {

  public static partial class Ows {
    public class Number : Variable {
      public double RawValue;
      public float FloatValue;
      public int IntValue;

      public Number(Program program, double value, string name = null) : base(program, value, name) {
        RawValue = value;
        FloatValue = (float)value;
        IntValue = (int)value;
      }
    }
  }
}
