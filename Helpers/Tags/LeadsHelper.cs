using System.Text.RegularExpressions;
using Global.Entities;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Helpers.Pdf;

namespace Service.Helpers.Tags;

public static class LeadsHelper
{
  public static void add_leads_admin_head_data(this HelperBase helper)
  {
    var (self, db) = getInstance();

    var leadUniqueValidationFields = db.get_option("lead_unique_validation");
    var leadAttachmentsDropzone = "";

    // Output script directly
    Console.WriteLine($@"
                <script>
                    var leadUniqueValidationFields = {leadUniqueValidationFields};
                    var leadAttachmentsDropzone;
                </script>");
  }

  public static bool IsLeadCreator(this HelperBase helper, int leadId, int? staffId = null)
  {
    var (self, db) = getInstance();
    var staffIdToCheck = staffId ?? helper.get_staff_user_id();
    var output = db.Leads.Any(x => x.AddedFrom == staffIdToCheck && x.Id == leadId);
    return output;
  }

  public static string GetLeadConsentUrl(this HelperBase helper, int id)
  {
    return $"{helper.site_url()}/consent/l/{helper.get_lead_hash(id)}";
  }

  public static string GetLeadsPublicUrl(this HelperBase helper, int id)
  {
    return $"{helper.site_url()}/forms/l/{helper.get_lead_hash(id)}";
  }

  public static string get_lead_hash(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var hash = "";

    var lead = db.Leads.FirstOrDefault(x => x.Id == id);
    if (lead == null) return hash;

    hash = lead.Hash;
    if (!string.IsNullOrEmpty(hash)) return hash;
    hash = $"{helper.uuid()}-{helper.uuid()}";
    db.Leads.Where(x => x.Id == id).Update(x => new Lead { Hash = hash });

    return hash;
  }

  public static List<LeadStatusSummary> get_leads_summary(this HelperBase helper)
  {
    var (self, db) = getInstance();
    var statuses = db.LeadsStatuses.ToList();
    var staffUserId = helper.get_staff_user_id();
    var hasPermissionView = helper.has_permission("leads", "view");

    statuses.Add(new LeadStatusSummary
    {
      Lost = true,
      Name = "Lost Leads",
      Color = "#fc2d42"
    });

    var whereNoViewPermission = db.Leads
      .Where(l => l.AddedFrom == staffUserId || l.Assigned == staffUserId || Convert.ToBoolean(l.IsPublic))
      .ToList();

    var results = statuses.Select(status =>
    {
      var leadsQuery = db.Leads.AsQueryable();

      leadsQuery = status.Lost is true
        ? leadsQuery.Where(l => l.Lost == true)
        : leadsQuery.Where(l => l.StatusId == status.Id);

      if (!hasPermissionView)
        leadsQuery = leadsQuery
          .Where(l => l.AddedFrom == staffUserId || l.Assigned == staffUserId || Convert.ToBoolean(l.IsPublic));

      var totalLeadsForStatus = leadsQuery.Count();
      var totalLeadValue = leadsQuery.Sum(l => (decimal?)l.LeadValue) ?? 0;

      return new LeadStatusSummary
      {
        Id = status.Id,
        Name = status.Name,
        Lost = status.Lost.Value,
        Color = status.Color,
        Total = totalLeadsForStatus,
        Value = totalLeadValue
      };
    }).ToList();

    var totalLeads = hasPermissionView ? db.Leads.Count() : whereNoViewPermission.Count();
    results.ForEach(status => { status.Percent = totalLeads > 0 ? Math.Round((double)(status.Total * 100) / totalLeads, 2) : 0; });

    return results;
  }

  public static string render_leads_status_select(this HelperBase helper, List<LeadsStatus> statuses, string? selected = null, string langKey = "", string name = "status", Dictionary<string, string> selectAttrs = null, bool excludeDefault = false)
  {
    var (self, db) = getInstance();
    var sender = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(
      JsonConvert.SerializeObject(statuses));

    if (helper.is_admin() || db.get_option("staff_members_create_inline_lead_status") == "1")
      return render_select_with_input_group(name, sender, new List<string> { "id" }, "name", langKey, selected, "<div class='input-group-btn'><a href='#' class='btn btn-default' onclick='new_lead_status_inline();return false;'><i class='fa fa-plus'></i></a></div>", selectAttrs);

    var selected_sender = JsonConvert.DeserializeObject<Dictionary<string, string>>(
      JsonConvert.SerializeObject(selected));
    return render_select(name, sender, new List<string> { "id" }, "name", langKey, selected_sender, selectAttrs);
  }

  public static string render_select_with_input_group(
    string name,
    List<Dictionary<string, object>> options,
    List<string> optionAttrs = null,
    string label = "",
    string selected = "",
    string inputGroupContents = "",
    // Dictionary<string, string> selectAttrs = null,
    object selectAttrs = null,
    Dictionary<string, string> formGroupAttr = null,
    string formGroupClass = "",
    string selectClass = "",
    bool includeBlank = true)
  {
    optionAttrs ??= new List<string> { };
    selectAttrs ??= new Dictionary<string, string>();
    formGroupAttr ??= new Dictionary<string, string>();

    selectClass += " _select_input_group";
    var sender = JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(selectAttrs));
    var select = render_select(name, options, optionAttrs, label, selected, sender, formGroupAttr, formGroupClass, selectClass, includeBlank);

    select = select.Replace("form-group", $"input-group input-group-select select-{name}");
    select = select.Replace("select-placeholder ", "");
    select = select.Replace("</select>", $"</select>{inputGroupContents}");

    var labelPattern = @"<label.*<\/label>";
    var labelMatch = Regex.Match(select, labelPattern, RegexOptions.IgnoreCase);

    if (!labelMatch.Success) return select;

    select = Regex.Replace(select, labelPattern, "");
    select = $"<div class=\"select-placeholder form-group form-group-select-input-{name} input-group-select\">{labelMatch.Value}{select}</div>";

    return select;
  }

  public static bool load_lead_language(this HelperBase helper, int leadId)
  {
    var (self, db) = getInstance();
    var lead = db.Leads.FirstOrDefault(x => x.Id == leadId);

    if (lead != null || string.IsNullOrEmpty(lead.DefaultLanguage)) return false;
    var language = lead.DefaultLanguage;

    if (!helper.file_exists("language/" + language)) return false;


    // self.Lang.IsLoaded.Clear();
    // self.Lang.Language.Clear();
    // self.Lang.Load(language + "_lang", language);
    helper.load_custom_lang_file(language);
    // self.Lang.set_last_loaded_language(language);

    return true;
  }

  public static bool load_client_language(this HelperBase helper, int leadId)
  {
    return false;
  }

  private static List<LeadsStatus> get_leads_statuses(this HelperBase helper)
  {
    var (self, db) = getInstance();
    var leads_model = self.model.leads_model();
    var output = leads_model.get_status(x => true);
    return output;
  }
}