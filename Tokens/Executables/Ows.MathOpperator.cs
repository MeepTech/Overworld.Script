using Meep.Tech.Data;

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

      protected internal override Variable _executeWith(Context context) {
        // add the extra param of "this" to the end
        return base._executeWith(context.AddExtraParameter(this));
      }
    }
  }
}
