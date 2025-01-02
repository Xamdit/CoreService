using System.Net;
using Service.Entities;
using Service.Framework.Core.Extensions;
using File = System.IO.File;

namespace Service.Framework.Library.Net;

public class Ftp(MyInstance instance,MyContext db, Dictionary<string, object> config = null)
{
  public string Hostname { get; set; } = string.Empty;
  public string Username { get; set; } = string.Empty;
  public string Password { get; set; } = string.Empty;
  public int Port { get; set; } = 21;
  public bool Passive { get; set; } = true;
  public bool Debug { get; set; } = false;

  private FtpWebRequest _request;
  private FtpWebResponse _response;
  private MyInstance self { get; set; }

  // public Ftp(MyInstance instance, Dictionary<string, object> config = null)
  // {
  //   self = instance;
  //   if (config != null) Initialize(config);
  //   db.log_message("FTP Class Initialized");
  // }

  public void Initialize(Dictionary<string, object> config)
  {
    foreach (var kv in config)
      switch (kv.Key.ToLower())
      {
        case "hostname":
          Hostname = kv.Value.ToString();
          break;
        case "username":
          Username = kv.Value.ToString();
          break;
        case "password":
          Password = kv.Value.ToString();
          break;
        case "port":
          Port = Convert.ToInt32(kv.Value);
          break;
        case "passive":
          Passive = Convert.ToBoolean(kv.Value);
          break;
        case "debug":
          Debug = Convert.ToBoolean(kv.Value);
          break;
      }

    Hostname = Hostname.Replace("ftp://", string.Empty);
  }

  public bool Connect(Dictionary<string, object> config = null)
  {
    if (config != null) Initialize(config);

    try
    {
      _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}");
      _request.Method = WebRequestMethods.Ftp.ListDirectory; // We will check the connection by listing the directory
      _request.Credentials = new NetworkCredential(Username, Password);
      _request.UsePassive = Passive;
      _request.UseBinary = true;

      using (_response = (FtpWebResponse)_request.GetResponse())
      {
        db.log_message($"Connected, status: {_response.StatusDescription}");
        return true;
      }
    }
    catch (WebException ex)
    {
      if (Debug)
        db.log_error($"Connection error: {ex.Message}");
      return false;
    }
  }

  public bool ChangeDir(string path, bool suppressDebug = false)
  {
    if (!IsConn()) return false;

    try
    {
      _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{path}");
      _request.Method = WebRequestMethods.Ftp.ListDirectory;
      _request.Credentials = new NetworkCredential(Username, Password);

      using (_response = (FtpWebResponse)_request.GetResponse())
      {
        db.log_message($"Changed directory to {path}, status: {_response.StatusDescription}");
        return true;
      }
    }
    catch (WebException ex)
    {
      if (Debug && !suppressDebug) db.log_error($"Unable to change directory: {ex.Message}");
      return false;
    }
  }

  public bool CreateDir(string path)
  {
    if (string.IsNullOrEmpty(path) || !IsConn()) return false;

    try
    {
      _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{path}");
      _request.Method = WebRequestMethods.Ftp.MakeDirectory;
      _request.Credentials = new NetworkCredential(Username, Password);

      using (_response = (FtpWebResponse)_request.GetResponse())
      {
        db.log_message($"Directory created: {path}, status: {_response.StatusDescription}");
        return true;
      }
    }
    catch (WebException ex)
    {
      if (Debug)
        db.log_error($"Unable to create directory: {ex.Message}");
      return false;
    }
  }

  public bool Upload(string locPath, string remPath, string mode = "auto")
  {
    if (!IsConn()) return false;

    if (!File.Exists(locPath))
    {
      db.log_error("No source file found for upload");
      return false;
    }

    var fileType = mode == "auto" ? GetFileExtension(locPath) : mode;
    _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{remPath}");
    _request.Method = WebRequestMethods.Ftp.UploadFile;
    _request.Credentials = new NetworkCredential(Username, Password);
    _request.UseBinary = fileType != "ascii";

    byte[] fileContents;
    using (var fs = File.OpenRead(locPath))
    {
      fileContents = new byte[fs.Length];
      fs.Read(fileContents, 0, fileContents.Length);
    }

    using (var requestStream = _request.GetRequestStream())
    {
      requestStream.Write(fileContents, 0, fileContents.Length);
    }

    using (_response = (FtpWebResponse)_request.GetResponse())
    {
      db.log_message($"Upload complete, status: {_response.StatusDescription}");
      return true;
    }
  }

  public bool Download(string remPath, string locPath)
  {
    if (!IsConn()) return false;

    _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{remPath}");
    _request.Method = WebRequestMethods.Ftp.DownloadFile;
    _request.Credentials = new NetworkCredential(Username, Password);

    try
    {
      using (_response = (FtpWebResponse)_request.GetResponse())
      using (var responseStream = _response.GetResponseStream())
      using (var fs = File.Create(locPath))
      {
        responseStream.CopyTo(fs);
      }

      db.log_message($"Download complete: {locPath}, status: {_response.StatusDescription}");
      return true;
    }
    catch (WebException ex)
    {
      if (Debug) db.log_error($"Unable to download file: {ex.Message}");
      return false;
    }
  }

  public bool Rename(string oldFile, string newFile)
  {
    if (!IsConn()) return false;

    _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{oldFile}");
    _request.Method = WebRequestMethods.Ftp.Rename;
    _request.Credentials = new NetworkCredential(Username, Password);
    _request.RenameTo = newFile;

    try
    {
      using (_response = (FtpWebResponse)_request.GetResponse())
      {
        db.log_message($"Renamed {oldFile} to {newFile}, status: {_response.StatusDescription}");
        return true;
      }
    }
    catch (WebException ex)
    {
      if (Debug)
        db.log_error($"Unable to rename file: {ex.Message}");
      return false;
    }
  }

  public bool DeleteFile(string filepath)
  {
    if (!IsConn()) return false;

    _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{filepath}");
    _request.Method = WebRequestMethods.Ftp.DeleteFile;
    _request.Credentials = new NetworkCredential(Username, Password);

    try
    {
      using (_response = (FtpWebResponse)_request.GetResponse())
      {
        db.log_message($"Deleted file: {filepath}, status: {_response.StatusDescription}");
        return true;
      }
    }
    catch (WebException ex)
    {
      if (Debug)
        db.log_error($"Unable to delete file: {ex.Message}");
      return false;
    }
  }

  public bool ListFiles(string path, out List<string> files)
  {
    files = new List<string>();
    if (!IsConn()) return false;

    _request = (FtpWebRequest)WebRequest.Create($"ftp://{Hostname}:{Port}/{path}");
    _request.Method = WebRequestMethods.Ftp.ListDirectory;
    _request.Credentials = new NetworkCredential(Username, Password);

    try
    {
      using (_response = (FtpWebResponse)_request.GetResponse())
      using (var reader = new StreamReader(_response.GetResponseStream()))
      {
        string line;
        while ((line = reader.ReadLine()) != null) files.Add(line);
      }

      return true;
    }
    catch (WebException ex)
    {
      if (Debug)
        db.log_error($"Unable to list files: {ex.Message}");
      return false;
    }
  }

  public void Close()
  {
    // In C#, FtpWebRequest does not need a close method as resources are released automatically
    _response?.Close();
    db.log_message("FTP connection closed.");
  }

  private bool IsConn()
  {
    if (_response != null) return true;
    if (Debug) db.log_error("No FTP connection.");
    return false;
  }

  private string GetFileExtension(string filePath)
  {
    return Path.GetExtension(filePath)?.TrimStart('.') ?? "txt";
  }
}
