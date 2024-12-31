using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using Service.Framework.Library.Merger;
using Service.Helpers;
using Service.Helpers.Tags;
using Service.Helpers.Template;
using Service.Models.Client;
using Service.Models.Tasks;
using Service.Models.Users;
using Chart = Service.Framework.Helpers.Entities.Chart;
using static Service.Framework.Core.Extensions.StringExtension;


namespace Service.Models.Tickets;

public class TicketsModel(MyInstance self, MyContext db) : MyModel(self)
{
  private bool piping = false;

  private DepartmentsModel departments_model = self.model.departments_model();
  private SpamFiltersModel spam_filters_model = self.model.spam_filters_model();
  private StaffModel staff_model = self.model.staff_model();
  private ClientsModel clients_model = self.model.clients_model();
  private TasksModel tasks_model = self.model.tasks_model();

  public async Task<int> ticket_count(int? status = null)
  {
    var query = db.Tickets.Where(x => x.MergedTicketId == null).AsQueryable();
    if (!is_admin)
    {
      var staff_deparments_ids = departments_model.get_staff_departments(staff_user_id)
        .Select(x => x.Id)
        .ToList();
      if (db.get_option("staff_access_only_assigned_departments") == "1")
      {
        var departments_ids = new List<int>();
        if (staff_deparments_ids.Any())
        {
          var departments = departments_model.get();
          departments_ids = departments.Select(x => x.Id).ToList();
        }
        else
        {
          departments_ids = staff_deparments_ids;
        }

        var where = "";

        if (departments_ids.Any())
        {
          var temp_items = db.StaffDepartments
            .Where(x => departments_ids.Contains(x.DepartmentId!.Value) && x.StaffId == staff_user_id)
            .Select(x => x.DepartmentId)
            .ToList();
          query = query.Where(x =>
            temp_items.Contains(x.DepartmentId)
          );
        }
      }
    }

    if (!status.HasValue) return query.Count();

    query = query.Where(x => x.Status == status);
    return query.Count();
  }

  // public async Task<string> insert_piped_ticket(dynamic data)
  // {
  //   var body = string.Empty;
  //   var subject = string.Empty;
  //   data = self.hooks.apply_filters("piped_ticket_data", body, subject);
  //   piping = true;
  //   var attachments = data.TicketAttachments;
  //   subject = data.Subject;
  //   var system_blocked_subjects = new List<string>()
  //   {
  //     "Mail delivery failed",
  //     "failure notice",
  //     "Returned mail: see transcript for details",
  //     "Undelivered Mail Returned to Sender"
  //   };
  //
  //   var subject_blocked = system_blocked_subjects.Any(sb => sb.Contains(subject));
  //
  //
  //   if (subject_blocked) return string.Empty;
  //
  //   var message = data.body;
  //   var name = data.from_name;
  //
  //   var email = data.Email;
  //   var to = $"{data.To}";
  //   var cc = data.Cc.Split(",").ToList();
  //
  //   var tid = string.Empty;
  //   var mailstatus = spam_filters_model.check(email, subject, message, "tickets");
  //
  //   // No spam found
  //   if (string.IsNullOrEmpty(mailstatus))
  //   {
  //     var pos = subject.IndexOf("[Ticket ID: ");
  //     if (pos > 0)
  //     {
  //       tid = subject[(pos + 12)..][..tid.IndexOf("]")];
  //       data = db.Tickets.FirstOrDefault(x => x.Id == Convert.ToInt32(tid));
  //       tid = $"{data.Id}";
  //     }
  //
  //     to = to.Trim();
  //
  //     var department_id = 0;
  //     int? userid = null;
  //     var toemails = to
  //       .Split(",")
  //       .ToList()
  //       .Select(toEmail =>
  //       {
  //         if (department_id != 0) return 0;
  //         var temp = db.Departments.FirstOrDefault(x => x.Email == toEmail.Trim());
  //         if (temp == null) return 0;
  //         department_id = data.Department.Id;
  //         to = data.Email;
  //         return 0;
  //       })
  //       .ToList();
  //
  //     if (department_id != 0)
  //     {
  //       mailstatus = "Department Not Found";
  //     }
  //     else
  //     {
  //       if (to == email)
  //       {
  //         mailstatus = "Blocked Potential Email Loop";
  //       }
  //       else
  //       {
  //         message = message.Trim();
  //         var result = db.Staff.FirstOrDefault(x => x.Active.HasValue && x.Email == email);
  //         if (result != null)
  //         {
  //           if (string.IsNullOrEmpty(tid))
  //           {
  //             // data = [];
  //             data.Message = message;
  //             data.Status = get_option<int>("default_ticket_reply_status");
  //             if (data.Status == 0)
  //               data.Status = 3; // Answered
  //             if (userid == 0)
  //             {
  //               data.Name = name;
  //               data.Email = email;
  //             }
  //
  //             if (cc.Any())
  //               data.Cc = string.Join(",", cc);
  //
  //             var reply_id = await this.add_reply(data, Convert.ToInt32(tid), result.Id, attachments);
  //             if (reply_id > 0) mailstatus = "Ticket Reply Imported Successfully";
  //           }
  //           else
  //           {
  //             mailstatus = "Ticket ID Not Found";
  //           }
  //         }
  //         else
  //         {
  //           var contact = db.Contacts.FirstOrDefault(x => x.Email == email);
  //           var contactid = 0;
  //           if (contact != null)
  //           {
  //             userid = contact.Id;
  //             contactid = contact.Id;
  //           }
  //
  //           if (userid == 0 && db.get_option("email_piping_only_registered") == "1")
  //           {
  //             mailstatus = "Unregistered Email Address";
  //           }
  //           else
  //           {
  //             var filterdate = DateTime.Now.AddMinutes(-15);
  //             var query = db.Tickets
  //               .Where(x => DateTime.Parse(x.Date) > filterdate && x.Email == email)
  //               .AsQueryable();
  //             if (userid > 0)
  //               query = query.Where(x => x.UserId == userid);
  //             var ticket = query.ToList();
  //             var abuse = false;
  //             if (10 < ticket.Count)
  //             {
  //               mailstatus = "Exceeded Limit of 10 Tickets within 15 Minutes";
  //             }
  //             else
  //             {
  //               if (!string.IsNullOrEmpty(tid))
  //               {
  //                 data = new Ticket
  //                 {
  //                   Message = message,
  //                   Status = 1
  //                 };
  //                 if (userid == 0)
  //                 {
  //                   data.Name = name;
  //                   data.Email = email;
  //                 }
  //                 else
  //                 {
  //                   data.UserId = userid;
  //                   data.ContactId = contactid;
  //
  //
  //                   var t = db.Tickets.FirstOrDefault(x =>
  //                     (x.Id == Convert.ToInt32(tid) && x.UserId == userid)
  //                     || x.Cc.Contains(email)
  //                   );
  //
  //
  //                   if (t == null) abuse = true;
  //                 }
  //
  //                 if (!abuse)
  //                 {
  //                   if (cc.Any()) data.Cc = string.Join(",", cc);
  //                   var reply_id = await this.add_reply(data, Convert.ToInt32(tid), null, attachments);
  //                   if (reply_id > 0)
  //                     // Dont change this line
  //                     mailstatus = "Ticket Reply Imported Successfully";
  //                 }
  //                 else
  //                 {
  //                   mailstatus = "Ticket ID Not Found For User";
  //                 }
  //               }
  //               else
  //               {
  //                 if (db.get_option_compare("email_piping_only_registered", 1) && !userid.HasValue)
  //                 {
  //                   mailstatus = "Blocked Ticket Opening from Unregistered User";
  //                 }
  //                 else
  //                 {
  //                   if (db.get_option("email_piping_only_replies") == "1")
  //                   {
  //                     mailstatus = "Only Replies Allowed by Email";
  //                   }
  //                   else
  //                   {
  //                     // data =  [] ;
  //                     data.DepartmentId = department_id;
  //                     data.Subject = subject;
  //                     data.Message = message;
  //                     data.ContactId = contactid;
  //                     data.Priority = get_option<int>("email_piping_default_priority");
  //                     if (userid == 0)
  //                     {
  //                       data.Name = name;
  //                       data.Email = email;
  //                     }
  //                     else
  //                     {
  //                       data.UserId = userid;
  //                     }
  //
  //                     tid = await add(data, null, attachments);
  //                     if (Convert.ToInt32(tid) > 0 && cc.Any())
  //                       // A customer opens a ticket by mail to "support@example".com, with one or many "Cc"
  //                       // Remember those "Cc".
  //                       await db.Tickets
  //                         .Where(x => x.Id == Convert.ToInt32(tid))
  //                         .UpdateAsync(x => new Ticket { Cc = string.Join(",", cc) });
  //                     // Dont change this line
  //                     mailstatus = "Ticket Imported Successfully";
  //                   }
  //                 }
  //               }
  //             }
  //           }
  //         }
  //       }
  //     }
  //   }
  //
  //   if (mailstatus == "") mailstatus = "Ticket Import Failed";
  //   db.TicketsPipeLogs.Add(new TicketsPipeLog()
  //   {
  //     DateCreated = DateTime.Now,
  //     EmailTo = to,
  //     Name = name ?? "Unknown",
  //     Email = email ?? "N/A",
  //     Subject = subject ?? "N/A",
  //     Message = message,
  //     Status = mailstatus
  //   });
  //   await db.SaveChangesAsync();
  //   return mailstatus;
  // }

