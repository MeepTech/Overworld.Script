using Meep.Tech.Data;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class CollectionConcatinator : Command {

      /// <summary>
      /// IF this adds or removes from the "left" collection
      /// </summary>
      public bool IsAdditive {
        get;
      }

      protected CollectionConcatinator(IBuilder<Command> builder) : base(builder) {
        IsAdditive = builder.GetAndValidateParamAs<bool>(nameof(IsAdditive));
      }

      public override string ToString()
        => _parameters.Count > 1
          ? $"({_parameters[0]} {(IsAdditive ? ConcatPhrase: CollectionExclusionPhrase)} {_parameters[1]})"
          : throw new System.ArgumentException($"Incorrect number of parameters for math opperator of type: {(IsAdditive ? ConcatPhrase : CollectionExclusionPhrase)}");
    }
  }
}
