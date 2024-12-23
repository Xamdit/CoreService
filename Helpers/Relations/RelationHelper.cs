using System.Dynamic;
using System.Linq.Expressions;
using Global.Entities;
using Microsoft.AspNetCore.Components;
using Service.Core.Extensions;
using Service.Framework.Core.Engine;
using Service.Helpers.Proposals;
using Service.Models.Projects;

namespace Service.Helpers.Relations;

public static class RelationHelper
{
  public static object get_relation_data(this HelperBase helper, string type, int rel_id = 0, object? extra = null)
  {
    var (self, db) = getInstance();
    var misc_model = self.model.misc_model();
    var q = "";
    if (self.input.post().TryGetValue("q", out var value))
    {
      q = value;
      q = q.Trim();
    }

    var tasks_model = self.model.tasks_model();
    var clients_model = self.model.clients_model();
    dynamic data = new ExpandoObject();
    switch (type)
    {
      case "customer" or "customers":
      {
        var where_clients = CreateCondition<Client>(x => x.Active);
        if (string.IsNullOrEmpty(q))
          where_clients = where_clients.And(x => x.Company.Contains(q));
        // where_clients = c => c.Company.Contains(q) || (c.Firstname + " " + c.Lastname).Contains(q) || (c.Email.Contains(q) && c.Active == 1);

        data = clients_model.get(rel_id, where_clients);
        break;
      }
      case "contact" or "contacts" when rel_id != 0:
        data = clients_model.get_contact(rel_id);
        break;
      case "contact" or "contacts":
      {
        var where_contacts = CreateCondition<Contact>(x => x.Active);
        if (isset(extra, "client_id") && self.helper.value<int>(extra, "client_id") != null)
          where_contacts = where_contacts.And(x => x.UserId == self.helper.value<int>(extra, "client_id"));
        if (self.input.post().ContainsKey("tickets_contacts"))
          if (!helper.has_permission("customers", "", "view") && db.get_option_compare("staff_members_open_tickets_to_all_contacts", 0))
            where_contacts = where_contacts.And(x => db.CustomerAdmins.Any(y => y.StaffId == helper.get_staff_user_id() && y.CustomerId == x.UserId));
        if (self.input.post().TryGetValue("contact_userid", out var contact_userid))
          where_contacts = where_contacts.And(x => x.UserId == Convert.ToInt32(contact_userid));
        var search = misc_model.search_contacts(q, 0, where_contacts);
        data = search.Result;
        break;
      }
      case "invoice" when rel_id != 0:
      {
        var invoices_model = self.model.invoices_model();
        data = invoices_model.get(rel_id);
        break;
      }
      case "invoice":
      {
        var search = misc_model.search_invoices(q);
        data = search.Result;
        break;
      }
      case "credit_note" when rel_id != 0:
      {
        var credit_notes_model = self.model.credit_notes_model();
        data = credit_notes_model.get(x => x.Id == rel_id);
        break;
      }
      case "credit_note":
      {
        var search = misc_model.search_credit_notes(q);
        data = search.Result;
        break;
      }
      case "estimate" when rel_id != 0:
      {
        var estimates_model = self.model.estimates_model();
        data = estimates_model.get(x => x.Id == rel_id);
        break;
      }
      case "estimate":
      {
        var search = misc_model.search_estimates(q);
        data = search.Result;
        break;
      }
      case "contract" or "contracts":
      {
        var contracts_model = self.model.contracts_model();

        if (rel_id != 0)
        {
          data = contracts_model.get(x => x.Id == rel_id);
        }
        else
        {
          var search = misc_model.search_contracts(q);
          data = search.Result;
        }

        break;
      }
      case "ticket" when rel_id != 0:
      {
        var tickets_model = self.model.tickets_model();
        data = tickets_model.get(x => x.Id == rel_id);
        break;
      }
      case "ticket":
      {
        var search = misc_model.search_tickets(q);
        data = search.Result;
        break;
      }
      case "expense" or "expenses" when rel_id != 0:
      {
        var expenses_model = self.model.expenses_model();
        data = expenses_model.get(x => x.Id == rel_id);
        break;
      }
      case "expense" or "expenses":
      {
        var search = misc_model.search_expenses(q);
        data = search.Result;
        break;
      }
      case "lead" or "leads" when rel_id != 0:
      {
        var leads_model = self.model.leads_model();
        data = leads_model.get(x => x.Id == rel_id);
        break;
      }
      case "lead" or "leads":
      {
        var search = misc_model.search_leads(q, 0, new { junk = 0 });
        data = search.Result;
        break;
      }
      case "proposal" when rel_id != 0:
      {
        var proposals_model = self.model.proposals_model();
        data = proposals_model.get(x => x.Id == rel_id);
        break;
      }
      case "proposal":
      {
        var search = misc_model.search_proposals(q);
        data = search.Result;
        break;
      }
      case "project" when rel_id != 0:
      {
        var projects_model = self.model.projects_model();
        data = projects_model.get(x => x.Id == rel_id);
        break;
      }
      case "project":
      {
        Expression<Func<Project, bool>> where_projects = null;
        if (self.input.post().ContainsKey("customer_id"))
          where_projects = proj => proj.ClientId == Convert.ToInt32(self.input.post("customer_id"));
        var search = misc_model.search_projects(q, 0, where_projects);
        data = search.Result;
        break;
      }
      case "staff" when rel_id != 0:
      {
        var staff_model = self.model.staff_model();
        data = staff_model.get(x => x.Id == rel_id);
        break;
      }
      case "staff":
      {
        var search = misc_model.search_staff(q);
        data = search.Result;
        break;
      }
      case "tasks" or "task":
      {
        // Tasks only have relation with custom fields when searching on top
        if (rel_id != 0)
          data = tasks_model.get(x => x.Id == rel_id);
        break;
      }
    }

