using Global.Entities;
using Service.Framework;

namespace Service.Models;

public class SpamFiltersModel(MyInstance self, MyContext db) : MyModel(self)
{
  public List<SpamFilter> get(string rel_type)
  {
    var rows = db.SpamFilters.Where(x => x.RelType == rel_type).ToList();
    return rows;
  }

  public int add(SpamFilter data, string type)
  {
    data.RelType = type;
    db.SpamFilters.Add(data);
    db.SaveChanges();
    var insert_id = data.Id;
    return insert_id > 0 ? insert_id : 0;
  }

  public bool edit(SpamFilter data)
  {
    db.SpamFilters.Where(x => x.Id == data.Id).Update(x => data);
    var affected_rows = db.SaveChanges();
    return affected_rows > 0;
  }

  public bool delete(int id, string type)
  {
    db.SpamFilters.Where(x => x.Id == id && x.RelType == type).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity("Spam Filter Deleted");
    return true;
  }

  public string check(string email, string subject, string message, string rel_type)
  {
    var status = string.Empty;
    var spam_filters = get(rel_type);

    foreach (var filter in spam_filters)
    {
      var type = filter.Type;
      var value = filter.Value;
      status = type switch
      {
        "sender" when value.ToLower() == email.ToLower() => "Blocked Sender",
        "subject" when ("x" + subject).ToLower().Contains(value.ToLower()) => "Blocked Subject",
        "phrase" when ("x" + subject).ToLower().Contains(value.ToLower()) => "Blocked Phrase",
        _ => status
      };
    }

    return status;
  }
}