  // private void process_pipe_attachments(List<(TicketAttachment attachment, string data)> args, int ticket_id, int? reply_id = null)
  private void process_pipe_attachments(List<TicketDto> args, int ticket_id, int? reply_id = null)
  {
    if (args.Any()) return;
    var ticket_attachments = new List<TicketAttachment>();
    var allowed_extensions = db.get_option("ticket_attachments_file_extensions")
      .Split(",")
      .ToList()
      .Select(x => x.Trim().ToLower())
      .ToList();

    var path = $"uploads/ticket_attachments/{ticket_id}/";

    foreach (var arg in args)
    {
      var filename = arg.attachment.FileName;
      var filenameparts = filename.Split(".").ToList().First();
      var extension = self.helper.file_extension(filenameparts);

      if (!allowed_extensions.Contains($".{extension}")) continue;
      //
      filename = filenameparts[..^1]; // Remove last character
      filename = Regex.Replace(filename, @"[^a-zA-Z0-9-_ ]", ""); // Remove unwanted characters
      filename = filename.Trim(); // Trim any spaces

      Console.WriteLine(filename); // Output: example_filename
      if (string.IsNullOrEmpty(filename)) filename = "attachment";
      if (!self.helper.file_exists(path))
        self.helper.file_create($"{path}index.html");
      filename = self.helper.unique_filename(path, $"{filename}.{extension}");
      self.helper.file_put_contents(path + filename, arg.data);
      ticket_attachments.Add(new TicketAttachment
      {
        FileName = filename,
        FileType = self.helper.get_mime_by_extension(filename)
      });
    }

    insert_ticket_attachments_to_database(ticket_attachments, ticket_id, reply_id);
  }

  public List<Ticket> get(Expression<Func<Ticket, bool>> where)
  {
    var query = db.Tickets
      .Include(x => x.Department)
      // .Include(x => x.TicketsStatus)
      .Include(x => x.Service)
      // .Include(x => x.Client)
      .Include(x => x.Contact)
      // .Include(x => x.Staff)
      // .Include(x => x.TicketsPriority)
      .AsQueryable();


    query = query.Where(where).OrderBy(x => x.LastReply);

    var _is_client_logged_in = self.helper.is_client_logged_in();
    if (_is_client_logged_in) query = query.Where(x => x.MergedTicketId == null);
    return query.ToList();
  }

  public Ticket? get(Expression<Func<Ticket, bool>> where, int id)
  {
    return db.Tickets
      // .Include(x => x.TicketsStatus)
      .Include(x => x.Department)
      // .Include(x => x.Client)
      // .Include(x => x.TicketsStatus)
      .Include(x => x.Service)
      // .Include(x => x.Staff)
      // .Include(x => x.TicketsPriority)
      // .Include(x => x.Client)
      .Include(x => x.Contact)
      // .Include(x => x.Staff)
      // .Include(x => x.TicketsPriority)
      .Where(where)
      .FirstOrDefault(x => x.Id == id);
  }

  /**
   * Get ticket by id and all data
   * @param  mixed  id     ticket id
   * @param  mixed userid Optional - Tickets from USER ID
   * @return object
   */
  public (Ticket ticket, string, string, List<TicketAttachment>) get_ticket_by_id(string uuid, int? userid = null)
  {
    var query = db.Tickets
      .Include(x => x.Department)
      // .Include(x => x.TicketsStatus)
      .Include(x => x.Service)
      // .Include(x => x.Client)
      .Include(x => x.Contact)
      // .Include(x => x.Staff)
      // .Include(x => x.TicketsPriority)
      .Where(x => x.TicketKey == uuid)
      .AsQueryable();
    if (userid.HasValue) query = query.Where(x => x.Contact.Id == userid.Value);
    var ticket = query.FirstOrDefault();
    if (ticket == null) return (ticket, string.Empty, string.Empty, new List<TicketAttachment>());
    var submitter = ticket.ContactId != 0
      ? $"{ticket.Responder.FirstName} {ticket.Responder.LastName}"
      : ticket.Name;
    var opened_by = ticket.Admin is not (null or 0)
      ? ticket.Responder.FirstName + " " + ticket.Responder.LastName
      : "";
    var attachments = get_ticket_attachments(ticket.Id);
    return (ticket, submitter, opened_by, attachments);
  }

