using System.Linq.Expressions;
using Service.Entities;
using Service.Framework;
using Service.Libraries.gateways;

namespace Service.Models.Payments;

public class PaymentModesModel(MyInstance self, MyContext db) : MyModel(self)
{
  /**
  * @deprecated 2.3.4
  * @see gateways
  * @var array
  */
  private List<PaymentMode> _paymentGateways = new();

  /**
   * New variable because the app_payment_gateways hook is moved in the method get_payment_gateways and the gateways be duplicated
   * After the deprecated filters are removed and access to payment_gateways is removed, this should work fine.
   * @since 2.3.4
   * @var array
   */
  private List<PaymentMode> _gateways = new();

  // public PaymentModesModel()
  // {
  //   /**
  //    * @deprecated 2.3.0 use app_payment_gateways
  //    * @var array
  //    */
  //
  //   // this.payment_gateways = apply_filters_deprecated('before_add_online_payment_modes', [[]], '2.3.0', "app_payment_gateways");
  //
  //   /**
  //    * @deprecated 2.3.2 use app_payment_gateways
  //    * @var array
  //    */
  //   // this.payment_gateways = apply_filters_deprecated('before_add_payment_gateways', [this.payment_gateways], '2.3.0', "app_payment_gateways");
  // }

  /**
   * Get payment mode
   * @param  string  id    payment mode id
   * @param  array   where additional where only for offline modes
   * @param  boolean include_inactive   whether to include inactive too
   * @param  boolean force force if it's inactive to return it back
   * @return array
   */
  public List<PaymentMode> get()
  {
    return db.PaymentModes.ToList();
  }

  public PaymentMode? Find(int id)
  {
    return db.PaymentModes.FirstOrDefault(x => x.Id == id);
  }

  public PaymentMode? Find(int id, Expression<Func<PaymentMode, bool>> where)
  {
    var query = db.PaymentModes.AsQueryable();
    query = query.Where(where);
    return query.FirstOrDefault(x => x.Id == id);
  }

  public List<PaymentMode> Find(Expression<Func<PaymentMode, bool>> where)
  {
    var query = db.PaymentModes.AsQueryable();
    query = query.Where(where);
    return query.ToList();
  }


  public List<AppGateway> get(Expression<Func<PaymentMode, bool>> where, bool includeInactive = false, bool force = false)
  {
    var query = db.PaymentModes.AsQueryable();
    query.Where(where);
    if (includeInactive != true)
      query.Where(x => x.Active);

    var modes = query.ToList();
    get_payment_gateways(true)
      // .Where(gateway => gateway.Id == id)
      .Where(gateway => gateway.Active || force)
      .Select(gateway =>
      {
        // The instance is already object and array_to_object is messing up
        // var instance = gateway['instance'];
        // unset(gateway['instance']);
        // var mode = array_to_object(gateway);
        // Add again the instance
        // mode.instance    = instance;
        gateway.ShowOnPdf = 0;
        return gateway;
      });

    get_payment_gateways(includeInactive).ForEach(pm => { modes.Add(pm); });
    // return modes;
    return new List<AppGateway>();
  }

  /**
   * Add new payment mode
   * @param array data payment mode _POST data
   */
  public bool Add(PaymentMode data)
  {
    var items = new List<string>
    {
      "Active", "ShowOnPdf", "SelectedByDefault", "InvoicesOnly", "ExpensesOnly"
    };

    // var set = JsonConvert.DeserializeObject<Dictionary<string,object>>(JsonConvert.SerializeObject(data));
    // foreach (var kvp in set)
    // {
    //   var key = kvp.Key;
    //   var value = kvp.Value;
    //   if (items.Contains(key))
    //   {
    //     data[key] = value;
    //   }
    // }
    //
    // items.ForEach(check =>
    // {
    //   data[check] = !isset(data[check]) ? 0 : 1;
    // });

    data = self.hooks.apply_filters("before_paymentmode_added", data);
    var dataset = new PaymentMode
    {
      Name = data.Name,
      Description = data.Description,
      Active = data.Active,
      ExpensesOnly = data.ExpensesOnly,
      InvoicesOnly = data.InvoicesOnly,
      ShowOnPdf = data.ShowOnPdf,
      SelectedByDefault = data.SelectedByDefault
    };

    db.PaymentModes.Add(dataset);
    db.SaveChanges();
    var insertId = data.Id;
    if (insertId <= 0) return false;
    log_activity($"New Payment Mode Added [ID: {insertId}, Name:{data.Name}]");
    self.hooks.do_action("after_paymentmode_added", new { id = insertId, data });
    return true;
  }

