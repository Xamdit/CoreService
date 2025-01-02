using Service.Framework.Core.Engine;


namespace Service.Helpers;

// info : complete
public static class GdprHelper
{
  public static void send_gdpr_email_template(this HelperBase helper, string template, int user_id)
  {
    var (self, db) = getInstance();
    var staff_model = self.staff_model(db);
    ignore(() =>
    {
      var staff = staff_model.get(x => x.Active.Value && x.IsAdmin);
      staff.ForEach(member => { self.helper.send_mail_template(template, member, user_id); });
    });
  }

  public static bool is_gdpr(this HelperBase helper)
  {
    var (self, db) = getInstance();
    return db.get_option_compare("enable_gdpr", "1");
  }
}
