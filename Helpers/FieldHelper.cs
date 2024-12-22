using System.Text;

namespace Service.Helpers;

public static class FieldHelper
{
  public static string render_select(
    string name,
    List<Dictionary<string, object>> options,
    List<string>? optionAttrs = null,
    string label = "",
    string selected = "",
    Dictionary<string, string>? selectAttrs = null,
    Dictionary<string, string>? formGroupAttr = null,
    string formGroupClass = "",
    string selectClass = "",
    bool includeBlank = true
  )
  {
    // Default values and initializations
    optionAttrs ??= new List<string>();
    selectAttrs ??= new Dictionary<string, string>();
    formGroupAttr ??= new Dictionary<string, string>();

    // Add default select attributes
    if (!selectAttrs.ContainsKey("data-width")) selectAttrs["data-width"] = "100%";

    if (!selectAttrs.ContainsKey("data-none-selected-text")) selectAttrs["data-none-selected-text"] = "Select an option"; // Placeholder text

    // Build the select attributes string
    var selectAttrString = string.Join(" ", selectAttrs.Select(attr => $"{attr.Key}=\"{attr.Value}\""));

    // Add form group wrapper attributes
    formGroupAttr["app-field-wrapper"] = name;
    var formGroupAttrString = string.Join(" ", formGroupAttr.Select(attr => $"{attr.Key}=\"{attr.Value}\""));

    // Build form group class
    formGroupClass = !string.IsNullOrEmpty(formGroupClass) ? $" {formGroupClass}" : "";

    // Build select class
    selectClass = !string.IsNullOrEmpty(selectClass) ? $" {selectClass}" : "";

    // Begin rendering the HTML output
    var selectHtml = new StringBuilder();
    selectHtml.Append($"<div class=\"select-placeholder form-group{formGroupClass}\" {formGroupAttrString}>");

    if (!string.IsNullOrEmpty(label)) selectHtml.Append($"<label for=\"{name}\" class=\"control-label\">{label}</label>");

    selectHtml.Append($"<select id=\"{name}\" name=\"{name}\" class=\"selectpicker{selectClass}\" {selectAttrString} data-live-search=\"true\">");

    if (includeBlank) selectHtml.Append("<option value=\"\"></option>");

    foreach (var option in options)
    {
      var key = option.ContainsKey(optionAttrs[0]) ? option[optionAttrs[0]].ToString() : "";
      var val = optionAttrs.Skip(1).Aggregate("", (current, attr) => current + (option.ContainsKey(attr) ? option[attr] + " " : "")).Trim();

      var selectedAttr = selected == key ? " selected" : "";

      var dataSubText = optionAttrs.Count > 2 && option.ContainsKey(optionAttrs[2])
        ? $" data-subtext=\"{option[optionAttrs[2]]}\""
        : "";

      // Check if there are any additional option attributes
      var dataContent = "";
      if (option.ContainsKey("option_attributes") && option["option_attributes"] is Dictionary<string, string> optionAttributes) dataContent = string.Join(" ", optionAttributes.Select(attr => $"{attr.Key}=\"{attr.Value}\""));

      // Build the option HTML
      selectHtml.Append($"<option value=\"{key}\"{selectedAttr}{dataContent}{dataSubText}>{val}</option>");
    }

    selectHtml.Append("</select>");
    selectHtml.Append("</div>");

    return selectHtml.ToString();
  }

  /// <summary>
  /// Renders input for admin area based on passed arguments.
  /// </summary>
  /// <param name="name">Input name.</param>
  /// <param name="label">Label name.</param>
  /// <param name="value">Default value.</param>
  /// <param name="type">Input type, e.g., text, number.</param>
  /// <param name="inputAttrs">Attributes on the input tag.</param>
  /// <param name="formGroupAttrs">HTML attributes for the form group div.</param>
  /// <param name="formGroupClass">Additional form group class.</param>
  /// <param name="inputClass">Additional class on the input tag.</param>
  /// <returns>Returns an HTML string for the input.</returns>
  public static string render_input(
    string name,
    string label = "",
    string value = "",
    string type = "text",
    Dictionary<string, string> inputAttrs = null,
    Dictionary<string, string> formGroupAttrs = null,
    string formGroupClass = "",
    string inputClass = "")
  {
    inputAttrs ??= new Dictionary<string, string>();
    formGroupAttrs ??= new Dictionary<string, string>();

    var input = new StringBuilder();
    var formGroupAttrString = new StringBuilder();
    var inputAttrString = new StringBuilder();

    // Process input attributes
    foreach (var attr in inputAttrs)
    {
      var attrValue = attr.Key == "title" ? Localize(attr.Value) : attr.Value;
      inputAttrString.Append($"{attr.Key}=\"{attrValue}\" ");
    }

    inputAttrString = inputAttrString.Length > 0 ? inputAttrString.Remove(inputAttrString.Length - 1, 1) : inputAttrString;

    // Add form-group attributes
    formGroupAttrs["app-field-wrapper"] = name;
    foreach (var attr in formGroupAttrs)
    {
      var attrValue = attr.Key == "title" ? Localize(attr.Value) : attr.Value;
      formGroupAttrString.Append($"{attr.Key}=\"{attrValue}\" ");
    }

    formGroupAttrString = formGroupAttrString.Length > 0 ? formGroupAttrString.Remove(formGroupAttrString.Length - 1, 1) : formGroupAttrString;

    // Apply additional form group and input class
    if (!string.IsNullOrEmpty(formGroupClass)) formGroupClass = " " + formGroupClass;

    if (!string.IsNullOrEmpty(inputClass)) inputClass = " " + inputClass;

    // Build the HTML string
    input.Append($"<div class=\"form-group{formGroupClass}\" {formGroupAttrString}>");

    if (!string.IsNullOrEmpty(label)) input.Append($"<label for=\"{name}\" class=\"control-label\">{Localize(label)}</label>");

    input.Append($"<input type=\"{type}\" id=\"{name}\" name=\"{name}\" class=\"form-control{inputClass}\" {inputAttrString} value=\"{SetValue(name, value)}\">");
    input.Append("</div>");

    return input.ToString();
  }

  /// <summary>
  /// Placeholder for a localization method (mimicking _l() in the original PHP code).
  /// </summary>
  private static string Localize(string value)
  {
    // Localization logic goes here. For now, it's a simple passthrough.
    return value;
  }

  /// <summary>
  /// Placeholder for set_value function used in the original PHP code.
  /// </summary>
  private static string SetValue(string name, string defaultValue)
  {
    // Logic to retrieve form value or return default if not available.
    return defaultValue;
  }
}
