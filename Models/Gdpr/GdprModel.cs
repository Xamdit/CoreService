using System.Linq.Expressions;
using Global.Entities;
using Microsoft.EntityFrameworkCore;
using Service.Framework;

namespace Service.Models.Gdpr;

public class GdprModel(MyInstance self, MyContext db) : MyModel(self)
{
  public int AddRequest(GdprRequest data)
  {
    if (string.IsNullOrEmpty(data.Status)) data.Status = "pending";
    data.RequestDate = today();
    var result = db.GdprRequests.Add(data);
    return result.Entity.Id;
  }

  public int AddRemovalRequestAsync(GdprRequest data)
  {
    data.RequestType = "account_removal";
    return AddRequest(data);
  }

  public async Task<bool> UpdateRequestAsync(int id, GdprRequest data)
  {
    var existingRequest = await db.GdprRequests.FindAsync(id);
    if (existingRequest == null)
      return false;

    db.Entry(existingRequest).CurrentValues.SetValues(data);
    return await db.SaveChangesAsync() > 0;
  }

  public async Task<List<GdprRequest>> GetRemovalRequestsAsync()
  {
    return await db.GdprRequests
      .Where(r => r.RequestType == "account_removal")
      .OrderByDescending(r => r.RequestDate)
      .ToListAsync();
  }

  public List<ConsentPurposeDto> get_consent_purposes(int? userId = null, string forType = "")
  {
    var query = db.ConsentPurposes.Select(p => new ConsentPurposeDto
    {
      Id = p.Id,
      Name = p.Name,
      TotalUsage = db.Consents.Count(c => c.PurposeId == p.Id),
      ConsentGiven = userId.HasValue && forType != "" && db.Consents.Any(c => EF.Property<int>(c, forType + "Id") == userId && c.PurposeId == p.Id && c.Action == "opt-in"),
      LastActionIsOptOut = userId.HasValue && forType != "" && db.Consents.Any(c => EF.Property<int>(c, forType + "Id") == userId && c.PurposeId == p.Id && c.Action == "opt-out"),
      ConsentLastUpdated = userId.HasValue && forType != ""
        ? db.Consents.Where(c => EF.Property<int>(c, forType + "Id") == userId && c.PurposeId == p.Id)
          .OrderByDescending(c => c.DateCreated)
          .Select(c => c.DateCreated)
          .FirstOrDefault()
        : null
    });

    return query.OrderByDescending(p => p.Name).ToList();
  }

  public ConsentPurposeDto? get_consent_purpose(int id)
  {
    return db.ConsentPurposes
      .Where(p => p.Id == id)
      .Select(p => new ConsentPurposeDto
      {
        Id = p.Id,
        Name = p.Name,
        TotalUsage = db.Consents.Count(c => c.PurposeId == p.Id)
      })
      .FirstOrDefault();
  }

  public int add_consent_purpose(ConsentPurpose data)
  {
    data.DateCreated = DateTime.UtcNow;
    db.ConsentPurposes.Add(data);
    db.SaveChanges();
    return data.Id;
  }

  public async Task<bool> UpdateConsentPurposeAsync(int id, ConsentPurpose data)
  {
    var existingPurpose = await db.ConsentPurposes.FindAsync(id);
    if (existingPurpose == null)
      return false;

    db.Entry(existingPurpose).CurrentValues.SetValues(data);
    existingPurpose.LastUpdated = today();
    return await db.SaveChangesAsync() > 0;
  }

  public async Task<bool> DeleteConsentPurposeAsync(int id)
  {
    var existingPurpose = await db.ConsentPurposes.FindAsync(id);
    if (existingPurpose == null)
      return false;

    db.ConsentPurposes.Remove(existingPurpose);
    var relatedConsents = db.Consents.Where(c => c.PurposeId == id);
    db.Consents.RemoveRange(relatedConsents);

    return await db.SaveChangesAsync() > 0;
  }

  public int add_consent(Consent data)
  {
    // data.Date = data.Date ?? DateTime.UtcNow;
    data.Ip = data.Ip ?? "user-ip-placeholder"; // Replace with actual IP fetching logic
    db.Consents.Add(data);
    db.SaveChanges();
    return data.Id;
  }

  public List<ConsentDto> GetConsentsAsync(Expression<Func<Consent, bool>>? condition = null)
  {
    var query = db.Consents
      .Include(c => c.Purpose)
      .OrderByDescending(c => c.DateCreated);

    if (condition != null) query.Where(condition);

    var rows = query.Select(c => new ConsentDto
      {
        Id = c.Id,
        PurposeName = c.Purpose.Name,
        Date = c.DateCreated,
        Action = c.Action,
        Ip = c.Ip
      })
      .ToList();
    return rows;
  }
}
