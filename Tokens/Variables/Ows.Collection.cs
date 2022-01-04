using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {


    /// <summary>
    /// A collection of values.
    /// </summary>
    public class Collection : Variable, IParameter {

      /// <summary>
      /// If this collection is restricted to a type:
      /// </summary>
      public System.Type RestrictedToType {
        get;
      }

      public new List<IParameter> Value {
        get
        => (List<IParameter>)base.Value;
        internal set 
          => base.Value = value;
      }

      /// <summary>
      /// Gets the compiled collection for the executor
      /// </summary>
      Variable IParameter.GetUltimateVariableFor(Command.Context context) {
        ArrayList values = new ArrayList();
        foreach(IParameter item in Value) {
          Variable variable = item.GetUltimateVariableFor(context);
          if(RestrictedToType != null) {
            if(!RestrictedToType.IsAssignableFrom(variable.GetType())) {
              throw new System.ArgumentException($"Incorrect item type: {variable.GetType()}, added to collection of type {RestrictedToType.Name}.");
            }
          }
          values.Add(variable);
        }

        return new Collection(context.Command.Program, values, RestrictedToType);
      }

      public Collection(Program program, ICollection value, System.Type restrictTo = null, string name = null)
        : base(program, value, name) {
        RestrictedToType = restrictTo;
      }

      public override string ToString()
        => $"[ {string.Join(" AND ", Value)} ]";
    }

    /// <summary>
    /// Used to show what type of collection a command wants.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public abstract class Collection<TValue>
      : Collection where TValue : Variable {

      public new List<TValue> Value {
        get => base.Value.Cast<TValue>().ToList();
      }

      protected Collection(Program program, ICollection value, Type restrictTo = null, string name = null) 
        : base(program, value, restrictTo, name) {}
    }
  }
}