  public (Ticket ticket, string? submitter, string opened_by, List<TicketAttachment> attachments) get_ticket_by_id(int id, int? userid = null)
  {
    var query = db.Tickets
      .Include(x => x.Department)
      .Include(x => x.StatusNavigation)
      .Include(x => x.Service)
      .Include(x => x.Contact)
      .Include(x => x.Contact)
      .Include(x => x.Responder)
      // .Include(x => x.TicketsPriority)
      .Where(x => x.Id == id)
      .AsQueryable();

    // if (userid.HasValue) query = query.Where(x => x.ClientId == userid.Value);
    if (userid.HasValue) query = query.Where(x => x.Id == userid.Value);
    var ticket = query.FirstOrDefault();
    if (ticket == null) return (ticket, null, "", new List<TicketAttachment>());

    var submitter = ticket.ContactId != 0
      ? $"{ticket.Contact.FirstName} {ticket.Contact.LastName}"
      : ticket.Name;
    var opened_by = !(ticket.Admin == null || ticket.Admin == 0)
      ? $"{ticket.Responder.FirstName} {ticket.Responder.LastName}"
      : "";
    var attachments = get_ticket_attachments(ticket.Id);
    return (ticket, submitter, opened_by, attachments);
  }

  /**
   * Insert ticket attachments to database
   * @param  array  attachments array of attachment
   * @param  mixed  ticketid
   * @param  boolean replyid If is from reply
   */
  public bool insert_ticket_attachments_to_database(List<TicketAttachment> attachments, int ticketid, int? replyid = 0)
  {
    attachments.ForEach(attachment =>
    {
      attachment.TicketId = ticketid;
      attachment.DateCreated = DateTime.Now;
      if (replyid is not null)
        attachment.ReplyId = replyid.Value;
      db.TicketAttachments.Add(attachment);
    });
    return true;
  }

  /**
   * Get ticket attachments from database
   * @param  mixed id      ticket id
   * @param  mixed replyid Optional - reply id if is from from reply
   * @return array
   */
  public List<TicketAttachment> get_ticket_attachments(int id, int reply_id = 0)
  {
    var rows = db.TicketAttachments.Where(x => x.TicketId == id && x.ReplyId == reply_id).ToList();
    return rows;
  }

  /**
   * Add new reply to ticket
   * @param mixed data  reply _POST data
   * @param mixed id    ticket id
   * @param boolean admin staff id if is staff making reply
   */
  /**
    * Add new reply to ticket
    * @param mixed $data  reply $_POST data
    * @param mixed $id    ticket id
    * @param boolean $admin staff id if is staff making reply
    */
  /**
* Add new reply to ticket
* @param mixed data  reply _POST data
* @param mixed id    ticket id
* @param boolean admin staff id if is staff making reply
*/
  public async Task<int> add_reply(Ticket data, int id, int? admin = null, List<TicketDto> pipe_attachments = default, int assign_to_current_user = 0)
  {
    var assigned = 0;
    if (assign_to_current_user == 0) assigned = self.helper.get_staff_user_id();
    var unsetters = new[]
    {
      "note_description",
      "department",
      "priority",
      "subject",
      "assigned",
      "project_id",
      "service",
      "status_top",
      "attachments",
      "DataTables_Table_0_length",
      "DataTables_Table_1_length",
      "custom_fields"
    };

    // foreach (var unset in unsetters)
    //   if (string.IsNullOrEmpty(data[unset]))
    //     unset(data[unset]);
    var status = 0;
    if (admin != null)
    {
      data.Admin = admin;
      status = data.Status!.Value;
    }
    else
    {
      status = 1;
    }

    data.Status ??= 0;

    var cc = "";
    if (string.IsNullOrEmpty(data.Cc))
    {
      cc = data.Cc;
      data.Cc = string.Empty;
    }

    // if ticket is merged
    var ticket = get(x => x.Id == id).FirstOrDefault();
    data.Id = ticket != null && ticket.MergedTicketId != null ? ticket.MergedTicketId : id;
    data.Date = date("Y-m-d H:i:s");
    data.Message = data.Message.Trim();

    if (piping) data.Message = preg_replace("/\v+/u", "<br>", data.Message);

    // admin can have html
    if (admin == null && self.hooks.apply_filters("ticket_message_without_html_for_non_admin", true))
    {
      data.Message = self.helper.strip_tags(data.Message);
      data.Message = nl2br_save_html(data.Message);
    }

    if (data.UserId == 0)
      data.UserId = 0;

    data.Message = self.helper.remove_emojis(data.Message);
    data = self.hooks.apply_filters("before_ticket_reply_add", data, id, admin);
    var sender = self.helper.convert<TicketReply>(data);
    var result = db.TicketReplies.Add(sender);
    var insert_id = result.Entity.Id;
    if (insert_id <= 0) return 0;


    var row = db.Tickets.Where(x => x.Id == id).First();
    var old_ticket_status = row.Status;


    var newStatus = self.hooks.apply_filters(
      "ticket_reply_status",
      new
      {
        // Id = insert_id,
        Id = id,
        Admin = admin,
        Status = old_ticket_status == 2 && admin == null ? old_ticket_status.Value : status
      }
    );

    if (assigned == 0)
    {
      await db.Tickets
        .Where(x => x.Id == id)
        .UpdateAsync(x => new Ticket
        {
          Assigned = assigned
        });
      db.SaveChanges();
    }

    if (pipe_attachments.Any())
    {
      process_pipe_attachments(pipe_attachments, id, insert_id);
    }
    else
    {
      var attachments = self.helper.handle_ticket_attachments(id);
      if (attachments.Any())
        insert_ticket_attachments_to_database(attachments, id, insert_id);
    }

    var _attachments = get_ticket_attachments(id, insert_id);

    log_activity("New Ticket Reply [ReplyID: " + insert_id + "]");
    var _ticket = convert<Ticket>(newStatus);
    await db.Tickets
      .Where(x => x.Id == id)
      .UpdateAsync(x => new Ticket
      {
        LastReply = DateTime.Now,
        //Status = _ticket.Status,
        AdminRead = false,
        ClientRead = false
      });
    await db.SaveChangesAsync();

    if (old_ticket_status != newStatus.OldStatus)
      self.hooks.do_action("after_ticket_status_changed", new
      {
        id,
        status = newStatus
      });

    var res = get_ticket_by_id(id);
    var userid = res.ticket.UserId;
    var isContact = false;
    var email = string.Empty;
    if (ticket.UserId != 0 && ticket.ContactId != 0)
    {
      email = clients_model.get_contact(ticket.ContactId!.Value)!.Email;
      isContact = true;
    }
    else
    {
      email = ticket.Email;
    }

    if (admin == null)
    {
      var departments_model = self.model.departments_model();
      var staff_model = self.model.staff_model();
      // this.load.model("departments_model");
      // this.load.model("staff_model");


      var staff = await get_staff_members_for_ticket_notification(ticket.Department!, ticket.Assigned ?? 0);
      var notifiedUsers = staff.Select(member =>
      {
        self.helper.send_mail_template("ticket_new_reply_to_staff", ticket, member, _attachments);
        if (!db.get_option_compare("receive_notification_on_new_ticket_replies", 1)) return 0;
        var notified = self.helper.add_notification(new Notification
        {
          Description = "not_new_ticket_reply",
          ToUserId = member.Id,
          FromCompany = true,
          FromUserId = 0,
          Link = "tickets/ticket/" + id,
          AdditionalData = JsonConvert.SerializeObject(new[] { ticket.Subject })
        });
        return notified ? member.Id : 0;
      }).ToList();
      self.helper.pusher_trigger_notification(notifiedUsers);
    }
    else
    {
      self.ignore(async () => await update_staff_replying(id));
      var total_staff_replies = db.TicketReplies.Count(x => x.Admin != null && x.Id == ticket.Id);
      if (
        ticket.Assigned == 0 &&
        db.get_option("automatically_assign_ticket_to_first_staff_responding") == "1" &&
        total_staff_replies == 1
      )
      {
        await db.Tickets
          .Where(x => x.Id == id)
          .UpdateAsync(x => new Ticket
          {
            Assigned = admin
          });
        await db.SaveChangesAsync();
      }

      var sendEmail = !(isContact && db.Contacts.Count(x => x.TicketEmails == 1 && x.Id == ticket.ContactId) == 0);
      if (sendEmail) self.helper.send_mail_template("ticket_new_reply_to_customer", ticket, email, _attachments, cc);
    }

    if (string.IsNullOrEmpty(cc))
    {
      // imported reply
      if (cc.Contains(","))
      {
        if (ticket.Cc.IsNullOrEmpty())
        {
          var currentCC = ticket.Cc.Split(",").ToList();
          var items = (List<string>)TypeMerger.Merge(cc, currentCC);
          // cc = array_unique([cc, currentCC]);
          cc = string.Join(",", items.Distinct());
        }

        cc = string.Join(",", cc);
      }

      await db.Tickets.Where(x => x.Id == id).UpdateAsync(x => new Ticket { Cc = cc });
    }

    self.hooks.do_action("after_ticket_reply_added", new
    {
      data,
      id,
      admin,
      replyid = insert_id
    });

    return insert_id;
  }


