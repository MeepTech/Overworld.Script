﻿namespace Overworld.Script {

  public static partial class Ows {

    public partial class Interpreter {
      public class Exception : System.Exception {
        public Exception(
          int lineNumber,
          System.Exception innerException = null,
          string lineText = null,
          int? charLocation = null,
          string customMessage = null
        ) : base(
          customMessage
            ?? $"Error In Ows Program on line {lineNumber}.",
          innerException
        ) {
        }
      }
    }
  }
}
