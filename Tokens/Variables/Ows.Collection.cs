using System.Collections;
using System.Collections.Generic;

namespace Overworld.Script {

  public static partial class Ows {


    /// <summary>
    /// A collection of values.
    /// Rarely used.
    /// </summary>
    public abstract class Collection : Variable {

      public new ICollection Value
        => (ICollection)base.Value;

      protected Collection(Program program, ICollection value, string name = null)
        : base(program, value, name) { }
    }

    /// <summary>
    /// A collection of values.
    /// Rarely used.
    /// </summary>
    public class Collection<TValue> : Collection
      where TValue : Variable 
    {
      public new IList<TValue> Value
        => (IList<TValue>)base.Value;

      public Collection(Program program, IList<TValue> value, string name = null) 
        : base(program, (ICollection)value, name) {}
    }
  }
}
