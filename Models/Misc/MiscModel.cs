using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Helpers;
using Service.Helpers.Sale;
using Service.Models.Tasks;
using File = Service.Entities.File;

namespace Service.Models.Misc;

public class MiscModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  public int notifications_limit = 5;
  private TasksModel taxes_model = self.tasks_model(db);

  public string get_taxes_dropdown_template(string name, string taxname, Taxis? type = null, int item_id = 0, bool is_edit = false, bool manual = false)
  {
    var taxes = new List<Taxis>();

    // if passed manually
    if (manual)
    {
      var taxArray = new List<string>();
      if (taxname.Contains("+") || taxname.Contains("|"))
      {
        var taxParts = taxname.Contains("+") ? taxname.Split('+') : new[] { taxname };
        taxParts
          .ToList()
          .ForEach(part =>
          {
            var split = part.Split('|');
            if (split.Length >= 2)
              taxArray.Add($"{split[0]}|{split[1]}");
          });
      }
      else
      {
        var split = taxname.Split('|');
        if (split.Length >= 2)
        {
          var tax = db.get_tax_by_name(split[0]);
          if (tax != null) taxname = $"{tax.Name}|{tax.TaxRate}";
        }
      }
    }

    // Fetch all system taxes using EF Core

    taxes = db.Taxes.Select(t => new Taxis
    {
      Name = t.Name + "|" + t.TaxRate,
      TaxRate = t.TaxRate
    }).ToList();

    if (is_edit)
    {
      // Function to retrieve item taxes based on type and item_id
      var item_taxes = self.helper.get_item_taxes(type, item_id);
      item_taxes.ForEach(itemTax =>
      {
        taxes.Add(new Taxis
        {
          Name = itemTax.Name,
          TaxRate = itemTax.TaxRate
        });
      });
    }

    // Add missing taxes from taxname to the dropdown
    if (!string.IsNullOrEmpty(taxname))
    {
      var selectedTaxes = taxname.Split('|');
      foreach (var selectedTax in selectedTaxes)
        if (taxes.All(t => t.Name != selectedTax))
          taxes.Add(new Taxis
          {
            Name = selectedTax,
            TaxRate = selectedTax.Split("|")[1]
          });
    }

    // Remove duplicates
    taxes = taxes.GroupBy(t => t.Name).Select(g => g.First()).ToList();

    // Build the dropdown HTML
    var select = $"<select class='selectpicker display-block tax' data-width='100%' name='{name}' multiple data-none-selected-text='No Tax'>";

    foreach (var tax in taxes)
    {
      var selected = taxname.Contains(tax.Name) ? "selected" : "";
      select += $"<option value='{tax.Name}' {selected} data-taxrate='{tax.TaxRate}' data-taxname='{tax.Name}' data-subtext='{tax.Name}'>{tax.TaxRate}%</option>";
    }

    select += "</select>";
    return select;
  }

  public int add_attachment_to_database(int rel_id, string rel_type, File attachment, string? external = null)
  {
    var data = new File
    {
      DateCreated = DateTime.Now,
      RelId = rel_id,
      RelType = rel_type,
      AttachmentKey = uuid(),
      StaffId = !attachment.StaffId.HasValue
        ? staff_user_id
        // Replace with your actual method to get staff user ID
        : attachment.StaffId.Value
      // Replace with your hash generation logic
    };

    if (attachment.TaskCommentId.HasValue) data.TaskCommentId = attachment.TaskCommentId.Value;
    if (attachment.ContactId.HasValue)
    {
      data.ContactId = attachment.ContactId.Value;
      data.VisibleToCustomer = true;
      data.StaffId = null; // Remove staffId if contact is provided
    }

    if (string.IsNullOrEmpty(external))
    {
      data.FileName = attachment.FileName;
      data.FileType = attachment.FileType;
    }
    else
    {
      var pathParts = Path.GetExtension(attachment.FileName);
      data.FileName = attachment.FileName;
      data.ExternalLink = attachment.ExternalLink;
      data.FileType = string.IsNullOrEmpty(attachment.FileType) ? self.helper.get_mime_by_extension(pathParts) : attachment.FileType; // Implement GetMimeByExtension as needed
      data.External = external;

      if (!string.IsNullOrEmpty(attachment.ThumbnailLink)) data.ThumbnailLink = attachment.ThumbnailLink;
    }

    // Insert the attachment record in the files table
    var result = db.Files.Add(data);

    var insertId = data.Id;

    // If related type is customer and contact is provided
    if (rel_type != "customer" || !data.ContactId.HasValue) return insertId;
    if (db.get_option_compare("only_own_files_contacts", 1)) // Implement get_option as needed
    {
      var sharedFile = new SharedCustomerFile
      {
        FileId = insertId,
        ContactId = data.ContactId.Value
      };
      db.SharedCustomerFiles.Add(sharedFile);
    }
    else
    {
      var contacts = db.Contacts
        .Where(c => c.UserId == rel_id)
        .Select(c => c.Id)
        .ToList();


      contacts
        .Select(contactId => new SharedCustomerFile
        {
          FileId = insertId,
          ContactId = contactId
        })
        .ToList()
        .ForEach(sharedFile => db.SharedCustomerFiles.Add(sharedFile));
    }

    return insertId;
  }

  public File? get_file(int id)
  {
    var row = db.Files.FirstOrDefault(x => x.Id == id);
    return row;
  }

  public SearchResult<Expense> search_expenses(string query, int limit = 0)
  {
    var result = new SearchResult<Expense>();

    var hasPermissionExpensesView = db.has_permission("expenses", "view");
    var hasPermissionExpensesViewOwn = db.has_permission("expenses", "view_own");

    if (!hasPermissionExpensesView && !hasPermissionExpensesViewOwn) return result;
    var expensesQuery = db.Expenses
      .Include(x => x.Client)
      .Include(e => e.PaymentMode)
      .Include(e => e.Tax)
      .Include(e => e.Category)
      .Include(e => e.Currency)
      .AsQueryable();


    // If the user doesn't have full expense view permission, limit to their own entries
    if (!hasPermissionExpensesView)
    {
      var currentUserId = staff_user_id;
      expensesQuery = expensesQuery.Where(e => e.AddedFrom == currentUserId);
    }

    // Search logic
    expensesQuery = expensesQuery.Where(e =>
      e.Client!.Company.Contains(query) ||
      // e.PaymentModeName.Contains(query) ||
      // e.Vat.Contains(query) ||
      e.Client.PhoneNumber.Contains(query) ||
      e.Client.City.Contains(query) ||
      e.Client.Zip.Contains(query) ||
      e.Client.Address.Contains(query) ||
      e.Client.State.Contains(query) ||
      e.Category!.Name.Contains(query) ||
      e.Note!.Contains(query) ||
      e.ExpenseName.Contains(query)
    );

    // Limit the number of results if specified
    if (limit > 0) expensesQuery = expensesQuery.Take(limit);
    // Order by date (descending)
    // expensesQuery = expensesQuery.OrderByDescending(e => e.Date).ToList();
    // result.Result = expensesQuery.ToList();
    result.Result = expensesQuery
      .OrderByDescending(e => e.Date)
      .ToList();

    return result;
  }

  public SearchResult<Estimate> search_estimates(string query, int limit = 0)
  {
    var result = new SearchResult<Estimate>();

    var hasPermissionViewEstimates = db.has_permission("estimates", "view");
    var hasPermissionViewEstimatesOwn = db.has_permission("estimates", "view_own");
    var allowStaffViewEstimatesAssigned = db.get_option("allow_staff_view_estimates_assigned") == "1";

    if (!hasPermissionViewEstimates && !hasPermissionViewEstimatesOwn && !allowStaffViewEstimatesAssigned) return result;
    // Trim and adjust query based on input type
    if (int.TryParse(query, out _))
      query = query.TrimStart('0');
    else if (query.StartsWith(db.get_option("estimate_prefix")))
      query = query.Substring(db.get_option("estimate_prefix").Length).TrimStart('0');

    // Build the query for searching estimates
    var estimatesQuery = db.Estimates
      .Include(e => e.Client)
      .Include(e => e.Currency)
      .Include(e => e.Invoice)
      .AsQueryable();

    // Handle permission logic
    if (!hasPermissionViewEstimates)
    {
      var noPermissionQuery = db.get_estimates_where_sql_for_staff(staff_user_id);
      estimatesQuery = estimatesQuery.Where(noPermissionQuery);
    }

    // Add search conditions based on the query
    estimatesQuery = estimatesQuery.Where(e =>
      // e.Number.Contains(query) ||
      e.Client.Company.Contains(query) ||
      // e.Client.CreditNotes.Contains(query) ||
      // e.Invoice.Vat.Contains(query) ||
      e.Client.PhoneNumber.Contains(query) ||
      e.Client.City.Contains(query) ||
      e.Client.State.Contains(query) ||
      e.Client.Zip.Contains(query) ||
      e.Client.BillingStreet.Contains(query) ||
      e.Client.BillingCity.Contains(query) ||
      e.Client.BillingState.Contains(query) ||
      e.Client.BillingZip.Contains(query) ||
      e.Client.ShippingStreet.Contains(query) ||
      e.Client.ShippingCity.Contains(query) ||
      e.Client.ShippingState.Contains(query) ||
      e.Client.ShippingZip.Contains(query) ||
      e.AdminNote.Contains(query) ||
      e.BillingStreet.Contains(query) ||
      e.BillingCity.Contains(query) ||
      e.BillingState.Contains(query) ||
      e.BillingZip.Contains(query) ||
      e.ShippingStreet.Contains(query) ||
      e.ShippingCity.Contains(query) ||
      e.ShippingState.Contains(query) ||
      e.ShippingZip.Contains(query)
    );

    // Order the results by number and date (descending)
    estimatesQuery = estimatesQuery.OrderByDescending(e => e.Number)
      .ThenByDescending(e => e.Date.Year);

    // Limit the results if necessary
    if (limit > 0)
      estimatesQuery = estimatesQuery.Take(limit);
    result.Result = estimatesQuery.ToList();
    return result;
  }

  public SearchResult<Staff> search_staff(string query, int limit = 0)
  {
    var output = new SearchResult<Staff>();
    var staff_can_view = db.has_permission("staff", "", "view");
    if (!staff_can_view) return output;
    var staffQuery = db.Staff
      .Where(s =>
        s.FirstName.Contains(query) ||
        s.LastName.Contains(query) ||
        (s.FirstName + " " + s.LastName).Contains(query) ||
        (s.LastName + " " + s.FirstName).Contains(query) ||
        s.Facebook!.Contains(query) ||
        s.LinkedIn!.Contains(query) ||
        s.PhoneNumber.Contains(query) ||
        s.Email.Contains(query) ||
        s.Skype.Contains(query)
      )
      .OrderBy(s => s.FirstName)
      .AsQueryable();

    if (limit > 0)
      staffQuery = staffQuery.Take(limit);
    output.Result = staffQuery.ToList();

    return output;
  }

  public ProjectSearchResult search_projects(string q, int limit = 0, Expression<Func<Project, bool>> condition = default)
  {
    var output = new ProjectSearchResult()
    {
      Result = new List<Project>(),
      Type = "projects",
      SearchHeading = self.helper.label("projects")
    };

    var hasPermissionViewProject = db.has_permission("projects", "", "view");
    // Projects
    var query = db.Projects
      .Include(p => p.Client)
      .Include(p => p.ProjectMembers)
      .Where(condition)
      .AsQueryable();
    if (!hasPermissionViewProject)
      query = query.Where(x => x.ProjectMembers.Where(y => y.StaffId == staff_user_id).Select(y => y.ProjectId).ToList().Contains(x.Id));

    query = !q.StartsWith("#")
      ? query.Where(x =>
        x.Description!.Contains(q) ||
        x.Name.Contains(q) ||
        x.Client.Vat.Contains(q) ||
        x.Client.Company.Contains(q) ||
        x.Client.PhoneNumber.Contains(q) ||
        x.Client.City.Contains(q) ||
        x.Client.Zip.Contains(q) ||
        x.Client.State.Contains(q) ||
        x.Client.Zip.Contains(q) ||
        x.Client.Address.Contains(q)
      )
      : query.Where(x => db.Taggables
        .Where(t => t.TagId == db.Tags
          .Where(t => t.Name == q)
          .Select(t => t.Id)
          .FirstOrDefault()
        )
        .Select(t => t.RelId)
        .ToList()
        .Contains(x.Id)
      );

    if (limit != 0)
      query = query.Take(limit);
    query = query.OrderBy(x => x.Name);
    output.Result = query.ToList();
    return output;
  }

  public SearchResult<Proposal> search_proposals(string query, int limit = 0)
  {
    var result = new SearchResult<Proposal> { SearchHeading = "Proposals" };

    // Simulated permission checks
    var hasPermissionViewProposals = db.has_permission("proposals", "", "view");
    var hasPermissionViewProposalsOwn = db.has_permission("proposals", "", "view_own");
    var allowStaffViewProposalsAssigned = db.get_option<bool>("allow_staff_view_proposals_assigned");

    if (!hasPermissionViewProposals && !hasPermissionViewProposalsOwn && !allowStaffViewProposalsAssigned) return result;
    query = PreprocessQuery(query);

    // Permission-based filtering
    var noPermissionQuery = GetProposalsWhereSqlForStaff(staff_user_id);

    var proposalsQuery = db.Proposals
      .Include(p => p.Currency)
      .Where(p =>
        p.Id.ToString().Contains(query) ||
        p.Subject.Contains(query) ||
        p.Content!.Contains(query) ||
        p.ProposalTo.Contains(query) ||
        p.Zip.Contains(query) ||
        p.State.Contains(query) ||
        p.City.Contains(query) ||
        p.Address.Contains(query) ||
        p.Email.Contains(query) ||
        p.Phone.Contains(query)
      );

    if (!hasPermissionViewProposals) proposalsQuery = proposalsQuery.Where(noPermissionQuery);

    proposalsQuery = proposalsQuery.OrderByDescending(p => p.Id);

    if (limit > 0) proposalsQuery = proposalsQuery.Take(limit);

    result.Result = proposalsQuery.ToList();

    return result;
  }

  private string PreprocessQuery(string query)
  {
    if (int.TryParse(query, out _)) return query.TrimStart('0');

    var proposalNumberPrefix = db.get_option<string>("proposal_number_prefix");
    if (query.StartsWith(proposalNumberPrefix)) query = query[proposalNumberPrefix.Length..].TrimStart('0');

    return query;
  }

  private Expression<Func<Proposal, bool>> GetProposalsWhereSqlForStaff(int staffId)
  {
    // Replace with actual logic for filtering proposals based on staff permissions
    return p => p.Assigned == staffId || p.AddedFrom == staffId;
  }


  public SearchResult<CreditNote> search_credit_notes(string query, int limit = 0)
  {
    var result = new SearchResult<CreditNote>
    {
      Result = new List<CreditNote>(),
      Type = "credit_note",
      SearchHeading = "Credit Notes"
    };

    var hasPermissionViewCreditNotes = db.has_permission("credit_notes", "view");
    var hasPermissionViewOwnCreditNotes = db.has_permission("credit_notes", "view_own");

    if (!hasPermissionViewCreditNotes && !hasPermissionViewOwnCreditNotes) return result;
    query = SanitizeQuery(query);

    var creditNotesQuery = db.CreditNotes
      .Include(cn => cn.Client)
      .Include(cn => cn.Currency)
      .Include(cn => cn.CreditNoteRefunds)
      .Where(joined =>
        joined.CreditNoteRefunds.Select(cnr => cnr.CreditNote.Number).Contains(Convert.ToInt32(query)) ||
        joined.Client.Company.Contains(query) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.ClientNote!.Contains(query)) ||
        joined.Client.Vat.Contains(query) ||
        joined.Client.PhoneNumber.Contains(query) ||
        joined.Client.City.Contains(query) ||
        joined.Client.State.Contains(query) ||
        joined.Client.Zip.Contains(query) ||
        joined.Client.Address.Contains(query) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.AdminNote!.Contains(query)) ||
        // (joined.contact.FirstName + " " + joined.contact.LastName).Contains(query) ||
        // (joined.contact.LastName + " " + joined.contact.FirstName).Contains(query) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.BillingStreet.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.BillingState.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.BillingCity.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.BillingZip.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.ShippingStreet.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.ShippingCity.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.ShippingState.Contains(query)) ||
        joined.CreditNoteRefunds.Any(x => x.CreditNote.ShippingZip.Contains(query))
      )
      .OrderByDescending(joined => joined.Number)
      .AsQueryable();

    if (limit > 0)
      creditNotesQuery = creditNotesQuery.Take(limit);
    result.Result = creditNotesQuery.ToList();
    return result;
  }

  private string SanitizeQuery(string query)
  {
    query = query?.TrimStart('0');
    // Add logic for handling prefixes and additional sanitization as needed
    return query;
  }

  public SearchResult<Lead> search_leads(string q, int i, object o)
  {
    var output = new SearchResult<Lead>();
    return output;
  }

  public SearchResult<Contract> search_contracts(string q)
  {
    var output = new SearchResult<Contract>();
    return output;
  }

  public SearchResult<Ticket> search_tickets(string q)
  {
    var output = new SearchResult<Ticket>();
    return output;
  }

  public SearchResult<Invoice> search_invoices(string q)
  {
    var output = new SearchResult<Invoice>();
    return output;
  }

  public SearchResult<Contact> search_contacts(string q, int i, Expression<Func<Contact, bool>> condition)
  {
    var output = new SearchResult<Contact>();
    return output;
  }
}
