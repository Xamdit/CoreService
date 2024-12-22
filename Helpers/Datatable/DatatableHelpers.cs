namespace Service.Helpers.Datatable;

public static class DatatableHelpers
{
  public static string render_datatable(List<object> headings = null, string className = "",
    List<string>? additionalClasses = null, dynamic tableAttributes = null)
  {
    var additionalClasses1 = string.Empty;
    var tableAttributes1 = string.Empty;
    if (additionalClasses != null && additionalClasses.Any())
      additionalClasses1 = " " + string.Join(" ", additionalClasses);
    var efix = string.Empty;
    if (tableAttributes != null)
    {
      var tableAttributesDict = (IDictionary<string, object>)tableAttributes;
      tableAttributes1 = tableAttributesDict.Keys.Aggregate(
        tableAttributes1,
        (current, key) => current + $"{key}='{tableAttributesDict[key]}' ");
    }

    var table =
      $"<div class='{efix}'><table {tableAttributes1}class='dt-table-loading table table-{className}{additionalClasses1}'>";
    table += "<thead>";
    table += "<tr>";
    foreach (var heading in headings)
      switch (heading)
      {
        case string headingText:
          table += $"<th>{headingText}</th>";
          break;
        case Dictionary<string, string> headingInfo:
        {
          var thAttrs = string.Empty;
          if (headingInfo.ContainsKey("th_attrs"))
          {
            var thAttrsDict = headingInfo["th_attrs"]
              .Split(',')
              .Select(pair => pair.Split('='))
              .ToDictionary(parts => parts[0], parts => parts[1]);
            thAttrs = thAttrsDict.Keys.Aggregate(thAttrs, (current, key) => current + $"{key}='{thAttrsDict[key]}' ");
          }

          table += $"<th{thAttrs}>{headingInfo["name"]}</th>";
          break;
        }
      }

    table += "</tr>";
    table += "</thead>";
    table += "<tbody></tbody>";
    table += "</table></div>";
    return table;
  }
}
