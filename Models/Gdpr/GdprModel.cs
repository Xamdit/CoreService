using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework;

namespace Service.Models.Gdpr;

public class GdprModel(MyInstance self, MyContext db) : MyModel(self, db)
{
  public bool AddRequest(GdprRequest data)
  {
    if (string.IsNullOrEmpty(data.Status)) data.Status = "pending";
    data.RequestDate = today();
    var result = db.GdprRequests.Add(data);
    return result.State == EntityState.Added;
  }

  public bool add_removal_request(GdprRequest data)
  {
    data.RequestType = "account_removal";
    return AddRequest(data);
  }

  public async Task<bool> update(int id, GdprRequest data)
  {
    var existingRequest = await db.GdprRequests.FindAsync(id);
    if (existingRequest == null)
      return false;

    db.Entry(existingRequest).CurrentValues.SetValues(data);
    return await db.SaveChangesAsync() > 0;
  }

  public List<GdprRequest> get_removal_requests()
  {
    return db.GdprRequests
      .Where(r => r.RequestType == "account_removal")
      .OrderByDescending(r => r.RequestDate)
      .ToList();
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

  public bool update_consent_purpose(int id, ConsentPurpose data)
  {
    var existingPurpose =   db.ConsentPurposes.Find(id);
    if (existingPurpose == null)
      return false;

    db.Entry(existingPurpose).CurrentValues.SetValues(data);
    existingPurpose.LastUpdated = today();
    return   db.SaveChanges() > 0;
  }

  public bool delete_consent_purpose(int id)
  {
    var existingPurpose =   db.ConsentPurposes.Find(id);
    if (existingPurpose == null)
      return false;

    db.ConsentPurposes.Remove(existingPurpose);
    var relatedConsents = db.Consents.Where(c => c.PurposeId == id);
    db.Consents.RemoveRange(relatedConsents);

    return   db.SaveChanges() > 0;
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
