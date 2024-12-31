using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using static Service.Helpers.Sms.SmsHelper;

namespace Service.Helpers.Proposals;

public static class ProposalHelper
{
  public static string get_proposal_short_link(this HelperBase helper, Proposal proposal)
  {
    var (self, db) = getInstance();
    var longUrl = $"{helper.site_url()}/proposal/{proposal.Id}/{proposal.Hash}";
    var bitlyToken = db.get_option("bitly_access_token");
    if (string.IsNullOrEmpty(bitlyToken)) return longUrl;

    if (!string.IsNullOrEmpty(proposal.ShortLink)) return proposal.ShortLink;

    var shortLink = helper.app_generate_short_link(new ShortLinkRequest
    {
      LongUrl = longUrl,
      Title = helper.format_proposal_number(proposal.Id)
    });


    if (shortLink == null) return longUrl;
    proposal.ShortLink = shortLink;
    db.Proposals.Update(proposal);
    db.SaveChanges();
    return shortLink;
  }

  public static void check_proposal_restrictions(this HelperBase helper, int id, string hash)
  {
    var (self, db) = getInstance();
    var proposal = db.Proposals.Find(id);
    if (proposal == null || proposal.Hash != hash) throw new Exception("Proposal not found");
  }

  public static bool is_proposals_email_expiry_reminder_enabled(this HelperBase helper)
  {
    var (self, db) = getInstance();
    var output = db.EmailTemplates.Any(t =>
      t.Slug == "proposal-expiry-reminder" &&
      t.Active == 1
    );
    return output;
  }

  /**
   * Check if there are sources for sending proposal expiry reminders
   * Will be either email or SMS
   * @return boolean
   */
  public static bool is_proposals_expiry_reminders_enabled(this HelperBase helper)
  {
    var (self, db) = getInstance();
    return helper.is_proposals_email_expiry_reminder_enabled() || helper.is_sms_trigger_active(self.globals<string>("SMS_TRIGGER_PROPOSAL_EXP_REMINDER"));
  }


  public static string proposal_status_color_class(this HelperBase helper, int id, bool replaceDefaultByMuted = false)
  {
    var @class = id switch
    {
      1 => "default",
      2 => "danger",
      3 => "success",
      4 or 5 => "info",
      6 => "default",
      _ => "default"
    };

    return replaceDefaultByMuted && @class == "default" ? "muted" : @class;
  }

  public static string format_proposal_status(this HelperBase helper, int status, string classes = "", bool label = true)
  {
    string statusText;
    string labelClass;

    switch (status)
    {
      case 1:
        statusText = "Open";
        labelClass = "default";
        break;
      case 2:
        statusText = "Declined";
        labelClass = "danger";
        break;
      case 3:
        statusText = "Accepted";
        labelClass = "success";
        break;
      case 4:
      case 5:
        statusText = "Sent/Revised";
        labelClass = "info";
        break;
      case 6:
        statusText = "Draft";
        labelClass = "default";
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    return label ? $"<span class='label label-{labelClass} {classes} s-status proposal-status-{status}'>{statusText}</span>" : statusText;
  }

  public static string format_proposal_number(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var prefix = db.get_option("proposal_number_prefix");
    var padding = int.Parse(db.get_option("number_padding_prefixes"));
    return prefix + id.ToString().PadLeft(padding, '0');
  }

  public static List<ProposalTax> get_proposal_item_taxes(this HelperBase helper, int itemId)
  {
    var (self, db) = getInstance();
    return db.ItemTaxes
      .Where(t => t.ItemId == itemId && t.RelType == "proposal")
      .Select(t => new ProposalTax
      {
        TaxName = $"{t.TaxName}|{t.TaxRate}"
      })
      .ToList();
  }

  public static ProposalPercentByStatus get_proposals_percent_by_status(this HelperBase helper, int status, int? totalProposals = null)
  {
    var (self, db) = getInstance();
    var staffId = helper.get_staff_user_id();
    var hasPermissionView = helper.has_permission("proposals", "view");
    var hasPermissionViewOwn = helper.has_permission("proposals", "view_own");
    var allowViewAssigned = bool.Parse(db.get_option("allow_staff_view_proposals_assigned"));

    IQueryable<Proposal> proposalsQuery = db.Proposals;

    if (!hasPermissionView)
      proposalsQuery = hasPermissionViewOwn
        ? proposalsQuery.Where(p => p.AddedFrom == staffId || (allowViewAssigned && p.Assigned == staffId))
        : proposalsQuery.Where(p => p.Assigned == staffId);

    var total = totalProposals ?? proposalsQuery.Count();
    var totalByStatus = proposalsQuery.Count(p => p.Status == status);

    var percent = total > 0 ? (double)totalByStatus * 100 / total : 0;
    return new ProposalPercentByStatus { TotalByStatus = totalByStatus, Percent = percent, Total = total };
  }

  public static Proposal parse_proposal_content_merge_fields(this HelperBase helper, Proposal proposal)
  {
    var (self, db) = getInstance();
    var id = proposal.Id;

    // $CI->load->library('merge_fields/proposals_merge_fields');
    // $CI->load->library('merge_fields/other_merge_fields');
    var content = proposal.Content;
    if (string.IsNullOrEmpty(content)) return proposal;

    // var merge_fields = new List<>();
    // merge_fields = array_merge(merge_fields, self.library.proposals_merge_fields().format(id));
    // merge_fields = array_merge(merge_fields, self.library.other_merge_fields().format());
    // foreach (merge_fields as $key => $val) {
    //   if (stripos(content,  $key) != false) {
    //     content = str_ireplace($key, $val, content);
    //   } else {
    //     content = str_ireplace($key, '', content);
    //   }
    // }


    proposal.Content = content;

    return proposal;
  }
}
