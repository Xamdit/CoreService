using System.Text.RegularExpressions;

namespace Service.Libraries.FormValidations;

public partial class FormValidation
{
  private readonly IHttpContextAccessor _httpContextAccessor;

  protected Dictionary<string, FieldData> _fieldData = new();

  // protected Dictionary<string, List<string>> _configRules = new();
  protected List<ConfigRules> _configRules = new();

  protected Dictionary<string, string> _errorArray = new();

  protected Dictionary<string, string> _errorMessages = new();
  protected ConfigRules _errorPrefix = new() { Value = "<p>" };
  protected ConfigRules _errorSuffix = new() { Value = "</p>" };
  protected string errorString = "";
  protected bool _safeFormData = false;
  public Dictionary<string, string> validationData = new();

  public FormValidation(IHttpContextAccessor httpContextAccessor, List<ConfigRules> rules = null)
  {
    _httpContextAccessor = httpContextAccessor;
    if (rules == null) return;
    if (rules.Any(x => x.Key == "error_prefix"))
    {
      _errorPrefix = rules.First(x => x.Key == "error_prefix");
      rules.Remove(rules.First(x => x.Key == "error_prefix"));
    }

    if (rules.Any(x => x.Key == "error_suffix"))
    {
      _errorSuffix = rules.First(x => x.Key == "error_suffix");
      rules.Remove(rules.First(x => x.Key == "error_suffix"));
    }

    _configRules = rules;
  }

  public FormValidation set_rules(object field, string label = "", object rules = null, Dictionary<string, string> errors = null)
  {
    var httpMethod = _httpContextAccessor.HttpContext.Request.Method;
    if (httpMethod != "POST" && _fieldData.Count == 0) return this;

    if (field is IEnumerable<object> fieldList)
    {
      foreach (var row in fieldList)
      {
        if (row is not Dictionary<string, object> rowDict ||
            !rowDict.ContainsKey("field") ||
            !rowDict.ContainsKey("rules"))
          continue;

        var currentField = rowDict["field"]?.ToString();
        var fieldLabel = rowDict.ContainsKey("label") ? rowDict["label"]?.ToString() : currentField;
        var fieldErrors = rowDict.ContainsKey("errors") && rowDict["errors"] is Dictionary<string, string> errorDict
          ? errorDict
          : new Dictionary<string, string>();

        set_rules(currentField, fieldLabel, rowDict["rules"], fieldErrors);
      }

      return this;
    }

    if (field is not string fieldName || string.IsNullOrWhiteSpace(fieldName) || rules == null) return this;

    var ruleList = rules switch
    {
      string ruleString => Regex.Split(ruleString, @"\|(?![^\[]*\])").ToList(),
      IEnumerable<string> ruleEnumerable => ruleEnumerable.ToList(),
      _ => new List<string>()
    };

    var finalLabel = string.IsNullOrWhiteSpace(label) ? fieldName : label;

    var indexes = new List<string>();
    var isArray = Regex.Matches(fieldName, @"\[(.*?)\]").Count > 0;

    if (isArray)
    {
      var baseField = Regex.Match(fieldName, @"^([^\[]*)").Groups[1].Value;
      indexes.Add(baseField);

      foreach (Match match in Regex.Matches(fieldName, @"\[(.*?)\]"))
        if (!string.IsNullOrEmpty(match.Groups[1].Value))
          indexes.Add(match.Groups[1].Value);
    }

    _fieldData[fieldName] = new FieldData()
    {
      Field = fieldName,
      Label = finalLabel,
      Rules = ruleList,
      Errors = errors ?? new Dictionary<string, string>(),
      IsArray = isArray,
      Keys = indexes,
      PostData = null,
      Error = string.Empty
    };

    return this;
  }


  public FormValidation set_data(Dictionary<string, string> data)
  {
    if (data != null && data.Count > 0) validationData = data;

    return this;
  }

  public FormValidation set_message(string lang, string val = "")
  {
    _errorMessages[lang] = val;
    return this;
  }

  public FormValidation set_error_delimiters(string prefix = "<p>", string suffix = "</p>")
  {
    _errorPrefix.Value = prefix;
    _errorSuffix.Value = suffix;
    return this;
  }

