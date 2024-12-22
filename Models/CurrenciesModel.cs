using Global.Entities;
using Service.Framework;

namespace Service.Models;

public class CurrenciesModel(MyInstance self, MyContext db) : MyModel(self)
{
  /**
   * @param  integer ID (optional)
   * @return mixed
   * Get currency object based on passed id if not passed id return array of all currencies
   */
  public List<Currency> get()
  {
    var currencies = app_object_cache.get<List<Currency>>("currencies-data");
    if (currencies != null) return currencies;
    currencies = db.Currencies.ToList();
    app_object_cache.add("currencies-data", currencies);
    return currencies;
  }

  public Currency get(int id)
  {
    var currency = db.Currencies.FirstOrDefault(x => x.Id == id);
    app_object_cache.add("currency-" + currency.Name, currency);
    return currency;
  }

  /**
   * Get currency by name/iso code
   * @since  2.3.2
   * @param  string name currency name/iso code
   * @return object
   */
  public Currency get_by_name(string name)
  {
    var currency = app_object_cache.get<Currency>("currency-" + name);
    if (currency != null) return currency;
    currency = db.Currencies.FirstOrDefault(x => x.Name == name);
    app_object_cache.add("currency-" + name, currency);

    return currency;
  }

  /**
   * @param array _POST data
   * @return boolean
   */
  public bool add(Currency data)
  {
    // unset(data['currencyid']);
    data.Name = data.Name.ToUpper();
    db.Currencies.Add(data);
    db.SaveChanges();
    var insert_id = data.Id;
    if (insert_id <= 0) return false;
    log_activity($"New Currency Added [ID: {data.Name}]");
    return true;
  }

  /**
   * @param  array _POST data
   * @return boolean
   * Update currency values
   */
  public bool edit(Currency data)
  {
    var currencyid = data.Id;
    data.Name = data.Name.ToUpper();
    db.Currencies.Where(x => x.Id == currencyid)
      .Update(x => data);
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    log_activity("Currency Updated [" + data.Name + "]");
    return true;
  }

  /**
   * @param  integer ID
   * @return mixed
   * Delete currency from database, if used return array with key referenced
   */
  public dynamic delete(int id)
  {
    // foreach (var tt in app.get_tables_with_currency())
    //   if (db.is_reference_in_table(tt['field'], tt['table'], id))
    //     return new { referenced = true };

    var currency = get(id);
    if (currency.IsDefault) return new { is_default = true };


    db.Currencies.Where(x => x.Id == id).Delete();
    var affected_rows = db.SaveChanges();


    if (affected_rows <= 0) return false;
    // var columns = db.list_fields('items');
    // foreach (var column in columns)
    //   if (column == "rate_currency_" + id)
    //     this.dbforge.drop_column("items", $"rate_currency_{id}");
    log_activity($"Currency Deleted [{id}]");
    return true;
  }

  /**
   * @param  integer ID
   * @return boolean
   * Make currency your base currency for better using reports if found invoices with more then 1 currency
   */
  public object make_base_currency(int id)
  {
    var @base = get_base_currency();
    // foreach (var tt in db.get_tables_with_currency())
    //   if (db.is_reference_in_table(tt.field, tt.table, @base.Id))
    //     return new { has_transactions_currency = true };
    db.Currencies.Where(x => x.Id == id).Update(x => new Currency { IsDefault = true });
    var affected_rows = db.SaveChanges();
    if (affected_rows <= 0) return false;
    db.Currencies.Where(x => x.Id != id).Update(x => new Currency { IsDefault = false });
    db.SaveChanges();
    return true;
  }

  /**
   * @return object
   * Get base currency
   */
  public Currency get_base_currency()
  {
    var output = db.Currencies.First(x => x.IsDefault);
    return output;
  }

  /**
   * @param  integer ID
   * @return string
   * Get the symbol from the currency
   */
  public string get_currency_symbol(int id = 0)
  {
    if (id == 0) id = get_base_currency().Id;

    var currencies = app_object_cache.get<List<Currency>>("currencies-data");
    return currencies == null
      ? db.Currencies.First(x => x.Id == id).Symbol
      : currencies.Where(currency => currency.Id == id).Select(currency => currency.Symbol).First();
  }
}
