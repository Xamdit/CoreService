using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Entities.Dto;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Helpers;
using Service.Helpers.Proposals;
using Service.Helpers.Sale;
using Service.Helpers.Sms;
using Service.Helpers.Tags;
using Service.Models.Client;
using Service.Models.Estimates;
using Service.Models.Leads;
using Service.Models.Projects;
using File = Service.Entities.File;
using static Service.Helpers.Template.TemplateHelper;

namespace Service.Models.Proposals;

public class ProposalsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private readonly List<int> statuses = hooks.apply_filters("before_set_proposal_statuses", new List<int> { 6, 4, 1, 5, 2, 3 });
  private EstimateRequestModel estimate_request_model = self.estimate_request_model(db);
  private ProjectsModel projects_model = self.projects_model(db);
  private ClientsModel clients_model = self.clients_model(db);
  private LeadsModel leads_model = self.leads_model(db);
  private bool _copy = false;

  public List<int> get_statuses()
  {
    return statuses;
  }

  public List<Proposal> get_sale_agents()
  {
    var rows = db.Proposals
      .Where(x => x.Assigned != 0)
      .GroupBy(x => x.Assigned)
      .Select(g => g.First()) // Select the first Proposal in each group
      .ToList();

    return rows;
  }


  public List<Proposal> get_proposals_years()
  {
    var rows = db.Proposals
      .GroupBy(x => x.Date.Year)
      .Select(g => g.First()) // Select the first Proposal in each group
      .Distinct()
      .ToList();
    return rows;
  }


  public int? add(DataSet<Proposal> dataset)
  {
    dataset.Data.AllowComments = dataset.Data.AllowComments > 0 ? 1 : 0;
    var save_and_send = convert<bool>(dataset["save_and_send"]);

    var tags = dataset.Tags.Any() ? dataset.Tags : new List<Taggable>();
    var custom_fields = new List<CustomField>();
    if (dataset.custom_fields.Any())
      custom_fields = dataset.custom_fields;

    var estimateRequestID = convert<int>(dataset["estimate_request_id"]);

    dataset.Data.Address = dataset.Data.Address.Trim().nl2br();
    dataset.Data.DateCreated = DateTime.Now;
    dataset.Data.AddedFrom = db.get_staff_user_id();
    dataset.Data.Hash = uuid();


    var items = new List<ItemableOption>();
    if (dataset.NewItems.Any())
      items = dataset.NewItems;

    if (_copy == false)
      dataset.Data.Content = "{proposal_items}";

    if (dataset.Data.RelId != 0 && !string.IsNullOrEmpty(dataset.Data.RelType) && dataset.Data.RelType != "customer")
      dataset.Data.ProjectId = null;
    var hook = new
    {
      Data = dataset.Data,
      Items = items
    };
    hook = hooks.apply_filters("before_create_proposal", hook);
    dataset.Data = hook.Data;
    items = hook.Items;
    var result = db.Proposals.Add(dataset.Data);
    if (result.IsAdded()) return null;
    if (estimateRequestID > 0)
    {
      var completedStatus = estimate_request_model.get_status_by_flag("completed");
      estimate_request_model.update_request_status(new EstimateRequest
      {
        Id = estimateRequestID,
        Status = completedStatus.Id
      });
    }

    var insert_id = result.Entity.Id;
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(insert_id, custom_fields);

    db.handle_tags_save(tags, insert_id, "proposal");
    items.ForEach(item =>
    {
      var itemid = db.add_new_sales_item_post(item, insert_id, "proposal");
      if (itemid > 0)
        db.maybe_insert_post_item_tax(itemid, convert<PostItem>(item), insert_id, "proposal");
    });

    foreach (var item in items.Where(item => item.Id == db.add_new_sales_item_post(item, insert_id, "proposal")))
      db.maybe_insert_post_item_tax(item.Id, convert<PostItem>(item), insert_id, "proposal");

    foreach (var item in items)
    {
      var itemid = db.add_new_sales_item_post(item, insert_id, "proposal");
      //key => item
      if (itemid > 0)
        db.maybe_insert_post_item_tax(itemid, convert<PostItem>(item), insert_id, "proposal");
    }

    var proposal = get(x => x.Id == insert_id);
    if (proposal.Assigned != 0)
      if (proposal.Assigned != db.get_staff_user_id())
      {
        var notified = db.add_notification(new Notification
        {
          Description = "not_proposal_assigned_to_you",
          ToUserId = proposal.Assigned,
          FromUserId = db.get_staff_user_id(),
          Link = $"proposals/list_proposals/{insert_id}",
          AdditionalData = JsonConvert.SerializeObject(new[]
          {
            proposal.Subject
          })
        });
        if (notified)
          db.pusher_trigger_notification(proposal.Assigned);
      }

    if (dataset.Data.RelType == "lead")
      leads_model.log_lead_activity(dataset.Data.RelId, "not_lead_activity_created_proposal", false, JsonConvert.SerializeObject(new[]
      {
        $"<a href='{self.navigation.admin_url($"proposals/list_proposals/{insert_id}")}' target='_blank'>{dataset.Data.Subject}</a>"
      }));
    db.update_sales_total_tax_column(insert_id, "proposal", "proposals");
    log_activity($"New Proposal Created [ID: {insert_id}]");
    if (save_and_send) send_proposal_to_email(insert_id);
    hooks.do_action("proposal_created", insert_id);
    return insert_id;
  }

  /**
   * Update proposal
   * @param  mixed data _POST data
   * @param  mixed id   proposal id
   * @return boolean
   */
  public async Task<bool> update(DataSet<Proposal> data, int id)
  {
    var affectedRows = 0;


    var current_proposal = get(x => x.Id == id);
    var save_and_send = data.SaveAndSend;

    if (string.IsNullOrEmpty(data.Data.RelType))
    {
      data.Data.RelId = 0;
      data.Data.RelType = "";
    }
    else
    {
      if (data.Data.RelId != 0)
      {
        data.Data.RelId = 0;
        data.Data.RelType = "";
      }
    }

    if (data.custom_fields.Any())
    {
      var custom_fields = data.custom_fields;
      if (self.helper.handle_custom_fields_post(id, custom_fields))
        affectedRows++;
    }

    var items = new List<ItemableOption>();
    if (data.NewItems.Any()) items = data.NewItems;

    var NewItems = new List<ItemableOption>();
    if (data.NewItems.Any()) NewItems = data.NewItems;

    if (data.Tags.Any())
      if (db.handle_tags_save(data.Tags, id, "proposal"))
        affectedRows++;

    data.Data.Address = data.Data.Address.Trim().nl2br();
    var hook = new
    {
      id,
      data,
      items,
      NewItems,
      removed_items = data.removed_items.Any()
        ? data.removed_items
        : new List<Proposal>()
    };
    hooks.apply_filters("before_proposal_updated", hook);

    data = hook.data;
    data.removed_items = hook.removed_items;
    NewItems = hook.NewItems;
    // var items                 = hook["items"];
    items.Clear();

    // Delete items checked to be removed from database
    affectedRows += data.removed_items.Select(x => x.Id).Count(remove_item_id => db.handle_removed_sales_item_post(remove_item_id, "proposal"));

    var affected_rows = db.Proposals.Where(x => x.Id == id).Update(x => data);
    if (affected_rows > 0)
    {
      affectedRows++;
      var proposal_now = get(x => x.Id == id);
      if (current_proposal.Assigned != proposal_now.Assigned)
        if (proposal_now.Assigned != db.get_staff_user_id())
        {
          var notified = db.add_notification(new Notification
          {
            Description = "not_proposal_assigned_to_you",
            ToUserId = proposal_now.Assigned,
            FromUserId = db.get_staff_user_id(),
            Link = $"proposals/list_proposals/{id}",
            AdditionalData = JsonConvert.SerializeObject(new[]
            {
              proposal_now.Subject
            })
          });
          if (notified) db.pusher_trigger_notification(proposal_now.Assigned);
        }
    }

    // foreach (var itemTaxes in items.Select(x => x.ItemTaxes).ToList())
    foreach (var item in items.ToList())
    {
      if (db.update_sales_item_post(item.Id, item)) affectedRows++;
      if (item.CustomFields.Items.Any())
      {
        var temp = db.CustomFields.Where(x => item.CustomFields.Items.Contains(x.Name)).ToList();
        if (self.helper.handle_custom_fields_post(item.Id, temp))
          affectedRows++;
      }

      var names = item.CustomFields.Items.ToList();
      names.ForEach(name =>
      {
        if (!string.IsNullOrEmpty(name))
        {
          if (db.delete_taxes_from_item(item.Id, "proposal"))
            affectedRows++;
        }
        else
        {
          var item_taxes = self.helper.get_proposal_item_taxes(item.Id);
          var _item_taxes_names = item_taxes
            .Where(x => x.TaxName == name)
            .Select(x => db.ItemTaxes.FirstOrDefault(y => y.TaxName == x.TaxName))
            .ToList();
          db.ItemTaxes.RemoveRange(_item_taxes_names);
          db.SaveChanges();
          db.maybe_insert_post_item_tax(item.Id, convert<PostItem>(item), id, "proposal");
        }
      });
    }

    foreach (var item in NewItems)
    {
      var new_item_added = db.add_new_sales_item_post(convert<Itemable>(item), id, "proposal");
      if (new_item_added <= 0) continue;
      db.maybe_insert_post_item_tax(new_item_added, convert<PostItem>(item), id, "proposal");
      affectedRows++;
    }

    if (affectedRows > 0)
    {
      db.update_sales_total_tax_column(id, "proposal", "proposals");
      log_activity($"Proposal Updated [ID:{id}]");
    }

    if (save_and_send) send_proposal_to_email(id);
    if (affectedRows <= 0) return false;
    hooks.do_action("after_proposal_updated", id);
    return true;
  }

  /**
   * Get proposals
   * @param  mixed id proposal id OPTIONAL
   * @return mixed
   */
  // public bool get( id = "", where = [], for_editor = false)
  public Proposal get(Expression<Func<Proposal, bool>> condition, bool for_editor = false)
  {
    var id = db.ExtractIdFromCondition(condition) ?? 0;
    var query = db.Proposals
      .Include(x => x.Currency)
      .Where(condition)
      .ToList();
    if (db.is_client_logged_in())
      query
        .Where(x => string.IsNullOrEmpty(x.State));
    var rows = query.ToList();
    // rows.Where(x => x.Attachments == get_attachments(x.Id));
    var proposal = query.ToList().First();
    if (proposal != null) return proposal;
    var attachments = get_attachments(id);
    var items = db.get_items_by_type("proposal", id);
    var visible_attachments_to_customer_found = attachments.Any(x => x.VisibleToCustomer);

    if (proposal.ProjectId.HasValue)
      proposal.Project = projects_model.get(x => x.Id == proposal.ProjectId).First();
    if (for_editor == false)
      proposal = self.helper.parse_proposal_content_merge_fields(proposal);
    return proposal;
  }

  public bool clear_signature(int id)
  {
    var proposal = db.Proposals
      .FirstOrDefault(x => x.Id == id);

    if (proposal == null) return false;

    db.Proposals.Where(x => x.Id == id).Update(x => new Proposal { Signature = null });
    if (!string.IsNullOrEmpty(proposal.Signature)) unlink($"{get_upload_path_by_type("proposal")}{id}/{proposal.Signature}");

    return true;
  }

  public bool update_pipeline(object data)
  {
    //  this.mark_action_status(data.status, data.ProposalId);
    // AbstractKanban::updateOrder(data.order, "pipeline_order", "proposals", data.status);
    return true;
  }

  public List<File> get_attachments(int proposal_id, int? id = null)
  {
    // If is passed id get return only 1 attachment
    var query = db.Files.Where(x => x.RelType == "proposal");
    query = id.HasValue
      ? query.Where(x => x.Id == id)
      : query.Where(x => x.RelId == proposal_id);
    query.Where(x => x.RelType == "proposal");
    var result = query.ToList();

    return result;
  }

  /**
   *  Delete proposal attachment
   * @param   mixed id  attachmentid
   * @return  boolean
   */
  public bool delete_attachment(int id)
  {
    var attachment = get_attachments(0, id).FirstOrDefault();
    var deleted = false;
    if (attachment == null) return deleted;
    if (string.IsNullOrEmpty(attachment.External)) unlink($"{get_upload_path_by_type("proposal")}{attachment.RelId}/{attachment.FileName}");

    var affected_rows = db.Files.Where(x => x.Id == attachment.Id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity($"Proposal Attachment Deleted [ID: {attachment.RelId}]");
    }

    if (!is_dir(get_upload_path_by_type("proposal") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(get_upload_path_by_type("proposal") + attachment.RelId);
    if (!other_attachments.Any())
      // okey only index.html so we can delete the folder also
      delete_dir(get_upload_path_by_type("proposal") + attachment.RelId);

    return deleted;
  }

  /**
   * Add proposal comment
   * @param mixed  data   _POST comment data
   * @param boolean client is request coming from the client side
   */
  public bool add_comment(ProposalComment data, bool client = false)
  {
    if (db.is_staff_logged_in())
      client = false;


    data.DateCreated = DateTime.Now;
    if (client == false)
      data.StaffId = db.get_staff_user_id();
    data.Content = data.Content.nl2br();
    var result = db.ProposalComments.Add(data);
    if (!result.IsAdded()) return false;
    var insert_id = result.Entity.Id;
    // var   proposal =  get(data.ProposalId);
    var proposal = get(x => x.Id == data.ProposalId);

    // No notifications client when proposal is with draft status
    if (proposal.Status == 6 && client == false) return true;

    if (client)
    {
      // Get creator and assigned


      var staff_proposal = db.Staff.Where(x => x.Id == proposal.AddedFrom || x.Id == proposal.Assigned).ToList();
      var notifiedUsers = new List<int>();
      notifiedUsers = staff_proposal
        .Select(member =>
        {
          var notified = db.add_notification(new Notification
          {
            Description = "not_proposal_comment_from_client",
            ToUserId = member.Id,
            FromCompany = true,
            FromUserId = 0,
            Link = $"proposals/list_proposals/{data.ProposalId}",
            AdditionalData = JsonConvert.SerializeObject(new[] { proposal.Subject })
          });
          if (notified) return member.Id;
          var template = this.mail_template("proposal_comment_to_staff", proposal.Id, member.Email);
          var merge_fields = template.get_merge_fields();
          template.send();
          self.library.app_sms().trigger(globals("SMS_TRIGGER_PROPOSAL_NEW_COMMENT_TO_STAFF"), member.PhoneNumber, merge_fields);
          return 0;
        })
        .ToList()
        .Where(x => x > 0)
        .ToList();
      db.pusher_trigger_notification(notifiedUsers.ToArray());
    }
    else
    {
      // Send email/sms to client that admin commented
      var template = this.mail_template("proposal_comment_to_customer", proposal);
      var merge_fields = template.get_merge_fields();
      template.send();
      self.library.app_sms().trigger(globals("SMS_TRIGGER_PROPOSAL_NEW_COMMENT_TO_CUSTOMER"), proposal.Phone, merge_fields);
    }

    return true;
  }

  public bool edit_comment(ProposalComment data, int id)
  {
    data.Content = data.Content.nl2br();
    var result = db.Proposals.Where(x => x.Id == id).Update(x => data);
    return result > 0;
  }

  /**
   * Get proposal comments
   * @param  mixed id proposal id
   * @return array
   */
  public List<ProposalComment> get_comments(int id)
  {
    var rows = db.ProposalComments.Where(x => x.ProposalId == id).OrderBy(x => x.DateCreated).ToList();
    return rows;
  }

  /**
   * Get proposal single comment
   * @param  mixed id  comment id
   * @return object
   */
  public ProposalComment? get_comment(int id)
  {
    var row = db.ProposalComments.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Remove proposal comment
   * @param  mixed id comment id
   * @return boolean
   */
  public bool remove_comment(int id)
  {
    var comment = get_comment(id);

    var affected_rows = db.ProposalComments.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    log_activity($"Proposal Comment Removed [ProposalID:{comment.ProposalId}, Comment Content: {comment.Content}]");

    return true;
  }

  /**
   * Copy proposal
   * @param  mixed id proposal id
   * @return mixed
   */
  public int? copy(int id)
  {
    var proposal = get(x => x.Id == id, true);
    var not_copy_fields = new List<string>
    {
      "addedfrom",
      "id",
      "datecreated",
      "hash",
      "status",
      "invoice_id",
      "estimate_id",
      "is_expiry_notified",
      "date_converted",
      "signature",
      "acceptance_firstname",
      "acceptance_lastname",
      "acceptance_email",
      "acceptance_date",
      "acceptance_ip"
    };
    // var fields      =   list_fields<Proposal>( );
    var insert_data = new DataSet<Proposal>();
    // foreach (var field in fields )
    //   if (!in_array(field, not_copy_fields))
    //     insert_data[field] = proposal.field;
    insert_data.Data.AddedFrom = db.get_staff_user_id();
    insert_data.Data.DateCreated = DateTime.Now;
    insert_data.Data.Date = DateTime.Now;
    insert_data.Data.Status = 6;
    insert_data.Data.Hash = uuid();

    // in case open till is expired set new 7 days starting from current date
    if (insert_data.Data.OpenTill.HasValue && !db.get_option_compare("proposal_due_after", 0))
    {
      insert_data.Data.OpenTill = DateTime.Now.AddDays(db.get_option<int>("proposal_due_after"));
    }
    else if (insert_data.Data.OpenTill.HasValue)
    {
      var dDiff = self.helper.diff(DateTime.Now, insert_data.Data.OpenTill.Value);
      insert_data.Data.OpenTill = DateTime.Now.AddDays(dDiff.TotalDays);
    }

    insert_data.NewItems.Clear();
    var custom_fields_items = db.get_custom_fields("items");
    var key = 1;
    var items = new List<DataSet<Proposal>>();
    // var items = new List<ProposalDto>();
    // foreach (var item in proposal.NewItems)
    foreach (var item in items)
    {
      insert_data.NewItems[key].Description = (string)item["description"];
      insert_data.NewItems[key].LongDescription = clear_textarea_breaks((string)item["long_description"]);
      insert_data.NewItems[key].Qty = (int)item["qty"];
      insert_data.NewItems[key].Unit = (string)item["unit"];
      insert_data.NewItems[key].Names = new List<string>();
      var taxes = self.helper.get_proposal_item_taxes(item.Data.Id);
      foreach (var tax in taxes)
        insert_data.NewItems[key].TaxNames.Add(tax.TaxName);
      insert_data.NewItems[key].Rate = (double)item["rate"];
      insert_data.NewItems[key].Order = (int)item["item_order"];
      foreach (var cf in custom_fields_items)
      {
        insert_data.NewItems[key].CustomFields.Items[cf.Id] = db.get_custom_field_value(item.Data.Id, cf.Id, "items", false);

        if (!defined("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST")) self.helper.define("COPY_CUSTOM_FIELDS_LIKE_HANDLE_POST", true);
      }

      key++;
    }

    var insert_id = add(insert_data);

    if (!insert_id.HasValue) return null;
    var custom_fields = db.get_custom_fields("proposal");
    foreach (var field in custom_fields)
    {
      var value = db.get_custom_field_value(proposal.Id, field.Id, "proposal", false);
      if (value == "") continue;
      db.CustomFieldsValues.Add(new CustomFieldsValue
      {
        RelId = id,
        FieldId = field.Id,
        FieldTo = "proposal",
        Value = value
      });
    }

    var tags = db.get_tags_in(proposal.Id, "proposal");
    db.handle_tags_save(tags, id, "proposal");
    log_activity($"Copied Proposal {db.format_proposal_number(proposal.Id)}");
    return id;
  }

  /**
   * Take proposal action (change status) manually
   * @param  mixed status status id
   * @param  mixed  id     proposal id
   * @param  boolean client is request coming from client side or not
   * @return boolean
   */
  public bool mark_action_status(int status, int id, bool client = false)
  {
    var original_proposal = get(x => x.Id == id);

    var affected_rows = db.Proposals.Where(x => x.Id == id).Update(x => new Proposal { Status = status });

    if (affected_rows <= 0) return false;
    // Client take action
    if (client)
    {
      var revert = false;
      // Declined
      var message = string.Empty;
      if (status == 2)
        message = "not_proposal_proposal_declined";
      else if (status == 3)
        message = "not_proposal_proposal_accepted";
      // Accepted
      else
        revert = true;
      // This is protection that only 3 and 4 statuses can be taken as action from the client side
      if (revert)
      {
        db.Proposals.Where(x => x.Id == id).Update(x => new Proposal { Status = original_proposal.Status });
        return false;
      }

      // Get creator and assigned;

      var staff_proposal = db.Staff.Where(x => x.Id == original_proposal.AddedFrom || x.Id == original_proposal.Assigned).ToList();
      var notifiedUsers = new List<int>();
      notifiedUsers = staff_proposal.Select(x =>
        {
          var notified = db.add_notification(new Notification
          {
            FromCompany = true,
            ToUserId = x.Id,
            Description = message,
            Link = $"proposals/list_proposals/{id}",
            AdditionalData = JsonConvert.SerializeObject(new[] { db.format_proposal_number(id) })
          });
          return notified ? x.Id : 0;
        })
        .Where(x => x > 0)
        .ToList();
      db.pusher_trigger_notification(notifiedUsers);

      // Send thank you to the customer email template
      if (status == 3)
      {
        staff_proposal.ForEach(x => db.send_mail_template("proposal_accepted_to_staff", original_proposal, x.Email));
        db.send_mail_template("proposal_accepted_to_customer", original_proposal);
        hooks.do_action("proposal_accepted", id);
      }
      else
      {
        // Client declined send template to admin
        staff_proposal.ForEach(member => db.send_mail_template("proposal_declined_to_staff", original_proposal, member.Email));
        hooks.do_action("proposal_declined", id);
      }
    }

    else
    {
      // in case admin mark as open the the open till date is smaller then current date set open till date 7 days more
      if (original_proposal.OpenTill < DateTime.Now && status == 1)
      {
        var open_till = DateTime.Now.AddDays(7);
        db.Proposals
          .Where(x => x.Id == id)
          .Update(x => new Proposal { OpenTill = open_till });
      }
    }

    log_activity($"Proposal Status Changes [ProposalID:{id}, Status:{self.helper.format_proposal_status(status, "", false)},Client Action: {client}]");

    return true;
  }

  /**
   * Delete proposal
   * @param  mixed id proposal id
   * @return boolean
   */
  public bool delete(int id)
  {
    var tasks_model = self.tasks_model(db);
    hooks.do_action("before_proposal_deleted", id);

    clear_signature(id);
    var proposal = get(x => x.Id == id);
    var affected_rows = db.Proposals.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;

    if (!string.IsNullOrEmpty(proposal.ShortLink))
      db.app_archive_short_link(proposal.ShortLink);

    delete_tracked_emails(id, "proposal");
    db.ProposalComments.Where(x => x.ProposalId == id).Delete();
    // Get related tasks

    db.Tasks.Where(x => x.RelId == id && x.RelType == "proposal").ToList().ForEach(x => tasks_model.delete_task(x.Id));
    var attachments = get_attachments(id);
    attachments
      .Select(x => x.Id)
      .ToList()
      .ForEach(x => delete_attachment(x));

    db.Notes.Where(x => x.RelId == id && x.RelType == "proposal").Delete();
    db.CustomFieldsValues.Where(x =>
        db.Itemables.Where(y => y.RelId == id && y.RelType == "proposal").Select(z => z.Id).Contains(x.RelId) &&
        x.FieldTo == "proposal")
      .Delete();

    db.Itemables.Where(x => x.RelId == id && x.RelType == "proposal").Delete();
    db.ItemTaxes.Where(x => x.RelId == id && x.RelType == "proposal").Delete();
    db.Taggables.Where(x => x.RelId == id && x.RelType == "proposal").Delete();

    // Delete the custom field values
    db.CustomFieldsValues.Where(x => x.RelId == id && x.FieldTo == "proposal").Delete();
    db.Reminders.Where(x => x.RelId == id && x.RelType == "proposal").Delete();
    db.ViewsTrackings.Where(x => x.RelId == id && x.RelType == "proposal").Delete();
    log_activity($"Proposal Deleted [ProposalID:{id}]");
    hooks.do_action("after_proposal_deleted", id);

    return true;
  }

  /**
   * Get relation proposal data. Ex lead or customer will return the necesary db fields
   * @param  mixed rel_id
   * @param  string rel_type customer/lead
   * @return object
   */
  public Lead get_relation_data_values(int rel_id, string rel_type)
  {
    var data = new Lead();
    if (rel_type == "customer")
    {
      var _data = db.Clients.FirstOrDefault(x => x.Id == rel_id);
      var primary_contact_id = self.helper.get_primary_contact_user_id(rel_id);
      Contact contact = null;
      if (primary_contact_id > 0)
      {
        contact = clients_model.get_contact(primary_contact_id);
        data.Email = contact.Email;
      }

      data.PhoneNumber = _data.PhoneNumber;
      var is_using_company = false;
      var to = string.Empty;
      if (contact != null)
      {
        to = $"{contact.FirstName} {contact.LastName}";
      }
      else
      {
        if (!string.IsNullOrEmpty(_data.Company))
        {
          to = _data.Company;
          is_using_company = true;
        }
      }

      data.Company = _data.Company;
      data.Address = clear_textarea_breaks(_data.Address);
      data.Zip = _data.Zip;
      data.Country = _data.Country!.Id;
      data.State = _data.State;
      data.City = _data.City;

      var currency = 0;
      var default_currency = clients_model.get_customer_default_currency(rel_id);
      if (default_currency != 0) currency = default_currency;
    }
    else if (rel_type == "lead")
    {
      var _data = db.Leads.Where(x => x.Id == rel_id).FirstOrDefault();
      data.PhoneNumber = _data.PhoneNumber;
      var is_using_company = false;
      var to = string.Empty;
      if (string.IsNullOrEmpty(_data.Company))
      {
        to = _data.Name;
      }
      else
      {
        to = _data.Company;
        is_using_company = true;
      }

      data.Company = _data.Company;
      data.Address = _data.Address;
      data.Email = _data.Email;
      data.Zip = _data.Zip;
      data.Country = _data.Country;
      data.State = _data.State;
      data.City = _data.City;
    }

    return data;
  }

  /**
   * Sent proposal to email
   * @param  mixed  id        proposalid
   * @param  string  template  email template to sent
   * @param  boolean attachpdf attach proposal pdf or not
   * @return boolean
   */
  public bool send_expiry_reminder(int id)
  {
    var proposal = get(x => x.Id == id);

    // For all cases update this to prevent sending multiple reminders eq on fail

    db.Proposals.Where(x => x.Id == proposal.Id).Update(x => new Proposal { IsExpiryNotified = 1 });

    var template = this.mail_template("proposal_expiration_reminder", proposal);
    var merge_fields = template.get_merge_fields();

    template.send();

    // if (self.helper.can_send_sms_based_on_creation_date(proposal.DateCreated))
    //   sms_sent = this.app_sms.trigger(SMS_TRIGGER_PROPOSAL_EXP_REMINDER, proposal.Phone, merge_fields);
    var sms_sent = self.helper.can_send_sms_based_on_creation_date(proposal.DateCreated) && self.library.app_sms().trigger(globals("SMS_TRIGGER_PROPOSAL_EXP_REMINDER"), proposal.Phone, merge_fields);
    return sms_sent;
  }

  public bool send_proposal_to_email(int id, bool attachpdf = true, string cc = "")
  {
    // Proposal status is draft update to sent
    if (db.Proposals.Any(x => x.Id == id && x.Status == 6)) db.Proposals.Where(x => x.Id == id).Update(x => new Proposal { Status = 4 });
    var proposal = get(x => x.Id == id);
    var sent = db.send_mail_template("proposal_send_to_customer", proposal, attachpdf, cc);
    if (!sent) return false;

    // Set to status sent
    db.Proposals.Where(x => x.Id == id)
      .Update(x => new Proposal { Status = 4 });
    hooks.do_action("proposal_sent", id);
    return true;
  }

  public bool do_kanban_query(int status, string search = "", int page = 1, Sorting sort = default, bool count = false)
  {
    // _deprecated_function("Proposal_model::do_kanban_query", "2.9.2", "ProposalsPipeline class");

    var kanBan = new ProposalsPipeline(status)
      .Search(search)
      .Page(page)
      .SortBy(sort.Sort ?? null, sort.SortBy ?? null)
      .Build();

    // if (count) return kanBan.CountAll();
    // return kanBan.get();
    return false;
  }
}