  public string Error(string field, ConfigRules? prefix = null, ConfigRules? suffix = null)
  {
    if (!_fieldData.ContainsKey(field) || string.IsNullOrEmpty(_fieldData[field].Error)) return "";
    prefix ??= _errorPrefix;
    suffix ??= _errorSuffix;

    return prefix + _fieldData[field].Error + suffix;
  }

  public Dictionary<string, string> ErrorArray()
  {
    return _errorArray;
  }

  public string ErrorString(ConfigRules? prefix = null, ConfigRules? suffix = null)
  {
    if (_errorArray.Count == 0) return "";
    prefix ??= _errorPrefix;
    suffix ??= _errorSuffix;

    return _errorArray.Values.Where(val => !string.IsNullOrEmpty(val)).Aggregate("", (current, val) => current + prefix + val + suffix + "\n");
  }

  public bool run(string group = "")
  {
    var validationArray = validationData;

    if (_fieldData.Count == 0)
    {
      if (_configRules.Count == 0)
        return false;
      // set_rules(group, _configRules.TryGetValue(group, out var rule) ? rule.FirstOrDefault() : _configRules["default"].FirstOrDefault());
      var item = _configRules.Any(x => x.Group == group)
        ? _configRules.Where(x => x.Group == group).ToList()
        : _configRules.Where(x => x.Group == "default").ToList();
      item.Select(x => x.Value).ToList().ForEach(x => set_rules(group, x));
      // set_rules(group, item.Select(x => x.Value).ToList());
    }

    foreach (var field in _fieldData.Values)
      if (field.IsArray && validationArray.ContainsKey(field.Field))
        field.PostData = validationArray[field.Field];
      else if (validationArray.ContainsKey(field.Field)) field.PostData = validationArray[field.Field];

    _fieldData.Values.Where(field => field.Rules.Count != 0).ToList().ForEach(field => Execute(field, field.Rules, field.PostData));

    var totalErrors = _errorArray.Count;
    if (totalErrors > 0) _safeFormData = true;

    return totalErrors == 0;
  }

  protected void Execute(FieldData field, List<string> rules, string postData = null, int cycles = 0)
  {
    if (postData == null || postData == "") return;

    rules = prepare_rules(rules);

    foreach (var _rule in rules)
    {
      var rule = _rule;
      var callback = false;
      var callable = false;
      string param = null;

      if (rule.StartsWith("callback_"))
      {
        rule = rule[9..];
        callback = true;
      }

      if (rule.Contains('['))
      {
        var match = MyRegex().Match(rule);
        if (match.Success)
        {
          rule = match.Groups[1].Value;
          param = match.Groups[2].Value;
        }
      }

      if (callback)
      {
        // Handle callback
      }
      else if (callable)
      {
        // Handle callable
      }
      else if (rule == "required" && string.IsNullOrEmpty(postData))
      {
        SetError(field, rule, param);
      }
      else if (!ValidateRule(rule, postData, param))
      {
        SetError(field, rule, param);
      }
    }
  }

  protected bool ValidateRule(string rule, string postData, string param)
  {
    switch (rule)
    {
      case "required":
        return !string.IsNullOrEmpty(postData);
      case "min_length":
        return postData.Length >= int.Parse(param);
      case "max_length":
        return postData.Length <= int.Parse(param);
      case "valid_email":
        return Regex.IsMatch(postData, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
      // Add other validations as needed
      default:
        return true;
    }
  }

  protected void SetError(FieldData field, string rule, string param)
  {
    var message = get_error_message(rule, field.Field);
    field.Error = string.Format(message, field.Label, param);

    if (!_errorArray.ContainsKey(field.Field)) _errorArray[field.Field] = field.Error;
  }

  protected string get_error_message(string rule, string field)
  {
    if (_errorMessages.ContainsKey(rule)) return _errorMessages[rule];

    return "The " + field + " field has an error.";
  }

  protected List<string> prepare_rules(List<string> rules)
  {
    var newRules = new List<string>();
    // Process rules here if needed
    return newRules;
  }

  [GeneratedRegex(@"(.*?)\[(.*)\]")]
  private static partial Regex MyRegex();
}
