using System.Linq.Expressions;
using Global.Entities;
using Microsoft.EntityFrameworkCore;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Extensions;

namespace Service.Helpers;

public static class CustomFieldHelper
{
  /**
   * Process field value based on the field type (e.g., date_picker, textarea, etc.)
   */
  private static string process_field_value(this HelperBase helper, string fieldType, string fieldValue)
  {
    switch (fieldType)
    {
      case "date_picker":
        fieldValue = convert_(fieldValue);
        break;
      case "date_picker_time":
        fieldValue = convert__time(fieldValue);
        break;
      case "textarea":
        fieldValue = fieldValue.Replace("\n", "<br>");
        break;
      case "checkbox":
      case "multiselect":
        if (fieldValue.Contains("cfk_hidden")) fieldValue = fieldValue.Replace("cfk_hidden", "").Trim();
        fieldValue = string.Join(", ", fieldValue.Split(',').Select(f => f.Trim()));
        break;
    }

    return fieldValue;
  }

  /**
   * Convert a string to SQL date format
   */
  private static string convert_(string dateStr)
  {
    // Implement your SQL date conversion logic here
    DateTime date;
    return DateTime.TryParse(dateStr, out date) ? date.ToString("yyyy-MM-dd") : dateStr;
  }

  /**
   * Convert a string to SQL datetime format
   */
  private static string convert__time(string dateTimeStr)
  {
    // Implement your SQL datetime conversion logic here
    DateTime dateTime;
    return DateTime.TryParse(dateTimeStr, out dateTime) ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : dateTimeStr;
  }

  /**
 * Check for custom fields, update on POST
 * @param  int relId        The main ID from the table
 * @param  Dictionary<string, Dictionary<int, string>> customFields  All custom fields with id and values
 * @return bool             True if any rows were affected
 */
  public static bool handle_custom_fields_post(this HelperBase helper, int relId, CustomField customField, bool isCfItems = false)
  {
    return helper.handle_custom_fields_post(relId, new List<CustomField> { customField }, isCfItems);
  }

  public static bool handle_custom_fields_post(this HelperBase helper, int relId, List<CustomField> customFields, bool isCfItems = false)
  {
    var (self, db) = getInstance();
    var affectedRows = 0;

    var isClientLoggedIn = false;
    self.ignore(() =>
    {
      // Fetch the existing record for the custom field
      isClientLoggedIn = is_client_logged_in();
      //
    });
    Thread.Sleep(500);


    foreach (var field in customFields)
    {
      // Fetch the existing record for the custom field
      var row = db.CustomFieldsValues
        .FirstOrDefault(x =>
          x.RelId == relId &&
          x.FieldId == field.Id &&
          // x.FieldTo == (isCfItems ? "items_pr" : key));
          x.FieldTo == (isCfItems ? "items_pr" : field.FieldTo));

      // Fetch the field metadata to apply custom rules based on the field type
      var fieldChecker = db.CustomFields.FirstOrDefault(x => x.Id == field.Id);
      if (fieldChecker != null)
      {
        // Handle different field types
        var fieldValue = string.Empty;
        // Handle different field types using LINQ
        fieldValue = fieldChecker.Type switch
        {
          "date_picker" or "date_picker_time" => today(),
          "textarea" => field.DefaultValue.nl2br(), // Equivalent of nl2br
          "checkbox" or "multiselect" when fieldChecker.DisalowClientToEdit == 1 && isClientLoggedIn => null, // Skip this iteration
          "checkbox" or "multiselect" when fieldValue.Contains(",") => string.Join(", ",
            fieldValue.Split(',').Where(v => v.Trim() != "cfk_hidden")),
          _ => fieldValue
        };

        // If the field value was null (skipped), continue the loop
        if (fieldValue == null) continue;
      }

      // Update or insert the custom field value
      if (row != null)
      {
        row.Value = field.DefaultValue;
        db.CustomFieldsValues.Update(row);
        affectedRows += db.SaveChanges();
      }
      else if (!string.IsNullOrEmpty(field.DefaultValue))
      {
        db.CustomFieldsValues.Add(new CustomFieldsValue
        {
          RelId = relId,
          FieldId = field.Id,
          FieldTo = isCfItems ? "items_pr" : field.FieldTo,
          Value = field.DefaultValue
        });
        affectedRows += db.SaveChanges();
      }
    }

    return affectedRows > 0;
  }

