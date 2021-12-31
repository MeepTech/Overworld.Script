
namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// These values can be parameters for Ows Program Commands
    /// </summary>
    public interface IParameter : IToken {

      /// <summary>
      /// Get the ultimate value from a parameter
      /// </summary>
      public object GetUltimateValueFor(Command.Context context)
        => GetUltimateVariableFor(context)?.Value;

      /// <summary>
      /// Get the ultimate variable from a parameter
      /// </summary>
      public Variable GetUltimateVariableFor(Command.Context context) {
        IParameter current = this;

        // while executable returned, reduce it
        while(current is Command command) {
          Command.Context derivedContext = context;
          derivedContext._swapTo(command);
          current = command._executeWith(derivedContext);
        }

        // return the ultimate value
        return (Variable)current;
      }

      /// <summary>
      /// Get the ultimate variable from a parameter
      /// </summary>
      public TVariable GetUltimateVariableAs<TVariable>(Command.Context context)
        where TVariable : Variable
          => (TVariable)GetUltimateVariableFor(context);
    }
  }
}
