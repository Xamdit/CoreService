namespace Service.Framework.Library.Locales;

public interface ILocaleReader
{
  Dictionary<string, string> Read(Stream stream);
}
