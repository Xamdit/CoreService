using System.Linq.Expressions;
using Global.Entities;
using Global.Entities.Helpers;
using Microsoft.AspNetCore.Components;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class EstimatesHelper
{
  /**
   * Format estimate number based on description
   * @param  mixed $id
   * @return string
   */
  public static string format_estimate_number(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var estimate = db.Estimates.FirstOrDefault(x => x.Id == id);
    if (estimate == null) return "";
    var number = helper.sales_number_format(estimate.Number, estimate.NumberFormat, estimate.Prefix, estimate.Date);
    self.hooks.apply_filters("format_estimate_number", new { id, number, estimate });
    return number;
  }


  /**
   * Format estimate status
   * @param  int status
   * @param  string classes additional CSS classes
   * @param  bool label Include in HTML label or not
   * @return object
   */
  public static object format_estimate_status(this HelperBase helper, int status, string classes = "", bool label = true)
  {
    var id = status;
    var labelClass = helper.estimate_status_color_class(status);
    var statusLabel = helper.estimate_status_by_id(status);

    if (label) return new MarkupString($"<span class=\"label label-{labelClass} {classes} s-status estimate-status-{id} estimate-status-{labelClass}\">{statusLabel}</span>");

    return statusLabel;
  }

  /**
   * Return estimate status translated by passed status id
   * @param  int id estimate status id
   * @return string
   */
  public static string estimate_status_by_id(this HelperBase helper, object id)
  {
    var status = string.Empty;

    switch (id)
    {
      case 1:
        status = helper._l("estimate_status_draft");
        break;
      case 2:
        status = helper._l("estimate_status_sent");
        break;
      case 3:
        status = helper._l("estimate_status_declined");
        break;
      case 4:
        status = helper._l("estimate_status_accepted");
        break;
      case 5:
        status = helper._l("estimate_status_expired");
        break;
      default:
        if (!helper.is_numeric(id) && id == "not_sent") status = helper._l("not_sent_indicator");
        break;
    }

    return helper.hooks().ApplyFilters("estimate_status_label", status, id);
  }

  /**
   * Return estimate status color class based on Twitter Bootstrap
   * @param  int id
   * @param  bool replaceDefaultByMuted
   * @return string
   */
  public static string estimate_status_color_class(this HelperBase helper, object id, bool replaceDefaultByMuted = false)
  {
    var @class = string.Empty;

    switch (id)
    {
      case 1:
        @class = replaceDefaultByMuted ? "muted" : "default";
        break;
      case 2:
        @class = "info";
        break;
      case 3:
        @class = "danger";
        break;
      case 4:
        @class = "success";
        break;
      case 5:
        @class = "warning";
        break;
      default:
        if (!helper.is_numeric(id) && id == "not_sent") @class = replaceDefaultByMuted ? "muted" : "default";
        break;
    }

    return helper.hooks().ApplyFilters("estimate_status_color_class", @class, id);
  }

  // Simulated helper methods (you'll need to implement these)
  private static string _l(this HelperBase helper, string key)
  {
    // Localization method implementation
    return key; // Replace with actual translation logic
  }

  private static bool is_numeric(this HelperBase helper, object value)
  {
    // Check if the value is numeric
    return int.TryParse(value.ToString(), out _);
  }

  private static dynamic hooks(this HelperBase helper)
  {
    // Simulate hooks method, needs actual implementation
    return new
    {
      ApplyFilters = new Func<string, string, int, string>((filterName, input, id) => input)
    };
  }

  public static Expression<Func<Estimate, bool>> get_estimates_where_sql_for_staff(this HelperBase helper, int staff_id)
  {
    var (self, db) = getInstance();
    var has_permission_view_own = helper.has_permission("estimates", "", "view_own");
    var allow_staff_view_estimates_assigned = db.get_option<bool>("allow_staff_view_estimates_assigned");
    // Build the expression
    Expression<Func<Estimate, bool>> whereUser;

    if (has_permission_view_own)
    {
      whereUser = e => e.AddedFrom == staff_id &&
                       db.StaffPermissions.Any(sp => sp.StaffId == staff_id && sp.Feature == "estimates" && sp.Capability == "view_own");

      if (allow_staff_view_estimates_assigned)
        whereUser = whereUser.Or(e => e.SaleAgent == staff_id);
    }
    else
    {
      whereUser = e => e.SaleAgent == staff_id;
    }

    return whereUser;
  }

  /**
 * Function that return estimate item taxes based on passed item id
 * @param  mixed $itemid
 * @return array
 */
  public static List<ItemTax> get_estimate_item_taxes(this HelperBase helper, int itemid)
  {
    var (self, db) = getInstance();
    var taxes = db.ItemTaxes
      .Where(x => x.ItemId == itemid && x.RelType == "estimate")
      .ToList()
      .Select(x =>
      {
        x.TaxName = x.TaxName + "|" + x.TaxRate;
        return x;
      })
      .ToList();
    return taxes;
  }

  /**
 * Check if the estimate id is last invoice
 * @param  mixed  $id estimateid
 * @return boolean
 */
  public static bool is_last_estimate(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var row = db.Estimates.OrderByDescending(x => x.Id).First();
    var last_estimate_id = row.Id;
    return last_estimate_id == id;
  }
}
