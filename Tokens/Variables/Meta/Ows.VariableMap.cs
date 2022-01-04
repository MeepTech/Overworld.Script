using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A collection of named variables.
    /// </summary>
    internal class VariableMap : Variable {

      public new Dictionary<string, IParameter> Value {
        get
        => (Dictionary<string, IParameter>)base.Value 
          ?? new Dictionary<string, IParameter>();
        internal set 
          => base.Value = value;
      }

      internal VariableMap(Program program, Dictionary<string, IParameter> values = null)
        : base(program, values ?? new Dictionary<string, IParameter>()) {
      }

      public override string ToString() {
        if(!Value.Any()) {
          return "";
        }

        string @return = ConcatPhrase + " [ ";
        @return += string.Join(
          " AND ", 
          Value.Select(item => $"{item.Key} {SetsAsPhrase} {item.Value}")
        );
        @return += " ]";

        return @return;
      }
    }
  }
}