  /**
   *  Delete ticket reply
   * @param   mixed ticket_id    ticket id
   * @param   mixed reply_id     reply id
   * @return  boolean
   */
  public bool delete_ticket_reply(int ticket_id, int reply_id)
  {
    self.hooks.do_action("before_delete_ticket_reply", new { ticket_id, reply_id });
    var affected_rows = db.TicketReplies.Where(x => x.Id == reply_id).Delete();
    if (affected_rows <= 0) return false;
    // Get the reply attachments by passing the reply_id to get_ticket_attachments method
    var attachments = get_ticket_attachments(ticket_id, reply_id);
    attachments.ForEach(attachment => delete_ticket_attachment(attachment.Id));


    return true;
  }

  /**
   * Remove ticket attachment by id
   * @param  mixed id attachment id
   * @return boolean
   */
  public bool delete_ticket_attachment(int id)
  {
    var deleted = false;
    var attachment = db.TicketAttachments.FirstOrDefault(x => x.Id == id);
    if (attachment == null) return deleted;
    if (self.helper.unlink($"{self.helper.get_upload_path_by_type("ticket")}{attachment.TicketId}/{attachment.FileName}"))
    {
      db.TicketAttachments.Where(x => x.Id == id).Delete();
      deleted = true;
    }

    // Check if no attachments left, so we can delete the folder also
    var other_attachments = self.helper.list_files(self.helper.get_upload_path_by_type("ticket") + attachment.TicketId);
    if (!other_attachments.Any())
      self.helper.delete_dir(self.helper.get_upload_path_by_type("ticket") + attachment.TicketId);

    return deleted;
  }

