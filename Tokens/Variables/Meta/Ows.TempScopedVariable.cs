
namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A container around variables that need a character to fetch their true value
    /// </summary>
    internal class TempScopedVariable : Variable {

      public override object Value
        => throw new System.Exception($"For Temp Scoped Specific Variables use GetFor instead");

      internal IParameter GetFor(VariableMap context)
        => context.Value[Name];

      internal TempScopedVariable(Program program, string name)
        : base(program, null, name) {}
    }
  }
}
