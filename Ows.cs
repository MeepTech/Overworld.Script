using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  /// <summary>
  /// Overworld Script
  /// </summary>
  public static partial class Ows {

    public static IReadOnlyDictionary<string, Command.Type> DefaultCommands {
      get;
    } = new Command.Type[] {
      Command.Types.Get<Command.END>(),
      Command.Types.Get<Command.GO_BACK>(),
      Command.Types.Get<Command.GO_TO>(),
      Command.Types.Get<Command.IF>(),
      Command.Types.Get<Command.IF_NOT>(),
      Command.Types.Get<Command.SET>(),
      Command.Types.Get<Command.SET_FOR>(),
      Command.Types.Get<Command.SET_FOR_PROGRAM>(),
      Command.Types.Get<Command.SET_FOR_WORLD>(),
      Command.Types.Get<Command.SET_LOCALLY>(),
      Command.Types.Get<Command.SET_LOCALLY_FOR>(),
      Command.Types.Get<Command.UN_SET>(),
      Command.Types.Get<Command.UN_SET_FOR>(),
      Command.Types.Get<Command.UN_SET_LOCALLY_FOR>(),
      Command.Types.Get<Command.UNTIL>(),
      Command.Types.Get<Command.WHILE>(),
      Command.Types.Get<Command.FOR>()
    }.ToDictionary(
      e => e.Id.Name,
      e => e
    );

    /// <summary>
    /// True global variables
    /// </summary>
    static Dictionary<string, Variable> _globals
      = new();

    /// <summary>
    /// The variables unique to each character, by variable name
    /// </summary>
    static Dictionary<string, Dictionary<string, Variable>> _globalVariablesByCharacter
        = new();

    /// <summary>
    /// Used to indicate "All" in some cases
    /// </summary>
    public const char CollectAllSymbol = '*';

    /// <summary>
    /// Used to indicate "All" in some cases
    /// </summary>
    public const string CollectAllPhrase = "ALL";

    /// <summary>
    /// An extra phrase that can be used with concatination
    /// </summary>
    public const string ConcatPhrase = "WITH";

    /// <summary>
    /// An extra phrase that can be used with concatination
    /// </summary>
    public static readonly string AndConcatPhrase 
      = Comparitors.AND.ToString();

    /// <summary>
    /// An extra phrase that can be used for the opposite of concatination
    /// </summary>
    public const string CollectionExclusionPhrase = "WITHOUT";

    /// <summary>
    /// The initial Symbol indicating the beginning of a collection
    /// </summary>
    public const char CollectionStartSymbol = '[';

    /// <summary>
    /// The initial Symbol indicating a label is beginning this line
    /// </summary>
    public const char LabelStartSymbol = '[';

    /// <summary>
    /// Symbol that indicates the end of a label
    /// </summary>
    public const char LabelEndSymbol = ']';

    /// <summary>
    /// Symbol that indicates the end of a opperator or condition section
    /// </summary>
    public const char LogicEndSymbol = ')';

    /// <summary>
    /// Symbol that indicates the start of a opperator or condition section
    /// </summary>
    public const char LogicStartSymbol = '(';

    /// <summary>
    /// Symbol that indicates the end of a collection
    /// </summary>
    public static char[] CollectionItemSeperatorSymbols
      = new[] {
        ',',
        (char)Comparitors.AND
      };

    /// <summary>
    /// Symbol that indicates the end of a collection
    /// </summary>
    public const char CollectionEndSymbol = ']';

    /// <summary>
    /// Symbol that seperates a function name and it's parameters
    /// </summary>
    public const char FunctionSeperatorSymbol = ':';

    /// <summary>
    /// Used for decimals in numbers
    /// </summary>
    private const char DecimalSymbol = '.';

    /// <summary>
    /// Symbol that is used to represent a string
    /// </summary>
    public const char StringQuotesSymbol = '"';

    /// <summary>
    /// Phrase used to set items to variables
    /// </summary>
    public const string SetToPhrase = " TO ";

    /// <summary>
    /// Phrase used to set items to variables
    /// </summary>
    public const char SetToSymbol = '=';

    /// <summary>
    /// Forces a line to end. Try not to abuse this it can make debugging harder
    /// </summary>
    public const char LineEndAlternateSymbol = ';';
  }
}
