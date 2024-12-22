namespace Service.Libraries.gateways;

public class AppGateway
{
  public bool ProcessingFees { get; set; } = false;

  /// <summary>
  /// Whether the gateway is registered.
  /// </summary>
  protected static List<string> Registered = new();

  /// <summary>
  /// Stores the gateway ID.
  /// </summary>
  protected string Id { get; set; } = "";

  /// <summary>
  /// Gateway name.
  /// </summary>
  public string Name { get; set; } = "";

  /// <summary>
  /// All gateway settings.
  /// </summary>
  protected List<Dictionary<string, object>> Settings { get; set; } = new();

  /// <summary>
  /// Constructor.
  /// </summary>
  public AppGateway()
  {
    // Assuming some dependency injection or framework functionality here.
    if (typeof(AppGateway).GetMethod("ProcessPayment") != null) RegisterAutoPaymentGateway();
  }

  /// <summary>
  /// Automatically register the payment gateway.
  /// </summary>
  public void RegisterAutoPaymentGateway()
  {
    if (!Registered.Contains(GetFullyQualifiedClassName())) RegisterPaymentGateway(this);
  }

  /// <summary>
  /// Initialize modes.
  /// </summary>
  public List<Dictionary<string, object>> InitMode(List<Dictionary<string, object>> modes)
  {
    if (!IsInitialized())
    {
      foreach (var option in Settings)
      {
        var defaultValue = option.ContainsKey("default_value") ? option["default_value"].ToString() : "";
        AddOption($"paymentmethod_{Id}_{option["name"]}", defaultValue, true);
      }

      AddOption($"paymentmethod_{Id}_initialized", "1");
    }

    if (!Registered.Contains(GetFullyQualifiedClassName()))
    {
      modes.Add(new Dictionary<string, object>
      {
        { "id", Id },
        { "name", GetSetting("label") },
        { "description", "" },
        { "selected_by_default", GetSetting("default_selected") },
        { "active", GetSetting("active") },
        { "instance", this }
      });

      Registered.Add(GetFullyQualifiedClassName());
    }

    return modes;
  }

  public void SetName(string name)
  {
    Name = name;
  }

  public string GetName()
  {
    return Name;
  }

  public void SetId(string id)
  {
    Id = id;
  }

  public string GetId()
  {
    return Id;
  }

  public void SetSettings(List<Dictionary<string, object>> settings)
  {
    var requiredSettings = new List<Dictionary<string, object>>
    {
      new() { { "name", "active" }, { "type", "yes_no" }, { "default_value", 0 }, { "label", "settings_paymentmethod_active" } },
      new() { { "name", "label" }, { "default_value", Name }, { "label", "settings_paymentmethod_mode_label" } }
    };

    if (ProcessingFees)
      requiredSettings.AddRange(new List<Dictionary<string, object>>
      {
        new() { { "name", "fee_fixed" }, { "default_value", 0 }, { "label", "payment_gateway_fee_fixed" } },
        new() { { "name", "fee_percent" }, { "default_value", 0 }, { "label", "payment_gateway_fee_percentage" } }
      });

    settings.InsertRange(0, requiredSettings);

    settings.Add(new Dictionary<string, object>
    {
      { "name", "default_selected" },
      { "type", "yes_no" },
      { "default_value", 1 },
      { "label", "settings_paymentmethod_default_selected_on_invoice" }
    });

    Settings = settings;
  }

  public bool AddPayment(Dictionary<string, object> data)
  {
    data["paymentmode"] = GetId();
    if (!ProcessingFees || !data.ContainsKey("payment_attempt_reference")) return true;

    var fee = get_fee((int)data["amount"]);
    data["amount"] = (int)data["amount"] - fee;
    return true;
  }

  public int get_fee(int amount)
  {
    var output = ProcessingFees
      ? (int)Math.Round(GetPercentageFee(amount) + GetFixedFee(), 2)
      : 0;
    return output;
  }

  public float GetFixedFee()
  {
    return float.Parse(GetSetting("fee_fixed") ?? "0");
  }

  public float GetPercentageFee(float amount)
  {
    var feePercent = float.Parse(GetSetting("fee_percent") ?? "0");
    return amount * (feePercent / 100);
  }

  public List<Dictionary<string, object>> GetSettings(bool formatted = true)
  {
    if (formatted)
      return Settings.Select(option =>
      {
        var newOption = new Dictionary<string, object>(option);
        newOption["name"] = $"paymentmethod_{Id}_{option["name"]}";
        return newOption;
      }).ToList();

    return Settings;
  }

  public string GetSetting(string name)
  {
    return $"SimulatedValueFor_{name}";
    // Placeholder for DB or config fetch logic.
  }

  protected bool IsInitialized()
  {
    return !string.IsNullOrEmpty(GetSetting("initialized"));
  }

  protected static void RegisterPaymentGateway(AppGateway gateway)
  {
    // Simulate registration logic.
  }

  protected static void AddOption(string key, string value, bool autoLoad = false)
  {
    // Simulate adding options logic.
  }

  private string GetFullyQualifiedClassName()
  {
    return GetType().FullName;
  }
}
