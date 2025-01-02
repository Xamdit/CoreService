using Service.Core.Extensions;
using Service.Entities;
using Service.Framework;
using Service.Framework.Core.Engine;
using Service.Framework.Core.Extensions;
using Service.Helpers.Database;
using SqlKata.Execution;

namespace Service.Helpers.Sale;

public static class SaleHelper
{
  /**
 * Get all items by type eq. invoice, proposal, estimates, credit note
 * @param  string  type rel_type value
 * @return array
 */
  public static List<Itemable> get_items_by_type(this MyContext db, string type, int id)
  {
    return db.Itemables
      .Where(x => x.RelType == type && x.RelId == id)
      .OrderBy(x => x.ItemOrder)
      .ToList();
  }

  /**
   * Helper function to get currency by ID or by Name
   * @since  2.3.2
   * @param  mixed id_or_name
   * @return object
   */
  public static Currency? get_currency(this MyContext db, object id_or_name)
  {
    return id_or_name is int
      ? db.Currencies.FirstOrDefault(x => x.Id == Convert.ToInt32(id_or_name))
      : db.Currencies.FirstOrDefault(x => x.Name == id_or_name.ToString());
  }


  /**
 * Helper function to get tax by passedid
 * @param  integer id taxid
 * @return object
 */
  public static Taxis? get_tax_by_id(this MyContext db, int id)
  {
    var row = db.Taxes.FirstOrDefault(x => x.Id == id);
    return row;
  }

  /**
 * Return decimal places
 * The srcipt do not support more then 2 decimal places but developers can use action hook to change the decimal places
 * @return [type] [description]
 */
  public static int get_decimal_places()
  {
    // return hooks.apply_filters("app_decimal_places", 2);
    return 2;
  }

  /**
 * Helper function to get tax by passed name
 * @param  string $name tax name
 * @return object
 */
  public static Taxis? get_tax_by_name(this MyContext db, string name)
  {
    var row = db.Taxes.FirstOrDefault(x => x.Name == name);
    return row;
  }

  /**
 * Get base currency
 * @since  2.3.2
 * @return object
 */
  public static Currency get_base_currency(this MyContext db)
  {
    var self = new MyInstance();
    var currencies_model = self.currencies_model(db);
    return currencies_model.get_base_currency();
  }


  public static string sales_number_format(this MyContext db, int number, int format, string appliedPrefix, DateTime date)
  {
    var originalNumber = number;
    var prefixPadding = db.get_option<int>("number_padding_prefixes"); // Assuming GetOption is a method to fetch options

    var formattedNumber = format switch
    {
      1 => // Number based
        appliedPrefix + number.ToString().PadLeft(prefixPadding, '0'),
      2 => // Year based
        appliedPrefix + date.Year + "/" + number.ToString().PadLeft(prefixPadding, '0'),
      3 => // Number-yy based
        appliedPrefix + number.ToString().PadLeft(prefixPadding, '0') + "-" + date.ToString("yy"),
      4 => // Number-mm-yyyy based
        appliedPrefix + number.ToString().PadLeft(prefixPadding, '0') + "/" + date.ToString("MM") + "/" + date.Year,
      _ => number.ToString()
    };

    // Assuming ApplyFilters is a method similar to the hooks()->apply_filters in PHP
    hooks.apply_filters("sales_number_format", new
    {
      formattedNumber,
      format,
      date,
      number = originalNumber,
      prefixPadding
    });
    return formattedNumber;
  }

  /**
   * Format money/amount based on currency settings
   * @since  2.3.2
   * @param  mixed    amount          amount to format
   * @param  mixed    currency        currency db object or currency name (ISO code)
   * @param  boolean $excludeSymbol   whether to exclude to symbol from the format
   * @return string
   */
  public static string app_format_money(this MyContext db, decimal amount, object id_or_name, bool excludeSymbol = false)
  {
    var currency = new Currency();
    var dbCurrency = db.get_currency(currency);
    if (dbCurrency != null)
      currency = dbCurrency;
    else
      currency = new Currency
      {
        Symbol = currency.Symbol,
        Name = currency.Name,
        Placement = "before",
        DecimalSeparator = db.get_option("decimal_separator"),
        ThousandSeparator = db.get_option("thousand_separator")
      };


    var symbol = !excludeSymbol ? currency.Symbol : "";
    var d = db.get_option_compare("remove_decimals_on_zero", 1) && !(amount is decimal) ? 0 : get_decimal_places();

    var amountFormatted = number_format(
      amount,
      d
      // currency.DecimalSeparator,
      // currency.ThousandSeparator
    );


    var formattedWithCurrency = currency.Placement == "after" ? amountFormatted + "" + symbol : symbol + "" + amountFormatted;

    // return
    // hooks.apply_filters("app_format_money", formattedWithCurrency, new
    // {
    //   amount,
    //   currency,
    //   excludeSymbol,
    //   decimal_places = d
    // });
    return formattedWithCurrency;
  }


