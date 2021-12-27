using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public enum Comparitors {
      IDENTITY = '$',
      NOT = '!',
      AND = '&',
      OR = '|',
      EQUALS = '=',
      GREATER_THAN = '>',
      LESS_THAN = '<'
    }

    public static string[] ComparitorPhrases {
      get;
    } = ((Comparitors[])Enum.GetValues(typeof(Comparitors)))
      .Select(e => e.ToString().Replace('_', '-')).ToArray();

    public static char[] ComparitorPhraseStartChars {
      get;
    } = ComparitorPhrases.Select(v => v[0]).ToArray();

    public static char[] ComparitorSymbols {
      get;
    } = ((Comparitors[])Enum.GetValues(typeof(Comparitors)))
      .Select(e => (char)e).ToArray();

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
      protected override Variable _executeFor(Data.Character executor, IEnumerable<IParameter> extraParams, Index indexReplacement = null) {
        // add the extra param of "this" to the end
        return base._executeFor(executor, extraParams.Append(this), indexReplacement);
      }

      /// <summary>
      /// <inheritdoc/>
      /// </summary>
      public Boolean ComputeFor(Data.Character executor)
        => (Boolean)ExecuteFor(executor);
    }
  }
}
