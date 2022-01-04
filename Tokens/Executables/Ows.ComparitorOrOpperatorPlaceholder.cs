using Meep.Tech.Data;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// A math opperator or comparitor we can't parse yet b/c we don't know the varaible type
    /// </summary>
    public partial class UnknownOperator 
      : Command, INumeric, ITextual, IConditional
    {

      /// <summary>
      /// The comparitor for this command.
      /// How it will compare it's value/values
      /// </summary>
      public string Operator {
        get;
      }

      /// <summary>
      /// Make a new conditional statement
      /// </summary>
      protected UnknownOperator(IBuilder<Command> builder) : base(builder) {
        Operator = builder.GetAndValidateParamAs<string>(nameof(Operator));
      }

      protected internal override Variable _executeWith(Context context) {
        // add the extra param of "this" to the end
        return base._executeWith(context.AddExtraParameter(this));
      }

      public Boolean ComputeFor(Data.Character executor)
        => (this as IParameter).GetUltimateVariableAs<Boolean>(new Context(this, executor));

      public override string ToString()
        => _parameters.Count > 1
          ? $"{_parameters[0]} {Operator} {_parameters[1]}"
            : throw new System.ArgumentException($"Incorrect number of parameters for unknown opperator of type: {Operator}");
    }
  }
}
