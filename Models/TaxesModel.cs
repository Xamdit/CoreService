using Global.Entities;
using Newtonsoft.Json;
using Service.Framework;
using Service.Helpers;
using Service.Helpers.Sale;

namespace Service.Models;

public class TaxesModel(MyInstance self) : MyModel(self)
{
  /**
   * Get tax by id
   * @param  mixed id tax id
   * @return mixed     if id passed return object else array
   */
  public List<Taxis> get()
  {
    var db = self.db();
    return db.Taxes.OrderBy(x => x.TaxRate).ToList();
  }

  public Taxis? get(int id)
  {
    var db = self.db();
    var row = db.Taxes.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
   * Add new tax
   * @param array data tax data
   * @return boolean
   */
  public bool add(Taxis data)
  {
    data.Id = 0;
    data.Name = data.Name.Trim();
    data.TaxRate = data.TaxRate.Trim();


    data = self.hooks.apply_filters("before_tax_created", data);
    var insert_id = data.Id;

    if (insert_id <= 0) return false;
    self.helper.log_activity("New Tax Added [ID: " + insert_id + ", " + data.Name + "]");
    self.hooks.do_action("after_tax_created", new
    {
      id = insert_id,
      data
    });
    return true;
  }

  /**
   * Edit tax
   * @param  array data tax data
   * @return boolean
   */
  public object edit(Taxis data)
  {
    var db = self.db();
    if (db.Expenses.Any(x => x.Tax == data.Id))
      return new { tax_is_using_expenses = true };

    var updated = false;
    var taxid = data.Id;
    var original_tax = self.helper.get_tax_by_id(taxid);

    data.Name = data.Name.Trim();
    data.TaxRate = data.TaxRate.Trim();

    data = self.hooks.apply_filters("before_update_tax", data);
    db.Taxes.Where(x => x.Id == taxid).Update(x => new Taxis
    {
      Name = data.Name,
      TaxRate = data.TaxRate
    });
    var affected_rows = db.SaveChanges();
    if (affected_rows > 0)
    {
      // Check if this task is used in settings
      var default_taxes = JsonConvert.DeserializeObject<List<Taxis>>(db.get_option("default_tax"));

      var i = 0;
      foreach (var current_tax in default_taxes.Select(tax => get(taxid)))
      {
        var tax_name = original_tax.Name + "|" + original_tax.TaxRate;
        if (tax_name.Contains("x" + tax_name)) default_taxes[i].TaxRate = default_taxes[i].TaxRate.Replace(tax_name, current_tax.Name + "|" + current_tax.TaxRate);
        default_taxes[i].TaxRate.Replace(tax_name, current_tax.Name + "|" + current_tax.TaxRate);
        i++;
      }

      db.update_option("default_tax", JsonConvert.SerializeObject(default_taxes));
      updated = true;
    }

    self.hooks.do_action("after_update_tax", new
    {
      Id = taxid,
      data,
      Updated = updated
    });

    if (updated) log_activity("Tax Updated [ID: " + taxid + ", " + data.Name + "]");

    return updated;
  }

  /**
   * Delete tax from database
   * @param  mixed id tax id
   * @return boolean
   */
  public bool delete(int id)
  {
    var db = self.db();
    // if (
    //   is_reference_in_table('tax', 'items', id)
    //   || is_reference_in_table('tax2', 'items', id)
    //   || is_reference_in_table('tax', 'expenses', id)
    //   || is_reference_in_table('tax2', 'expenses', id)
    //   || is_reference_in_table('tax_id', 'subscriptions', id)
    //   || is_reference_in_table('tax_id_2', 'subscriptions', id)
    // )
    //   return new { referenced = true };

    db.Taxes.Where(x => x.Id == id).Delete();
    if (db.SaveChanges() <= 0) return false;
    log_activity("Tax Deleted [ID: " + id + "]");
    return true;
  }
}
