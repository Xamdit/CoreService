using System.Text.RegularExpressions;
using Service.Framework.Core.Engine;

namespace Service.Helpers.Template;

public static class EmojiHelper
{
  public static string remove_emojis(this HelperBase helper, string input)
  {
    if (string.IsNullOrEmpty(input)) return input;

    // Regular expression to match most emojis based on Unicode ranges
    var emojiPattern = @"[\u231A-\u231B" + // Miscellaneous technical
                       @"\u23E9-\u23EF" + // Control icons
                       @"\u23F0-\u23F3" + // Alarm clocks
                       @"\u25FD-\u25FE" + // Geometric shapes
                       @"\u2614-\u2615" + // Weather icons
                       @"\u2648-\u2653" + // Zodiac signs
                       @"\u267F" + // Accessibility symbol
                       @"\u2693" + // Anchor
                       @"\u26A1" + // Lightning
                       @"\u26AA-\u26AB" + // Geometric shapes
                       @"\u26BD-\u26BE" + // Sports balls
                       @"\u26C4-\u26C5" + // Weather icons
                       @"\u26D1" + // Helmet
                       @"\u26F2-\u26F3" + // Park symbols
                       @"\u26F5" + // Boat
                       @"\u26FA" + // Tent
                       @"\u26FD" + // Gas pump
                       @"\u2702" + // Scissors
                       @"\u2705" + // Check mark
                       @"\u2708-\u2709" + // Airplane and envelope
                       @"\u274C-\u274E" + // Cross marks
                       @"\u2753-\u2755" + // Question and exclamation marks
                       @"\u2764" + // Heart
                       @"\u2795-\u2797" + // Plus, minus, and divide symbols
                       @"\u27A1" + // Arrow
                       @"\u2B05-\u2B07" + // Arrows
                       @"\u2B50" + // Star
                       @"\u2B55" + // Circle
                       @"\u3030" + // Wavy dash
                       @"\u303D" + // Part alternation mark
                       @"\u3297-\u3299" + // Circled ideographs
                       @"\u1F004" + // Mahjong tile
                       @"\u1F0CF" + // Joker card
                       @"\u1F170-\u1F171" + // Alphanumeric enclosed
                       @"\u1F17E-\u1F17F" + // Alphanumeric enclosed
                       @"\u1F18E" + // Alphanumeric enclosed
                       @"\u1F191-\u1F19A" + // Alphanumeric enclosed
                       @"\u1F201-\u1F202" + // Japanese characters
                       @"\u1F21A" + // Japanese character
                       @"\u1F22F" + // Japanese character
                       @"\u1F232-\u1F236" + // Japanese characters
                       @"\u1F238-\u1F23A" + // Japanese characters
                       @"\u1F250-\u1F251" + // Japanese characters
                       @"\u1F300-\u1F321" + // Weather, time, nature, etc.
                       @"\u1F324-\u1F393" + // Additional emojis
                       @"\u1F396-\u1F397" + // Medals
                       @"\u1F399-\u1F39B" + // Studio icons
                       @"\u1F39E-\u1F3F0" + // Additional emojis
                       @"\u1F3F3-\u1F3F5" + // Flags
                       @"\u1F3F7-\u1F4FD" + // More emojis
                       @"\u1F4FF-\u1F53D" + // More emojis
                       @"\u1F549-\u1F54E" + // More emojis
                       @"\u1F550-\u1F567" + // Clock faces
                       @"\u1F57A" + // Man dancing
                       @"\u1F595-\u1F596" + // Hand gestures
                       @"\u1F5A4" + // Black heart
                       @"\u1F5FB-\u1F64F" + // More emojis
                       @"\u1F680-\u1F6C5" + // Transport and map symbols
                       @"\u1F6CB-\u1F6D2" + // More emojis
                       @"\u1F6E0-\u1F6EC" + // More emojis
                       @"\u1F6F0-\u1F6F3" + // More emojis
                       @"\u1F910-\u1F93E" + // Smileys and gestures
                       @"\u1F940-\u1F945" + // Flowers
                       @"\u1F947-\u1F9FF" + // More emojis
                       @"\u1FA70-\u1FA73" + // Ballet shoes, etc.
                       @"\u1FA78-\u1FA7A" + // Mask and other symbols
                       @"\u1FA80-\u1FA82" + // Kite, etc.
                       @"\u1FA90-\u1FA95" + // Paint palette, etc.
                       "]";

    return Regex.Replace(input, emojiPattern, "");
  }
}
