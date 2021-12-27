
namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A container around variables that need a character
    /// </summary>
    public class CharacterSpecificVariable : Variable {

      public override object Value
        => throw new System.Exception($"For Character Specific Variables use GetFor instead");

      internal Variable GetFor(Data.Character character)
        => Program.GetVariableByName(character, Name);

      public CharacterSpecificVariable(Program program, string name)
        : base(program, null, name) {}
    }
  }
}
