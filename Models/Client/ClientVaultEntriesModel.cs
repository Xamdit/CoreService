using System.Linq.Expressions;
using Global.Entities;
using Global.Entities.Tools;
using Service.Framework;

namespace Service.Models.Client;

public class ClientVaultEntriesModel(MyInstance self, MyContext db) : MyModel(self)
{
  /**
       * Get single vault entry
       * @param  mixed id vault entry id
       * @return object
       */
  public Vault get(int id)
  {
    var output = db.Vaults.Find(id);
    return output;
  }

  /**
   * Get customer vault entries
   * @param  mixed customer_id
   * @param  array  where       additional wher
   * @return array
   */
  public List<Vault> get_by_customer_id(int customer_id, Expression<Func<Vault, bool>> where)
  {
    var rows = db.Vaults.Where(x => x.CustomerId == customer_id).OrderByDescending(x => x.DateCreated).Where(where).ToList();
    return rows;
  }

  /**
   * Create new vault entry
   * @param  array data        _POST data
   * @param  mixed customer_id customer id
   * @return boolean
   */
  public bool create(Vault data, int customer_id)
  {
    data.DateCreated = DateTime.Now;
    data.CustomerId = customer_id;
    var result = db.Vaults.Add(data);
    log_activity("Vault Entry Created [Customer ID: " + customer_id + "]");
    return result.IsAdded();
  }

  /**
   * Update vault entry
   * @param  mixed id   vault entry id
   * @param  array data _POST data
   * @return boolean
   */
  public bool update(int id, Vault data)
  {
    var vault = get(id);
    var last_updated_from = data.LastUpdatedFrom;
    data.LastUpdatedFrom = string.Empty;
    db.Vaults.Where(x => x.Id == id).Update(x => data);
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    db.Vaults
      .Where(x => x.Id == id)
      .Update(x =>
        new Vault
        {
          LastUpdated = today(),
          LastUpdatedFrom = last_updated_from
        });
    db.SaveChanges();
    log_activity("Vault Entry Updated [Customer ID: " + vault.CustomerId + "]");
    return true;
  }

  /**
   * Delete vault entry
   * @param  mixed id entry id
   * @return boolean
   */
  public bool delete(int id)
  {
    var vault = get(id);
    db.Vaults.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity("Vault Entry Deleted [Customer ID: " + vault.CustomerId + "]");
    self.hooks.do_action("customer_vault_entry_deleted", new Vault { Id = id, CustomerId = vault.CustomerId });
    return true;
  }
}
