using Meep.Tech.Data;
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
      To_The_Power_Of = '^',
      Squared = '`',
    }

    public static char[] NumberOpperatorChars {
      get;
    } = ((Opperators[])Enum.GetValues(typeof(Opperators))).Where(
      v => v != Opperators.Squared
    ).Select(v => ((char)v)).Append('X').ToArray();

    /// <summary>
    /// An executable Ows command
    /// </summary>
    public partial class Command 
      : Token,
        IParameter,
        IModel<Command, Command.Type>,
        IModel.IUseDefaultUniverse
    {

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
      public virtual Variable ExecuteFor(Data.Character executor)
        => _executeWithExtraParams(executor, Parameters);

      /// <summary>
      /// Execute this command for the given character with som eextra provided commands
      /// </summary>
      protected internal Variable _executeWithExtraParams(Data.Character executor, IEnumerable<IParameter> extraParams)
        => Archetype.Execute(Program, executor, _parameters.Concat(extraParams).Select(param => param is CharacterSpecificVariable characterSpecific
          // get the character specific variable if there's one
          ? characterSpecific.GetFor(executor)
          : param).ToList());
    }
  }
}
