namespace Service.Framework.Library.Locales;

public class I18NException(string message, Exception innerException = null) : Exception(message, innerException)
{
}
