using Service.Core.Extensions;
using Service.Entities;


namespace Service.Helpers;

// info : complete
public static class GdprHelper
{
  public static void send_gdpr_email_template(this MyModel model, string template, int user_id)
  {
    var (self, db) = model.getInstance();
    var staff_model = self.staff_model(db);
    ignore(() =>
    {
      var staff = staff_model.get(x => x.Active.Value && x.IsAdmin);
      staff.ForEach(member => { db.send_mail_template(template, member, user_id); });
    });
  }

  public static bool is_gdpr(this MyContext db)
  {
    return db.get_option_compare("enable_gdpr", "1");
  }
}
