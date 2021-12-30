using Meep.Tech.Data;
using Meep.Tech.Data.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public enum Opperators {
      PLUS = '+',
      MINUS = '-',
      TIMES = '*', //checks for X too for numbers
      DIVIDED_BY = '/',
      TO_THE_POWER_OF = '^',
      SQUARED = '`',
      MODULO = '%'
    }

    public static char[] NumberOpperatorChars {
      get;
    } = ((Opperators[])Enum.GetValues(typeof(Opperators))).Where(
      v => v != Opperators.SQUARED
    ).Select(v => ((char)v)).Append('X').ToArray();

    /// <summary>
    /// An executable Ows command
    /// </summary>
    public partial class Command
      : Token,
        IParameter,
        IModel<Command, Command.Type>,
        IModel.IUseDefaultUniverse {

      /// <summary>
      /// All registered command types
      /// </summary>
      public static Command.Type.ArchetypeCollection Types {
        get;
      } = new Archetype<Command, Type>.ArchetypeCollection();

      /// <summary>
      /// The type of archetype
      /// </summary>
      public virtual Type Archetype {
        get;
      }

      /// <summary>
      /// The parameters provided to this command
      /// </summary>
      public virtual IEnumerable<IParameter> Parameters
        => _parameters;
      List<IParameter> _parameters {
        get;
      }

      /// <summary>
      /// The default value, providing null as the character.
      /// Use ExecuteFor if you need a character passed in.
      /// </summary>
      public override object Value
        => ExecuteFor(null);

      /// <summary>
      /// Make a command with the given params
      /// </summary>
      protected Command(IBuilder<Command> builder)
        : base(builder.GetAndValidateParamAs<Program>(nameof(Program)), builder.Archetype.Id.Name) {
        Archetype ??= (Type)builder.Archetype;
        var @params = builder.GetParam(nameof(Command.Parameters), new List<IParameter>());
        if(@params.Count() != Archetype.ParameterTypes.Count) {
          throw new ArgumentException($"Provided only {@params.Count()} parameters to command: {this}, which requires {Archetype.ParameterTypes.Count}");
        }
        _parameters
          ??= @params;
      }

      /// <summary>
      /// Execute this command for the given character
      /// </summary>
      public Variable ExecuteFor(Data.Character executor)
        => _executeWith(new Context(this, executor));

      /// <summary>
      /// Executes this and all commands that it returns until the return is no longer a command
      /// This leaves the last command unexecuted.
      /// </summary>
      public Variable ExecuteUltimateCommandFor(Context context) {
        IParameter current = this;

        // while executable returned, reduce it
        while(current is Command command) {
          current = command._executeWith(context._swapTo(command));
        }

        // return the ultimate value
        return (Variable)current;
      }

      /// <summary>
      /// Execute this command for the given character
      /// </summary>
      internal Variable _executeWithExtraParams(
          Data.Character executor,
          IList<IParameter> extraParameters = null,
          VariableMap scopedParameters = null,
          Index indexReplacement = null)
        => _executeWith(new Context(this, executor, extraParameters, scopedParameters, indexReplacement));

      /// <summary>
      /// Execute this command for the given character
      /// </summary>
      protected internal virtual Variable _executeWith(Context context)
        => Archetype.Execute(context);
    }
  }
}
