namespace Overworld.Script {

  public static partial class Ows {
    /// <summary>
    /// A boolean in Ows
    /// </summary>
    public class Boolean :
      Variable, 
      IConditional 
    {

      /// <summary>
      /// The boolean value
      /// </summary>
      public new virtual bool Value
        => (bool)base.Value;

      /// <summary>
      /// Get a reverse of this boolean
      /// </summary>
      public Boolean Not
        => new(Program, !Value);

      /// <summary>
      /// Make a new boolean variable
      /// </summary>
      public Boolean(Program program, bool value = false, string name = null) 
        : base(program, value, name) {}

      /// <summary>
      /// And this and another boolean
      /// </summary>
      public Boolean And(Boolean other)
        => new(Program, Value && other.Value);

      /// <summary>
      /// Or this and another boolean
      /// </summary>
      public Boolean Or(Boolean other)
        => new(Program, Value || other.Value);

      /// <summary>
      /// Just returns itself.
      /// </summary>
      public Boolean ComputeFor(Data.Character character)
        => this;

      public override string ToString()
        => Value == true ? "TRUE" : "FALSE";
    }
  }
}