  /**
* Function that update total tax in sales table eq. invoice, proposal, estimates, credit note
* @param  mixed id
* @return void
*/
  private static List<Taxis> call_user_func(string func_taxes, int id)
  {
    return new List<Taxis>();
  }

  public static void update_sales_total_tax_column(this MyContext db, int id, string type, string table)
  {
    var data = db.kata(table).Where("id", id).Get().FirstOrDefault();
    var items = db.get_items_by_type(type, id);

    double total_tax = 0;
    // var taxes = new List<TaxInfo>();
    var taxes = new Dictionary<string, TaxInfo>();
    // var taxes = new TaxInfo();
    var _calculated_taxes = new List<string>();
    var func_taxes = $"get_{type}_item_taxes";
    foreach (var item in items)
    {
      var item_taxes = call_user_func(func_taxes, item.Id);
      if (!item_taxes.Any()) continue;
      foreach (var tax in item_taxes)
      {
        var calc_tax = 0;
        var tax_not_calc = false;
        if (!_calculated_taxes.Contains(tax.Name))
        {
          _calculated_taxes.Add(tax.Name);
          tax_not_calc = true;
        }

        if (tax_not_calc)
        {
          var info = new TaxInfo();
          // taxes.Add(tax.Name, new TaxInfo());
          var total = item.Qty * item.Rate / 100 * Convert.ToDouble(tax.TaxRate);
          info.Totals.Add(total);
          info.TaxName = tax.Name;
          info.TaxRate = Convert.ToDecimal(tax.TaxRate);
          taxes.Add(tax.Name, info);
        }
        else
        {
          taxes[tax.Name].Totals.Add(item.Qty * item.Rate / 100 * Convert.ToDouble(tax.TaxRate));
        }
      }
    }

    total_tax = taxes.Select(kvp =>
    {
      var total = kvp.Value.Totals.Sum();
      if (data.discount_percent != 0 && data.discount_type == "before_tax")
      {
        var total_tax_calculated = total * data.discount_percent / 100;
        total -= total_tax_calculated;
      }
      else if (data.discount_total != 0 && data.discount_type == "before_tax")
      {
        var t = data.discount_total / data.subtotal * 100;
        total -= total * t / 100;
      }

      return total;
    }).Sum();

    db.kata(table).Where("id", id).Update(new { total_tax });
  }


  /**
 * Add new item do database, used for proposals,estimates,credit notes,invoices
 * This is repetitive action, that's why this function exists
 * @param array  item     item from $_POST
 * @param mixed $rel_id   relation id eq. invoice id
 * @param string rel_type relation type eq invoice
 */
  public static int add_new_sales_item_post(this MyContext db, Itemable item, int rel_id, string rel_type)
  {
    var custom_fields = new List<CustomField>();
    // if (item["custom_fields"])
    //   custom_fields = item["custom_fields"];

    var result = db
      .Itemables
      .Add(new Itemable
      {
        Description = item.Description,
        LongDescription = item.LongDescription,
        Qty = item.Qty,
        Rate = item.Rate,
        RelId = rel_id,
        RelType = rel_type,
        ItemOrder = item.ItemOrder,
        Unit = item.Unit
      });
    var id = result.Entity.Id;
    if (custom_fields != null)
      db.handle_custom_fields_post(id, custom_fields);

    return id;
  }

  /**
 * Update sales item from $_POST, eq invoice item, estimate item
 * @param  mixed  item_id item id to update
 * @param  array  data    item $_POST data
 * @param  string$field   field is require to be passed for long_description,rate,item_order to do some additional checkings
 * @return boolean
 */
  public static bool update_sales_item_post(this MyContext db, int item_id, Itemable data, string field = "")
  {
    // Initialize a new Itemable object to store the updates
    var update = new Itemable();

    // Check if a specific field update is provided
    if (!string.IsNullOrEmpty(field))
      switch (field)
      {
        case "long_description":
          update.LongDescription = data.LongDescription.nl2br();
          break;
        case "rate":
          update.Rate = Convert.ToDouble(number_format(data.Rate, get_decimal_places()));
          break;
        case "item_order":
          update.ItemOrder = data.ItemOrder;
          break;
        default:
        {
          var property = typeof(Itemable).GetProperty(field);
          if (property != null && property.CanWrite)
            property.SetValue(update, property.GetValue(data));
          else
            throw new ArgumentException($"Field '{field}' is not valid or writable.");
          break;
        }
      }
    else
      // Update all fields if no specific field is provided
      update = new Itemable
      {
        ItemOrder = data.ItemOrder,
        Description = data.Description,
        LongDescription = data.LongDescription!.nl2br(),
        Rate = Convert.ToDouble(number_format(data.Rate, get_decimal_places())),
        Qty = data.Qty,
        Unit = data.Unit
      };

    // Perform the database update
    var affected_rows = db
      .Itemables
      .Where(x => x.Id == item_id)
      .Update(x => new Itemable
      {
        ItemOrder = update.ItemOrder,
        Description = update.Description,
        LongDescription = update.LongDescription,
        Rate = update.Rate,
        Qty = update.Qty,
        Unit = update.Unit
      });
    return affected_rows > 0;
  }

