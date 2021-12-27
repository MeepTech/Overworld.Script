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
    public static string Until(this IEnumerable<char> @string, Func<char, bool> untilFalse) {
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
