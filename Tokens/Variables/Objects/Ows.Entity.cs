namespace Overworld.Script {

  public static partial class Ows {
    public class Entity : Object {

      public new Data.Entity Value 
        => (Data.Entity)base.Value;

      public Entity(Program program, Data.Entity value, string name = null)
        : base(program, value, name) { }
    }
  }
}
