using Global.Entities;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class SettingHelper
{
  public static bool add_option(this MyContext db, string name, object? value = null)
  {
    if (db.option_exists(name)) return false;
    var newData = new Option
    {
      Name = name,
      Value = value?.ToString() ?? ""
    };
    db.Options.Add(newData);
    db.SaveChanges();
    return newData.Id > 0;
  }

  /**
   * Get option value
   * @param  string   name Option name
   * @return mixed
   */
  public static string get_option(this MyContext db, string name)
  {
    var row = db.Options.FirstOrDefault(x => x.Name == name);
    var output = row?.Value;
    return output ?? string.Empty;
  }

  public static T? get_option<T>(this MyContext db, string name)
  {
    var row = db.Options.FirstOrDefault(x => x.Name == name);
    var output = row?.Value;
    return (T)Convert.ChangeType(output, typeof(T));
  }

  public static bool get_option_compare(this MyContext db, string name, object value)
  {
    var row = db.get_option(name);
    if (row == null)
      return value == null; // Both should be null for the comparison to be true
    var output = Convert.ChangeType(row, value.GetType()); // Convert the value to the type T
    return value?.Equals(output) ?? false; // Use Equals to compare or check if both are null
  }

  /**
   * Updates option by name
   *
   * @param  string   name     Option name
   * @param  string  value    Option Value
   * @param  mixed $autoload  Whether to update the autoload
   *
   * @return boolean
   */
  public static bool update_option(this MyContext db, string name, object? value = null, bool? autoload = null)
  {
    /**
     * Create the option if not exists
     * @since  2.3.3
     */
    if (!db.option_exists(name)) return db.add_option(name, value);
    db.Options
      .Where(row => row.Name == name)
      // .Update(x => new Option { Value = value, Autoload = autoload ?? x.Autoload });
      .Update(x => new Option
      {
        Value = (value == null ? Convert.ToString(value) : "") ?? string.Empty,
        Autoload = autoload ?? x.Autoload
      });
    var affected_rows = db.SaveChanges();
    return affected_rows > 0;
  }

  /**
   * Delete option
   * @since  Version 1.0.4
   * @param  mixed   name option name
   * @return boolean
   */
  public static bool delete_option(this HelperBase helper, string name)
  {
    var (self, db) = getInstance();
    // app.Options.FirstOrDefault(row=>row.Name==name);
    if (!db.Options.Any(row => row.Name == name)) return false;

    var affected_rows = db.Options.Where(row => row.Name == name).Delete();

    return affected_rows > 0;
  }

  /**
   * @since  2.3.3
   * Check whether an option exists
   *
   * @param  string   name option name
   *
   * @return boolean
   */
  public static bool option_exists(this MyContext db, string name)
  {
    return db.Options.Any(row => row.Name == name);
  }

  public static void app_init_settings_tabs()
  {
    // app_tabs.add_settings_tab('general', [
    //   'name' => _l('settings_group_general'),
    // 'view' => 'admin/settings/includes/general',
    // 'position' => 5,
    // 'icon' => 'fa fa-cog',
    //   ]);
    //
    // app_tabs.add_settings_tab('company', [
    //   'name' => _l('company_information'),
    // 'view' => 'admin/settings/includes/company',
    // 'position' => 10,
    // 'icon' => 'fa-solid fa-bars-staggered',
    //   ]);
    //
    // app_tabs.add_settings_tab('localization', [
    //   'name' => _l('settings_group_localization'),
    // 'view' => 'admin/settings/includes/localization',
    // 'position' => 15,
    // 'icon' => 'fa-solid fa-globe',
    //   ]);
    //
    // app_tabs.add_settings_tab('email', [
    //   'name' => _l('settings_group_email'),
    // 'view' => 'admin/settings/includes/email',
    // 'position' => 20,
    // 'icon' => 'fa-regular fa-envelope',
    //   ]);
    //
    // app_tabs.add_settings_tab('sales', [
    //   'name' => _l('settings_group_sales'),
    // 'view' => 'admin/settings/includes/sales',
    // 'position' => 25,
    // 'icon' => 'fa-solid fa-receipt',
    //   ]);
    //
    // app_tabs.add_settings_tab('subscriptions', [
    //   'name' => _l('subscriptions'),
    // 'view' => 'admin/settings/includes/subscriptions',
    // 'position' => 30,
    // 'icon' => 'fa fa-repeat',
    //   ]);
    //
    // app_tabs.add_settings_tab('payment_gateways', [
    //   'name' => _l('settings_group_online_payment_modes'),
    // 'view' => 'admin/settings/includes/payment_gateways',
    // 'position' => 35,
    // 'icon' => 'fa-regular fa-credit-card',
    //   ]);
    //
    // app_tabs.add_settings_tab('clients', [
    //   'name' => _l('settings_group_clients'),
    // 'view' => 'admin/settings/includes/clients',
    // 'position' => 40,
    // 'icon' => 'fa-regular fa-user',
    //   ]);
    //
    // app_tabs.add_settings_tab('tasks', [
    //   'name' => _l('tasks'),
    // 'view' => 'admin/settings/includes/tasks',
    // 'position' => 45,
    // 'icon' => 'fa-regular fa-circle-check',
    //   ]);
    //
    // app_tabs.add_settings_tab('tickets', [
    //   'name' => _l('support'),
    // 'view' => 'admin/settings/includes/tickets',
    // 'position' => 50,
    // 'icon' => 'fa-regular fa-life-ring',
    //   ]);
    //
    // app_tabs.add_settings_tab('leads', [
    //   'name' => _l('leads'),
    // 'view' => 'admin/settings/includes/leads',
    // 'position' => 55,
    // 'icon' => 'fa fa-tty',
    //   ]);
    //
    // app_tabs.add_settings_tab('calendar', [
    //   'name' => _l('settings_calendar'),
    // 'view' => 'admin/settings/includes/calendar',
    // 'position' => 60,
    // 'icon' => 'fa-regular fa-calendar',
    //   ]);
    //
    // app_tabs.add_settings_tab('pdf', [
    //   'name' => _l('settings_pdf'),
    // 'view' => 'admin/settings/includes/pdf',
    // 'position' => 65,
    // 'icon' => 'fa-regular fa-file-pdf',
    //   ]);
    //
    // app_tabs.add_settings_tab('e_sign', [
    //   'name' => 'E-Sign',
    // 'view' => 'admin/settings/includes/e_sign',
    // 'position' => 70,
    // 'icon' => 'fa-solid fa-signature',
    //   ]);
    //
    // app_tabs.add_settings_tab('cronjob', [
    //   'name' => _l('settings_group_cronjob'),
    // 'view' => 'admin/settings/includes/cronjob',
    // 'position' => 75,
    // 'icon' => 'fa-solid fa-microchip',
    //   ]);
    //
    // app_tabs.add_settings_tab('tags', [
    //   'name' => _l('tags'),
    // 'view' => 'admin/settings/includes/tags',
    // 'position' => 80,
    // 'icon' => 'fa-solid fa-tags',
    //   ]);
    //
    // app_tabs.add_settings_tab('pusher', [
    //   'name' => 'Pusher.com',
    // 'view' => 'admin/settings/includes/pusher',
    // 'position' => 85,
    // 'icon' => 'fa-regular fa-bell',
    //   ]);
    //
    // app_tabs.add_settings_tab('google', [
    //   'name' => 'Google',
    // 'view' => 'admin/settings/includes/google',
    // 'position' => 90,
    // 'icon' => 'fa-brands fa-google',
    //   ]);
    //
    // app_tabs.add_settings_tab('misc', [
    //   'name' => _l('settings_group_misc'),
    // 'view' => 'admin/settings/includes/misc',
    // 'position' => 95,
    // 'icon' => 'fa-solid fa-gears',
    //   ]);
  }
}
