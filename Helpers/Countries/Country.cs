using Service.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers.Countries;

public static class CountryExtension
{
  public static Country? get_country(this HelperBase helper, int country_id)
  {
    var (self, db) = getInstance();
    var output = db.Countries.FirstOrDefault(x => x.Id == country_id);
    return output;
  }
}
