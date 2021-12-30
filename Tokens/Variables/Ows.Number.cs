namespace Overworld.Script {

  public static partial class Ows {
    public class Number : Variable {
      public double RawValue {
        get => DoubleValue;
        set {
          DoubleValue = value;
          FloatValue = (float)value;
          IntValue = (int)value;
        }
      }

      public float FloatValue {
        get;
        private set;
      }
      public double DoubleValue {
        get;
        private set;
      }
      public int IntValue {
        get;
        private set;
      }

      public Number(Program program, double value, string name = null) : base(program, value, name) {
        RawValue = value;
      }
    }
  }
}
