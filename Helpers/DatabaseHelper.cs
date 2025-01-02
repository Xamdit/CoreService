using Microsoft.EntityFrameworkCore;
using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers.Entities;

namespace Service.Helpers;

public static class DatabaseHelper
{
  /**
   * Function that add views tracking for proposals,estimates,invoices,knowledgebase article in database.
   * This function tracks activity only per hour
   * Eq customer viewed invoice at 15:00 and then 15:05 the activity will be tracked only once.
   * If customer view the invoice again in 16:01 there will be activity tracked.
   * @param string rel_type
   * @param mixed rel_id
   */
  private static void add_views_tracking(this HelperBase helper, string rel_type, int rel_id)
  {
  }

  /**
   * Get views tracking based on rel type and rel id
   * @param  string rel_type
   * @param  mixed rel_id
   * @return array
   */
  private static void get_views_tracking(this HelperBase helper, string rel_type, int rel_id)
  {
  }

  /**
   * @since  2.3.2 because of deprecation of logActivity
   * Log Activity for everything
   * @param  string description Activity Description
   * @param  integer staffid    The user who performs the activity, if null, the logged in staff member will used (if logged in)
   */
  public static void log_activity(this HelperBase helper, int project_id, string description, string staff_id = "", bool visible_to_customer = false)
  {
  }

  /**
   * Return last system activity id
   * @return mixed
   */
  public static ActivityLog get_last_system_activity_id(this HelperBase helper)
  {
    return new ActivityLog();
  }

  /**
   * Add user notifications
   * @param array values array of values [description,fromuserid,touserid,fromcompany,isread]
   */
  public static bool add_notification(this MyContext db, Notification data)
  {


    var staff_user_id = helper.get_staff_user_id();
    // var staff_user_id =Convert.ToByte( await get_staff_user_id());
    var _is_client_logged_in = self.db.is_client_logged_in();
    if (_is_client_logged_in)
    {
      data.FromUserId = 0;

      data.FromClientId = helper.get_contact_user_id();
      data.FromFullname = helper.get_contact_full_name(helper.get_contact_user_id());
    }
    else
    {
      data.FromUserId = staff_user_id;
      data.FromClientId = 0;
      data.FromFullname = helper.get_staff_full_name(staff_user_id);
    }

    if (data.FromCompany.HasValue)
    {
      data.FromUserId = 0;
      data.FromFullname = "";
    }

    data.Date = DateTime.Now;

    data = hooks.apply_filters("notification_data", data);
    var query = db.Staff.AsQueryable();
    // Prevent sending notification to non active users.
    if (data.ToUserId != 0)
    {
      query.Where(x => x.Id == data.ToUserId);
      var user = query.FirstOrDefault();
      if (user != null || user.Active is false) return false;
    }

    var result = db.Notifications.Add(data);
    if (!result.IsAdded()) return true;
    var notification_id = result.Entity.Id;
    hooks.do_action("notification_created", notification_id);
    return true;
  }


  /**
   * Prefix field name with table ex. table.column
   * @param  string table
   * @param  string alias
   * @param  string field field to check
   * @return string
   */
  private static string prefixed_table_fields_wildcard(this HelperBase helper, string table, string alias, string field)
  {
    var (self, db) = getInstance();
    var columns = db.ColumnInfos.FromSql($"SHOW COLUMNS FROM {table}").ToList();
    var field_names = columns.Select(x => x.Field).ToList();

    var prefixed = field_names.Select(field_name =>
        field == field_name
          ? "`{alias}`.`{field_name}` AS `{alias}.{field_name}`"
          : string.Empty
      )
      .Where(string.IsNullOrEmpty)
      .ToList();
    return string.Join(", ", prefixed);
  }


  /**
   * Get department email address
   * @param  mixed id department id
   * @return mixed
   */
  private static string get_department_email(this HelperBase helper, int id)
  {
    var (self, db) = getInstance();
    var row = db.Departments.FirstOrDefault(x => x.Id == id);
    return row == null ? string.Empty : row.Email;
  }


  /**
   * @param string table       Table name
   * @param string foreign_key Collumn name having the Foreign Key
   *
   * @return string SQL command
   */
  public static string drop_foreign_key(this HelperBase helper, string table, string foreign_key)
  {
    return "ALTER TABLE `{table}` DROP FOREIGN KEY `{table}_{foreign_key}_fk`";
  }


  /**
   * @param string trigger_name Trigger name
   * @param string table        Table name
   * @param string statement    Command to run
   * @param string time         BEFORE or AFTER
   * @param string event        INSERT, UPDATE or DELETE
   * @param string type         FOR EACH ROW [FOLLOWS|PRECEDES]
   *
   * @return string SQL Command
   */
  public static void add_trigger(this HelperBase helper, string trigger_name, string table, string statement, string time = "BEFORE", string @event = "INSERT", string type = "FOR EACH ROW")
  {
    // return 'DELIMITER ;; CREATE TRIGGER `{trigger_name}` {time} {event} ON `{table}` {type}".PHP_EOL. 'BEGIN'.PHP_EOL. statement.PHP_EOL. 'END;'.PHP_EOL. 'DELIMITER ;;';
  }


  /**
   * @param string trigger_name Trigger name
   *
   * @return string SQL Command
   */
  public static string drop_trigger(this HelperBase helper, string trigger_name)
  {
    return "DROP TRIGGER {trigger_name};";
  }


  /**
   * Check whether table exists
   * Custom function because Codeigniter is caching the tables and this is causing issues in migrations
   * @param  string table table name to check
   * @return boolean
   */
  public static bool table_exists(this HelperBase helper, string table)
  {
    // result = get_instance().db.query("SELECT * FROM information_schema.tables WHERE table_schema = ''AND table_name = '" . table . "'LIMIT 1;").row();
    // return (bool) result;
    return false;
  }

  /**
* Triggers
* @param  array  users id of users to receive notifications
* @return null
*/
  public static bool pusher_trigger_notification(this MyContext db, List<int> users)
  {
    return db.pusher_trigger_notification(users.ToArray());
  }

  public static bool pusher_trigger_notification(this MyContext db, params int[] users)
  {

    if (db.get_option_compare("pusher_realtime_notifications", 0)) return false;
    if (!users.ToList().Any()) return false;
    var channels = users.Select(x => $"notifications-channel-{x}")
      .Distinct()
      .ToList();
    // $CI->app_pusher->trigger($channels, 'notification', [])
    return true;
  }

  public static void log_activity(this HelperBase helper, string newTaxAddedId)
  {
    throw new NotImplementedException();
  }
}
