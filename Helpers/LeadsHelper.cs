using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;

namespace Service.Helpers;

public static class LeadsHelper
{
  /**
* Lead public form URL
* @param  mixed $id lead id
* @return string
*/
  public static string leads_public_url(this MyModel model, int id)
  {
    var hash = model.get_lead_hash(id);
    return site_url($"forms/l/{hash}");
  }

  /**
 * Get and generate lead hash if don't exists.
 * @param  mixed $id  lead id
 * @return string
 */
  public static string get_lead_hash(this MyModel model, int id)
  {
    var (self, db) = model.getInstance();
    var hash = string.Empty;
    var lead = db.Leads.FirstOrDefault(x => x.Id == id);
    if (lead == null) return hash;
    hash = lead.Hash;
    if (!string.IsNullOrEmpty(hash)) return hash;
    hash = uuid();
    db.Leads
      .Where(x => x.Id == id)
      .UpdateAsync(x => new Lead
      {
        Hash = hash
      });
    return hash;
  }
}
