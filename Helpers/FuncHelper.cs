using System.Dynamic;
using System.Text.RegularExpressions;
using Service.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class FuncHelper
{
  public static bool in_array_multidimensional<T>(this MyContext db, List<T> array, string value, params string[] additionalValues)
  {
    // var allValues = new List<T> { value };
    // allValues.AddRange(additionalValues);

    // return array.Any(subArray => subArray.Any(item =>
    // {
    //   // Handle string comparison for case-insensitive matching
    //   if (item is string itemString && value is string valueString) return string.Equals(itemString, valueString, StringComparison.OrdinalIgnoreCase);
    //   // For non-string types, use default equality
    //   return EqualityComparer<T>.Default.Equals(item, value);
    // }));
    return false;
  }


  // Method to merge two dynamic objects (ExpandoObject)
  public static dynamic merge_options(this HelperBase helper, dynamic defaults, dynamic? options = null)
  {
    var result = new ExpandoObject();
    var dictResult = (IDictionary<string, object>)result;
    var dictDefaults = (IDictionary<string, object>)defaults;
    var dictOptions = options != null ? (IDictionary<string, object>)options : null;
    // Copy defaults
    foreach (var kvp in dictDefaults) dictResult[kvp.Key] = kvp.Value;
    // Override defaults with options
    if (dictOptions == null) return result;
    foreach (var kvp in dictOptions)
      dictResult[kvp.Key] = kvp.Value;
    return result;
  }

  public static string slug_it(this MyContext db, string str, dynamic options = null)
  {
    // Set the default options
    dynamic defaults = new ExpandoObject();
    defaults.lang = db.get_option("active_language");
    // Merge defaults and options (manual merge for ExpandoObject)
    // var settings = helper.merge_options(defaults, options);
    // Convert the string to a slug format
    var slug = Regex.Replace(str.ToLowerInvariant(), @"[^a-z0-9\s-]", ""); // Remove non-alphanumeric characters
    slug = Regex.Replace(slug, @"\s+", "-").Trim('-'); // Replace spaces with hyphens and trim hyphens

    return slug;
  }

  public static string adjust_hex_brightness(this HelperBase helper, string hex, double percent)
  {
    // Work out if hash is given
    var hash = "#";
    if (hex.Contains("#")) hex = hex.Replace("#", "");

    // HEX TO RGB
    var rgb = new int[3];
    rgb[0] = Convert.ToInt32(hex[..2], 16);
    rgb[1] = Convert.ToInt32(hex.Substring(2, 2), 16);
    rgb[2] = Convert.ToInt32(hex.Substring(4, 2), 16);

    // CALCULATE
    for (var i = 0; i < 3; i++)
    {
      if (percent > 0)
      {
        // Lighter
        rgb[i] = (int)Math.Round(rgb[i] * percent) + (int)Math.Round(255 * (1 - percent));
      }
      else
      {
        // Darker
        var positivePercent = percent - percent * 2;
        rgb[i] = (int)Math.Round(rgb[i] * (1 - positivePercent));
      }

      // Ensure the value is within valid RGB range (0-255)
      rgb[i] = Math.Min(255, rgb[i]);
    }

    // RGB to HEX
    var hexOutput = "";
    for (var i = 0; i < 3; i++)
    {
      var hexDigit = rgb[i].ToString("X2");
      hexOutput += hexDigit;
    }

    return hash + hexOutput;
  }
}
