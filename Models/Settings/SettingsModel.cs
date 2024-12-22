using Global.Entities;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers.Security;
using Service.Helpers;
using Service.Helpers.Template;
using Service.Models.Extras;
using Service.Models.Payments;

namespace Service.Models.Settings;

public class SettingOption
{
  public string Name { get; set; }
  public object Value { get; set; }
  public List<object> default_tax = new();
}

public class SettingsModel(MyInstance self, MyContext db) : MyModel(self)
{
  private PaymentModesModel payment_modes_model = self.model.payment_modes_model();

  // private List<string> encryptedFields = new()
  // {
  //   "smtp_password",
  //   "microsoft_mail_client_secret",
  //   "google_mail_client_secret"
  // };
  private List<string> encryptedFields
  {
    get
    {
      var paymentGateways = payment_modes_model.get_payment_gateways(true);
      return paymentGateways
        .SelectMany(gateway => gateway.GetSettings())
        .Where(setting => setting.Name.Contains("encrypted"))
        .Select(setting => setting.Name)
        .Distinct()
        .ToList();
    }
  }


  public int Update(Option data, List<CustomField> custom_fields = default, List<Tag> tags = default, List<SettingOption> settings = default)
  {
    var originalEncryptedFields = new Dictionary<string, object>();
    foreach (var ef in encryptedFields) originalEncryptedFields[ef] = db.get_option(ef);
    var affectedRows = 0;
    data = self.hooks.apply_filters("before_settings_updated", data);

    if (tags.Any())
    {
      var tagsExists = false;
      foreach (var tag in tags)
      {
        var name = tag.Name;
        var id = Convert.ToInt32(tag.Name);
        var existingTag = db.Tags.FirstOrDefault(t => t.Name == name && t.Id != id);
        if (existingTag == null)
        {
          db.Tags.Update(new Tag { Id = id, Name = name });
          affectedRows += db.SaveChanges();
        }
        else
        {
          tagsExists = true;
        }
      }

      if (!tagsExists) return affectedRows;
      set_alert("warning", "Tags update replace warning");
      return 0;
    }

    // if (!data.ContainsKey("settings") || !((Dictionary<string, object>)data["settings"]).ContainsKey("default_tax"))
    //   data["settings"]["default_tax"] = new List<object>();
    //
    // if (settings.Any() && settings.default_tax != null)
    // {
    //   var defaultTax = settings.default_tax;
    //   if (defaultTax is not List<object> || !defaultTax.Any()) settings.default_tax = new List<object>();
    // }


    var allSettingsLooped = new List<string>();
    foreach (var setting in settings)
    {
      var name = setting.Name;
      var val = setting.Value;

      if (val is string strVal && name != "thousand_separator") val = strVal.Trim();

      allSettingsLooped.Add(name);

      var hookData = new Dictionary<string, object>
      {
        { "name", name },
        { "value", val }
      };
      hookData = self.hooks.apply_filters("before_single_setting_updated_in_loop", hookData);
      name = (string)hookData["name"];
      val = hookData["value"];

      if (name == "default_contact_permissions")
      {
        val = JsonConvert.SerializeObject(val);
      }
      else if (name == "lead_unique_validation")
      {
        val = JsonConvert.SerializeObject(val);
      }
      else if (name == "visible_customer_profile_tabs")
      {
        if (val.ToString() == "")
        {
          val = "all";
        }
        else
        {
          var tabs = self.helper.get_customer_profile_tabs();
          var newVisibleTabs = new Dictionary<string, bool>();
          foreach (var tab in tabs) newVisibleTabs[tab.Name] = ((List<string>)val).Contains(tab.Name);
          val = JsonConvert.SerializeObject(newVisibleTabs);
        }
      }
      else if (name == "email_signature")
      {
        val = HtmlEntity.DeEntitize(val.ToString());

        if (val.ToString() == self.helper.StripTags(val.ToString()))
          val = val.ToString().nl2br();
      }
      else if (name is "email_header" or "email_footer")
      {
        val = HtmlEntity.DeEntitize(val.ToString());
      }
      else if (name == "default_tax")
      {
        val = ((List<object>)val).Where(v => !string.IsNullOrEmpty(v.ToString())).ToList();
        val = JsonConvert.SerializeObject(val);
      }
      else if (new[] { "company_info_format", "customer_info_format", "proposal_info_format" }.Contains(name) || name.StartsWith("sms_trigger_"))
      {
        val = self.helper.StripTags(val.ToString());
        val = val.ToString().nl2br();
      }
      else if (encryptedFields.Contains(name))
      {
        if (!string.IsNullOrEmpty(val.ToString()))
        {
          var originalDecrypted = self.helper.decrypt(originalEncryptedFields[name].ToString());
          if (originalDecrypted == val.ToString()) continue;
          val = self.helper.encrypt(val.ToString());
        }
      }
      else if (new[] { "staff_notify_completed_but_not_billed_tasks", "reminder_for_completed_but_not_billed_tasks_days" }.Contains(name))
      {
        val = JsonConvert.SerializeObject(val);
      }

      if (!db.update_option(name, val)) continue;
      affectedRows++;
      if (name == "save_last_order_for_tables") db.UserMeta.RemoveRange(db.UserMeta.Where(um => um.MetaKey.Contains("-table-last-order")));
    }

    if (!allSettingsLooped.Contains("default_contact_permissions") && allSettingsLooped.Contains("customer_settings"))
    {
      db.Options.Update(new Option { Name = "default_contact_permissions", Value = JsonConvert.SerializeObject(new List<object>()) });
      if (db.SaveChanges() > 0) affectedRows++;
    }
    else if (!allSettingsLooped.Contains("visible_customer_profile_tabs") && allSettingsLooped.Contains("customer_settings"))
    {
      db.Options.Update(new Option { Name = "visible_customer_profile_tabs", Value = "all" });
      if (db.SaveChanges() > 0) affectedRows++;
    }
    else if (!allSettingsLooped.Contains("lead_unique_validation") && allSettingsLooped.Contains("_leads_settings"))
    {
      db.Options.Update(new Option { Name = "lead_unique_validation", Value = JsonConvert.SerializeObject(new List<object>()) });
      if (db.SaveChanges() > 0) affectedRows++;
    }

    if (!custom_fields.Any()) return affectedRows;
    if (self.helper.handle_custom_fields_post(0, custom_fields))
      affectedRows++;

    return affectedRows;
  }

