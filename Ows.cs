using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {

  /// <summary>
  /// Overworld Script
  /// </summary>
  public static partial class Ows {

    #region Comparitors

    /// <summary>
    /// Used to compare values
    /// </summary>
    public enum Comparitors {
      IDENTITY = '$',
      NOT = '!',
      AND = '&',
      OR = '|',
      EQUALS = '=',
      GREATER_THAN = '>',
      LESS_THAN = '<'
    }

    public static string NotComparitorPrefixPhrase {
      get;
    } = Comparitors.NOT.ToString() + "-";

    /// <summary>
    /// Comparitor phrases used for finding comparitors in code
    /// </summary>
    public static string[] ComparitorPhrases {
      get;
    } = ((Comparitors[])Enum.GetValues(typeof(Comparitors)))
      .Except(new[] { Comparitors.IDENTITY })
      .Except(new[] { Comparitors.NOT })
      .Select(e => e.ToString().Replace('_', '-'))
      .Append(NotComparitorPrefixPhrase)
      .ToArray();

    /// <summary>
    /// cached Characters starting the comparitor phrases
    /// </summary>
    /*public static char[] ComparitorPhraseStartChars {
      get;
    } = ComparitorPhrases.Select(v => v[0]).ToArray();*/

    /// <summary>
    /// Symbols used to find comparitors in code
    /// </summary>
    public static char[] ComparitorSymbols {
      get;
    } = ((Comparitors[])Enum.GetValues(typeof(Comparitors)))
      .Except(new [] {Comparitors.IDENTITY})
      .Select(e => (char)e).ToArray();

    #endregion

    /// <summary>
    /// Used to opperate on values
    /// </summary>
    #region Opperators

    public enum Opperators {
      PLUS = '+',
      MINUS = '-',
      TIMES = '*', //checks for X too for numbers
      DIVIDED_BY = '/',
      TO_THE_POWER_OF = '^',
      SQUARED = '²',
      MODULO = '%'
    }

    /// <summary>
    /// An extra symbol used to check for times opperations.
    /// This is not reserved as a variable name, it shouldn't need to be for how it's used.
    /// </summary>
     public const char TimesOpperatorExtraSymbol 
        = 'X';

    /// <summary>
    /// Symbols used for number maths in code
    /// </summary>
    public static char[] NumberOpperatorSymbols {
      get;
    } = ((Opperators[])Enum.GetValues(typeof(Opperators))).Where(
      // The squared symbol isn't used:
      v => v != Opperators.SQUARED
    ).Select(v => ((char)v)).Append(TimesOpperatorExtraSymbol).ToArray();

    /// <summary>
    /// Words used for number maths in code
    /// </summary>
    public static string[] NumberOpperatorPhrases {
      get;
    } = ((Opperators[])Enum.GetValues(typeof(Opperators)))
      .Select(v => v.ToString()).ToArray();

    #endregion

    #region Commands

    /// <summary>
    /// Built In Ows commands
    /// </summary>
    public static IReadOnlyDictionary<string, Command.Type> DefaultCommands {
      get;
    } = GetDefaultCommands().ToDictionary(
      e => e.Id.Name,
      e => e
    );

    static HashSet<Command.Type> GetDefaultCommands() {
      HashSet<Command.Type> commandHashSet = new() {
        Command.Types.Get<Command.ALWAYS>(),
        Command.Types.Get<Command.COUNT_UP>(),
        Command.Types.Get<Command.COUNT_UP_WITH>(),
        Command.Types.Get<Command.COUNTDOWN>(),
        Command.Types.Get<Command.COUNTDOWN_WITH>(),
        Command.Types.Get<Command.DO>(),
        Command.Types.Get<Command.ELSE>(),
        Command.Types.Get<Command.END>(),
        Command.Types.Get<Command.FOR>(),
        Command.Types.Get<Command.GO_BACK>(),
        Command.Types.Get<Command.GO_TO>(),
        Command.Types.Get<Command.GOTO>(),
        Command.Types.Get<Command.IF>(),
        Command.Types.Get<Command.IF_NOT>(),
        Command.Types.Get<Command.RETURN>(),
        Command.Types.Get<Command.SET>(),
        Command.Types.Get<Command.SET_FOR>(),
        Command.Types.Get<Command.SET_FOR_PROGRAM>(),
        Command.Types.Get<Command.SET_FOR_WORLD>(),
        Command.Types.Get<Command.SET_HERE>(),
        Command.Types.Get<Command.SET_LOCALLY>(),
        Command.Types.Get<Command.SET_LOCALLY_FOR>(),
        Command.Types.Get<Command.UN_SET>(),
        Command.Types.Get<Command.UN_SET_FOR>(),
        Command.Types.Get<Command.UN_SET_LOCALLY>(),
        Command.Types.Get<Command.UN_SET_LOCALLY_FOR>(),
        Command.Types.Get<Command.UNTIL>(),
        Command.Types.Get<Command.WHILE>()
      };

      return commandHashSet;
    }

    #endregion

    #region Keywords and Symbols

    /// <summary>
    /// Reserved label for specifying the start of a program
    /// </summary>
    public const string StartLabel = "START";

    /// <summary>
    /// Phrase used for Else syntax with if
    /// </summary>
    public const string ElsePhrase = "ELSE";

    /// <summary>
    /// Used to indicate "All" in some cases
    /// </summary>
    public const char CollectAllSymbol = '*';

    /// <summary>
    /// Used to indicate "All" in some cases
    /// </summary>
    public const string CollectAllPhrase = "ALL";

    /// <summary>
    /// Used to indicate the whole program
    /// </summary>
    public const string ProgramPhrase = "PROGRAM";

    /// <summary>
    /// Used to indicate the whole world
    /// </summary>
    public const char WorldSymbol = '@';

    /// <summary>
    /// Used to indicate the whole program
    /// </summary>
    public const char ProgramSymbol = '^';

    /// <summary>
    /// Used to indicate the whole world
    /// </summary>
    public const string WorldPhrase = "WORLD";

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
    /// Phrase used to set items to variables
    /// </summary>
    public const string SetToPhrase = " TO ";

    /// <summary>
    /// Phrase used to set items to variables
    /// </summary>
    public const string SetsAsPhrase = " AS ";

    /// <summary>
    /// Phrase used to set items to variables
    /// </summary>
    public const string SetIsPhrase = " IS ";

    /// <summary>
    /// Symbol that seperates a function name and it's parameters
    /// </summary>
    public const char FunctionSeperatorSymbol = ':';

    /// <summary>
    /// The initial Symbol indicating the beginning of a collection
    /// </summary>
    public const char CollectionStartSymbol = '[';

    /// <summary>
    /// Symbol that indicates the end of a collection
    /// </summary>
    public const char CollectionEndSymbol = ']';

    /// <summary>
    /// The initial Symbol indicating a label is beginning this line
    /// </summary>
    public const char LabelStartSymbol = '[';

    /// <summary>
    /// Symbol that indicates the end of a label
    /// </summary>
    public const char LabelEndSymbol = ']';

    /// <summary>
    /// Symbol that indicates the start of a opperator or condition section
    /// </summary>
    public const char LogicStartSymbol = '(';

    /// <summary>
    /// Symbol that indicates the end of a opperator or condition section
    /// </summary>
    public const char LogicEndSymbol = ')';

    /// <summary>
    /// Used for decimals in numbers
    /// </summary>
    private const char DecimalSymbol = '.';

    /// <summary>
    /// Used for sub variables in objects
    /// </summary>
    private const char SubVariableSymbol = '.';

    /// <summary>
    /// Symbol that is used to represent a string
    /// </summary>
    public const char StringQuotesSymbol = '"';

    /// <summary>
    /// Phrase used to set items to variables
    /// </summary>
    public const char SetToSymbol = '=';

    /// <summary>
    /// Forces a line to end. Try not to abuse this it can make debugging harder
    /// </summary>
    public const char LineEndAlternateSymbol = ';';

    /// <summary>
    /// Symbol that indicates the end of a collection
    /// </summary>
    public static char[] CollectionItemSeperatorSymbols
      = new[] {
        ',',
        (char)Comparitors.AND
      };

    /// <summary>
    /// Keywords you can't use for other reasons.
    /// </summary>
    public static HashSet<string> ReservedKeywords = new List<string> {
      StartLabel,
      CollectAllPhrase,
      ConcatPhrase,
      SetToPhrase,
      SetsAsPhrase,
      SetIsPhrase,
      ProgramPhrase,
      WorldPhrase,
      NotComparitorPrefixPhrase
    }.Concat(NumberOpperatorPhrases)
    .Concat(ComparitorPhrases)
    .Select(keyword => {
      return keyword.ToUpper().Trim();
    })
    .ToHashSet();

    /// <summary>
    /// Keywords you can't use for other reasons.
    /// </summary>
    public static HashSet<char> ReservedSymbols = new List<char> {
      CollectAllSymbol,
      SetToSymbol,
      ProgramSymbol,
      WorldSymbol,
      LogicEndSymbol,
      LogicStartSymbol,
      FunctionSeperatorSymbol,
      CollectionStartSymbol,
      CollectionEndSymbol,
      LabelEndSymbol,
      LabelStartSymbol,
      LineEndAlternateSymbol,
      DecimalSymbol,
      SubVariableSymbol
    }.Concat(NumberOpperatorSymbols)
    .Concat(ComparitorSymbols)
    .ToHashSet();

    #endregion

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
  }
}
