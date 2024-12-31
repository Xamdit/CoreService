using Service.Entities;
using Service.Framework;
using Service.Framework.Helpers;
using Service.Schemas;

namespace Service.Models;

public class AuthModel(MyInstance self, MyContext db) : MyModel(self)
{
  public UserSchema? Signin(string email, string password, bool isStaff = false)
  {
    try
    {
      var user = new UserSchema();
      if (isStaff)
      {
        var staff = db.Staff.FirstOrDefault(x => x.Email == email);
        if (staff != null && self.VerifyPassword(password, staff.Password))
        {
          user.Uuid = staff.Uuid;
          user.Firstname = staff.FirstName;
          user.Lastname = staff.LastName;
          user.Email = staff.Email;
          user.Fullname = $"{staff.FirstName} {staff.LastName}";
          user.Type = "admin";
        }
      }
      else
      {
        var client = db.Contacts.FirstOrDefault(x => x.Email == email);
        if (client != null && self.VerifyPassword(password, client.Password))
        {
          user.Uuid = client.Uuid;
          user.Firstname = client.FirstName;
          user.Lastname = client.LastName;
          user.Email = client.Email;
          user.Fullname = $"{client.FirstName} {client.LastName}";
          user.Type = "client";
        }
      }

      if (!string.IsNullOrEmpty(user.Type))
      {
        // await self.Cache.Init(result.Token);
        // await self.Cache.Set("user", result.Data);
        self.cache.assign("uuid", user.Uuid);
        // await self.Cache.Set("user_type", isStaff ? "admin" : "user");
        return user;
      }


      return null;
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex.Message);
    }

    return null;
  }
}
