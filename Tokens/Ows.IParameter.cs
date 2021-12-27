
namespace Overworld.Script {

  public static partial class Ows {
    /// <summary>
    /// These values can be parameters for Ows Program Commands
    /// </summary>
    public interface IParameter : IToken {

      /// <summary>
      /// Get the ultimate value from a parameter
      /// </summary>
      public object GetUltimateValueFor(Data.Character character) {
        IParameter current = this;
        // while executable returned, reduce it
        while(current is Command command) {
          current = command.ExecuteFor(character);
        }

        // return the ultimate value
        return current?.Value;
      }

      /// <summary>
      /// Get the ultimate variable from a parameter
      /// </summary>
      public Variable GetUltimateVariableFor(Data.Character character) {
        IParameter current = this;
        // while executable returned, reduce it
        while(current is Command command) {
          current = command.ExecuteFor(character);
        }

        // return the ultimate value
        return (Variable)current;
      }
    }
  }
}
