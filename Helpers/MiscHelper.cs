using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class MiscHelper
{
  /**
 * Generate random alpha numeric string
 * @param  integerlength the length of the string
 * @return string
 */
  public static string generate_two_factor_auth_key()
  {
    // return bin2hex(get_instance()->encryption->create_key(4));
    return "";
  }

  /**
 * Used for estimate and proposal acceptance info array
 * @param  booleanempty should the array values be empty or taken from_POST
 * @return array
 */
  public static T get_acceptance_info_array<T>() where T : class
  {
    return get_acceptance_info_array<T>(false);
  }

  public static T get_acceptance_info_array<T>(bool empty = false) where T : class
  {
    var _httpContextAccessor = new HttpContextAccessor();
    string signature = null;
    if (_httpContextAccessor.HttpContext.Items.ContainsKey("processed_digital_signature"))
    {
      signature = _httpContextAccessor.HttpContext.Items["processed_digital_signature"] as string;
      _httpContextAccessor.HttpContext.Items.Remove("processed_digital_signature");
    }

    var request = _httpContextAccessor.HttpContext.Request;
    var data = new
    {
      Signature = signature,
      AcceptanceFirstName = empty == true ? null : request.Form["acceptance_firstname"].ToString(),
      AcceptanceLastName = empty == true ? null : request.Form["acceptance_lastname"].ToString(),
      AcceptanceEmail = empty == false && Convert.ToBoolean(request.Form["acceptance_email"]),
      AcceptanceDate = empty == true ? null : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
      AcceptanceIp = empty == true ? null : _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString()
    };
    var output = Convert.ChangeType(data, typeof(T));
    return (T)output;
  }


  public static bool process_digital_signature_image(string partBase64, string path)
  {
    if (string.IsNullOrEmpty(partBase64)) return false;

    var filename = unique_filename(path, "signature.png");
    var decodedImage = Convert.FromBase64String(partBase64);
    var retval = false;
    path = Path.Combine(path.TrimEnd(Path.DirectorySeparatorChar), filename);

    try
    {
      File.WriteAllBytes(path, decodedImage);
      retval = true;
      // Assuming a similar global variable usage
      // The following line is an example and may need to be adapted
      // according to your application's context
      // Globals.ProcessedDigitalSignature = filename;
    }
    catch (Exception)
    {
      retval = false;
    }

    return retval;
  }


  // Method to process the digital signature image
  public static bool process_digital_signature_image(this HelperBase helper, string partBase64, string path)
  {
    if (string.IsNullOrEmpty(partBase64)) return false;

    MaybeCreateUploadPath(path);
    var filename = UniqueFilename(path, "signature.png");

    var decodedImage = Convert.FromBase64String(partBase64);

    var retval = false;

    path = Path.Combine(path.TrimEnd(Path.DirectorySeparatorChar), filename);

    using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
    {
      fs.Write(decodedImage, 0, decodedImage.Length);
      retval = true;
      ProcessedDigitalSignature = filename;
    }

    return retval;
  }

  // Method to create upload path if it does not exist
  private static void MaybeCreateUploadPath(string path)
  {
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
  }

  // Method to generate a unique filename
  private static string UniqueFilename(string path, string filename)
  {
    var fullPath = Path.Combine(path, filename);
    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
    var extension = Path.GetExtension(filename);
    var count = 1;

    while (File.Exists(fullPath))
    {
      var tempFileName = $"{fileNameWithoutExtension}({count++}){extension}";
      fullPath = Path.Combine(path, tempFileName);
    }

    return Path.GetFileName(fullPath);
  }
}
