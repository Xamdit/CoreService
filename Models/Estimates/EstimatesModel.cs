using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Entities.Dto;
using Service.Framework.Helpers.Entities.Tools;
using Service.Helpers;
using Service.Helpers.Sale;
using Service.Helpers.Sms;
using Service.Helpers.Tags;
using Service.Models.Client;
using Service.Models.Contracts;
using Service.Models.Invoices;
using Service.Models.Payments;
using Service.Models.Projects;
using Service.Models.Tasks;
using File = Service.Entities.File;
using static Service.Helpers.Template.TemplateHelper;

namespace Service.Models.Estimates;

public class EstimatesModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  // private List<int> statuses => hooks.apply_filters("before_set_estimate_statuses", new List<int> { 1, 2, 5, 3, 4 });
  private List<string> shipping_fields = new() { "shipping_street", "shipping_city", "shipping_city", "shipping_state", "shipping_zip", "shipping_country" };
  private ProjectsModel projects_model = self.projects_model(db);
  private EmailScheduleModel email_schedule_model = self.email_schedule_model(db);
  private PaymentModesModel payment_modes_model = self.payment_modes_model(db);
  private CurrenciesModel currencies_model = self.currencies_model(db);
  private InvoicesModel invoices_model = self.invoices_model(db);
  private ClientsModel clients_model = self.clients_model(db);
  private EstimateRequestModel estimate_request_model = self.estimate_request_model(db);
  private TasksModel tasks_model = self.tasks_model(db);

  /**
   * Get unique sale agent for estimates / Used for filters
   * @return array
   */
  public List<Estimate> get_sale_agents()
  {
    var rows = db.Estimates
      .Include(x => x.SaleAgentNavigation)
      .Where(x => x.SaleAgent != 0)
      // .Select(x => new { x.SaleAgent, FullName = x.SaleAgentNavigation.FirstName + " " + x.SaleAgentNavigation.LastName })
      // .Distinct(x => x.SaleAgentNavigation.Id)
      .ToList();
    return rows;
  }

  public List<int> get_statuses()
  {
    return hooks.apply_filters("before_set_estimate_statuses", new List<int> { 1, 2, 5, 3, 4 });
  }


  /**
   * Get estimates
   * @param mixed  id estimate id
   * @param array  where perform where
   * @return mixed
   */
  public List<Estimate> get(Expression<Func<Estimate, bool>> condition)
  {
    // this.db.Estimates
    //   .OrderBy(x => x.Number);

    return db.Estimates
      .Include(x => x.Currency)
      .Where(condition)
      .OrderByDescending(x => x.Number)
      .ThenByDescending(x => x.Date)
      .ToList();
  }

  public (Estimate estimate, List<FileResult> temp, List<Itemable> items, ScheduledEmail? scheduled_email) get(int id, Expression<Func<Estimate, bool>> condition)
  {
    var estimate = db.Estimates
      .Include(x => x.Currency)
      .Include(x => x.Project)
      .Where(condition)
      .FirstOrDefault(x => x.Id == id);
    if (estimate == null)
      return (null, null, null, null)!;

    // estimate.visible_attachments_to_customer_found = false;
    var temp = get_attachments(id)
      .Select(attachment =>
      {
        var output = new FileResult
        {
          attachment = attachment
        };
        if (output.attachment.VisibleToCustomer) return output;
        output.visible_attachments_to_customer_found = true;
        return output;
      })
      .ToList();

    var items = db.get_items_by_type("estimate", id);
    estimate.Project ??= projects_model.get(x => x.Id == estimate.ProjectId).FirstOrDefault();
    estimate.Client = clients_model.get(x => x.Id == estimate.ClientId).FirstOrDefault();

    if (estimate.Client != null)
      estimate.Client = new Entities.Client { Company = estimate.DeletedCustomerName };
    var scheduled_email = email_schedule_model.get(id, "estimate");
    return (estimate, temp, items, scheduled_email);
  }

  public bool clear_signature(int id)
  {
    var estimate = db.Estimates.FirstOrDefault(x => x.Id == id);
    if (estimate == null) return false;
    db.Estimates
      .Where(x => x.Id == id)
      .Update(x => new Estimate { Signature = null });
    if (!string.IsNullOrEmpty(estimate.Signature)) unlink($"{get_upload_path_by_type("estimate")}{id}/{estimate.Signature}");
    return true;
  }

  /**
   * Convert estimate to invoice
   * @param mixed  id estimate id
   * @return mixed     New invoice ID
   */
  public int convert_to_invoice(int id, bool client = false, bool draft_invoice = false)
  {
    // Recurring invoice date is okey lets convert it to new invoice
    var _estimate = get(x => x.Id == id).First();

    var new_invoice_data = new InvoiceOption();
    if (draft_invoice)
      new_invoice_data.save_as_draft = true;
    new_invoice_data.ClientId = _estimate.ClientId!.Value;
    new_invoice_data.ProjectId = _estimate.ProjectId;
    new_invoice_data.Number = db.get_option<int>("next_invoice_number");
    new_invoice_data.Date = today();
    new_invoice_data.DueDate = today();
    // if (!db.get_option_compare("invoice_due_after", 0))
    //   new_invoice_data.DueDate = _d(date('Y-m-d', strtotime('.' + db.get_option("invoice_due_after") + ' DAY', strtotime(date('Y-m-d')))));
    new_invoice_data.ShowQuantityAs = _estimate.ShowQuantityAs;
    new_invoice_data.Currency = _estimate.Currency;
    new_invoice_data.Subtotal = _estimate.Subtotal;
    new_invoice_data.Total = _estimate.Total;
    new_invoice_data.Adjustment = _estimate.Adjustment;
    new_invoice_data.DiscountPercent = _estimate.DiscountPercent;
    new_invoice_data.DiscountTotal = _estimate.DiscountTotal;
    new_invoice_data.DiscountType = _estimate.DiscountType;
    new_invoice_data.SaleAgentNavigation = _estimate.SaleAgentNavigation;
    // Since version 1.0.6
    new_invoice_data.BillingStreet = clear_textarea_breaks(_estimate.BillingStreet);
    new_invoice_data.BillingCity = _estimate.BillingCity;
    new_invoice_data.BillingState = _estimate.BillingState;
    new_invoice_data.BillingZip = _estimate.BillingZip;
    new_invoice_data.BillingCountry = _estimate.BillingCountry;
    new_invoice_data.ShippingStreet = clear_textarea_breaks(_estimate.ShippingStreet);
    new_invoice_data.ShippingCity = _estimate.ShippingCity;
    new_invoice_data.ShippingState = _estimate.ShippingState;
    new_invoice_data.ShippingZip = _estimate.ShippingZip;
    new_invoice_data.ShippingCountry = _estimate.ShippingCountry;

    if (_estimate.IncludeShipping)
      new_invoice_data.IncludeShipping = true;

    new_invoice_data.ShowShippingOnInvoice = _estimate.ShowShippingOnEstimate;
    new_invoice_data.Terms = db.get_option("predefined_terms_invoice");
    new_invoice_data.ClientNote = db.get_option("predefined_clientnote_invoice");
    // Set to unpaid status automatically
    new_invoice_data.Status = 1;
    new_invoice_data.AdminNote = "";

    var modes = payment_modes_model.Find(x => x.ExpensesOnly != 1);
    new_invoice_data.AllowedPaymentModes = string.Join(",", modes.Select(mode => mode.SelectedByDefault != 0 ? mode.Id : 0).Where(x => x > 0).ToList());
    new_invoice_data.newitems.Clear();
    var custom_fields_items = db.get_custom_fields("items");
    var key = 0;
    var items = new List<Itemable>();
    // foreach (var item in _estimate.items)
    foreach (var item in items)
    {
      new_invoice_data.newitems[key].Description = item.Description;
      new_invoice_data.newitems[key].LongDescription = clear_textarea_breaks(item.LongDescription);
      new_invoice_data.newitems[key].Qty = item.Qty;
      new_invoice_data.newitems[key].Unit = item.Unit;
      new_invoice_data.newitems[key].TaxNames = new List<string>();
      var taxes = db.get_estimate_item_taxes(item.Id);
      new_invoice_data.newitems[key].TaxNames = taxes.Select(tax => tax.TaxName).ToList();
      new_invoice_data.newitems[key].Rate = item.Rate;
      new_invoice_data.newitems[key].Order = item.ItemOrder!.Value;
      foreach (var cf in custom_fields_items)
      {
        new_invoice_data.newitems[key].CustomFields.Items[cf.Id] = db.get_custom_field_value(item.Id, cf.Id, "items", false);
        if (!defined("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST"))
          self.helper.define("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST", true);
      }

      key++;
    }

    id = invoices_model.add(new_invoice_data);
    if (id <= 0) return id;

    // Customer accepted the estimate and is auto converted to invoice
    if (!db.is_staff_logged_in())
    {
      db.SalesActivities
        .Where(x => x.RelType == "invoice" && x.RelId == id)
        .Delete();
      invoices_model.log(id, "invoice_activity_auto_converted_from_estimate", true, JsonConvert.SerializeObject(new string[]
      {
        $"<a href='{self.navigation.admin_url($"estimates/list_estimates/{_estimate.Id}")}'>{db.format_estimate_number(_estimate.Id)}</a>"
      }));
    }

    // For all cases update addefrom and sale agent from the invoice
    // May happen staff is not logged in and these values to be 0
    db.Invoices
      .Where(x => x.Id == id)
      .Update(x => new Invoice { AddedFrom = _estimate.AddedFrom, SaleAgent = _estimate.SaleAgent });

    // Update estimate with the new invoice data and set to status accepted
    db.Estimates
      .Where(x => x.Id == _estimate.Id)
      .Update(x => new Estimate
      {
        InvoicedDate = today(),
        InvoiceId = id,
        Status = 4
      });

    if (self.helper.is_custom_fields_smart_transfer_enabled())
    {
      var cfEstimates = db.CustomFields
        .Where(x => x.FieldTo == "invoice" && x.Active)
        .ToList();
      foreach (var field in cfEstimates)
      {
        var tmpSlug = split_string(field.Slug, "_", 2);
        if (tmpSlug.Count() == 2) continue;
        var cfTransfer = db.CustomFieldsValues
          .Include(
            x =>
              x.Field
          )
          .FirstOrDefault(x => x.RelId == id &&
                               x.FieldId == field.Id &&
                               x.FieldTo == "invoice" &&
                               x.Field!.Slug.StartsWith($"invoice_{tmpSlug[1]}") &&
                               x.Field.Type == field.Type &&
                               x.Field.Options == field.Options &&
                               x.Field.Active);

        // Don't make mistakes
        // Only valid if 1 result returned
        // + if field names similarity is equal or more then CUSTOM_FIELD_TRANSFER_SIMILARITY%
        // if (cfTransfer != 1 || self.helper.similarity(field.Name, cfTransfer[0].Name) * 100 < CUSTOM_FIELD_TRANSFER_SIMILARITY) continue;
        if (cfTransfer != null || self.helper.similarity(field.Name, cfTransfer.Field.Name) * 100 < globals<int>("CUSTOM_FIELD_TRANSFER_SIMILARITY")) continue;
        var value = db.get_custom_field_value(_estimate.Id, field.Id, "estimate", false);

        if (string.IsNullOrEmpty(value)) continue;
        db.CustomFieldsValues.Add(new CustomFieldsValue
        {
          RelId = id,
          FieldId = cfTransfer.Id,
          FieldTo = "invoice",
          Value = value
        });
        db.SaveChanges();
      }
    }

    if (client == false)
      log_estimate_activity(_estimate.Id, "estimate_activity_converted", false, JsonConvert.SerializeObject(new[]
      {
        $"<a href='{self.navigation.admin_url($"invoices/list_invoices/{id}")}'>{db.format_invoice_number(id)}</a>"
      }));

    hooks.do_action("estimate_converted_to_invoice", new { invoice_id = id, estimate_id = _estimate.Id });


    return id;
  }

  /**
   * Copy estimate
   * @param mixed  id estimate id to copy
   * @return mixed
   */
  public int copy(int id)
  {
    var _estimates = get(x => x.Id == id);
    var _estimate = _estimates.First();
    if (_estimate == null) return 0;
    var new_estimate_data = new EstimateDto();
    new_estimate_data.ClientId = _estimate.ClientId;
    new_estimate_data.ProjectId = _estimate.ProjectId;
    new_estimate_data.Number = db.get_option<int>("next_estimate_number");
    // new_estimate_data.Date = today();
    new_estimate_data.Date = DateTime.Now;
    if (_estimate.ExpiryDate != null && !db.get_option_compare("estimate_due_after", 0))
      new_estimate_data.ExpiryDate = DateTime.Now.AddDays(db.get_option<int>("estimate_due_after"));
    new_estimate_data.ShowQuantityAs = _estimate.ShowQuantityAs;
    new_estimate_data.Currency = _estimate.Currency;
    new_estimate_data.Subtotal = _estimate.Subtotal;
    new_estimate_data.Total = _estimate.Total;
    new_estimate_data.AdminNote = _estimate.AdminNote;
    new_estimate_data.Adjustment = _estimate.Adjustment;
    new_estimate_data.DiscountPercent = _estimate.DiscountPercent;
    new_estimate_data.DiscountTotal = _estimate.DiscountTotal;
    new_estimate_data.DiscountType = _estimate.DiscountType;
    new_estimate_data.Terms = _estimate.Terms;
    new_estimate_data.SaleAgent = _estimate.SaleAgentNavigation.Id;
    new_estimate_data.ReferenceNo = _estimate.ReferenceNo;
    // Since version 1.0.6
    new_estimate_data.BillingStreet = clear_textarea_breaks(_estimate.BillingStreet);
    new_estimate_data.BillingCity = _estimate.BillingCity;
    new_estimate_data.BillingState = _estimate.BillingState;
    new_estimate_data.BillingZip = _estimate.BillingZip;
    new_estimate_data.BillingCountry = _estimate.BillingCountry;
    new_estimate_data.ShippingStreet = clear_textarea_breaks(_estimate.ShippingStreet);
    new_estimate_data.ShippingCity = _estimate.ShippingCity;
    new_estimate_data.ShippingState = _estimate.ShippingState;
    new_estimate_data.ShippingZip = _estimate.ShippingZip;
    new_estimate_data.ShippingCountry = _estimate.ShippingCountry;
    if (_estimate.IncludeShipping)
      new_estimate_data.IncludeShipping = _estimate.IncludeShipping;
    new_estimate_data.ShowShippingOnEstimate = _estimate.ShowShippingOnEstimate;
    // Set to unpaid status automatically
    new_estimate_data.Status = 1;
    new_estimate_data.ClientNote = _estimate.ClientNote;
    new_estimate_data.AdminNote = "";
    new_estimate_data.newitems = new List<ItemableOption>();
    var custom_fields_items = db.get_custom_fields("items");
    var key = 1;
    var items = new List<Itemable>();
    foreach (var item in items)
    {
      new_estimate_data.newitems[key].Description = item.Description;
      new_estimate_data.newitems[key].LongDescription = clear_textarea_breaks(item.LongDescription!);
      new_estimate_data.newitems[key].Qty = item.Qty;
      new_estimate_data.newitems[key].Unit = item.Unit;
      var taxes = db.get_estimate_item_taxes(item.Id);
      new_estimate_data.newitems[key].TaxNames = taxes.Select(x => x.TaxName).ToList();
      new_estimate_data.newitems[key].Rate = item.Rate;
      new_estimate_data.newitems[key].Order = item.ItemOrder!.Value;
      foreach (var cf in custom_fields_items)
      {
        new_estimate_data.newitems[key].CustomFields.Items[cf.Id] = db.get_custom_field_value(item.Id, cf.Id, "items", false);
        if (!defined("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST"))
          self.helper.define("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST", true);
      }

      key++;
    }

    id = add(new_estimate_data);
    if (id == 0) return 0;
    var custom_fields = db.get_custom_fields("estimate");
    foreach (var field in custom_fields)
    {
      var value = db.get_custom_field_value(_estimate.Id, field.Id, "estimate", false);
      if (string.IsNullOrEmpty(value)) continue;
      db.CustomFieldsValues.Add(new CustomFieldsValue
      {
        RelId = id,
        FieldId = field.Id,
        FieldTo = "estimate",
        Value = value
      });
    }

    var temps = db.get_tags_in(_estimate.Id, "estimate");
    var tags = db.Taggables.Where(x => temps.Select(taggable => taggable.RelType).Contains(x.RelType)).ToList();
    db.handle_tags_save(tags, id, "estimate");
    log_activity($"Copied Estimate {db.format_estimate_number(_estimate.Id)}");
    return id;
  }


  public List<object> get_estimates_total(Dictionary<string, object> data)
  {
    var statuses = get_statuses();
    var hasPermissionView = db.has_permission("estimates", "view");

    int currencyId;
    if (data.ContainsKey("currency"))
    {
      currencyId = Convert.ToInt32(data["currency"]);
    }
    else if (data.ContainsKey("customer_id") && !string.IsNullOrEmpty(data["customer_id"].ToString()))
    {
      currencyId = clients_model.get_customer_default_currency(Convert.ToInt32(data["customer_id"]));

      if (currencyId == 0) currencyId = currencies_model.get_base_currency().Id;
    }
    else if (data.ContainsKey("project_id") && !string.IsNullOrEmpty(data["project_id"].ToString()))
    {
      currencyId = projects_model.get_currency(Convert.ToInt32(data["project_id"])).Id;
    }
    else
    {
      currencyId = currencies_model.get_base_currency().Id;
    }

    var currency = db.get_currency(currencyId);


    Expression<Func<Estimate, bool>> whereClauses = estimate => true;

    if (data.ContainsKey("customer_id") && !string.IsNullOrEmpty(data["customer_id"].ToString()))
      whereClauses = whereClauses.And(x => x.ClientId == (int)data["customer_id"]);

    if (data.ContainsKey("project_id") && !string.IsNullOrEmpty(data["project_id"].ToString()))
      whereClauses = whereClauses.And(x => x.ProjectId == (int)data["project_id"]);

    if (!hasPermissionView)
    {
      var staffCondition = db.get_estimates_where_sql_for_staff(db.get_staff_user_id());
      whereClauses.And(staffCondition);
    }

    var statusQuery = CreateCondition<Estimate>(x => true);
    foreach (var status in statuses)
    {
      statusQuery = statusQuery.And(x => x.Status == status && x.CurrencyId == currencyId);
      statusQuery = data.ContainsKey("years") && data["years"] is List<int> years && years.Any()
        ? statusQuery.And(x => years.Contains(x.Date.Year))
        : statusQuery.And(x => x.Date.Year == DateTime.UtcNow.Year);
    }

    var result = db.Estimates.Where(statusQuery).ToList();
    var response = new List<object>();
    var index = 1;

    foreach (var row in result)
    foreach (var property in row.GetType().GetProperties())
    {
      response.Add(new
      {
        Total = property.GetValue(row),
        currency.Symbol,
        CurrencyName = currency.Name,
        Status = property.Name
      });
      index++;
    }

    response.Add(new { CurrencyId = currencyId });
    return response;
  }

  /**
   * Insert new estimate to database
   * @param array  data invoiec data
   * @return mixed - false if not insert, estimate ID if succes
   */
  // public int add((Estimate estimate, EstimateOption option) data)
  public int add(EstimateDto data)
  {
    data.DateCreated = DateTime.Now;
    data.AddedFrom = db.get_staff_user_id();
    data.Prefix = db.get_option("estimate_prefix");
    data.NumberFormat = db.get_option<int>("estimate_number_format");

    var save_and_send = data.saveAndSend;
    var estimateRequestID = 0;
    estimateRequestID = data.EstimateRequestId;
    var custom_fields = data.CustomFields;
    data.Hash = uuid();
    var tags = data.Tags.Any() ? data.Tags : new List<Taggable>();
    var items = data.newitems;

    var _estimate = map_shipping_columns(estimate(data));

    _estimate.BillingStreet = _estimate.BillingStreet.Trim().nl2br();
    _estimate.ShippingStreet = _estimate.ShippingStreet.Trim().nl2br();


    var hook = hooks.apply_filters("before_estimate_added", new { data = _estimate, items });
    _estimate = hook.data;
    items = hook.items;
    db.Estimates.Add(_estimate);
    db.SaveChanges();
    var insert_id = data.Id;
    if (insert_id <= 0) return 0;
    // Update next estimate number in settings
    db.Options
      .Where(x => x.Name == "next_estimate_number")
      .Update(x => new Option { Value = x.Value + 1 });
    db.SaveChanges();

    if (estimateRequestID != 0)
    {
      var completedStatus = estimate_request_model.get_status_by_flag("completed");
      estimate_request_model.update_request_status(new EstimateRequest()
      {
        Id = estimateRequestID,
        Status = completedStatus.Id
      });
    }


    self.helper.handle_custom_fields_post(insert_id, custom_fields);

    db.handle_tags_save(tags, insert_id, "estimate");

    foreach (var item in items)
    {
      var itemid = db.add_new_sales_item_post(item, insert_id, "'estimate");
      if (itemid > 0)
        db.maybe_insert_post_item_tax(itemid, convert<PostItem>(item), insert_id, "'estimate");
    }

    db.update_sales_total_tax_column(insert_id, "'estimate", "estimates");
    log_estimate_activity(insert_id, "estimate_activity_created");

    hooks.do_action("after_estimate_added", insert_id);

    // if (save_and_send == true) this.send_estimate_to_client(insert_id, '', true, '', true);
    if (save_and_send!.Value)
      send_estimate_to_client(insert_id, attachpdf: true, manually: true);

    return insert_id;
  }

  /**
   * Get item by id
   * @param mixed  id item id
   * @return object
   */
  public Itemable? get_estimate_item(int id)
  {
    var row = db.Itemables.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Update estimate data
   * @param array  data estimate data
   * @param mixed  id estimateid
   * @return boolean
   */
  public bool update(EstimateDto data)
  {
    var id = data.Id;
    var affectedRows = 0;


    var original_estimate = get(x => x.Id == id).First();

    var original_status = original_estimate.Status;

    var original_number = original_estimate.Number;

    var original_number_formatted = db.format_estimate_number(id);

    var save_and_send = data.saveAndSend.HasValue && data.saveAndSend.Value;

    var items = data.newitems;
    var newitems = data.newitems;
    if (data.CustomFields.Any())
    {
      var custom_fields = data.CustomFields;
      if (self.helper.handle_custom_fields_post(id, custom_fields))
        affectedRows++;
    }

    if (data.Tags.Any())
      if (db.handle_tags_save(data.Tags, id, "estimate"))
        affectedRows++;

    data.BillingStreet = data.BillingStreet.Trim();
    data.BillingStreet = data.BillingStreet.nl2br();

    data.ShippingStreet = data.ShippingStreet.Trim();
    data.ShippingStreet = data.ShippingStreet.nl2br();

    var _estimate = map_shipping_columns(estimate(data));
    var hook = hooks.apply_filters("before_estimate_updated", new
    {
      data,
      items,
      newitems,
      removed_items = data.RemovedItems.Any() ? data.RemovedItems : default
    }, id);

    data = hook.data;
    // var items = hook.newitems;
    items.Clear();
    // var newitems = hook.newitems;
    newitems.Clear();
    data.RemovedItems = hook.RemovedItems;

    // Delete items checked to be removed from database
    data.RemovedItems.ForEach(remove_item_id =>
    {
      var original_item = get_estimate_item(remove_item_id.Id);
      if (!db.handle_removed_sales_item_post(remove_item_id.Id, "estimate")) return;
      affectedRows++;
      log_estimate_activity(
        id,
        "invoice_estimate_activity_removed_item",
        false,
        JsonConvert.SerializeObject(new[] { original_item.Description })
      );
    });


    var affected_rows = db.Estimates
      .Where(x => x.Id == id)
      .Update(x => data);

    if (affected_rows > 0)
    {
      // Check for status change
      if (original_status != data.Status)
      {
        log_estimate_activity(original_estimate.Id, "not_estimate_status_updated", false, JsonConvert.SerializeObject(new[]
        {
          "<original_status>" + original_status + "</original_status>",
          "<new_status>" + data.Status + "</new_status>"
        }));
        if (data.Status == 2)
          db.Estimates
            .Where(x => x.Id == id)
            .Update(x => new Estimate { Sent = 1, DateSend = today() });
      }

      if (original_number != data.Number)
        log_estimate_activity(original_estimate.Id, "estimate_activity_number_changed", false, JsonConvert.SerializeObject(new[]
        {
          original_number_formatted,
          db.format_estimate_number(original_estimate.Id)
        }));

      affectedRows++;
    }

    foreach (var item in items)
    {
      var original_item = get_estimate_item(item.Id);

      if (db.update_sales_item_post(item.Id, item, "item_order")) affectedRows++;
      if (db.update_sales_item_post(item.Id, item, "unit")) affectedRows++;
      if (db.update_sales_item_post(item.Id, item, "rate"))
      {
        log_estimate_activity(id, "invoice_estimate_activity_updated_item_rate", false, JsonConvert.SerializeObject(new[]
        {
          original_item.Rate,
          item.Rate
        }));
        affectedRows++;
      }

      if (db.update_sales_item_post(item.Id, item, "qty"))
      {
        log_estimate_activity(id, "invoice_estimate_activity_updated_qty_item", false, JsonConvert.SerializeObject(new
        {
          Description = item.Description,
          Qty = original_item.Qty,
          itemQty = item.Qty
        }));
        affectedRows++;
      }

      if (db.update_sales_item_post(item.Id, item, "description"))
      {
        log_estimate_activity(id, "invoice_estimate_activity_updated_item_short_description", false, JsonConvert.SerializeObject(new[]
        {
          original_item.Description,
          item.Description
        }));
        affectedRows++;
      }

      if (db.update_sales_item_post(item.Id, item, "long_description"))
      {
        log_estimate_activity(id, "invoice_estimate_activity_updated_item_long_description", false, JsonConvert.SerializeObject(new[]
        {
          original_item.LongDescription,
          item.LongDescription
        }));
        affectedRows++;
      }

      // if (item.CustomFields.Items.Any() && self.helper.handle_custom_fields_post(item.Id, item.CustomFields))
      //   affectedRows++;
      // self.helper.handle_custom_fields_post(item.Id, item.CustomFields);

      if (!item.TaxNames.Any())
      {
        if (db.delete_taxes_from_item(item.Id, "estimate")) affectedRows++;
      }
      else
      {
        var item_taxes = db.get_estimate_item_taxes(item.Id);
        var _item_taxes_names = item_taxes.Select(x => x.TaxName);
        var i = 0;
        foreach (var _item_tax in _item_taxes_names)
        {
          // if (!in_array(_item_tax, item.ItemTaxes.TaxNames))
          if (!item.ItemTaxes.Any(x => x.TaxName == _item_tax))
          {
            db.ItemTaxes.Where(x => x.Id == item_taxes[i].Id).Delete();
            if (affected_rows > 0)
              affectedRows++;
          }

          i++;
        }

        if (db.maybe_insert_post_item_tax(item.Id, convert<PostItem>(item), id, "estimate")) affectedRows++;
      }


      newitems
        // .Where(newitems => newitems.new_item_added = db.add_new_sales_item_post(itemable(newitems), id, "estimate"))
        .Select(newitem => db.add_new_sales_item_post(itemable(newitem), id, "estimate"))
        .ToList()
        .ForEach(x =>
        {
          db.maybe_insert_post_item_tax(x, convert<PostItem>(item), id, "estimate");
          log_estimate_activity(id, "invoice_estimate_activity_added_item", false, JsonConvert.SerializeObject(new[]
          {
            item.Description
          }));
          affectedRows++;
        });


      if (affectedRows > 0) db.update_sales_total_tax_column(id, "estimate", "estimates");
      if (save_and_send) send_estimate_to_client(id, "", true, "", true);
      if (affectedRows <= 0) return false;
      hooks.do_action("after_estimate_updated", id);
    }

    return true;
  }

  public (bool is_success, Invoice? invoice) mark_action_status(int action, int id, bool client = false)
  {
    db.Estimates.Where(x => x.Id == id).Update(x => new Estimate { Status = action });
    var affected_rows = db.SaveChanges();
    var notifiedUsers = new List<int>();
    if (affected_rows <= 0) return (false, null);

    var estimate = get(x => x.Id == id).FirstOrDefault();
    if (client)
    {
      var staff_estimate = db.Staff
        .Where(x => x.Id == estimate.AddedFrom || x.Id == estimate.SaleAgent)
        .ToList();

      var invoiceid = 0;
      var invoiced = false;

      var contact_id = !db.is_client_logged_in()
        ? self.helper.get_primary_contact_user_id(estimate.ClientId)
        : db.get_contact_user_id();

      if (action == 4)
      {
        if (db.get_option_compare("estimate_auto_convert_to_invoice_on_client_accept", 1))
        {
          invoiceid = convert_to_invoice(id, true);

          if (invoiceid > 0)
          {
            invoiced = true;
            var invoice = invoices_model.get(invoiceid);
            log_estimate_activity(id, "estimate_activity_client_accepted_and_converted", true, JsonConvert.SerializeObject(new[]
            {
              $"<a href='{self.navigation.admin_url($"invoices/list_invoices/{invoiceid}")}'>{db.format_invoice_number(invoice.Id)}</a>"
            }));
          }
        }
        else
        {
          log_estimate_activity(id, "estimate_activity_client_accepted", true);
        }

        // Send thank you email to all contacts with permission estimates
        var contacts = clients_model.get_contacts(x => x.Id == estimate.ClientId, x => x.Active && x.EstimateEmails == 1);
        contacts.ForEach(contact => { db.send_mail_template("estimate_accepted_to_customer", estimate, contact); });
        staff_estimate.ForEach(member =>
        {
          var notified = db.add_notification(new Notification
          {
            FromCompany = true,
            ToUserId = member.Id,
            Description = "not_estimate_customer_accepted",
            Link = $"estimates/list_estimates/{id}",
            AdditionalData = JsonConvert.SerializeObject(new[]
            {
              db.format_estimate_number(estimate.Id)
            })
          });
          if (notified != null) notifiedUsers.Add(member.Id);
          db.send_mail_template("estimate_accepted_to_staff", estimate, member.Email, contact_id);
        });
        db.pusher_trigger_notification(notifiedUsers);
        hooks.do_action("estimate_accepted", id);
        // return new { invoiced, invoiceid };
        return (invoiceid > 0, null);
      }

      if (action != 3) return (false, null);

      staff_estimate.ForEach(member =>
      {
        var notified = db.add_notification(new Notification
        {
          FromCompany = true,
          ToUserId = member.Id,
          Description = "not_estimate_customer_declined",
          Link = $"estimates/list_estimates/{id}",
          AdditionalData = JsonConvert.SerializeObject(new[]
          {
            db.format_estimate_number(estimate.Id)
          })
        });
        if (notified != null) notifiedUsers.Add(member.Id);
        // Send staff email notification that customer declined estimate
        db.send_mail_template("estimate_declined_to_staff", estimate, member.Email, contact_id);
      });

      db.pusher_trigger_notification(notifiedUsers);
      log_estimate_activity(id, "estimate_activity_client_declined", true);
      hooks.do_action("estimate_declined", id);
      //return new { invoiced, invoiceid };
      return (invoiceid > 0, null);
    }

    if (action == 2)
      db.Estimates
        .Where(x => x.Id == id)
        .Update(x => new Estimate
        {
          Sent = 1,
          DateSend = today()
        });
    // Admin marked estimate
    log_estimate_activity(id, "estimate_activity_marked", false, JsonConvert.SerializeObject(new[]
    {
      $"<status>{action}</status>"
    }));

    return (true, null);
  }

  /**
   * Get estimate attachments
   * @param mixed  estimate_id
   * @param string  id attachment id
   * @return mixed
   */
  public File get_attachment(int estimate_id, int id)
  {
    var query = db.Files.AsQueryable();
    query = query.Where(x => x.Id == id);
    query = query.Where(x => x.RelType == "estimate");
    var result = query.FirstOrDefault();
    return result;
  }

  public List<File> get_attachments(int estimate_id)
  {
    var query = db.Files.AsQueryable();
    query = query.Where(x => x.RelId == estimate_id);
    query = query.Where(x => x.RelType == "estimate");
    var result = query.ToList();
    return result;
  }

  /**
   *  Delete estimate attachment
   * @param mixed  id attachmentid
   * @return  boolean
   */
  public bool delete_attachment(int id)
  {
    var attachment = db.Files
      .FirstOrDefault(x => x.Id == id && x.RelType == "estimate");
    var deleted = false;
    if (attachment == null) return deleted;

    if (string.IsNullOrEmpty(attachment.External))
      unlink($"{get_upload_path_by_type("estimate")}{attachment.RelId}/{attachment.FileName}");

    db.Files.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity($"Estimate Attachment Deleted [EstimateID: {attachment.RelId}]");
    }

    if (!is_dir(get_upload_path_by_type("estimate") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(get_upload_path_by_type("estimate") + attachment.RelId);
    if (!other_attachments.Any())
      // okey only index.html so we can delete the folder also
      delete_dir(get_upload_path_by_type("estimate") + attachment.RelId);


    return deleted;
  }

  /**
   * Delete estimate items and all connections
   * @param mixed  id estimateid
   * @return boolean
   */
  public bool delete(int id, bool simpleDelete = false)
  {
    if (db.get_option_compare("delete_only_on_last_estimate", 1) && simpleDelete == false)
      if (!db.is_last_estimate(id))
        return false;
    var estimate = get(x => x.Id == id).FirstOrDefault();
    if (estimate.InvoiceId.HasValue && simpleDelete == false)
      // return new { is_invoiced_estimate_delete_error = true };
      return true;
    hooks.do_action("before_estimate_deleted", id);
    var number = db.format_estimate_number(id);
    clear_signature(id);


    db.Estimates.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;

    if (!string.IsNullOrEmpty(estimate.ShortLink))
      db.app_archive_short_link(estimate.ShortLink);

    if (db.get_option_compare("estimate_number_decrement_on_delete", 1) && simpleDelete == false)
    {
      var current_next_estimate_number = db.get_option<int>("next_estimate_number");
      if (current_next_estimate_number > 1)
      {
        // Decrement next estimate number to
        db.Options
          .Where(x => x.Name == "next_estimate_number")
          .Update(x => new Option { Value = Convert.ToString(Convert.ToInt32(x.Value) - 1) });
        db.SaveChanges();
      }
    }

    if (db.Proposals.Any(x => x.EstimateId == id))
    {
      var proposal = db.Proposals.FirstOrDefault(x => x.Id == estimate.Id);
      db.Proposals
        .Where(x => x.EstimateId == estimate.Id)
        .Update(x => new Proposal
        {
          EstimateId = null,
          DateConverted = null
        });
      db.SaveChanges();
    }

    delete_tracked_emails(id, "estimate");


    db.CustomFieldsValues
      .Where(x =>
        db.Itemables.Where(x => x.RelType == "estimate" && x.RelId == id).Select(x => x.Id).Contains(x.RelId) &&
        x.FieldTo == "estimate")
      .Delete();

    db.Notes.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.ViewsTrackings.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.Taggables.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.Reminders.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.Itemables.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.ItemTaxes.Where(x => x.RelId == id && x.RelType == "estimate").Delete();
    db.SalesActivities.Where(x => x.RelId == id && x.RelType == "estimate").Delete();

    // Delete the custom field values
    db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "estimate").Delete();

    var attachments = get_attachments(id);
    attachments.ForEach(attachment => delete_attachment(attachment.Id));
    db.ScheduledEmails.Where(x => x.RelId == id && x.RelType == "estimate").Delete();

    // Get related tasks
    db.Tasks
      .Where(x => x.RelId == id && x.RelType == "estimate")
      .ToList()
      .ForEach(task => tasks_model.delete_task(task.Id));

    if (simpleDelete == false) log_activity($"Estimates Deleted [Number: {number}]");
    hooks.do_action("after_estimate_deleted", id);
    return true;
  }

  /**
   * Set estimate to sent when email is successfuly sended to client
   * @param mixed  id estimateid
   */
  public void set_estimate_sent(int id, List<string> emails_sent = default)
  {
    db.Estimates
      .Where(x => x.Id == id)
      .Update(x => new Estimate
      {
        Sent = 1,
        DateSend = today()
      });

    log_estimate_activity(id, "invoice_estimate_activity_sent_to_client", false, JsonConvert.SerializeObject(new[]
    {
      $"<custom_data>{string.Join(", ", emails_sent)}</custom_data>"
    }));

    // Update estimate status to sent
    db.Estimates
      .Where(x => x.Id == id)
      .Update(x => new Estimate
      {
        Status = 2
      });

    db.ScheduledEmails
      .Where(x =>
        x.RelId == id &&
        x.RelType == "estimate"
      )
      .Delete();
  }

  /**
   * Send expiration reminder to customer
   * @param mixed  id estimate id
   * @return boolean
   */
  public bool send_expiry_reminder(int id)
  {
    var estimate = get(x => x.Id == id).First();
    var estimate_number = db.format_estimate_number(estimate.Id);
    self.helper.set_mailing_constant();
    var pdf = self.library.estimate_pdf(estimate);
    var attach = pdf.Output($"{estimate_number}.pdf");
    var emails_sent = new List<string>();
    var sms_sent = false;
    var sms_reminder_log = new List<string>();

    // For all cases update this to prevent sending multiple reminders eq on fail
    db.Estimates
      .Where(x => x.Id == estimate.Id)
      .Update(x => new Estimate
      {
        IsExpiryNotified = true
      });

    var contacts = clients_model.get_contacts(x => x.Id == estimate.ClientId, x => x.Active && x.EstimateEmails != 0);
    contacts.ForEach(contact =>
    {
      var template = this.mail_template("estimate_expiration_reminder", estimate, contact);
      var merge_fields = template.get_merge_fields();
      template.add_attachment(new MailAttachment
      {
        attachment = attach,
        filename = $"{estimate_number}.pdf".Replace("/", "-"),
        type = "application/pdf"
      });

      if (template.send())
        emails_sent.Add(contact.Email);

      if (!self.helper.can_send_sms_based_on_creation_date(estimate.DateCreated)
          || !self.library.app_sms().trigger(globals("SMS_TRIGGER_ESTIMATE_EXP_REMINDER"), contact.PhoneNumber, merge_fields)) return;
      sms_sent = true;
      sms_reminder_log.Add($"{contact.FirstName} ({contact.PhoneNumber})");
    });


    switch (emails_sent.Count)
    {
      case <= 0 when !sms_sent:
        return false;
      case > 0:
        log_estimate_activity(id, "not_expiry_reminder_sent", false, JsonConvert.SerializeObject(new[]
        {
          $"<custom_data>{string.Join(", ", emails_sent)}</custom_data>"
        }));
        break;
    }

    if (sms_sent)
      log_estimate_activity(id, "sms_reminder_sent_to", false, JsonConvert.SerializeObject(new[]
      {
        string.Join(", ", sms_reminder_log)
      }));

    return true;
  }

  /**
   * Send estimate to client
   * @param mixed  id estimateid
   * @param string  template email template to sent
   * @param boolean  attachpdf attach estimate pdf or not
   * @return boolean
   */
  public bool send_estimate_to_client(int id, string template_name = "", bool attachpdf = true, string cc = "", bool manually = false)
  {
    var estimate = get(x => x.Id == id).First();

    if (string.IsNullOrEmpty(template_name))
      template_name = estimate.Sent == 0 ? "estimate_send_to_customer" : "estimate_send_to_customer_already_sent";

    var estimate_number = db.format_estimate_number(estimate.Id);

    var emails_sent = new List<string>();
    var send_to = new List<int>();

    // Manually is used when sending the estimate via add/edit area button Save & Send
    if (!defined("CRON") && manually == false)
      send_to = split_int(self.input.post("sent_to"), ",");
    else if (!string.IsNullOrEmpty(globals("scheduled_email_contacts")))
      send_to = split_int(globals("scheduled_email_contacts"), ",");
    else
      clients_model.get_contacts(
          x =>
            x.Id == estimate.ClientId,
          x =>
            x.Active && x.EstimateEmails == 1
        )
        .ForEach(contact => { send_to.Add(contact.Id); });

    var status_auto_updated = false;
    var status_now = estimate.Status;

    if (send_to.Any())
    {
      var i = 0;

      // Auto update status to sent in case when user sends the estimate is with status draft
      if (status_now == 1)
      {
        db.Estimates.Where(x => x.Id == estimate.Id).Update(x => new Estimate { Status = 2 });
        status_auto_updated = true;
      }

      var _pdf_estimate = new Estimate();
      var attach = false;
      if (attachpdf)
      {
        _pdf_estimate = get(x => x.Id == estimate.Id).First();
        self.helper.set_mailing_constant();
        var pdf = self.library.estimate_pdf(_pdf_estimate);
        attach = pdf.Output($"{estimate_number}.pdf");
      }

      foreach (var contact_id in send_to)
      {
        if (contact_id != 0)
        {
          // Send cc only for the first contact
          if (!string.IsNullOrEmpty(cc) && i > 0) cc = "";

          var contact = clients_model.get_contact(contact_id);

          if (contact == null) continue;

          var template = this.mail_template(template_name, estimate, contact, cc);

          if (attachpdf)
          {
            var hook = hooks.apply_filters("send_estimate_to_customer_file_name", new
            {
              file_name = (estimate_number + ".pdf").Replace("/", "-"),
              estimate = _pdf_estimate
            });

            template.add_attachment(new MailAttachment
            {
              attachment = attach,
              filename = hook.file_name,
              type = "application/pdf"
            });
          }

          if (template.send()) emails_sent.Add(contact.Email);
        }

        i++;
      }
    }
    else
    {
      return false;
    }

    if (emails_sent.Any())
    {
      set_estimate_sent(id, emails_sent);
      hooks.do_action("estimate_sent", id);
      return true;
    }

    if (status_auto_updated)
      // Estimate not send to customer but the status was previously updated to sent now we need to revert back to draft
      db.Estimates.Where(x => x.Id == estimate.Id).Update(x => new Estimate { Status = 1 });

    return false;
  }

  /**
   * All estimate activity
   * @param mixed  id estimateid
   * @return array
   */
  public List<SalesActivity> get_estimate_activity(int id)
  {
    var output = db.SalesActivities
      .Where(x => x.RelId == id && x.RelType == "estimate")
      .OrderBy(x => x.Date)
      .ToList();
    return output;
  }

  /**
   * Log estimate activity to database
   * @param mixed  id estimateid
   * @param string  description activity description
   */
  public void log_estimate_activity(int id, string description = "", bool client = false, string additional_data = "")
  {
    var staffid = $"{db.get_staff_user_id()}";
    var full_name = db.get_staff_full_name(db.get_staff_user_id());
    if (is_cron())
    {
      staffid = "[CRON]";
      full_name = "[CRON]";
    }
    else if (client)
    {
      staffid = null;
      full_name = "";
    }

    db.SalesActivities.Add(new SalesActivity
    {
      Description = description,
      Date = DateTime.Now,
      RelId = id,
      RelType = "estimate",
      StaffId = staffid,
      FullName = full_name,
      AdditionalData = additional_data
    });
    db.SaveChanges();
  }

  /**
   * Updates pipeline order when drag and drop
   * @param mixe  data  _POST data
   * @return void
   */
  public void update_pipeline(Estimate data)
  {
    mark_action_status(data.Status, data.Id);
    // AbstractKanban::updateOrder(data.Order, 'pipeline_order', 'estimates', data.Status);
  }

  /**
   * Get estimate unique year for filtering
   * @return array
   */
  public List<Estimate> get_estimates_years()
  {
    return db.Estimates
      // .Select(x => new { year = x.Date.Year })
      .Distinct()
      // .OrderByDescending(x => x.year)
      .ToList();
  }

  private Estimate map_shipping_columns(Estimate data)
  {
    if (!data.IncludeShipping)
    {
      // foreach (var _s_field in shipping_fields.Where(_s_field => isset(data, _s_field)))
      //   data[_s_field] = null;
      data.ShowShippingOnEstimate = true;
      data.IncludeShipping = false;
    }
    else
    {
      data.IncludeShipping = true;
    }

    return data;
  }

  public object do_kanban_query(int status, string search = "", int page = 1, Sorting sort = default, bool count = false)
  {
    // _deprecated_function("Estimates_model::do_kanban_query", "2.9.2", "EstimatesPipeline class");

    var kanBan = new EstimatesPipeline(self, status)
      .search(search)
      .page(page)
      .sortBy(sort.Sort ?? null, sort.SortBy ?? null);

    return count
      ? kanBan.countAll()
      : kanBan.get();
  }
}
