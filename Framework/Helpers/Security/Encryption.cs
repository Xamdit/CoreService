using System.Security.Cryptography;
using System.Text;
using Service.Framework.Core.Engine;

namespace Service.Framework.Helpers.Security;

public static class EncryptionHelper
{
  public static string? encrypt(this HelperBase helper, string? plainText, string key = "hello")
  {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = new byte[16]; // Use a zero IV or generate a random one

    using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
    using var ms = new MemoryStream();
    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
    using (var sw = new StreamWriter(cs))
    {
      sw.Write(plainText);
    }

    return Convert.ToBase64String(ms.ToArray());
  }

  public static string decrypt(this HelperBase helper, string? cipherText, string key = "hello")
  {
    using var aes = Aes.Create();
    aes.Key = Encoding.UTF8.GetBytes(key);
    aes.IV = new byte[16]; // Ensure the IV matches the one used for encryption
    using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
    using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
    using var sr = new StreamReader(cs);
    return sr.ReadToEnd();
  }
}
