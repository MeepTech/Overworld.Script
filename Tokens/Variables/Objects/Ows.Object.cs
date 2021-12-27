namespace Overworld.Script {

  public static partial class Ows {
    public class Object : Variable {
      protected Object(Program program, object value, string name = null) 
        : base(program, value, name) {}
    }
  }
}
