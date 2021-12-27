namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A string variable in an Ows program
    // TODO: add {Value} syntax
    /// </summary>
    public class String : Variable {

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public new string Value
        => (string)base.Value;

      /// <summary>
      /// Make a new string variable
      /// </summary>
      public String(Program program, string value, string name = null) 
        : base(program, value, name) {}
    }
  }
}