  /**
   * Get ticket attachment by id
   * @param  mixed id attachment id
   * @return mixed
   */
  public TicketAttachment? get_ticket_attachment(int id)
  {
    var row = db.TicketAttachments.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * This functions is used when staff open client ticket
   * @param  mixed userid client id
   * @param  mixed id     ticketid
   * @return array
   */
  public List<TicketResult> get_user_other_tickets(int userid, int id)
  {
    var rows = db.Tickets
      .Include(x => x.Department)
      .Include(x => x.StatusNavigation)
      .Include(x => x.Service)
      .Include(x => x.Contact)
      .Include(x => x.Contact)
      .Include(x => x.Responder)
      // .Include(x => x.TicketsPriority)
      .Where(x => x.UserId == userid && x.Id != id)
      .ToList();

    var tickets = rows.Select(row =>
    {
      var ticketResult = new TicketResult
      {
        Ticket = row,
        Submiter = $"{row.Contact.FirstName} {row.Contact.LastName}"
      };
      if (row.Admin is not (null or 0))
        ticketResult.OpenedBy = $"{row.Responder.FirstName} {row.Responder.LastName}";
      ticketResult.Ticket.TicketAttachments = get_ticket_attachments(row.Id);
      return ticketResult;
    });

    return tickets.ToList();
  }

  /**
   * Get all ticket replies
   * @param  mixed  id     ticketid
   * @param  mixed userid specific client id
   * @return array
   */
  public List<TicketReplyDto> get_ticket_replies(int id)
  {
    var ticket_replies_order = db.get_option("ticket_replies_order");

    ticket_replies_order = self.hooks.apply_filters("ticket_replies_order", ticket_replies_order);

    var replies = db.TicketReplies
      .Include(x => x.Contact)
      .Include(x => x.AdminNavigation)
      .Include(x => x.Contact)
      .Where(x => x.TicketId == id)
      .OrderBy(x => x.Date)
      .ToList()
      .Select(x => new TicketReplyDto
      {
        Id = x.Id,
        TicketId = x.TicketId,
        Admin = x.Admin,
        ContactId = x.ContactId,
        Message = x.Message,
        Date = x.Date,
        Attachment = x.Attachment
      })
      .ToList();
    replies = replies.Select(reply =>
      {
        if (reply.Admin is not (null or 0))
          reply.Submiter = $"{reply.AdminNavigation.FirstName} {reply.AdminNavigation.LastName}";
        else
          reply.Submiter = reply.ContactId != 0
            ? $"{reply.Contact.FirstName} {reply.Contact.FirstName}"
            : reply.FromName;

        reply.Attachments = get_ticket_attachments(id, reply.Id);
        return reply;
      })
      .ToList();


    return replies;
  }

  /**
   * Add new ticket to database
   * @param mixed data  ticket _POST data
   * @param mixed admin If admin adding the ticket passed staff id
   */
  public async Task<int> add(TicketOption data, int? admin = null, List<TicketDto> pipe_attachments = default)
  {
    if (admin != null) data.Admin = admin;
    // (data["ticket_client_search"]);
    if (data.Assigned.HasValue) data.Assigned = 0;
    if (data.ProjectId == 0)
      data.ProjectId = 0;

    if (admin == null)
    {
      if (!string.IsNullOrEmpty(data.Email))
      {
        data.UserId = 0;
        data.ContactId = 0;
      }
      else
      {
        // Opened from customer portal otherwise is passed from pipe or admin area
        if (data.UserId == 0 && data.ContactId == 0)
        {
          data.UserId = self.helper.get_client_user_id();
          data.ContactId = self.helper.get_contact_user_id();
        }
      }

      data.Status = 1;
    }

    var custom_fields = data.custom_fields.Any()
      ? data.custom_fields
      : new List<CustomField>();


    // CC is only from admin area
    var cc = data.Cc;
    data.Date = date("Y-m-d H:i:s");
    data.TicketKey = self.helper.uuid();
    data.Status = 1;
    data.Message = data.Message.Trim();
    data.Subject = data.Subject.Trim();
    if (piping) data.Message = preg_replace("/\v+/u", "<br>", data.Message);

    // Admin can have html
    if (admin == null && self.hooks.apply_filters("ticket_message_without_html_for_non_admin", true))
    {
      data.Message = self.helper.strip_tags(data.Message);
      data.Subject = self.helper.strip_tags(data.Subject);
      data.Message = nl2br_save_html(data.Message);
    }

    if (!data.UserId.HasValue) data.UserId = 0;
    if (!data.Priority.HasValue) data.Priority = 0;

    var tags = data.Tags;
    data.Message = self.helper.remove_emojis(data.Message);
    // data = self.hooks.apply_filters("before_ticket_created", data, admin);
    data = self.hooks.apply_filters("before_ticket_created", data);

    var result = db.Tickets.Add(data);
    var ticketid = result.Entity.Id;

    if (ticketid == 0) return 0;
    self.helper.handle_tags_save(tags, ticketid, "ticket");
    if (custom_fields.Any())
      self.helper.handle_custom_fields_post(ticketid, custom_fields);

    if (!data.Assigned.HasValue && data.Assigned != 0)
      if (data.Assigned != staff_user_id)
      {
        var notified = self.helper.add_notification(new Notification
        {
          Description = "not_ticket_assigned_to_you",
          ToUserId = data.Assigned!.Value,
          FromCompany = true,
          FromUserId = 0,
          Link = $"tickets/ticket/{ticketid}",
          AdditionalData = JsonConvert.SerializeObject(new { data.Subject })
        });

        if (notified)
          self.helper.pusher_trigger_notification(new List<int> { data.Assigned.Value });
        self.helper.send_mail_template("ticket_assigned_to_staff", self.helper.get_staff(data.Assigned).Email, data.Assigned, ticketid, data.UserId, data.ContactId);
      }

    if (pipe_attachments.Any())
    {
      process_pipe_attachments(pipe_attachments, ticketid);
    }
    else
    {
      var attachments = self.helper.handle_ticket_attachments(ticketid);
      if (attachments.Any())
        insert_ticket_attachments_to_database(attachments, ticketid);
    }

    var _attachments = get_ticket_attachments(ticketid);

    var isContact = false;
    var email = data.Email;
    if (data.UserId != 0)
    {
      email = clients_model.get_contact(data.ContactId!.Value)!.Email;
      isContact = true;
    }

    var template = "ticket_created_to_customer";
    if (admin == null)
    {
      template = "ticket_auto_response";
      var notifiedUsers = new List<int>();
      var staffToNotify = await get_staff_members_for_ticket_notification(data.Department, data.Assigned ?? 0);
      foreach (var member in staffToNotify)
      {
        self.helper.send_mail_template("ticket_created_to_staff", ticketid, data.UserId, data.ContactId, member, _attachments);
        if (!db.get_option_compare("receive_notification_on_new_ticket", 1)) continue;
        var notified = self.helper.add_notification(new Notification
        {
          Description = "not_new_ticket_created",
          ToUserId = member.Id,
          FromCompany = true,
          FromUserId = 0,
          Link = $"tickets/ticket/{ticketid}",
          AdditionalData = JsonConvert.SerializeObject(new[] { data.Subject })
        });

        if (notified) notifiedUsers.Add(member.Id);
      }

      self.helper.pusher_trigger_notification(notifiedUsers);
    }
    else
    {
      if (cc.Any())
        db.Tickets
          .Where(x => x.Id == ticketid)
          .Update(x => new Ticket { Cc = string.Join(",", cc) });
    }

    var sendEmail = !(isContact && db.Contracts.Any(x => x.AcceptanceEmail && x.Id == data.ContactId));
    if (sendEmail)
    {
      var ticket = get_ticket_by_id(ticketid);
      // admin == null ? [] : _attachments - Admin opened ticket from admin area add the attachments to the email
      self.helper.send_mail_template(
        template,
        ticket,
        email,
        admin == null ? new List<string>() : _attachments,
        cc);
    }

    self.hooks.do_action("ticket_created", ticketid);
    log_activity($"New Ticket Created [ID: {ticketid}]");
    return ticketid;
  }

  /**
   * Get latest 5 client tickets
   * @param  integer limit  Optional limit tickets
   * @param  mixed userid client id
   * @return array
   */
  public List<Ticket> get_client_latests_ticket(int limit = 5, int? userid = null)
  {
    var query = db.Tickets
      .Include(x => x.StatusNavigation)
      .Include(x => x.Department)
      .Include(x => x.Service)
      .Include(x => x.Contact)
      .Include(x => x.Contact)
      .Include(x => x.Responder)
      .AsQueryable();

    query = userid.HasValue
      ? query.Where(x => x.UserId == userid)
      : query.Where(x => x.UserId == client_user_id);


    var rows = query.Take(limit)
      .Where(x => x.MergedTicketId == null).ToList();

    return rows;
  }

  /**
   * Delete ticket from database and all connections
   * @param  mixed ticketid ticketid
   * @return boolean
   */
  public bool delete(int ticketid)
  {
    var affectedRows = 0;
    self.hooks.do_action("before_ticket_deleted", ticketid);
    // final delete ticket

    var affected_rows = db.Tickets.Where(x => x.Id == ticketid).Delete();
    if (affected_rows > 0) affectedRows++;
    if (affected_rows > 0)
    {
      affectedRows++;

      db.Tickets
        .Where(x => x.MergedTicketId == ticketid)
        .Update(x => new Ticket { MergedTicketId = 0 });


      db.TicketAttachments.Where(x => x.TicketId == ticketid)
        .ToList()
        .ForEach(attachment =>
        {
          if (self.helper.is_dir(self.helper.get_upload_path_by_type("ticket") + ticketid))
            if (self.helper.delete_dir(self.helper.get_upload_path_by_type("ticket") + ticketid))
            {
              db.TicketAttachments.Where(x => x.Id == attachment.Id).Delete();
              if (affected_rows > 0) affectedRows++;
            }

          delete_ticket_attachment(attachment.Id);
        });

      db.CustomFieldsValues
        .Where(x => x.RelId == ticketid && x.FieldTo == "tickets")
        .Delete();

      // Delete replies
      db.TicketReplies
        .Where(x => x.TicketId == ticketid)
        .Delete();
      db.Notes
        .Where(x => x.RelId == ticketid && x.RelType == "ticket")
        .Delete();
      db.Taggables
        .Where(x => x.RelId == ticketid && x.RelType == "ticket")
        .Delete();
      db.Reminders
        .Where(x => x.RelId == ticketid && x.RelType == "ticket")
        .Delete();

      // Get related tasks
      db.Tasks
        .Where(x => x.RelId == ticketid && x.RelType == "ticket")
        .ToList()
        .ForEach(task => { tasks_model.delete_task(task.Id); });
    }

    if (affectedRows <= 0) return false;
    log_activity($"Ticket Deleted [ID: {ticketid}]");

    self.hooks.do_action("after_ticket_deleted", ticketid);

    return true;
  }

  /**
   * Update ticket data / admin use
   * @param  mixed data ticket _POST data
   * @return boolean
   */
  public bool update_single_ticket_settings(TicketOption data)
  {
    var affectedRows = 0;
    data = self.hooks.apply_filters("before_ticket_settings_updated", data);

    var ticketBeforeUpdate = get_ticket_by_id(data.Id);

    if (data.MergedTicketId != 0)
    {
      var tickets = new List<int>();
      tickets.Add(data.MergedTicketId);
      if (merge(data.Id, ticketBeforeUpdate.ticket.Status, tickets.ToArray()))
        affectedRows++;
    }

    if (data.custom_fields.Any())
      if (self.helper.handle_custom_fields_post(data.Id, data.custom_fields))
        affectedRows++;

    List<Taggable> tags = new();
    if (data.Tags.Any())
      tags = data.Tags;

    if (self.helper.handle_tags_save(tags, data.Id, "ticket")) affectedRows++;

    // if ((data.Priority && data.Priority == "") || !data.Priority)
    //   data.Priority = 0;
    data.Assigned ??= 0;
    if (data.ProjectId == 0) data.ProjectId = 0;

    if (data.ContactId != 0)
    {
      data.Name = null;
      data.Email = null;
    }

    var affected_rows = db.Tickets.Where(x => x.Id == data.Id).Update(x => data);
    if (affected_rows > 0)
    {
      self.hooks.do_action(
        "ticket_settings_updated",
        new
        {
          data.Id,
          original_ticket = ticketBeforeUpdate,
          data
        });
      affectedRows++;
    }

    var sendAssignedEmail = false;

    var current_assigned = ticketBeforeUpdate.ticket.Assigned;
    if (current_assigned != 0)
    {
      if (current_assigned != data.Assigned)
        if (data.Assigned != 0 && data.Assigned != staff_user_id)
        {
          sendAssignedEmail = true;
          var notified = self.helper.add_notification(new Notification
          {
            Description = "not_ticket_reassigned_to_you",
            ToUserId = data.Assigned.Value,
            FromCompany = false,
            FromUserId = 0,
            Link = $"tickets/ticket/{data.Id}",
            AdditionalData = JsonConvert.SerializeObject(new[] { data.Subject })
          });


          if (notified) self.helper.pusher_trigger_notification(new List<int> { data.Assigned.Value });
        }
    }
    else
    {
      if (data.Assigned != 0 && data.Assigned != staff_user_id)
      {
        sendAssignedEmail = true;
        var notified = self.helper.add_notification(new Notification
        {
          Description = "not_ticket_assigned_to_you",
          ToUserId = data.Assigned.Value,
          FromCompany = true,
          FromUserId = 0,
          Link = $"tickets/ticket/{data.Id}",
          AdditionalData = JsonConvert.SerializeObject(new[] { data.Subject })
        });
        if (notified != null)
          self.helper.pusher_trigger_notification(new List<int> { data.Assigned.Value });
      }
    }

    if (sendAssignedEmail)
    {
      var row = db.Staff.FirstOrDefault(x => x.Id == data.Assigned);
      var assignedEmail = row.Email;
      self.helper.send_mail_template("ticket_assigned_to_staff", assignedEmail, data.Assigned, data.Id, data.UserId, data.ContactId);
    }

    if (affectedRows <= 0) return false;
    log_activity($"Ticket Updated [ID: {data.Id}]");
    return true;
  }

  /**
   * C<ha></ha>nge ticket status
   * @param  mixed id     ticketid
   * @param  mixed status status id
   * @return array
   */
  public (string, string) change_ticket_status(int id, int status)
  {
    var affected_rows = db.Tickets.Where(x => x.Id == id).Update(x => new Ticket { Status = status });
    var alert = "warning";
    var message = self.helper.label("ticket_status_changed_fail");
    if (affected_rows <= 0) return (alert, message);
    alert = "success";
    message = self.helper.label("ticket_status_changed_successfully");
    self.hooks.do_action("after_ticket_status_changed", new
    {
      id,
      status
    });

    return (alert, message);
  }

  // Priorities

  /**
   * Get ticket priority by id
   * @param  mixed id priority id
   * @return mixed     if id passed return object else array
   */
  public List<TicketsPriority> get_priority()
  {
    var rows = db.TicketsPriorities.ToList();
    return rows;
  }

  public TicketsPriority? get_priority(int id)
  {
    var row = db.TicketsPriorities.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Add new ticket priority
   * @param array data ticket priority data
   */
  public int add_priority(TicketsPriority data)
  {
    var result = db.TicketsPriorities.Add(data);
    if (result.IsAdded())
      log_activity($"New Ticket Priority Added [ID: {result.Entity.Id}, Name: {data.Name}]");
    return result.Entity.Id;
  }

  /**
   * Update ticket priority
   * @param  array data ticket priority _POST data
   * @param  mixed id   ticket priority id
   * @return boolean
   */
  public bool update_priority(TicketsPriority data, int id)
  {
    var result = db.TicketsPriorities.Where(x => x.Id == id).Update(x => data);
    if (result <= 0) return false;
    log_activity($"Ticket Priority Updated [ID: {id} Name: {data.Name}]");
    return true;
  }

  /**
   * Delete ticket priorit
   * @param  mixed id ticket priority id
   * @return mixed
   */
  public bool delete_priority(int id)
  {
    var current = get(x => x.Id == id);
    // Check if the priority id is used in tickets table
    if (db.is_reference_in_table<TicketsPriority>("tickets", id)) return true;
    var result = db.TicketsPriorities.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    if (db.get_option_compare("email_piping_default_priority", id))
      db.update_option("email_piping_default_priority", "");
    log_activity($"Ticket Priority Deleted [ID: {id}]");
    return true;
  }

  // Predefined replies

  /**
   * Get predefined reply  by id
   * @param  mixed id predefined reply id
   * @return mixed if id passed return object else array
   */
  public List<TicketsPredefinedReply> get_predefined_reply()
  {
    var rows = db.TicketsPredefinedReplies.ToList();
    return rows;
  }

  public TicketsPredefinedReply? get_predefined_reply(int id)
  {
    var row = db.TicketsPredefinedReplies.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Add new predefined reply
   * @param array data predefined reply _POST data
   */
  public int add_predefined_reply(TicketsPredefinedReply data)
  {
    var result =
      db.TicketsPredefinedReplies.Add(data);
    log_activity($"New Predefined Reply Added [ID: {result.Entity.Id}, {data.Name}]");
    return result.Entity.Id;
  }

  /**
   * Update predefined reply
   * @param  array data predefined _POST data
   * @param  mixed id   predefined reply id
   * @return boolean
   */
  public bool update_predefined_reply(TicketsPredefinedReply data, int id)
  {
    var result = db.TicketsPredefinedReplies.Where(x => x.Id == id).Update(x => data);
    if (result <= 0) return false;
    log_activity($"Predefined Reply Updated [ID: {id}, {data.Name}]");
    return true;
  }

  /**
   * Delete predefined reply
   * @param  mixed id predefined reply id
   * @return boolean
   */
  public bool delete_predefined_reply(int id)
  {
    var result = db.TicketsPredefinedReplies.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    log_activity($"Predefined Reply Deleted [{id}]");

    return true;
  }

  // Ticket statuses

  /**
   * Get ticket status by id
   * @param  mixed id status id
   * @return mixed     if id passed return object else array
   */
  public List<TicketsStatus> get_ticket_status()
  {
    var rows = db.TicketsStatuses.OrderBy(x => x.StatusOrder).ToList();
    return rows;
  }

  public TicketsStatus? get_ticket_status(int id)
  {
    var row = db.TicketsStatuses.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Add new ticket status
   * @param array ticket status _POST data
   * @return mixed
   */
  public bool add_ticket_status(TicketsStatus data)
  {
    var result = db.TicketsStatuses.Add(data);
    var insert_id = result.Entity.Id;
    if (insert_id <= 0) return false;
    log_activity($"New Ticket Status Added [ID: {insert_id}, {data.Name}]");
    return true;
  }

  /**
   * Update ticket status
   * @param  array data ticket status _POST data
   * @param  mixed id   ticket status id
   * @return boolean
   */
  public bool update_ticket_status(TicketsStatus data)
  {
    var result = db.TicketsStatuses.Where(x => x.Id == data.Id).Update(x => data);
    if (result <= 0) return false;
    log_activity($"Ticket Status Updated [ID: {data.Id} Name: {data.Name}]");
    return true;
  }

  /**
   * Delete ticket status
   * @param  mixed id ticket status id
   * @return mixed
   */
  public bool delete_ticket_status(int id)
  {
    var current = get_ticket_status(id);
    // Default statuses cant be deleted
    if (current.IsDefault) return true;
    if (db.is_reference_in_table<TicketsStatus>("tickets", id)) return true;
    var result = db.TicketsStatuses.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    log_activity($"Ticket Status Deleted [ID: {id}]");
    return true;
  }

  // Ticket services
  public List<Entities.Service> get_service()
  {
    var rows = db.Services.OrderBy(x => x.Name).ToList();
    return rows;
  }

  public Entities.Service? get_service(int id)
  {
    var row = db.Services.FirstOrDefault(x => x.Id == id);
    return row;
  }

  public int add_service(Entities.Service data)
  {
    var result = db.Services.Add(data);
    var insert_id = result.Entity.Id;
    if (insert_id > 0) log_activity($"New Ticket Service Added [ID: {insert_id}.{data.Name}]");
    return insert_id;
  }

  public bool update_service(Entities.Service data)
  {
    var id = data.Id;
    var result = db.Services.Where(x => x.Id == id).Update(x => data);
    if (result <= 0) return false;
    log_activity($"Ticket Service Updated [ID: {id} Name: {data.Name}]");
    return true;
  }

  public bool delete_service(int id)
  {
    if (db.is_reference_in_table<Entities.Service>("tickets", id)) return true;
    var result = db.Services.Where(x => x.Id == id).Delete();
    if (result <= 0) return false;
    log_activity($"Ticket Service Deleted [ID: {id}]");
    return true;
  }

  /**
   * @return array
   * Used in home dashboard page
   * Displays weekly ticket openings statistics (chart)
   */
  public Chart get_weekly_tickets_opening_statistics()
  {
    var departments_ids = new List<int>();
    if (!is_admin)
      if (db.get_option_compare("staff_access_only_assigned_departments", 1))
      {
        var staff_deparments_ids = departments_model.get_staff_departments(staff_user_id).Select(x => x.Id).ToList();
        departments_ids.Clear();
        departments_ids = !staff_deparments_ids.Any()
          ? departments_model.get().Select(x => x.Id).ToList()
          : staff_deparments_ids;
      }

    var chart = new Chart
    {
      Labels = self.helper.get_weekdays(),
      Datasets = new List<ChartDataset>
      {
        new()
        {
          Label = self.helper.label("home_weekend_ticket_opening_statistics"),
          BackgroundColor = "rgba(197, 61, 169, 0.5)",
          BorderColor = "#c53da9",
          BorderWidth = 1,
          Tension = false,
          Data = new List<int> { 0, 0, 0, 0, 0, 0, 0 }
        }
      }
    };

    var monday = self.helper.get_monday_this_week();
    var sunday = self.helper.get_sunday_this_week();
    var thisWeekDays = self.helper.get_weekdays_between_dates(monday, sunday);
    var byDepartments = departments_ids.Any();
    if (!thisWeekDays.Any()) return chart;
    var i = 0;
    chart.Datasets[0].Data = thisWeekDays
      .Select(x =>
      {
        var query = db.Tickets
          .Where(y => y.MergedTicketId == null)
          .AsQueryable();
        if (!byDepartments) return 0;
        var items = db.StaffDepartments
          .Where(y => departments_ids.Contains(y.DepartmentId!.Value))
          .Select(y => y.StaffId)
          .Where(staffId => db.StaffDepartments
            .Any(y => departments_ids.Contains(y.DepartmentId!.Value) && y.StaffId == staffId))
          .ToList();
        return items.First();
      })
      .ToList();

    return chart;
  }

  public List<int?> get_tickets_assignes_disctinct()
  {
    var rows = db.Tickets
      .Where(x => x.Assigned != 0 && x.MergedTicketId == null)
      .Select(x => x.Assigned)
      .Distinct()
      .ToList();
    return rows;
    // return db.query("SELECT DISTINCT(assigned) as assigned FROM tickets WHERE assigned != 0 AND merged_ticket_id IS NULL").result_array();
  }

  /**
   * Check for previous tickets opened by this email/contact and link to the contact
   * @param  string email      email to check for
   * @param  mixed contact_id the contact id to transfer the tickets
   * @return boolean
   */
  public bool transfer_email_tickets_to_contact(string email, int contact_id)
  {
    // Some users don"t want to fill the email
    if (string.IsNullOrEmpty(email)) return false;

    var customer_id = self.helper.get_user_id_by_contact_id(contact_id);
    db.Tickets
      .Where(x => x.UserId == 0)
      .Where(x => x.ContactId == 0)
      .Where(x => x.Admin == null)
      .Where(x => x.Email == email)
      .Update(x => new Ticket
      {
        Email = null,
        Name = null,
        UserId = customer_id.Value,
        ContactId = contact_id
      });


    db.TicketReplies
      .Where(x => x.UserId == 0)
      .Where(x => x.ContactId == 0)
      .Where(x => x.Admin == null)
      .Where(x => x.Email == email)
      .Update(x => new TicketReply
      {
        Email = null,
        Name = null,
        UserId = customer_id,
        ContactId = contact_id
      });


    return true;
  }

  /**
   * Check whether the given ticketid is already merged into another primary ticket
   *
   * @param  int  id
   *
   * @return boolean
   */
  public bool is_merged(int id)
  {
    return db.Tickets.Any(x => x.Id == id && x.MergedTicketId != null);
  }

  /**
   * @param primary_ticket_id
   * @param status
   * @param  array  ids
   *
   * @return bool
   */
  public bool merge(int primary_ticket_id, int? status, params int[] ids)
  {
    // if (is_merged(primary_ticket_id)) return false;
    // if ((index = array_search(primary_ticket_id, ids)) != false)
    //   (ids[index]);
    // if ( !ids.ToList().Any() ) return false;
    // return new MergeTickets(primary_ticket_id, ids)
    //     .markPrimaryTicketAs(status)
    //     .merge();
    return false;
  }

  /**
   * @param array tickets id"s of tickets to check
   * @return array
   */
  public List<int> get_already_merged_tickets(List<Ticket> tickets)
  {
    if (!tickets.Any()) return new List<int>();

    var alreadyMerged = tickets.Where(x => is_merged(x.Id)).Select(x => x.Id).ToList();

    return alreadyMerged;
  }

  /**
   * @param primaryTicketId
   * @return array
   */
  public List<Ticket> get_merged_tickets_by_primary_id(int primaryTicketId)
  {
    var rows = db.Tickets.Where(x => x.MergedTicketId == primaryTicketId).ToList();
    return rows;
  }

  public async Task<bool> update_staff_replying(int ticketId, int? userId = null)
  {
    if (!userId.HasValue)
      return await db.Tickets
        .Where(x => x.Id == ticketId)
        .UpdateAsync(x => new Ticket
        {
          ResponderId = userId
        }) > 0;

    var row = get(x => x.Id == ticketId);
    var ticket = row.FirstOrDefault();

    if (ticket.ResponderId != userId && !ticket.ResponderId.HasValue) return false;
    if (ticket.ResponderId == userId) return true;

    return await db.Tickets
      .Where(x => x.Id == ticketId)
      .UpdateAsync(x => new Ticket
      {
        ResponderId = userId
      }) > 0;
  }

  public Ticket? get_staff_replying(int ticketId)
  {
    var row = db.Tickets.FirstOrDefault(x => x.Id == ticketId);
    return row;
  }

  private async Task<List<Staff>> get_staff_members_for_ticket_notification(Department department, int assignedStaff = 0)
  {
    var staffToNotify = new List<Staff>();
    if (assignedStaff == 0 || !db.get_option_compare("staff_related_ticket_notification_to_assignee_only", 1))
      return staff_model.get(x => x.Active!.Value)
        .Where(x =>
          !self.helper.is_staff_member(x.Id) &&
          db.get_option_compare("access_tickets_to_none_staff_members", 0) &&
          departments_model.get_staff_departments(x.Id).Contains(department))
        // .Select(x=>x.Id)
        .ToList();

    var member = await staff_model.get(assignedStaff, x => x.Active!.Value);
    staffToNotify.Add(member);

    var output = staff_model.get(x => x.Active!.Value)
      .Where(x =>
        !self.helper.is_staff_member(x.Id) &&
        db.get_option_compare("access_tickets_to_none_staff_members", 0) &&
        departments_model.get_staff_departments(x.Id).Contains(department))
      // .Select(x=>x.Id)
      .ToList();
    return output;
  }
}
