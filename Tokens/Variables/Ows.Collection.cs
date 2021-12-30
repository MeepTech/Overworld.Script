using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {


    /// <summary>
    /// A collection of values.
    /// Rarely used.
    /// </summary>
    public abstract class Collection : Variable {

      public new ArrayList Value {
        get
        => (ArrayList)base.Value;
        internal set 
          => base.Value = value;
      }

      protected Collection(Program program, ICollection value, string name = null)
        : base(program, value, name) { }
    }

    /// <summary>
    /// A collection of values.
    /// Rarely used.
    /// </summary>
    public class Collection<TValue> : Collection, IParameter
      where TValue : Variable 
    {

      public new IList<TValue> Value
        => _value ?? base.Value.Cast<TValue>().ToList();
      IList < TValue > _value;

      /// <summary>
      /// Gets the compiled collection for the executor
      /// </summary>
      Variable IParameter.GetUltimateVariableFor(Command.Context context)
        => new Collection<TValue>(Program, (IList)_value 
          ?? Value.Cast<IParameter>()
              .Select(param => param.GetUltimateVariableFor(context))
              .Cast<TValue>()
              .ToList());

      public Collection(Program program, IList value, string name = null) 
        : base(program, value, name) {}
    }
  }
}
