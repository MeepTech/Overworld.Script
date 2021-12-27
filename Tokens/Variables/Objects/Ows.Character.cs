namespace Overworld.Script {

  public static partial class Ows {
    public class Character : Entity {

      public new Data.Character Value 
        => (Data.Character)base.Value;

      public Character(Program program, Data.Character value, string name = null)
        : base(program, value, name) { }
    }
  }
}
