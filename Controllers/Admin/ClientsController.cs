using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;

namespace Service.Controllers.Admin;

[ApiController]
[Route("api/admin/clients")]
public class ClientsController(ILogger<MyControllerBase> logger, MyInstance self, MyContext db) : AdminControllerBase(logger, self, db)
{
  [HttpGet]
  public IActionResult index([FromQuery] string token)
  {
    if (!db.has_permission("customers", "", "view"))
      if (!db.have_assigned_customers() && !db.has_permission("customers", "", "create"))
        return access_denied("customers");
    var contracts_model = self.contracts_model(db);
    data.contract_types = contracts_model.get_contract_types();
    var clients_model = self.clients_model(db);
    data.groups = clients_model.get_groups();
    data.title = label("clients");
    var proposals_model = self.proposals_model(db);
    data.proposal_statuses = proposals_model.get_statuses();
    var invoices_model = self.invoices_model(db);
    data.invoice_statuses = invoices_model.get_statuses();
    var estimates_model = self.estimates_model(db);
    data.estimate_statuses = estimates_model.get_statuses();
    var projects_model = self.projects_model(db);
    data.project_statuses = projects_model.get_project_statuses();
    data.customer_admins = clients_model.get_customers_admin_unique_ids();
    data.contacts_logged_in_today = null;
    var whereContactsLoggedIn = "";
    if (!db.has_permission("customers", "", "view"))
    {
      var where_in = db.CustomerAdmins.Where(x => x.StaffId == db.get_staff_user_id()).Select(x => x.CustomerId).ToList();
      var rows = clients_model.get_contacts(x => x.LastLogin == DateTime.Now, x => where_in.Contains(x.UserId));
      data.contacts_logged_in_today = rows;
    }

    data.countries = clients_model.get_clients_distinct_countries();
    return MakeResult(data);
  }
}
