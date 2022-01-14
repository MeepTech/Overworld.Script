using Meep.Tech.Data;
using System.Collections.Generic;

namespace Overworld.Script {

  public static partial class Ows {

    public partial class Program {

      /// <summary>
      /// Context for an Ows program
      /// </summary>
      public class ContextData {

        /// <summary>
        /// The commands within this context
        /// </summary>
        public virtual Dictionary<string, Command.Type> Commands
          => _commands;
        readonly Dictionary<string, Command.Type> _commands;

        /// <summary>
        /// The characters in the world of thdis script
        /// </summary>
        public Dictionary<string, Data.Character> Characters 
          => _characters;
        readonly Dictionary<string, Data.Character> _characters;
        
        /// <summary>
        /// The entities in the world of this script
        public Dictionary<string, Data.Entity> Entities
          => _entities;
        readonly Dictionary<string, Data.Entity> _entities;

        /// <summary>
        /// Create a context for a new program
        /// </summary>
        public ContextData(
          Dictionary<string, Command.Type> commands = null, 
          Dictionary<string, Data.Entity> entities = null,
          Dictionary<string, Data.Character> characters = null
        ) {
          _commands = commands ?? new Dictionary<string, Command.Type>();
          Ows.DefaultCommands.ForEach(defaultCommand => _commands.Append(defaultCommand.Key, defaultCommand.Value));
          _entities = entities ?? new Dictionary<string, Data.Entity>();
          _characters = characters ?? new Dictionary<string, Data.Character>();
        }
      }
    }
  }
}
