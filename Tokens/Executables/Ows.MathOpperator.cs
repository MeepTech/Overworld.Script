using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A math opperator
    /// </summary>
    public partial class MathOpperator 
      : Command
    {

      /// <summary>
      /// The comparitor for this command.
      /// How it will compare it's value/values
      /// </summary>
      public Opperators Opperator {
        get;
      }

      /// <summary>
      /// The final value direved from the math
      /// </summary>
      public new double Value
        => (double)base.Value;

      /// <summary>
      /// Make a new conditional statement
      /// </summary>
      protected MathOpperator(IBuilder<Command> builder) : base(builder) {
        Opperator = builder.GetAndValidateParamAs<Opperators>(nameof(Opperator));
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected override Variable _executeFor(Data.Character executor, IEnumerable<IParameter> extraParams, Index indexReplacement = null) {
        // add the extra param of "this" to the end
        return base._executeFor(executor, extraParams.Append(this), indexReplacement);
      }
    }
  }
}
