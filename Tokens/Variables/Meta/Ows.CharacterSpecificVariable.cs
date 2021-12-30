
namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A container around variables that need a character to fetch their true value
    /// </summary>
    internal class CharacterSpecificVariable : Variable {

      public override object Value
        => throw new System.Exception($"For Character Specific Variables use GetFor instead");

      internal Variable GetFor(Data.Character character)
        => Program.GetVariableByName(character, Name);

      internal CharacterSpecificVariable(Program program, string name)
        : base(program, null, name) {}
    }
  }
}
