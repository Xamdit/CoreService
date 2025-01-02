// <copyright file="Index.razor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Linq.Expressions;
using Service.Entities;
using Service.Framework.Core.AppHook;
using Service.Framework.Helpers.Entities.Extras;
using Service.Helpers;
using Service.Models.Invoices;
using Service.Schemas.Ui.Entities;
using Task = System.Threading.Tasks.Task;

namespace Service.Components.Pages.Admin.Clients;

public class ManageRazor : AdminComponentBase
{
  public string Title = string.Empty;

  public string Group = string.Empty;
  public string ClassName = string.Empty;
  public List<CustomerGroup> groups { get; set; } = new();
  public List<ContractsType> contract_types { get; set; } = new();
  public List<InvoiceStatus> invoice_statuses { get; set; } = new();
  public List<EstimateStatus> estimate_statuses { get; set; } = new();
  public List<ProjectStatus> project_statuses { get; set; } = new();
  public List<ProposalStatus> proposal_statuses { get; set; } = new();
  public List<CustomerAdmin> customer_admins { get; set; } = new();
  public List<Country> countries { get; set; } = new();
  public bool customer_create { get; set; } = new();
  public bool customer_edit { get; set; } = new();
  public bool customers_view { get; set; } = new();
  public bool customers_delete { get; set; } = new();
  public List<Contact> contacts_logged_in_today { get; set; } = new();
  public string contactsTemplate { get; set; } = "";

  public List<object> table_data = new();

  public List<CustomField> custom_fields => db.get_custom_fields("customers", x => x.ShowOnTable);


  public Expression<Func<Entities.Client, bool>> conditionSummary()
  {
    if (!customers_view) return default;
    // Get the list of StaffIds first, outside the expression
    var staffUserId = self.helper.get_staff_user_id();
    var staffIds = db.CustomerAdmins
      .Where(x => x.StaffId == staffUserId)
      .Select(x => x.StaffId)
      .ToList();
    // Use the staffIds list in the expression
    Expression<Func<Entities.Client, bool>> where_summary = e =>
      e.Id == staffUserId && staffIds.Contains(e.Id);
    return where_summary;
  }

  public Expression<Func<Contact, bool>> conditionSummaryOfContact()
  {
    if (!customers_view) return default;
    // Get the list of StaffIds first, outside the expression
    var staffUserId = self.helper.get_staff_user_id();
    var staffIds = db.CustomerAdmins
      .Where(x => x.StaffId == staffUserId)
      .Select(x => x.StaffId)
      .ToList();
    // Use the staffIds list in the expression
    Expression<Func<Contact, bool>> where_summary = e =>
      e.Id == staffUserId && staffIds.Contains(e.Id);
    return where_summary;
  }

  protected override async Task OnInitializedAsync()
  {
    await base.OnInitializedAsync();
    customer_create = self.helper.has_permission("customers", 0, "create");
    customer_edit = self.helper.has_permission("customers", 0, "edit");
    customers_view = self.helper.has_permission("customers", 0, "edit");
    customers_delete = self.helper.has_permission("customers", 0, "delete");
  }

  protected override async Task OnAfterRenderAsync(bool firstRender)
  {
    await base.OnAfterRenderAsync(firstRender);
    if (!firstRender) return;
    // Client = new Root.Entities.Client();
    // ClassName = Client.Id == 0 ? "9" : "12";
    var uiHook = UiHook.Instance;
    var breadcrumb = new List<MenuItem>
    {
      new() { Title = "Home", Url = "/admin/dashboards" },
      new() { Title = "Clients" }
    };
    await uiHook.CallAsync("update_toolbar", breadcrumb);
  }

  public int ContactsCount()
  {
    return 0;
  }
}