  public bool AddNewCompanyPdfField(Dictionary<string, object> data)
  {
    var field = "custom_company_field_" + data["field"].ToString().Trim().Replace(" ", "_");
    return db.add_option(field, data["value"]);
  }
  // Other properties and methods...


  private bool CheckCustomFieldExists(int id, string key)
  {
    // Example implementation to check if the custom field exists
    var existingField = db.CustomFields
      .FirstOrDefault(x => x.Id == id && x.Name == key);
    return existingField != null;
  }

  private bool SaveCustomField(string fieldName, object fieldValue)
  {
    // Sanitize the field name and value as necessary
    fieldName = self.helper.StripTags(fieldName.Trim());

    // Convert the field value to a string, if needed
    var valueString = fieldValue?.ToString();

    // Validate field name and value
    if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(valueString)) throw new ArgumentException("Field name and value cannot be null or empty.");

    // Create the custom field option if it doesn't already exist
    if (!db.option_exists(fieldName))
    {
      // Add the new custom field option to the database
      if (!db.add_option(fieldName, valueString)) return false; // Failed to add the option
    }
    else
    {
      // Update the existing custom field option
      if (!db.update_option(fieldName, valueString)) return false; // Failed to update the option
    }

    return true; // Successfully saved the custom field
  }
}
