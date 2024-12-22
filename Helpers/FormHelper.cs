using System.Text;
using System.Web;
using Microsoft.AspNetCore.Components;

namespace Service.Helpers;

public static class FormHelper
{
  private static StringBuilder form = new();

  public static MarkupString form_hidden(object name, object? value = null, bool recursing = false)
  {
    if (!recursing) form.Append("\n");
    if (name is IDictionary<string, object> dictionary)
    {
      foreach (var entry in dictionary) form.Append(form_hidden(entry.Key, entry.Value?.ToString(), true));
      return new MarkupString(form.ToString());
    }

    if (name is string fieldName && value is not IEnumerable<object>)
      form.AppendFormat($"<input type='hidden' name='{HttpUtility.HtmlEncode(fieldName)}' value='{HttpUtility.HtmlEncode(value)}' />\n");
    else if (value is IEnumerable<object> values)
      foreach (var v in values)
        form.Append(form_hidden($"{name}[]", v.ToString(), true));
    return new MarkupString(form.ToString());
  }
}
