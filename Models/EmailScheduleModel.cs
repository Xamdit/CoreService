using Service.Entities;
using Service.Framework;

namespace Service.Models;

public class EmailScheduleModel(MyInstance self, MyContext db) : MyModel(self,db)
{
  public void create(int rel_id, string rel_type, ScheduledEmail data)
  {
    var contacts = data.Contacts;
    // if (is_array(contacts))
    //   contacts = string.Join(',', contacts);
    db.ScheduledEmails.Add(new ScheduledEmail
    {
      RelType = rel_type,
      RelId = rel_id,
      ScheduledAt = data.ScheduledAt,
      Contacts = contacts,
      Cc = data.Cc,
      AttachPdf = data.AttachPdf,
      Template = data.Template
    });
    db.SaveChanges();
  }

  public bool update(ScheduledEmail data)
  {
    // if (is_array($data['contacts'])) {
    //   data.Contacts = string.Join(',',  $data['contacts']);
    // }

    db.ScheduledEmails.Where(x => x.Id == data.Id)
      .Update(x => data);
    return db.SaveChanges() > 0;
  }

  public ScheduledEmail? getById(int id)
  {
    return db.ScheduledEmails.FirstOrDefault(x => x.Id == id);
  }

  public ScheduledEmail? get(int rel_id, string rel_type)
  {
    return db.ScheduledEmails.FirstOrDefault(x => x.RelId == rel_id && x.RelType == rel_type);
  }
}
