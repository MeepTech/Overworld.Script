﻿using Meep.Tech.Data;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// Represents a parameter that can be equated to true or false
    /// </summary>
    internal interface IConditional : IParameter {

      /// <summary>
      /// Compute the boolean value for this conditional
      /// </summary>
      public Boolean ComputeFor(Data.Character executor);
    }

    /// <summary>
    /// A conditional statement/command (true or false)
    /// Takes in 1 or 2[default:null] items and uses the comparitor to return a boolean result
    /// </summary>
    public partial class Condition 
      : Command,
        IConditional
    {

      /// <summary>
      /// The comparitor for this command.
      /// How it will compare it's value/values
      /// </summary>
      public Comparitors Comparitor {
        get;
      }

      /// <summary>
      /// The boolean value direved from the conditional
      /// </summary>
      public new bool Value
        => (bool)base.Value;

      /// <summary>
      /// Make a new conditional statement
      /// </summary>
      protected Condition(IBuilder<Command> builder) : base(builder) {
        Comparitor = builder.GetParam(nameof(Comparitor), Comparitors.IDENTITY);
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      protected internal override Variable _executeWith(Context context) {
        // add the extra param of "this" to the end
        return base._executeWith(context.AddExtraParameter(this));
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public Boolean ComputeFor(Data.Character executor)
        => (Boolean)ExecuteFor(executor);

      public override string ToString()
        => _parameters[1] is not null
          ? $"({_parameters[0]} {Comparitor} {_parameters[1]})"
          : Comparitor == Comparitors.IDENTITY
            ? $"{_parameters[0]}"
            : Comparitor == Comparitors.NOT
              ? $"{Comparitor}-{_parameters[0]}"
              : throw new System.ArgumentException($"Incorrect number of parameters for math opperator of type: {Comparitor}");
    }
  }
}