  /**
 * Get custom fields
 * @param  string  $field_to
 * @param  array   $where
 * @param  boolean $exclude_only_admin
 * @return array
 */
  /**
 * Get custom fields
 * @param  string  $field_to
 * @param  array   $where
 * @param  boolean $exclude_only_admin
 * @return array
 */
  public static List<CustomField> get_custom_fields(this HelperBase helper, string fieldTo, Expression<Func<CustomField, bool>> condition = null, bool excludeOnlyAdmin = false)
  {
    var (self, db) = getInstance();
    var _isAdmin = helper.is_admin();
    var query = db
      .CustomFields
      .Where(cf => cf.FieldTo == fieldTo && cf.Active == true)
      .Where(condition);
    // Applying additional conditions if provided
    // If the user is not admin or we need to exclude admin-only fields
    if (!_isAdmin || excludeOnlyAdmin) query = query.Where(cf => cf.OnlyAdmin == false);

    // Ordering by field_order
    var results = query
      .OrderBy(cf => cf.FieldOrder)
      .ToList().Select(x =>
      {
        x.Name = helper.maybe_translate_custom_field_name(x.Name, x.Slug);
        return x;
      })
      .ToList();
    return results;
  }

  /**
   * Get custom field value
   * @param  mixed $rel_id              the main ID from the table, e.q. the customer id, invoice id
   * @param  mixed $field_id_or_slug    field id, the custom field ID or custom field slug
   * @param  string $field_to           belongs to e.q leads, customers, staff
   * @param  string $format             format date values
   * @return string
   */
  public static string get_custom_field_value(this HelperBase helper, int relId, int FieldIdOrSlug, string fieldTo, bool format = true)
  {
    return helper.get_custom_field_value(relId, $"{FieldIdOrSlug}", fieldTo, format);
  }

  public static string get_custom_field_value(this HelperBase helper, int relId, string FieldIdOrSlug, string fieldTo, bool format = true)
  {
    var (self, db) = getInstance();
    db.CustomFieldsValues
      .Include(x => x.Field)
      .Where(x => x.RelId == relId && x.FieldTo == fieldTo)
      .Select(x => new { x.FieldId, x.Value })
      .ToList();

    var fieldId = 0;
    var query = db.CustomFieldsValues
      .Include(x => x.Field)
      .Where(x =>
        x.RelId == relId &&
        x.FieldTo == fieldTo &&
        (
          int.TryParse(FieldIdOrSlug, out fieldId)
            ? x.FieldId == fieldId
            : x.Field.Slug == FieldIdOrSlug
        )
      )
      .Select(x => new
      {
        x.Value,
        x.Field.Type
      });


    var result = query.FirstOrDefault();

    if (result == null) return string.Empty;

    var fieldValue = result.Value;
    if (!format) return fieldValue;
    fieldValue = result.Type switch
    {
      "date_picker" => FormatDate(fieldValue),
      "date_picker_time" => FormatDateTime(fieldValue),
      _ => fieldValue
    };
    return fieldValue;
  }

  private static string FormatDate(string value)
  {
    return DateTime.TryParse(value, out var date) ? date.ToString("yyyy-MM-dd") : value;
  }

  private static string FormatDateTime(string value)
  {
    return DateTime.TryParse(value, out var dateTime) ? dateTime.ToString("yyyy-MM-dd HH:mm:ss") : value;
  }

  private static string maybe_translate_custom_field_name(this HelperBase help, string name, string slug)
  {
    // Translation logic for the custom field name can be implemented here
    // For simplicity, just returning the name as-is for now
    return name;
  }

  public static bool is_custom_fields_smart_transfer_enabled(this HelperBase helper)
  {
    if (!helper.defined("CUSTOM_FIELDS_SMART_TRANSFER")) return true;
    if (helper.defined("CUSTOM_FIELDS_SMART_TRANSFER")) return true;
    return false;
  }
}
