
namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A container around variables that  need a character or scoped context to fetch their true value
    /// </summary>
    internal class ScopedVariable : Variable {

      public override object Value
        => throw new System.Exception($"For Character Specific Variables use GetFor instead");

      internal Variable GetFor(Command.Context context, string charachterId = null)
        => context.GetFirstVariable(Name, charachterId);

      internal ScopedVariable(Program program, string name)
        : base(program, null, name) {}

      public override string ToString()
        => Name;
    }
  }
}