  /**
* Function used for sales eq. invoice, estimate, proposal, credit note
* @param  mixed $item_id   item id
* @param  array post_item $item from $_POST
* @param  mixed $rel_id    rel_id
* @param  string rel_type  where this item tax is related
*/
  public static bool maybe_insert_post_item_tax(this MyContext db, int itemId, PostItem? itemTax, int relId, string relType)
  {
    var affectedRows = 0;

    if (!itemTax.TaxNames.Any()) return affectedRows > 0;
    foreach (var taxName in itemTax.TaxNames)
      if (!string.IsNullOrWhiteSpace(taxName))
      {
        var taxArray = taxName.Split('|');
        if (taxArray.Length != 2) continue;
        var taxNameTrimmed = taxArray[0].Trim();
        var taxRateTrimmed = taxArray[1].Trim();

        if (!decimal.TryParse(taxRateTrimmed, out var taxRate)) continue;
        // Check if the tax record already exists
        var exists = db.ItemTaxes.Any(t =>
          t.ItemId == itemId &&
          t.TaxRate == taxRate &&
          t.TaxName == taxNameTrimmed &&
          t.RelId == relId &&
          t.RelType == relType);

        if (exists) continue;
        // Insert new tax record
        db.ItemTaxes.Add(new ItemTax
        {
          ItemId = itemId,
          TaxRate = taxRate,
          TaxName = taxNameTrimmed,
          RelId = relId,
          RelType = relType
        });

        affectedRows++;
      }

    // Save changes if there are any new records
    if (affectedRows > 0)
      db.SaveChanges();
    return affectedRows > 0;
  }


  /**
 * Remove taxes from item
 * @param  mixed $item_id  item id
 * @param  string rel_type relation type eq. invoice, estimate etc.
 * @return boolean
 */
  public static bool delete_taxes_from_item(this MyContext db, int item_id, string rel_type)
  {
    var result = db.ItemTaxes
      .Where(x => x.ItemId == item_id)
      .Delete();
    return result > 0;
  }

  /**
 * When item is removed eq from invoice will be stored in removed_items in $_POST
 * With foreach loop this function will remove the item from database and it's taxes
 * @param  mixed id       item id to remove
 * @param  string rel_type item relation eq. invoice, estimate
 * @return boolean
 */
  public static bool handle_removed_sales_item_post(this MyContext db, int id, string rel_type)
  {
    var affected_rows = db.Itemables
      .Where(x => x.Id == id)
      .Delete();
    if (affected_rows <= 0) return false;

    db.delete_taxes_from_item(id, rel_type);
    db.CustomFieldsValues
      .Where(x => x.RelId == id && x.FieldTo == "items")
      .Delete();
    return true;
  }

  /**
 * Custom format number function for the app
 * @param  mixed  $total
 * @param  boolean $foce_check_zero_decimals whether to force check
 * @return mixed
 */
  public static object app_format_number(this MyContext db, object total, bool force_check_zero_decimals = false)
  {
    // Ensure the input is convertible to a decimal
    if (!decimal.TryParse(total?.ToString(), out var numericTotal)) return total; // Return the original input if it's not numeric

    // Get formatting options
    var decimal_separator = db.get_option("decimal_separator") ?? ".";
    var thousand_separator = db.get_option("thousand_separator") ?? ",";

    // Determine decimal places
    var decimalPlaces = get_decimal_places();
    if (db.get_option_compare("remove_decimals_on_zero", 1) || force_check_zero_decimals)
      if (numericTotal % 1 == 0) // Check if the number is effectively an integer
        decimalPlaces = 0;

    // Format the number using the specified decimal and thousand separators
    // var formatted = number_format(numericTotal, decimalPlaces, decimal_separator, thousand_separator);
    // Apply hooks and return the final formatted result
    return hooks.apply_filters("number_after_format",
      // formatted,
      new
      {
        total,
        decimal_separator,
        thousand_separator,
        decimal_places = decimalPlaces
      });
  }

  public static double sec2qty(this HelperBase helperBase, double sec)
  {
    return sec / 3600;
  }
}
