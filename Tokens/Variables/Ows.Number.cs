namespace Overworld.Script {

  public static partial class Ows {

    public class Number : Variable, INumeric {

      public override object Value {
        get => base.Value;
        protected internal set {
          base.Value = value;
          if(_int is not null) {
            _int = (int)DoubleValue;
          } 
        }
      }

      public float FloatValue 
        => (float)DoubleValue;

      public double DoubleValue
        => (double)Value;

      public int IntValue {
        get => _int ??= (int)DoubleValue;
        set {
          int diff = IntValue - value;
          Value = DoubleValue - diff;
        }
      } int? _int;

      public Number(Program program, double value, string name = null) 
        : base(program, value, name) {}

      public override string ToString() 
        => ((double)IntValue) == DoubleValue
          ? IntValue.ToString()
          : DoubleValue.ToString();
    }
  }
}
