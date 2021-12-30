using System.Collections.Generic;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A collection of named variables.
    /// </summary>
    internal class VariableMap : Variable {

      public new Dictionary<string, Variable> Value {
        get
        => (Dictionary<string, Variable>)base.Value 
          ?? new Dictionary<string, Variable>();
        internal set 
          => base.Value = value;
      }

      internal VariableMap(Program program, Dictionary<string, Variable> values)
        : base(program, values) { }
    }
  }
}
