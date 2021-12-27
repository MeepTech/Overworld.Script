using Meep.Tech.Data;
using Overworld.Script;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class Program {

      /// <summary>
      /// Context for an Ows program
      /// </summary>
      public class ContextData {

        /// <summary>
        /// components attached to this context data
        /// </summary>
        public object[] Components {
          get;
        }

        /// <summary>
        /// The commands within this context
        /// </summary>
        public virtual IReadOnlyDictionary<string, Command.Type> Commands
          => _commands;
        readonly Dictionary<string, Command.Type> _commands;

        /// <summary>
        /// The characters in the world of thdis script
        /// </summary>
        public IReadOnlyDictionary<string, Data.Character> Characters 
          => _characters;
        readonly Dictionary<string, Data.Character> _characters;
        
        /// <summary>
        /// The entities in the world of this script
        public IReadOnlyDictionary<string, Data.Entity> Entities
          => _entities;
        readonly Dictionary<string, Data.Entity> _entities;

        /// <summary>
        /// Create a context for a new program
        /// </summary>
        public ContextData(
          Dictionary<string, Command.Type> commands, 
          Dictionary<string, Data.Entity> entities,
          Dictionary<string, Data.Character> characters,
          params object[] components
        ) {
          Components = components ?? new object[0];
          _commands = commands?.MergeIn(Ows.DefaultCommands) 
            ?? new Dictionary<string, Command.Type>(Ows.DefaultCommands);
          _entities = entities ?? new Dictionary<string, Data.Entity>();
          _characters = characters ?? new Dictionary<string, Data.Character>();
        }

        /// <summary>
        /// Create a context for a new program
        /// </summary>
        public ContextData(
          params object[] components
        ) : this() {
          Components = components;
        }

        /// <summary>
        /// Create a context for a new program
        /// </summary>
        public ContextData(
          Dictionary<string, Command.Type> commands = null, 
          Dictionary<string, Data.Entity> entities = null,
          Dictionary<string, Data.Character> characters = null
        ) {
          Components = new object[0];
          _commands = commands?.MergeIn(Ows.DefaultCommands)
            ?? new Dictionary<string, Command.Type>(Ows.DefaultCommands);
          _entities = entities ?? new Dictionary<string, Data.Entity>();
          _characters = characters ?? new Dictionary<string, Data.Character>();
        }
      }
    }
  }
}
