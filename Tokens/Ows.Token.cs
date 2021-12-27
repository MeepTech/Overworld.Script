using System;

namespace Overworld.Script {

  public static partial class Ows {

    /// <summary>
    /// The base part of the OWs language
    /// </summary>
    public interface IToken {

      /// <summary>
      /// The program this command is for
      /// </summary>
      Program Program {
        get;
      }

      /// <summary>
      /// The token's name identifier
      /// </summary>
      string Name {
        get;
      }

      /// <summary>
      /// The final computed value of this token
      /// </summary>
      object Value {
        get;
      }
    }

    /// <summary>
    /// The base part of the OWs language
    /// </summary>
    public class Token : IToken {

      /// <summary>
      /// The program this command is for
      /// </summary>
      public Program Program {
        get;
        internal set;
      }

      /// <summary>
      /// The token's name identifier
      /// </summary>
      public string Name {
        get;
      }

      /// <summary>
      /// The final computed value of this token
      /// </summary>
      public virtual object Value {
        get;
        internal protected set;
      }

      /// <summary>
      /// Base to make a new token
      /// </summary>
      protected Token(Program program, string name = null) {
        Program = program;
        Name = name ?? new Guid().ToString();
      }
    }
  }
}
