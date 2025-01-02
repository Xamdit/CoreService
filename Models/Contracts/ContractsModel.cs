using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Core.InputSet;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Framework.Library.Merger;
using Service.Helpers;
using Service.Helpers.Pdf;
using Service.Models.Client;
using Service.Models.Tasks;
using File = Service.Entities.File;


namespace Service.Models.Contracts;

public class ContractsModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  private ContractTypesModel contract_types_model = self.contract_types_model(db);
  private TasksModel tasks_model = self.tasks_model(db);
  private ClientsModel clients_model = self.clients_model(db);

  /**
     * Get contract/s
     * @param  mixed  id         contract id
     * @param  array   where      perform where
     * @param  boolean for_editor if for editor is false will replace the field if not will not replace
     * @return mixed
     */
  public List<(Contract contract, List<File> attachment)> get(Expression<Func<Contract, bool>> condition)
  {
    var query = db.Contracts
      .Include(x => x.ContractType)
      .Include(x => x.Client)
      .Where(condition);


    var rows = query.ToList();


    var output = rows
      .Select(contract =>
      {
        var attachment = get_contract_attachments(contract.Id);
        return (contract, attachment);
      })
      .ToList();

    return output;
  }


  /**
   * Select unique contracts years
   * @return array
   */
  public List<int> get_contracts_years()
  {
    var rows = db.Contracts
      .ToList()
      .Select(x => x.DateStart!.Value.Year)
      .Distinct()
      .ToList();
    return rows;
  }

  /**
   * @param  integer ID
   * @return object
   * Retrieve contract attachments from database
   */
  public List<File> get_contract_attachments(int? attachment_id = null, int? id = null)
  {
    if (attachment_id.HasValue) return db.Files.Where(x => x.Id == attachment_id.Value).ToList();
    var rows = db.Files
      .Include(x => x.RelType)
      .Include(x => x.RelId)
      .Where(x => x.RelId == id && x.RelType == "contract")
      .ToList();
    return rows;
  }

  /**
   * @param   array _POST data
   * @return  integer Insert ID
   * Add new contract
   */
  public int add((Contract contract, CustomField? customField) data)
  {
    data.contract.DateCreated = DateTime.Now;
    data.contract.AddedFrom = staff_user_id;


    if (!string.IsNullOrEmpty(data.contract.NotVisibleToClient) && data.contract.NotVisibleToClient == "on")
      data.contract.NotVisibleToClient = "true";
    else
      data.contract.NotVisibleToClient = "false";

    CustomField? custom_fields = null;

    if (data.customField != null)
    {
      custom_fields = data.customField;
      data.customField = null;
    }

    data.contract.Hash = Guid.NewGuid().ToString();

    data = hooks.apply_filters("before_contract_added", data);
    var result = db.Contracts.Add(data.contract);
    if (!result.IsAdded()) return 0;
    if (custom_fields != null)
      self.helper.handle_custom_fields_post(result.Entity.Id, custom_fields);
    hooks.do_action("after_contract_added", result.Entity.Id);
    log_activity($"New Contract Added [{data.contract.Subject}]");
    return result.Entity.Id;
  }

  /**
   * @param  array _POST data
   * @param  integer Contract ID
   * @return boolean
   */
  public bool update((Contract contract, CustomField? customField) data)
  {
    var id = data.contract.Id;
    var affectedRows = 0;
    var contract = db.Contracts.FirstOrDefault(x => x.Id == id);

    if (data.contract is { DateStart: not null, DateEnd: not null })
      if (data.contract.DateEnd.HasValue && data.contract.DateEnd != contract.DateEnd)
        data.contract.IsExpiryNotified = 0;

    data = hooks.apply_filters("before_contract_updated", data, id);
    if (data.customField != null)
    {
      var custom_fields = data.customField;
      if (self.helper.handle_custom_fields_post(id, custom_fields)) affectedRows++;
      data.customField = null;
    }

    var affected_rows = db.Contracts.Where(x => x.Id == id).Update(x => data);
    if (affected_rows <= 0) return affectedRows > 0;
    hooks.do_action("after_contract_updated", id);
    log_activity($"Contract Updated [{data.contract.Subject}]");
    return true;
  }

  public bool clear_signature(int id)
  {
    var contract = db.Contracts.FirstOrDefault(x => x.Id == id);
    if (contract == null) return false;
    db.Contracts
      .Where(x => x.Id == id)
      .Update(x => new Contract { Signature = "" });
    if (!string.IsNullOrEmpty(contract.Signature))
      unlink($"{get_upload_path_by_type("contract")}{id}/{contract.Signature}");
    return true;
  }

  /**
  * Add contract comment
  * @param mixed  data   _POST comment data
  * @param boolean client is request coming from the client side
  */
  public bool add_comment(ContractComment data, bool client = false)
  {
    if (db.is_staff_logged_in()) client = false;
    // if (data['action'])
    //   unset(data['action']);
    data.DateCreated = DateTime.Now;
    if (client == false) data.StaffId = staff_user_id;

    data.Content = data.Content.nl2br();

    var result = db.ContractComments.Add(data);
    if (!result.IsAdded()) return false;
    var (contract, attachment) = get(x => x.Id == data.ContractId).First();
    if ((contract.NotVisibleToClient == "1" || contract.Trash) && client == false) return true;
    if (client)
    {
      // Get creator
      var staff_contract = db.Staff.Where(x => x.Id == contract.AddedFrom).ToList();
      var notifiedUsers = staff_contract.Select(x =>
        {
          var notified = db.add_notification(new Notification
          {
            Description = "not_contract_comment_from_client",
            ToUserId = x.Id,
            FromCompany = true,
            FromUserId = 0,
            Link = $"contracts/contract/{data.ContractId}",
            AdditionalData = JsonConvert.SerializeObject(new { contract.Subject })
          });


          if (!notified) return 0;
          var template = mail_template("contract_comment_to_staff", contract, x);
          var merge_fields = template.get_merge_fields();
          template.send();
          return contract.Id;
          // this.app_sms.trigger(SMS_TRIGGER_CONTRACT_NEW_COMMENT_TO_STAFF, x.PhoneNumber, merge_fields);
        })
        .ToList();
      db.pusher_trigger_notification(notifiedUsers);
    }
    else
    {
      var contacts = clients_model.get_contacts(x => x.Id == contract.Client && x.Active && x.ContractEmails == 1, x => true);
      contacts.ForEach(contract =>
      {
        var template = mail_template("contract_comment_to_customer", contract);
        var merge_fields = template.get_merge_fields();
        template.send();
        // this.app_sms.trigger(SMS_TRIGGER_CONTRACT_NEW_COMMENT_TO_CUSTOMER, contact.PhoneNumber, merge_fields);
      });
    }

    return true;
  }

  public bool edit_comment(ContractComment data)
  {
    var affected_rows = db.ContractComments
      .Where(x => x.Id == data.Id)
      .Update(x => new ContractComment
      {
        Content = data.Content.nl2br()
      });

    return affected_rows > 0;
  }

  /**
   * Get contract comments
   * @param  mixed id contract id
   * @return array
   */
  public List<ContractComment> get_comments(int id)
  {
    var rows = db.ContractComments.Where(x => x.Id == id).OrderBy(x => x.DateCreated).ToList();
    return rows;
  }

  /**
   * Get contract single comment
   * @param  mixed id  comment id
   * @return object
   */
  public ContractComment get_comment(int id)
  {
    var row = db.ContractComments.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Remove contract comment
   * @param  mixed id comment id
   * @return boolean
   */
  public bool remove_comment(int id)
  {
    var comment = get_comment(id);
    var affected_rows = db.ContractComments.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    log_activity($"Contract Comment Removed [Contract ID:{comment.ContractId}, Comment Content: {comment.Content}]");
    return true;
  }

  public int copy(int id)
  {
    // var contract       = get(id, [], true);
    var (contract, attachment) = get(x => x.Id == id).First();
    var newContactData = contract;

    newContactData.Trash = false;
    newContactData.IsExpiryNotified = 0;
    newContactData.Signed = false;
    newContactData.MarkedAsSigned = 0;
    newContactData.Signature = null;
    newContactData = (Contract)TypeMerger.Merge(newContactData, get_acceptance_info_array<Contract>(true));
    if (contract.DateEnd.HasValue)
    {
      var dStart = contract.DateStart;
      var dEnd = contract.DateEnd;
      var dDiff = self.helper.diff(dStart.Value, dEnd.Value);
      newContactData.DateEnd = DateTime.Now.AddDays(dDiff.Days);
    }
    else
    {
      newContactData.DateEnd = null;
    }

    var newId = add((newContactData, null));

    if (newId == 0) return newId;
    var custom_fields = db.get_custom_fields("contracts");
    foreach (var field in custom_fields)
    {
      var value = db.get_custom_field_value(id, field.Id, "contracts", false);
      if (value == "") return newId;
      db.CustomFieldsValues.Add(new CustomFieldsValue
      {
        RelId = newId,
        FieldId = field.Id,
        FieldTo = "contracts",
        Value = value
      });
    }

    return newId;
  }

  /**
   * @param  integer ID
   * @return boolean
   * Delete contract, also attachment will be removed if any found
   */
  public bool delete(int id)
  {
    hooks.do_action("before_contract_deleted", id);
    clear_signature(id);
    var contract = get(x => x.Id == id);
    var affected_rows = db.Contracts.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    db.ContractComments
      .Where(x => x.ContractId == id)
      .Delete();
    // Delete the custom field values
    db.CustomFieldsValues
      .Where(x => x.RelId == id && x.FieldTo == "contract")
      .Delete();
    db.Files
      .Where(x => x.RelId == id && x.RelType == "contract")
      .ToList()
      .ForEach(attachment => { delete_contract_attachment(attachment.Id); });

    db.Notes.Where(x => x.RelId == id && x.RelType == "contract").Delete();
    db.ContractRenewals.Where(x => x.ContractId == id).Delete();
    // Get related tasks

    db.Tasks.Where(x => x.RelId == id && x.RelType == "contract").ToList().ForEach(task => { tasks_model.delete_task(task.Id); });


    delete_tracked_emails(id, "contract");

    log_activity($"Contract Deleted [{id}]");

    hooks.do_action("after_contract_deleted", id);

    return true;
  }

  /**
   * Mark the contract as signed manually
   *
   * @param  int id contract id
   *
   * @return boolean
   */
  public bool mark_as_signed(int id)
  {
    var affected_rows = db.Contacts.Where(x => x.Id == id).Update(x => new Contract { MarkedAsSigned = 1 });

    return affected_rows > 0;
  }

  /**
   * Unmark the contract as signed manually
   *
   * @param  int id contract id
   *
   * @return boolean
   */
  public bool unmark_as_signed(int id)
  {
    var result = db.Contracts.Where(x => x.Id == id).Update(x => new Contract { MarkedAsSigned = 0 });
    return result > 0;
  }

  /**
   * Function that send contract to customer
   * @param  mixed  id        contract id
   * @param  boolean attachpdf to attach pdf or not
   * @param  string  cc        Email CC
   * @return boolean
   */
  public bool send_contract_to_client(int id, bool attachpdf = true, string cc = "")
  {
    var (contract, attachment) = get(x => x.Id == id).First();
    var attach = false;
    if (attachpdf)
    {
      self.helper.set_mailing_constant();
      var pdf = self.helper.contract_pdf(contract);
      // attach = pdf.Output($"{slug_it(contract.Subject)}.pdf", 'S');
      attach = pdf.Output($"{slug_it(contract.Subject)}.pdf");
    }

    var sent_to_str = self.input.post<string>("sent_to");
    var sent_to = sent_to_str.Split(",").Select(int.Parse).ToList();
    var sent = false;

    if (sent_to.Any())
    {
      var i = 0;
      sent_to.ForEach(contact_id =>
      {
        if (contact_id == 0) return;
        var contact = clients_model.get_contact(contact_id);
        // Send cc only for the first contact
        if (!string.IsNullOrEmpty(cc) && i > 0) cc = "";
        var template = mail_template("contract_send_to_customer", contract, contact, cc);
        if (attachpdf)
          template.add_attachment(new MailAttachment()
          {
            attachment = attach,
            filename = $"{slug_it(contract.Subject)}.pdf",
            type = "application/pdf"
          });

        if (template.send()) sent = true;
      });
    }
    else
    {
      return false;
    }

    if (!sent) return false;
    var contactsSent = new List<int>();
    if (!string.IsNullOrEmpty(contract.ContactsSentTo))
    {
      dynamic sentTo = JsonConvert.SerializeObject(contract.ContactsSentTo);
      // cc = array_unique(array_merge(is_array(sentTo.Cc) ? sentTo.Cc : explode(',', sentTo.Cc), explode(',', cc)));
      cc = TypeMerger.Merge(sentTo.Cc, cc);
      cc.Split(",").ToList().Distinct();
      contactsSent = sentTo.ContactIds;
    }

    db.Contracts
      .Where(x => x.Id == id)
      .Update(x => new Contract
      {
        LastSentAt = DateTime.Now
        // ContactsSentTo = JsonConvert.SerializeObject(new { contact_ids = contactsSent.Union(sent_to), cc })
      });

    return true;
  }

  /**
   * Delete contract attachment
   * @param  mixed attachment_id
   * @return boolean
   */
  public bool delete_contract_attachment(int attachment_id)
  {
    var deleted = false;
    var attachment = get_contract_attachments(attachment_id).FirstOrDefault();
    if (attachment == null) return deleted;
    if (string.IsNullOrEmpty(attachment.External))
      unlink($"{get_upload_path_by_type("contract")}{attachment.RelId}/{attachment.FileName}");


    var affected_rows = db.Files.Where(x => x.Id == attachment_id).Delete();
    if (affected_rows > 0)
    {
      deleted = true;
      log_activity($"Contract Attachment Deleted [ContractID: {attachment.RelId}]");
    }

    if (!is_dir(get_upload_path_by_type("contract") + attachment.RelId)) return deleted;
    // Check if no attachments left, so we can delete the folder also
    var other_attachments = list_files(get_upload_path_by_type("contract") + attachment.RelId);
    if (!other_attachments.Any())
      // okey only index.html so we can delete the folder also
      delete_dir(get_upload_path_by_type("contract") + attachment.RelId);

    return deleted;
  }

  /**
   * Renew contract
   * @param  mixed data All _POST data
   * @return mixed
   */
  public async Task<bool> renew(ContractRenewal data)
  {
    // var keepSignature = data.RenewKeepSignature;
    var keepSignature = false;
    var (contract, attachment) = get(x => x.Id == data.ContractId).FirstOrDefault();
    if (contract == null) return false;
    if (keepSignature)
      data.NewValue = contract.ContractValue == 1;
    data.DateRenewed = DateTime.Now;
    data.RenewedBy = db.get_staff_full_name(staff_user_id);
    data.RenewedByStaffId = staff_user_id;
    if (data.NewEndDate.HasValue)
      data.NewEndDate = null;
    // get the original contract so we can check if is expiry notified on delete the expiry to revert
    var (_contract, attachment2) = get(x => x.Id == data.ContractId).First();
    data.IsOnOldExpiryNotified = _contract.IsExpiryNotified;

    var result = db.ContractRenewals.Add(data);

    if (!result.IsAdded()) return false;
    var row = db.Contracts.Where(x => x.Id == data.ContractId).AsQueryable();

    var _data = new Contract
    {
      DateStart = data.NewStartDate,
      ContractValue = data.NewValue ? 1 : 0,
      IsExpiryNotified = 0
    };

    if (data.NewEndDate.HasValue)
      _data.DateEnd = data.NewEndDate;

    if (!keepSignature)
    {
      _data = (Contract)TypeMerger.Merge(_data, get_acceptance_info_array<Contract>(true));
      _data.Signed = false;
      if (!string.IsNullOrEmpty(_contract.Signature)) unlink($"{get_upload_path_by_type("contract")}{data.ContractId}/{_contract.Signature}");
    }

    if (db.Contracts.Update(_data).IsModified())
    {
      log_activity($"Contract Renewed [ID: {data.ContractId}]");

      return true;
    }
    // delete the previous entry

    db.ContractRenewals.Where(x => x.Id == result.Entity.Id).Delete();
    return false;
  }

  /**
   * Delete contract renewal
   * @param  mixed id         renewal id
   * @param  mixed contract_id contract id
   * @return boolean
   */
  public bool delete_renewal(int id, int contract_id)
  {
    // check if this renewal is last so we can revert back the old values, if is not last we wont do anything

    var row = db.ContractRenewals.Where(x => x.ContractId == contract_id).OrderByDescending(x => x.Id).FirstOrDefault();
    var last_contract_renewal = row.Id;
    var is_last = false;
    var original_renewal = new ContractRenewal();
    if (last_contract_renewal == id)
    {
      is_last = true;
      original_renewal = db.ContractRenewals.FirstOrDefault(x => x.Id == id);
    }

    var (contract, attachment) = get(x => x.Id == id).First();
    var affected_rows = db.ContractRenewals.Where(x => x.Id == id).Delete();
    if (affected_rows <= 0) return false;
    if (!string.IsNullOrEmpty(contract.ShortLink))
      db.app_archive_short_link(contract.ShortLink);

    if (is_last)
    {
      var query = db.Contracts.Where(x => x.Id == contract_id).AsQueryable();

      var data = new Contract
      {
        DateStart = original_renewal.OldStartDate,
        ContractValue = original_renewal.OldValue ? 1 : 0,
        IsExpiryNotified = original_renewal.IsOnOldExpiryNotified!.Value
      };

      if (original_renewal.OldEndDate.HasValue)
        data.DateEnd = original_renewal.OldEndDate;
      query.Update(x => data);
    }

    log_activity($"Contract Renewed [RenewalID: {id}, ContractID: {contract_id}]");

    return true;
  }

  /**
   * Get the contracts about to expired in the given days
   *
   * @param  integer|null staffId
   * @param  integer days
   *
   * @return array
   */
  public List<Contract> get_contracts_about_to_expire(int? staffId = null, int days = 7)
  {
    var diff1 = DateTime.Now.AddDays(-days);
    var diff2 = DateTime.Now.AddDays(+days);
    var query = db.Contracts.AsQueryable();
    // if (staffId.HasValue && !self.helper.staff_can("view", "contracts", staffId.Value))
    if (staffId.HasValue && !db.staff_can(view: "contracts", staffId: staffId.Value))
      query.Where(x => x.AddedFrom == staffId);
    var rows = query
      .Where(x =>
        x.DateEnd == null &&
        x.Trash == false &&
        x.DateEnd >= diff1 &&
        x.DateEnd <= diff2
      ).ToList();
    return rows;
  }

  /**
   * Get contract renewals
   * @param  mixed id contract id
   * @return array
   */
  public List<ContractRenewal> get_contract_renewal_history(int id)
  {
    var rows = db.ContractRenewals.Where(x => x.ContractId == id)
      .OrderBy(x => x.DateRenewed)
      .ToList();
    return rows;
  }

  /**
   * @param  integer ID (optional)
   * @return mixed
   * Get contract type object based on passed id if not passed id return array of all types
   */
  public List<ContractsType> get_contract_types()
  {
    return contract_types_model.get();
  }

  public ContractsType? get_contract_types(int id)
  {
    return contract_types_model.get(id);
  }

  /**
   * @param  integer ID
   * @return mixed
   * Delete contract type from database, if used return array with key referenced
   */
  public bool delete_contract_type(int id)
  {
    return contract_types_model.delete(id);
  }

  /**
   * Add new contract type
   * @param mixed data All _POST data
   */
  public int add_contract_type(ContractsType data)
  {
    return contract_types_model.add(data);
  }

  /**
   * Edit contract type
   * @param mixed data All _POST data
   * @param mixed id Contract type id
   */
  public bool update_contract_type(ContractsType data)
  {
    return contract_types_model.update(data);
  }

  /**
   * Get contract types data for chart
   * @return array
   */
  public async Task<Chart> get_contracts_types_chart_data()
  {
    var output = await contract_types_model.get_chart_data();
    return output;
  }

  /**
  * Get contract types values for chart
  * @return array
  */
  public async Task<Chart> get_contracts_types_values_chart_data()
  {
    var output = await contract_types_model.get_values_chart_data();
    return output;
  }
}
