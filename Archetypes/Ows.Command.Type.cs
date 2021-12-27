using Meep.Tech.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class Command {

      /// <summary>
      /// A type of command in Ows
      /// </summary>
      public abstract class Type : Archetype<Command, Command.Type> {

        protected override Dictionary<string, object> DefaultTestParams
          => new() {
            { nameof(Program), null},
            { nameof(Command.Parameters), ParameterTypes.Select(type => default(Variable)).Cast<IParameter>().ToList()}
          };

        /// <summary>
        /// The types of params that this command requires
        /// </summary>
        public virtual IList<System.Type> ParameterTypes {
          get;
          internal set;
        }

        /// <summary>
        /// Execute logic for this command.
        /// Parameters: 
        ///   The Program,
        ///   The Character Executing the Command,
        ///   The Ordered Parameters provided to the Command
        /// </summary>
        public virtual Func<Program, Data.Character, IList<IParameter>, Variable> Execute {
          get;
        }

        #region Initialization and Configuration

        /// <summary>
        /// Make a new command
        /// </summary>
        protected Type(Identity id, IEnumerable<System.Type> paramTypes)
          : base(id, Types) {
          ParameterTypes = paramTypes.Select(
            paramType => typeof(IParameter).IsAssignableFrom(paramType)
              ? paramType
              : throw new ArgumentException($"Param type must inherit from Ows.IParameter: {paramType.FullName}."))
            .ToList();
        }

        #endregion

        /// <summary>
        /// Make a command of the given type
        /// </summary>
        public TCommand Make<TCommand>(Program program, IEnumerable<IParameter> orderedParameters)
          where TCommand : Command 
            => Make<TCommand>(
              (nameof(Command.Parameters), orderedParameters.ToList()),
              (nameof(Command.Program), program)
            );
        
        /// <summary>
        /// Make a command of the given type
        /// </summary>
        public TCommand Make<TCommand>(Program program, params IParameter[] orderedParameters)
          where TCommand : Command
            => Make<TCommand>(program, orderedParameters.ToList());

        /// <summary>
        /// Make a command of the given type
        /// </summary>
        public Command Make(Program program, IEnumerable<IParameter> orderedParameters)
          => Make(
            (nameof(Command.Parameters), orderedParameters.ToList()),
            (nameof(Command.Program), program)
          );
        
        /// <summary>
        /// Make a command of the given type
        /// </summary>
        public Command Make(Program program, params IParameter[] orderedParameters)
          => Make(program, orderedParameters.ToList());
      }
    }
  }
}