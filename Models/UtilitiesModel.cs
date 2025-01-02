using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers.Entities.Extras;
using Service.Helpers;
using Service.Models.Invoices;

namespace Service.Models;

public class UtilitiesService(MyInstance self, MyContext db) : MyModel(self,db)
{
  public async Task<bool> AddOrUpdateEvent(Event eventData)
  {
    eventData.UserId = staff_user_id;
    // eventData.Start = ToSqlDate(eventData.Start);
    eventData.End ??= today();
    eventData.Public = eventData.Public == 1 ? 1 : 0;
    eventData.Description = eventData.Description.Replace("\n", "<br>");

    if (eventData.Id != 0)
    {
      var existingEvent = db.Events.FirstOrDefault(e => e.Id == eventData.Id);

      if (existingEvent == null)
        return false;

      if (existingEvent.IsStartNotified == 1 && DateTime.Parse(eventData.Start) > DateTime.Parse(existingEvent.Start))
        eventData.IsStartNotified = 0;

      eventData = hooks.apply_filters("event_update_data", eventData);

      db.Entry(existingEvent).CurrentValues.SetValues(eventData);
      return true;
    }

    eventData = hooks.apply_filters("event_create_data", eventData);

    db.Events.Add(eventData);


    return true;
  }

  public Event GetEventById(int id)
  {
    return db.Events
      .Where(e => e.Id == id)
      .FirstOrDefault();
  }

  public async Task<List<Event>> GetAllEvents(DateTime start, DateTime end)
  {
    var userId = staff_user_id;
    var isStaffMember = self.helper.is_staff_member();

    return db.Events
      .Where(e => (DateTime.Parse(e.Start) >= start && DateTime.Parse(e.Start) <= end && e.UserId == userId) || (isStaffMember && e.Public == 1))
      // .Select(e => new { e.Title, e.Start, e.End, e.Id, e.UserId, e.Color, e.Public })
      .ToList();
  }

  public async Task<List<Invoice>> GetInvoicesForCalendar(DateTime start, DateTime end, int? clientId = null)
  {
    var query = db.Invoices
      .Where(i => DateTime.Parse(i.DueDate) >= start && DateTime.Parse(i.DueDate) <= end)
      .Where(i => i.Status != InvoiceStatus.STATUS_PAID && i.Status != InvoiceStatus.STATUS_CANCELLED);

    if (clientId.HasValue) query = query.Where(i => i.ClientId == clientId);

    return query
      .Select(i => new Invoice
      {
        Id = i.Id,
        Number = i.Number,
        ClientId = i.ClientId,
        DueDate = i.DueDate
      })
      .ToList();
  }

  public async Task<List<Estimate>> GetEstimatesForCalendar(DateTime start, DateTime end, int? clientId = null)
  {
    var query = db.Estimates
      .Where(e => e.ExpiryDate >= start && e.ExpiryDate <= end && e.Status != EstimateStatus.Rejected);

    if (clientId.HasValue) query = query.Where(e => e.ClientId == clientId);

    return query
      .Select(e => new Estimate
      {
        Id = e.Id,
        Number = e.Number,
        ClientId = e.ClientId,
        ExpiryDate = e.ExpiryDate ?? e.DateCreated
      })
      .ToList();
  }
}
