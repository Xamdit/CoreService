using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Models;

public class UserAutoLoginModel(MyInstance self, MyContext db) : MyModel(self)
{
  /**
 * Check if autologin found
 * @param int userId clientid/staffid
 * @param string key key from cookie to retrieve from database
 * @return UserAutoLogin or null
 */
  public async Task<UserAutoLogin> get(int userId, string key)
  {
    var db = self.db();
    // Check if user exists in `UserAutoLogin` table
    var userAutoLogin = db.UserAutoLogins.FirstOrDefault(u => u.UserId == userId && u.Key == key);

    if (userAutoLogin == null) return null;

    // Determine if the user is staff or a contact
    if (userAutoLogin.IsStaff)
    {
      // Use LINQ method chaining for staff
      var staff = db.Staff
        .Join(db.UserAutoLogins,
          s => s.Id,
          u => u.UserId,
          (s, u) => new { s, u })
        .Where(joined => joined.u.UserId == userId && joined.u.Key == key)
        .Select(joined => new
        {
          joined.s.Id,
          Staff = true
        })
        .FirstOrDefault();

      if (staff != null)
        return new UserAutoLogin
        {
          UserId = staff.Id,
          IsStaff = true
        };
    }
    else
    {
      // Use LINQ method chaining for contacts
      var contact = db.Contacts
        .Join(db.UserAutoLogins,
          c => c.Id,
          u => u.UserId,
          (c, u) => new { c, u })
        .Where(joined => joined.u.UserId == userId && joined.u.Key == key)
        .Select(joined => new
        {
          joined.c.Id,
          Staff = false
        })
        .FirstOrDefault();

      if (contact != null)
        return new UserAutoLogin
        {
          UserId = contact.Id,
          IsStaff = false
        };
    }

    return null;
  }

  /**
   * Set new autologin if user have clicked remember me
   * @param mixed user_id clientid/userid
   * @param string key     cookie key
   * @param integer staff   is staff or client
   */
  public bool set(int user_id, string key, bool is_staff)
  {
    var db = self.db();
    db.UserAutoLogins.Add(new UserAutoLogin
    {
      UserId = user_id,
      Key = key,
      // UserAgent = Request.UserAgent,
      // LastIp = Request.UserHostAddress,
      IsStaff = is_staff
    });
    db.SaveChanges();
    return true;
  }

  /**
   * Delete user autologin
   * @param  mixed user_id clientid/userid
   * @param  string key     cookie key
   * @param integer staff   is staff or client
   */
  public void delete(int user_id, string key, bool is_staff)
  {
    var db = self.db();
    db.UserAutoLogins.RemoveRange(
      db.UserAutoLogins.Where(x => x.UserId == user_id && x.Key == key && x.IsStaff == is_staff)
    );
    db.SaveChanges();
  }
}
