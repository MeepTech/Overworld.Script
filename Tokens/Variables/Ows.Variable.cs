namespace Overworld.Script {

  public static partial class Ows {

    public class Variable : Token, IParameter {

      /// <summary>
      /// Available variable scopes
      /// </summary>
      public enum Scopes {
        Program,
        World
      }

      /// <summary>
      /// Used to make a new variable
      /// </summary>
      protected Variable(Program program, object value, string name = null) : base(program, name) {
        Value = value;
      }

      public bool IsCharacterSpecific {
        get;
        internal set;
      }

      /// <summary>
      /// Make a new named variable of the right basic type
      /// </summary>
      public static Variable Make(Program program, string name, object @object) {
        switch(@object) {
          case string @string:
            return new String(program, name, @string);
          case double number:
            return new Number(program, number, name);
          case int number:
            return new Number(program, number, name);
          case float number:
            return new Number(program, number, name);
          case bool @bool:
            return new Boolean(program, @bool, name);
          // TODO: Modular object creation
          case Data.Character character:
            return new Character(program, character, name);
          case Data.Entity entity:
            return new Entity(program, entity, name);
          // TODO: add world object
          default:
            throw new System.NotSupportedException($"Variable Type {@object.GetType()}, not supported by Ows");
        }
      }
    }
  }
}
