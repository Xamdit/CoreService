using Service.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers.Countries;

public static class CountryExtension
{
  public static Country? get_country(this MyContext db, int country_id)
  {
    var output = db.Countries.FirstOrDefault(x => x.Id == country_id);
    return output;
  }
}
