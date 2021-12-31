using System;
using System.Collections.Generic;
using System.Linq;

namespace Overworld.Script {
  public static class StringExtensions {

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string Until(this IEnumerable<char> @string, char end) {
      var @return = "";
      foreach(char character in @string) {
        if(character == end) {
          break;
        }
        @return += character;
      }

      return @return;
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static bool StartsWith(this string @string, IEnumerable<string> anyPrefix, out string foundPrefix) {
      foreach(string prefix in anyPrefix) {
        if(@string.StartsWith(prefix)) {
          foundPrefix = prefix;
          return true;
        }
      }

      foundPrefix = null;
      return false;
    }

    /// <summary>
    /// Converts all lines to upercase, except in string literals on the same line
    /// </summary>
    public static IEnumerable<string> ToUpperExeptStringLiterals(this IEnumerable<string> @string) {
      return @string.Select(@string => {
        string[] chunks = @string.Split(Ows.StringQuotesSymbol);
        bool isString = true;

        chunks = chunks.Select(chunk => {
          isString = !isString;
          if(isString) {
            return chunk;
          } else {
            return chunk.ToUpper();
          }
        }).ToArray();

        return string.Join(Ows.StringQuotesSymbol, chunks);
      });
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string UntilAny(this IEnumerable<char> @string, params char[] end) {
      var @return = "";
      foreach(char character in @string) {
        if(end.Contains(character)) {
          break;
        }
        @return += character;
      }

      return @return;
    }

    /// <summary>
    /// Get a full closure from begining to end, with subcolsurs included
    /// </summary>
    public static string UntilClosure(this string @string, char start, char end) {
      var @return = "";
      int depth = 1;
      int index = 0;
      @string = @string.After(start);

      while(depth > 0 && index < @string.Count()) {
        if(@string[index] == end) {
          depth--; 
        } else if(@string[index] == start) {
          depth++;
        }

        @return += @string[index];
        index++;
      }

      return @return[0..^1];
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string UntilAny(this IEnumerable<char> @string, IEnumerable<string> enders, out string foundEnding) {
      var @return = "";
      IEnumerable<char> endInitialCharactes = enders.Select(e => e[0]);
      foreach(char character in @string) {
        if(endInitialCharactes.Contains(character)) {
          string remainder = ((string)@string).Substring(@return.Length);
          if((foundEnding = enders.FirstOrDefault(e => remainder.StartsWith(e))) != null) {
            return @return;
          }
        }
        @return += character;
      }

      foundEnding = null;
      return @return;
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string UntilNot(this IEnumerable<char> @string, Func<char, bool> untilFalse) {
      var @return = "";
      foreach(char character in @string) {
        if(!untilFalse(character)) {
          break;
        }
        @return += character;
      }

      return @return;
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string Until(this IEnumerable<char> @string, char end, out string remainder) {
      var @return = "";
      foreach(char character in @string) {
        if(character == end) {
          break;
        }
        @return += character;
      }

      remainder = (string)@string.Skip(@return.Length + 1);

      return @return;
    }

    /// <summary>
    /// Get a string until a characher appears
    /// </summary>
    public static string After(this IEnumerable<char> @string, char start) {
      string @return = string.Empty;
      bool beginRecording = false;
      foreach(char currentCharacter in @string) {
        if(!beginRecording) {
          if(currentCharacter == start) {
            beginRecording = true;
          }
        } else
          @return += currentCharacter;
      }

      return @return;
    }
  }
}
