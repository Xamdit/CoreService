using Global.Entities;

namespace Service.Models;

public class TwocheckoutModel
{
  public MyContext db = new();

  public bool add(TwoCheckoutLog data)
  {
    data.DateCreated = DateTime.Now;
    db.TwoCheckoutLogs.Add(data);
    db.SaveChanges();
    var insert_id = data.Id;
    return insert_id > 0;
  }

  public TwoCheckoutLog? get(string reference)
  {
    return db.TwoCheckoutLogs.FirstOrDefault(x => x.Reference == reference);
  }

  public void delete(int id)
  {
    db.TwoCheckoutLogs.RemoveRange(db.TwoCheckoutLogs.Where(x => x.Id == id));
    db.SaveChanges();
  }
}
