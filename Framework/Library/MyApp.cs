namespace Service.Framework.Library;

public class TableWithCurrency
{
  public string table { get; set; }
  public string field { get; set; }
}

public class MyApp
{
  private readonly Dictionary<string, string> _options = new();
  private readonly List<object> _quickActions = new();
  private List<TableWithCurrency> tables_with_currency = new();

  private readonly List<string> _available_reminders = new()
  {
    "customer",
    "lead",
    "estimate",
    "invoice",
    "proposal",
    "expense",
    "credit_note",
    "ticket",
    "task"
  };

  private List<string> _availableLanguages = new();
  private string _mediaFolder = "media";
  private bool _showSetupMenu = true;
  private readonly HttpClient _httpClient;

  public MyApp(HttpClient httpClient)
  {
    _httpClient = httpClient;
    Init();
  }

  public bool is_db_upgrade_required(int? version = null)
  {
    version ??= get_current_db_version();
    var migrationVersion = get_migration_version();
    return migrationVersion != version;
  }

  public int get_current_db_version()
  {
    // Mocking database call for example purposes
    return 1; // Replace with actual DB query to get the current migration version
  }

  public void UpgradeDatabase()
  {
    var update = UpgradeDatabaseSilent();
    if (!update.Success) throw new Exception(update.Message);

    Console.WriteLine("Your database is up to date");
  }

  public async Task<string> GetUpdateInfoAsync()
  {
    var postData = new Dictionary<string, string>
    {
      { "update_info", "true" },
      { "current_version", get_current_db_version().ToString() },
      { "php_version", Environment.Version.ToString() },
      { "purchase_key", GetOption("purchase_key") }
    };

    var response = await _httpClient.PostAsync("UPDATE_INFO_URL", new FormUrlEncodedContent(postData));
    if (!response.IsSuccessStatusCode) throw new Exception("Failed to retrieve update info");

    return await response.Content.ReadAsStringAsync();
  }

  public List<string> get_available_languages()
  {
    return _availableLanguages;
  }

  public string GetOption(string name)
  {
    if (_options.ContainsKey(name)) return _options[name];

    // Mocking database call for example purposes
    return string.Empty; // Replace with actual DB query to fetch the option
  }

  public void AddQuickActionsLink(object item)
  {
    _quickActions.Add(item);
  }

  public List<object> GetQuickActionsLinks()
  {
    return _quickActions.OrderBy(action => get_action_position(action)).ToList();
  }

  private void Init()
  {
    // Mocking fetching options and initializing languages
    _options["example_option"] = "value";
    _availableLanguages = new List<string> { "en", "fr", "es" };
    tables_with_currency = new List<TableWithCurrency>
    {
      new()
      {
        table = "invoices",
        field = "currency"
      },
      new()
      {
        table = "expenses",
        field = "currency"
      },
      new()
      {
        table = "proposals",
        field = "currency"
      },
      new()
      {
        table = "estimates",
        field = "currency"
      },
      new()
      {
        table = "clients",
        field = "default_currency"
      },
      new()
      {
        table = "creditnotes",
        field = "currency"
      },
      new()
      {
        table = "subscriptions",
        field = "currency"
      }
    };
  }


  private (bool Success, string Message) UpgradeDatabaseSilent()
  {
    try
    {
      var beforeUpdateVersion = get_current_db_version();
      var updateToVersion = get_migration_version();

      // Mocking migration process
      Console.WriteLine($"Upgrading database from version {beforeUpdateVersion} to {updateToVersion}");

      // Simulate DB upgrade success
      return (true, "Database upgraded successfully");
    }
    catch (Exception ex)
    {
      return (false, ex.Message);
    }
  }

  private int get_migration_version()
  {
    // Mocking config retrieval for example purposes
    return 2; // Replace with actual config fetching logic
  }

  private int get_action_position(object action)
  {
    // Mocking action sorting for example purposes
    return 0; // Replace with actual logic to determine action position
  }

  /**
     * Return tables that currency id is used
     * @return array
     */
  public List<TableWithCurrency> get_tables_with_currency()
  {
    return hooks.apply_filters("tables_with_currency", tables_with_currency);
  }
}
