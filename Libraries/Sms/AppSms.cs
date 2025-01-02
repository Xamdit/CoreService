using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Libraries.Sms;

public abstract class AppSms(MyInstance instance, MyContext db)
{
  private static Dictionary<string, SmsGateway> gateways = new();


  public HttpClient client
  {
    get
    {
      var output = new HttpClient
      {
        DefaultRequestHeaders =
        {
          { "Content-Type", "application/json" },
          { "Accept", "application/json" }
        }
      };
      output.DefaultRequestHeaders.ExpectContinue = false;
      set_default_triggers();
      return output;
    }
  }

  private readonly List<Trigger> _triggers = new();
  public static string TriggerBeingSent { get; private set; }
  public bool TestMode { get; set; }
  public abstract bool send(string number, string message);

  public void add_gateway(string id, SmsGateway data)
  {
    if (!is_initialized(id))
    {
      foreach (var option in data.Options)
        db.add_option(get_option_name(id, option.Label), option.Value);
      db.add_option(get_option_name(id, "active"), "0");
      db.add_option(get_option_name(id, "initialized"), "1");
    }

    data.Id = id;
    gateways[id] = data;
  }

  public string get_option(string id, string option)
  {
    return get_option_from_db(get_option_name(id, option));
  }

  public SmsGateway GetGateway(string id)
  {
    return gateways.TryGetValue(id, out var Smsgateway) ? Smsgateway : null;
  }

  public void SetTestMode(bool value)
  {
    TestMode = value;
  }

  public List<SmsGateway> GetGateways()
  {
    return apply_filters("get_gateways", gateways.Values.ToList());
  }

  public string get_trigger_value(string trigger)
  {
    // Simulating caching and retrieving the value from options
    var message = get_from_cache($"sms-trigger-{trigger}-value");
    if (message != null) return message;
    message = get_option_from_db(get_trigger_option_name(trigger));
    add_to_cache($"sms-trigger-{trigger}-value", message);

    return message;
  }

  public void add_trigger(List<Trigger> triggers)
  {
    _triggers.AddRange(triggers);
  }

  public List<Trigger> get_available_triggers()
  {
    var triggers = apply_filters("sms_gateway_available_triggers", _triggers);
    foreach (var trigger in triggers)
    {
      if (is_options_page())
        db.add_option(get_trigger_option_name(trigger.Id), string.Empty);

      trigger.Value = get_trigger_value(trigger.Id);
    }

    return triggers;
  }

  public bool trigger(string trigger, string phone, Dictionary<string, string> mergeFields)
  {
    if (string.IsNullOrEmpty(phone)) return false;

    var gateway = get_active_gateway();
    if (gateway == null || !is_trigger_active(trigger)) return false;
    var message = parse_merge_fields(mergeFields, get_trigger_value(trigger));
    TriggerBeingSent = trigger;
    var success = gateway.SendSms(phone, message);
    TriggerBeingSent = null;

    if (success) log_success(phone, message);
    else log_error($"Failed to send SMS for trigger {trigger}");

    return success;
  }

  private string parse_merge_fields(Dictionary<string, string> mergeFields, string message)
  {
    var output = mergeFields.Aggregate(message, (current, field) => current.Replace($"{{{field.Key}}}", field.Value));
    return output;
  }

  public bool is_trigger_active(string trigger)
  {
    return !string.IsNullOrEmpty(trigger) && !string.IsNullOrWhiteSpace(get_trigger_value(trigger));
  }

  public SmsGateway? get_active_gateway()
  {
    return gateways.Values.FirstOrDefault(g => get_option(g.Id, "active") == "1");
  }

  private bool is_options_page()
  {
    // Simulate the check for current page and group
    return get_current_page() == "sms" && get_current_group() == "settings";
  }

  private bool is_initialized(string id)
  {
    return get_option(id, "initialized") == "1";
  }

  private void set_default_triggers()
  {
    _triggers.Add(new Trigger
    {
      Id = "invoice_overdue_notice",
      MergeFields = new List<string> { "{invoice_number}", "{due_date}" },
      Label = "Invoice Overdue Notice",
      Info = "Triggered when an invoice is overdue."
    });

    // Add more triggers as needed...
  }

  private void log_success(string number, string message)
  {
    Console.WriteLine($"SMS sent to {number}. Message: {message}");
  }

  private void log_error(string error)
  {
    Console.WriteLine($"Error: {error}");
  }


  private string get_option_from_db(string name)
  {
    return "Simulated Option Value";
    // Simulate fetching an option from the database
  }

  private void add_to_cache(string key, string value)
  {
    Console.WriteLine($"Cache added: {key} = {value}");
  }

  private string get_from_cache(string key)
  {
    return null;
    // Simulate fetching from cache
  }

  private List<T> apply_filters<T>(string filterName, List<T> values)
  {
    return values;
    // Simulate applying filters
  }

  private string get_option_name(string id, string option)
  {
    return $"sms_{id}_{option}";
  }

  private string get_trigger_option_name(string trigger)
  {
    return $"sms_trigger_{trigger}";
  }

  private string get_current_page()
  {
    return "sms";
  }

  private string get_current_group()
  {
    return "settings";
  }
}
