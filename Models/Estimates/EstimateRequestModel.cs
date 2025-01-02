using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Extensions;
using Service.Helpers;
using File = Service.Entities.File;

namespace Service.Models.Estimates;

public class EstimateRequestModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  public const int STATUS_PROCESSING = 2;

  public bool update_request_assigned(EstimateRequest data)
  {
    var oldAssigned = db.EstimateRequests
      .FirstOrDefault(x => x.Id == data.Id);

    db.EstimateRequests
      .Where(x => x.Id == data.Id)
      .Update(x => new EstimateRequest { Assigned = data.Assigned });

    var currentAssigned = db.EstimateRequests
      // .Select(x => x.Assigned)
      .FirstOrDefault(x => x.Id == data.Id);

    if (db.SaveChanges() <= 0) return false;
    if (currentAssigned != oldAssigned && oldAssigned != null)
    {
      var logMessage = "not_estimate_request_activity_assigned_updated";
      // Trigger hooks
      hooks.do_action("estimate_request_assigned_changed", new
      {
        estimate_request_id = data.Id,
        old_staff = oldAssigned,
        new_staff = currentAssigned
      });
      log_activity(logMessage + " staff [id:  " + currentAssigned.Assigned + "]");
    }

    if (currentAssigned == oldAssigned) return false;
    assigned_member_notification(data.Id, currentAssigned.Id);
    return true;
  }

  public bool update_request_status(EstimateRequest data)
  {
    var oldStatus = db.EstimateRequests
      .FirstOrDefault(x => x.Id == data.Id);

    var oldStatusName = get_status_name(oldStatus.Id);
    var currentStatusName = get_status_name(data.Status.Value);

    db.EstimateRequests
      .Where(x => x.Id == data.Id)
      .Update(x => new EstimateRequest { Status = data.Status });

    if (db.SaveChanges() <= 0) return false;
    if (currentStatusName == oldStatusName || string.IsNullOrEmpty(oldStatusName)) return true;
    var logMessage = "not_estimate_request_activity_status_updated";

    hooks.do_action("estimate_request_status_changed", new
    {
      estimate_request_id = data.Id,
      old_status = oldStatusName,
      new_status = currentStatusName
    });

    return true;
  }

  public List<EstimateRequest> Get(int id = 0, Dictionary<string, object> where = null)
  {
    var query = db.EstimateRequests
      .Include(x => x.FromForm)
      .Where(x => EF.Functions.Like(x.Id.ToString(), where["id"].ToString()));
    if (id <= 0) return query.ToList();
    var requests = query.ToList();
    var rows = requests
      .Where(x => x.FromFormId != 0)
      .ToList()
      .Select(x =>
      {
        x.FromForm = get_form(new { id = x.FromFormId });
        // x.Attachments = get_estimate_request_attachments(x.Id);
        return x;
      })
      .ToList();
    return rows;
  }

  public List<EstimateRequestForm> get_forms()
  {
    return db.EstimateRequestForms.ToList();
  }

  public EstimateRequestForm? get_form(object where)
  {
    return db.EstimateRequestForms
      .FirstOrDefault(x => EF.Functions.Like(x.Id.ToString(), where.ToString()));
  }

  public int AddForm(EstimateRequestForm data)
  {
    data.SuccessSubmitMsg = data.SuccessSubmitMsg.Replace("\n", "<br>");
    data.FormKey = self.helper.uuid();
    data.DateCreated = DateTime.Now;

    db.EstimateRequestForms.Add(data);
    db.SaveChanges();

    log_activity($"New Estimate Request Form Added [{data.Name}]");
    return data.Id;
  }

  public bool UpdateForm(int id, EstimateRequestForm data)
  {
    data.SuccessSubmitMsg = data.SuccessSubmitMsg.Replace("\n", "<br>");
    db.EstimateRequestForms
      .Where(x => x.Id == id)
      .Update(x => data);

    return db.SaveChanges() > 0;
  }

  public bool DeleteForm(int id)
  {
    db.EstimateRequestForms.Remove(db.EstimateRequestForms.Find(id));
    db.EstimateRequests
      .Where(x => x.FromFormId == id)
      .Update(x => new EstimateRequest { FromFormId = 0 });

    return db.SaveChanges() > 0;
  }

  public string get_status_name(int statusId)
  {
    var row = db.EstimateRequestStatuses
      // .Select(x => x.Name)
      .FirstOrDefault(x => x.Id == statusId);
    return row?.Name;
  }

  public List<File> get_estimate_request_attachments(int id)
  {
    var rows = db.Files
      .Where(x => x.RelId == id && x.RelType == "estimate_request")
      .OrderByDescending(x => x.DateCreated)
      .ToList();
    return rows;
  }

  public bool assigned_member_notification(int estimateRequestId, int assigned)
  {
    if (assigned == 0) return false;
    var notification = new Notification
    {
      Description = "estimate_request_assigned_to_staff",
      ToUserId = assigned,
      Link = $"estimate_request/view/{estimateRequestId}"
    };

    db.add_notification(notification);
    var email = db.Staff
      .FirstOrDefault(x => x.Id == assigned);
    db.send_mail_template("estimate_request_assigned", estimateRequestId, email);
    return true;
  }

  public EstimateRequestStatus? get_status_by_flag(string flag)
  {
    var row = db.EstimateRequestStatuses.FirstOrDefault(x => x.Flag == flag);
    return row;
  }
}
