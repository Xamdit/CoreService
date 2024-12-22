namespace Service.Core;

public class Common
{
  public static string _stringify_attributes(object attributes, bool js = false)
  {
    if (attributes == null) return string.Empty;
    if (attributes is string strAttributes) return " " + strAttributes;
    var attributeDict = attributes.GetType()
      .GetProperties()
      .ToDictionary(prop => prop.Name, prop => prop.GetValue(attributes));
    return string.Join(js ? "," : " ",
        attributeDict.Select(attr => js ? $"{attr.Key}={attr.Value}" : $"{attr.Key}=\"{attr.Value}\"")
      )
      .Trim();
  }

  public static string bcsub(string left, string right, int scale = 0)
  {
    var leftDecimal = DecimalParse(left, scale);
    var rightDecimal = DecimalParse(right, scale);
    var result = leftDecimal - rightDecimal;
    return result.ToString($"F{scale}");
  }

  public static string bcadd(string left, string right, int scale = 0)
  {
    var leftDecimal = DecimalParse(left, scale);
    var rightDecimal = DecimalParse(right, scale);
    var result = leftDecimal + rightDecimal;
    return result.ToString($"F{scale}");
  }

  private static decimal DecimalParse(string input, int scale)
  {
    if (!decimal.TryParse(input, out var result)) throw new ArgumentException("Invalid input format for a decimal number.");
    return Math.Round(result, scale);
  }
}
