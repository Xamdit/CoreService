namespace Service.Core.Synchronus;

public class SyncDriver(string api)
{
  protected string EscapeChar { get; } = "`";


  protected string ProtectIdentifiers(string identifier, bool escapeChar = false, object? param1 = null, bool param2 = false)
  {
    return escapeChar ? $"{EscapeChar}{identifier}{EscapeChar}" : identifier;
  }
}