  /**
   * Update payment mode
   * @param  array data payment mode _POST data
   * @return boolean
   */
  public bool Edit(PaymentMode data)
  {
    var id = data.Id;
    var updated = false;

    db.PaymentModes.Where(x => x.Id == id)
      .Update(x => new PaymentMode
      {
        Name = data.Name,
        Description = data.Description,
        Active = data.Active,
        ExpensesOnly = data.ExpensesOnly,
        InvoicesOnly = data.InvoicesOnly,
        ShowOnPdf = data.ShowOnPdf,
        SelectedByDefault = data.SelectedByDefault
      });

    var affectedRows = db.SaveChanges();
    if (affectedRows > 0) updated = true;

    self.hooks.do_action("after_update_paymentmode", new
    {
      id,
      data
      // updated = &updated
    });

    if (updated) log_activity($"Payment Mode Updated [ID: {id}, Name:{data.Name}]");
    return updated;
  }

  /**
   * Delete payment mode from database
   * @param  mixed id payment mode id
   * @return mixed / if referenced array else boolean
   */
  public bool Delete(int id)
  {
    // Check if the payment mode is using in the invoiec payment records table.
    db.PaymentModes.RemoveRange(db.PaymentModes.Where(x => x.Id == id));
    var affectedRows = db.SaveChanges();
    if (affectedRows <= 0) return false;
    log_activity($"Payment Mode Deleted [{id}]");
    return true;
  }

  /**
   * @since  2.3.0
   * Get payment gateways
   * @param  boolean includeInactive whether to include the inactive ones too
   * @return array
   */
  public List<PaymentMode> get_payment_gateways(bool includeInactive = false)
  {
    if (!_gateways.Any())
    {
      self.hooks.do_action("before_get_payment_gateways");
      _gateways = self.hooks.apply_filters("app_payment_gateways", _paymentGateways);
    }

    var modes = new List<PaymentMode>();
    _gateways.ForEach(mode =>
    {
      if (includeInactive != true && !mode.Active) return;
      // The the gateways unique in case duplicate ID's are found.
      // if (!value_exists_in_array_by_key(modes, "id", mode.Id))
      // {
      //   modes.Add(mode);
      // }
      // else
      // {
      //   if (Env != "production")
      //     trigger_error(!"Payment Gateway ID '{mode.Id}' already exists, ignoring duplicate gateway ID...");
      // }
    });


    return modes;
  }

  /**
   * Get all online payment modes
   * @deprecated 2.3.0 use get_payment_gateways instead
   * @since   1.0.1
   * @return array payment modes
   */
  public List<PaymentMode> get_online_payment_modes(bool all = false)
  {
    return get_payment_gateways(all);
  }

  /**
   * @since  Version 1.0.1
   * @param  integer ID
   * @param  integer Status ID
   * @return boolean
   * Update payment mode status Active/Inactive
   */
  public bool change_payment_mode_status(int id, bool status)
  {
    db.PaymentModes.Where(x => x.Id == id)
      .Update(x => new PaymentMode { Active = status });
    var affectedRows = db.SaveChanges();
    if (affectedRows <= 0) return false;
    log_activity("Payment Mode Status Changed [ModeID: " + id + " Status(Active/Inactive): " + status + "]");
    return true;
  }

  /**
   * @since  Version 1.0.1
   * @param  integer ID
   * @param  integer Status ID
   * @return boolean
   * Update payment mode show to client Active/Inactive
   */
  public bool change_payment_mode_show_to_client_status(int id, int status)
  {
    db.PaymentModes
      .Where(x => x.Id == id)
      .Update(x => new PaymentMode { ShowOnPdf = status });
    var affectedRows = db.SaveChanges();
    if (affectedRows <= 0) return false;
    log_activity($"Payment Mode Show to Client Changed [ModeID: {id} Status(Active/Inactive): {status}]");
    return true;
  }

  /**
   * Inject custom payment gateway into the payment gateways array
   * @param string gateway_name payment gateway name, should equal like the libraries/classname
   * @param string module       module name to load the gateway if not already loaded
   */
  public void add_payment_gateway(string gateway, string module = null)
  {
    var @class = "";
    // if (is_string(gateway))
    // {
    //   gateway = gateway.ToLower();
    //   // Perhaps is in subfolder e.q. gateways/Example_gateway?
    //   // var basename = basename(gateway);
    //   // if (!this.load.is_loaded(basename) && module)
    //   //   this.load.library(module + '/' + gateway);
    //   // @class = this.{ basename } ;
    // }
    // else
    // {
    //   // register_payment_gateway(new Example_gateway(), '[module_name]');
    //   var @class = gateway;
    //   var name = get_class(@class);
    //   if (!@class instanceof AppGateway) {
    //     throw new Exception(name + " must be an instance of 'App_gateway'");
    //   }
    // }

    // if (hooks().has_filter("app_payment_gateways", new { @class, mode = "initMode" }) == false)
    //   hooks().add_filter("app_payment_gateways", new { @class, mode = "initMode" });
  }
}