    // data = self.hooks.apply_filters("get_relation_data", data, compact(type, rel_id, extra));
    return data;
  }

  /**
 * Ger relation values eq invoice number or project name etc based on passed relation parsed results
 * from function get_relation_data
 * relation can be object or array
 * @param  mixed relation
 * @param  string  type
 * @return mixed
 */
  public static RelationValues get_relation_values(this NavigationManager nav, RelationValues? relation = null, string type = "")
  {
    var (self, db) = getInstance();
    if (relation == null)
      return new RelationValues()
      {
        name = "",
        id = 0,
        link = "",
        addedfrom = 0,
        subtext = ""
      };

    var addedfrom = 0;
    var name = "";
    var id = 0;
    var link = "";
    var subtext = "";
    var userid = 0;
    var clientId = 0;
    switch (type)
    {
      case "customer" or "customers":
      {
        if (relation != null)
        {
          id = relation.userid;
          name = relation.company;
        }
        else
        {
          id = relation.userid;
          name = relation.company;
        }

        link = nav.admin_url($"clients/client/{id}");
        break;
      }
      case "contact" or "contacts":
      {
        if (relation != null)
        {
          userid = relation.userid > 0 ? relation.userid : relation.rel_id;
          id = relation.id;
          name = $"{relation.firstname} {relation.lastname}";
        }
        else
        {
          userid = relation.userid;
          id = relation.id;
          name = $"{relation.firstname} {relation.lastname}";
        }

        subtext = self.helper.get_company_name(userid);
        link = nav.admin_url($"clients/client/{userid}?contactid={id}");
        break;
      }
      case "invoice":
      {
        if (relation != null)
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
        }
        else
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
        }

        name = self.helper.format_invoice_number(id);
        link = nav.admin_url($"invoices/list_invoices/{id}");
        break;
      }
      case "credit_note":
      {
        if (relation != null)
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
        }
        else
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
        }

        name = self.helper.format_credit_note_number(id);
        link = nav.admin_url($"credit_notes/list_credit_notes/{id}");
        break;
      }
      case "estimate":
      {
        if (relation != null)
        {
          id = relation.estimate_id;
          addedfrom = relation.addedfrom;
        }
        else
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
        }

        name = self.helper.format_estimate_number(id);
        link = nav.admin_url($"estimates/list_estimates/{id}");
        break;
      }
      case "contract" or "contracts":
      {
        if (relation != null)
        {
          id = relation.id;
          name = relation.subject;
          addedfrom = relation.addedfrom;
        }
        else
        {
          id = relation.id;
          name = relation.subject;
          addedfrom = relation.addedfrom;
        }

        link = nav.admin_url($"contracts/contract/{id}");
        break;
      }
      case "ticket":
      {
        if (relation != null)
        {
          id = relation.ticket_id;
          name = $"#{relation.ticket_id}";
          name += $" - {relation.subject}";
        }
        else
        {
          id = relation.ticket_id;
          name = $"#{relation.ticket_id}";
          name += $" - {relation.subject}";
        }

        link = nav.admin_url($"tickets/ticket/{id}");
        break;
      }
      case "expense" or "expenses":
      {
        if (relation != null)
        {
          id = relation.expense_id;
          name = relation.category_name;
          addedfrom = relation.addedfrom;

          if (!string.IsNullOrEmpty(relation.expense_name)) name += $" ({relation.expense_name})";
        }
        else
        {
          id = relation.expense_id;
          name = relation.category_name;
          addedfrom = relation.addedfrom;
          if (!string.IsNullOrEmpty(relation.expense_name)) name += $" ({relation.expense_name})";
        }

        link = nav.admin_url($"expenses/list_expenses/{id}");
        break;
      }
      case "lead" or "leads":
      {
        if (relation != null)
        {
          id = relation.id;
          name = relation.name;
          if (relation.email != "") name += $" - {relation.email}";
        }
        else
        {
          id = relation.id;
          name = relation.name;
          if (relation.email != "") name += $" - {relation.email}";
        }

        link = nav.admin_url($"leads/index/{id}");
        break;
      }
      case "proposal":
      {
        if (relation != null)
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
          if (!string.IsNullOrEmpty(relation.subject)) name += $" - {relation.subject}";
        }
        else
        {
          id = relation.id;
          addedfrom = relation.addedfrom;
          if (!string.IsNullOrEmpty(relation.subject)) name += $" - {relation.subject}";
        }

        name = self.helper.format_proposal_number(id);
        link = nav.admin_url($"proposals/list_proposals/{id}");
        break;
      }
      case "tasks" or "task":
      {
        if (relation != null)
        {
          id = relation.id;
          name = relation.name;
        }
        else
        {
          id = relation.id;
          name = relation.name;
        }

        link = nav.admin_url($"tasks/view/{id}");
        break;
      }
      case "staff":
      {
        if (relation != null)
        {
          id = relation.staff_id;
          name = $"{relation.firstname} {relation.lastname}";
        }
        else
        {
          id = relation.staff_id;
          name = $"{relation.firstname} {relation.lastname}";
        }

        link = nav.admin_url($"profile/{id}");
        break;
      }
      case "project":
      {
        if (relation != null)
        {
          id = relation.id;
          name = relation.name;
          clientId = relation.client_id;
        }
        else
        {
          id = relation.id;
          name = relation.name;
          clientId = relation.client_id;
        }

        name = $"#{id} - {name} - {self.helper.get_company_name(clientId)}";
        link = nav.admin_url($"projects/view/{id}");
        break;
      }
    }

    return self.hooks.apply_filters("relation_values", new
    {
      id,
      name,
      link,
      addedfrom,
      subtext,
      type
    });
  }

  /**
   * Function used to render <option> for relation
   * This function will do all the necessary checking and return the options
   * @param  mixed data
   * @param  string  type   rel_type
   * @param  string rel_id rel_id
   * @return string
   */
  public static List<object> init_relation_options(this HelperBase helper, List<RelationValues> data, string type, int rel_id = 0)
  {
    var (self, db) = getInstance();

    var _data = new List<object>();

    var has_permission_projects_view = helper.has_permission("projects", "", "view");
    var has_permission_customers_view = helper.has_permission("customers", "", "view");
    var has_permission_contracts_view = helper.has_permission("contracts", "", "view");
    var has_permission_invoices_view = helper.has_permission("invoices", "", "view");
    var has_permission_estimates_view = helper.has_permission("estimates", "", "view");
    var has_permission_expenses_view = helper.has_permission("expenses", "", "view");
    var has_permission_proposals_view = helper.has_permission("proposals", "", "view");
    var is_admin = helper.is_admin();
    var projects_model = self.model.projects_model();
    foreach (var relation in data)
    {
      var relation_values = self.navigation.get_relation_values(relation, type);
      switch (type)
      {
        case "project":
        {
          if (!has_permission_projects_view)
            if (!projects_model.is_member(relation_values.id) && rel_id != relation_values.id)
              continue;
          break;
        }
        case "lead":
        {
          if (!helper.has_permission("leads", "", "view"))
            if (relation.assigned != helper.get_staff_user_id() && relation.addedfrom != helper.get_staff_user_id() && relation.is_public != true && rel_id != relation_values.id)
              continue;
          break;
        }
        case "customer" when !has_permission_customers_view && !helper.have_assigned_customers() && rel_id != relation_values.id:
          continue;
        case "customer":
        {
          if (helper.have_assigned_customers() && rel_id != relation_values.id && !has_permission_customers_view)
            if (!helper.is_customer_admin(relation_values.id))
              continue;
          break;
        }
        case "contract" when !has_permission_contracts_view && rel_id != relation_values.id && relation_values.addedfrom != helper.get_staff_user_id():
        case "invoice" when !has_permission_invoices_view && rel_id != relation_values.id && relation_values.addedfrom != helper.get_staff_user_id():
        case "estimate" when !has_permission_estimates_view && rel_id != relation_values.id && relation_values.addedfrom != helper.get_staff_user_id():
        case "expense" when !has_permission_expenses_view && rel_id != relation_values.id && relation_values.addedfrom != helper.get_staff_user_id():
        case "proposal" when !has_permission_proposals_view && rel_id != relation_values.id && relation_values.addedfrom != helper.get_staff_user_id():
          continue;
      }

      _data.Add(relation_values);
    }

    _data = self.hooks.apply_filters("init_relation_options", _data, compact(("type", rel_id)));
    return _data;
  }
}
