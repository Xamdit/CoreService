using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Models.Tickets;

public static class TicketModelExtension
{
  private static MyContext db { get; set; }
  private static MyInstance self { get; set; }
  private static TicketsModel my_model { get; set; }


  public static async Task<string> insert_piped_ticket(this TicketsModel model, (Ticket ticket, string body, string to) data)
  {
    self = model.root;
    db = self.db();
    my_model = model;
    var spam_filters_model = self.model.spam_filters_model();
    var system_blocked_subjects = new[]
    {
      "Mail delivery failed",
      "failure notice",
      "Returned mail: see transcript for details",
      "Undelivered Mail Returned to Sender"
    };

    data = self.hooks.apply_filters("piped_ticket_data", "", "");
    var piping = true;

    var subject = data.ticket.Subject;
    if (system_blocked_subjects.Any(sb => subject.Contains(sb))) return string.Empty;

    var email = data.ticket.Email;
    var items = data.ticket.Cc?.Split(",").ToList() ?? new List<string>();
    var cc = items.Select(x => x.Trim()).ToList() ?? new List<string>();
    var tid = ExtractTicketId(subject);
    var mailstatus = spam_filters_model.check(email, subject, data.body, "tickets");

    if (!string.IsNullOrEmpty(mailstatus))
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, mailstatus);

    var department = db.Departments.FirstOrDefault(x => x.Email == data.to.Trim());
    var departmentId = department?.Id ?? 0;

    if (departmentId == 0)
      return await HandleDepartmentNotFound(data, email, cc, tid, subject);
    if (email == data.to)
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Blocked Potential Email Loop");

    var user = db.Staff.FirstOrDefault(x => x.Active.HasValue && x.Email == email);
    if (user != null)
      return await HandleStaffResponse(data, user, tid, cc, subject);

    var contact = db.Contacts.FirstOrDefault(x => x.Email == email);
    var userId = contact?.Id ?? 0;

    if (userId == 0 && db.get_option("email_piping_only_registered") == "1")
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Unregistered Email Address");

    return await HandleTicketCreation(data, email, cc, tid, userId, subject, departmentId);
  }

  private static string ExtractTicketId(string subject)
  {
    var pos = subject.IndexOf("[Ticket ID: ");
    if (pos < 0) return null;

    var tidStart = pos + 12;
    var tidEnd = subject.IndexOf("]", tidStart);
    return tidEnd > tidStart ? subject[tidStart..tidEnd] : null;
  }

  private static async Task<string> FinalizeLogAndReturn(dynamic data, string email, List<string> cc, string tid, string subject, string mailstatus)
  {
    db.TicketsPipeLogs.Add(new TicketsPipeLog
    {
      DateCreated = DateTime.Now,
      EmailTo = data.To,
      Name = data.FromName ?? "Unknown",
      Email = email ?? "N/A",
      Subject = subject ?? "N/A",
      Message = data.Body,
      Status = mailstatus
    });
    await db.SaveChangesAsync();
    return mailstatus;
  }

  private static async Task<string> HandleDepartmentNotFound(dynamic data, string email, List<string> cc, string tid, string subject)
  {
    return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Department Not Found");
  }

  private static async Task<string> HandleStaffResponse(dynamic data, Staff user, string tid, List<string> cc, string subject)
  {
    if (!string.IsNullOrEmpty(tid)) return await FinalizeLogAndReturn(data, user.Email, cc, tid, subject, "Ticket ID Not Found");
    data.Status = db.get_option<int>("default_ticket_reply_status") == 0 ? 3 : db.get_option<int>("default_ticket_reply_status");
    var replyId = await my_model.add_reply(data, Convert.ToInt32(tid), user.Id, data.TicketAttachments);
    return replyId > 0 ? "Ticket Reply Imported Successfully" : "Ticket Import Failed";
  }

  private static async Task<string> HandleTicketCreation(dynamic data, string email, List<string> cc, string tid, int userId, string subject, int departmentId)
  {
    var filterdate = DateTime.Now.AddMinutes(-15);
    var ticketQuery = db.Tickets
      .Where(x => DateTime.Parse(x.Date) > filterdate && x.Email == email);
    if (userId > 0) ticketQuery = ticketQuery.Where(x => x.UserId == userId);

    if (ticketQuery.Count() > 10)
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Exceeded Limit of 10 Tickets within 15 Minutes");

    if (!string.IsNullOrEmpty(tid))
    {
      data.Status = 1;
      var ticket = db.Tickets.FirstOrDefault(x => x.Id == Convert.ToInt32(tid));
      if (ticket == null || (!ticket.Cc.Contains(email) && ticket.UserId != userId))
        return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Ticket ID Not Found For User");

      data.Cc = string.Join(",", cc);
      var replyId = await my_model.add_reply(data, Convert.ToInt32(tid), null, data.TicketAttachments);
      return replyId > 0 ? "Ticket Reply Imported Successfully" : "Ticket Import Failed";
    }

    if (db.get_option_compare("email_piping_only_registered", 1) && userId == 0)
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Blocked Ticket Opening from Unregistered User");

    if (db.get_option("email_piping_only_replies") == "1")
      return await FinalizeLogAndReturn(data, email, cc, tid, subject, "Only Replies Allowed by Email");

    data.DepartmentId = departmentId;
    data.Priority = db.get_option<int>("email_piping_default_priority");
    data.Cc = string.Join(",", cc);
    var newTid = await my_model.add(data, null, data.TicketAttachments);

    return newTid > 0 ? "Ticket Imported Successfully" : "Ticket Import Failed";
  }
}
