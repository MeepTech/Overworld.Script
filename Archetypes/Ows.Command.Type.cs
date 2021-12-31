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
        public IList<System.Type> ParameterTypes {
          get;
          internal set;
        }

        /// <summary>
        /// Execute logic for this command.
        /// Parameters: 
        ///   The Command context,
        /// </summary>
        public abstract Func<Command.Context, Variable> Execute {
          get;
        }

        /// Optional:

        /// A type of token this command is expected to return
        public virtual IEnumerable<System.Type> ExpectedReturnTypes {
          get;
        } = new[] { typeof(Token), null };

        /// <summary>
        /// A description of this command
        /// </summary>
        public virtual string Description {
          get;
        } = "A Command";

        /// <summary>
        /// Examples of command usage
        /// </summary>
        public virtual (string code, string summary)[] Examples {
          get;
        } = new (string code, string summary)[0];

        #region Initialization and Configuration

        /// <summary>
        /// A command Id
        /// </summary>
        public new class Identity : Archetype<Command, Type>.Identity {
          public Identity(string name, string keyPrefixEndingAdditions = null) 
            : base(name, keyPrefixEndingAdditions) 
          {}
        }

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
          if(ExpectedReturnTypes is not null) {
            foreach(System.Type expextedReturnType in ExpectedReturnTypes) {
              if(expextedReturnType != null && !typeof(Token).IsAssignableFrom(expextedReturnType)) {
                throw new ArgumentException($"Return type must be null or inherit from Token: {expextedReturnType.FullName}");
              }
            }
          }
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