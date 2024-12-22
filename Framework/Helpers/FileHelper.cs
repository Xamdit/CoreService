using Newtonsoft.Json;
using Service.Framework.Core.Engine;
using Task = System.Threading.Tasks.Task;

namespace Service.Framework.Helpers;

public static class FileHelper
{
  private static Dictionary<string, string> Mimes = new();
  private static bool hour12 = true;

  public static bool file_exists(this MyInstance self, string path, bool createIfNotExists = false)
  {
    var output = File.Exists(path);
    if (!createIfNotExists || output) return output;
    File.Create(path);
    output = true;
    return output;
  }

  public static void file_delete(this MyInstance self, string path)
  {
    var output = File.Exists(path);
    if (!output) return;
    File.Delete(path);
  }

  public static string pathinfo(this MyInstance self, string path, string info)
  {
    var output = info switch
    {
      "PATHINFO_FILENAME" => Path.GetFileNameWithoutExtension(path),
      "PATHINFO_EXTENSION" => Path.GetExtension(path),
      "PATHINFO_DIRNAME" => Path.GetDirectoryName(path),
      _ => string.Empty // Default case
    };

    return output;
  }

  public static string file_extension(this MyInstance self, string path)
  {
    var output = Path.GetExtension(path).TrimStart('.').ToLower();
    return output;
  }

  public static string get_mime_by_extension(this MyInstance self, string path)
  {
    var extension = self.file_extension(path);
    return mimetype(extension);
  }

  // Returns a file size limit in bytes based on the PHP upload_max_filesize
  // and post_max_size
  public static int file_upload_max_size(this HelperBase helper)
  {
    return 100;
  }

  public static bool file_is_image(this HelperBase self, string path)
  {
    string[] possibleBigFiles =
    {
      "pdf", "zip", "mp4", "ai",
      "psd", "ppt", "gzip", "rar",
      "tar", "tgz", "mpeg", "mpg",
      "flv", "mov", "wav", "avi",
      "dwg"
    };
    var pathArray = path.Split('.');
    var ext = pathArray[^1].ToLower();
    // Causing performance issues if the file is too big
    if (Array.Exists(possibleBigFiles, e => e.Equals(ext))) return false;
    // try
    // {
    //   using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
    //   using var image = MediaTypeNames.Image.FromStream(fs);
    //   if (Array.Exists(new[] { ImageFormat.Gif, ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Bmp },
    //         format => image.RawFormat.Equals(format)))
    //     return true;
    // }
    // catch (Exception)
    // {
    //   return false;
    // }
    return false;
  }


  public static string protected_file_url_by_path(this HelperBase helper, string path, bool preview = false)
  {
    return string.Empty;
  }

  public static string _dt(this HelperBase helper, DateTime parsedDate, bool isTimesheet = false)
  {
    var self = MyInstance.Instance;

    var dateFormat = self.config.get("date_format", "HH:mm:ss tt");
    var timeFormat = isTimesheet ? "HH:mm" : "HH:mm:ss tt";
    var formattedDate = hour12
      ? parsedDate.ToString($"{dateFormat} {timeFormat}")
      : parsedDate.ToString($"{dateFormat} HH:mm:ss");
    return formattedDate;
  }

  public static string _dt(this HelperBase helper, string date, bool isTimesheet = false)
  {
    var self = MyInstance.Instance;
    var dateFormat = self.config.get("date_format", "HH:mm:ss tt");
    if (string.IsNullOrWhiteSpace(date) || date == "0000-00-00 00:00:00") return "";
    var parsedDate = DateTime.Parse(date);
    var timeFormat = isTimesheet ? "HH:mm" : "HH:mm:ss tt";
    var formattedDate = hour12
      ? parsedDate.ToString($"{dateFormat} {timeFormat}")
      : parsedDate.ToString($"{dateFormat} HH:mm:ss");
    return formattedDate;
  }

  /**
 * Unique filename based on folder
 * @since  Version 1.0.1
 * @param  string dir      directory to compare
 * @param  string filename filename
 * @return string           the unique filename
 */
  public static string unique_filename(this HelperBase helper, string dir, string filename)
  {
    return filename;
  }

  /**
 * Get the number of seconds to delete temporary old files
 * @since  2.3.2
 * @return integer
 */
  public static int Delete_temporary_files_older_then(this HelperBase helper)
  {
    // return hooks.ApplyFilters<int>("delete_old_temporary_files_older_than", 1800); // 30 minutes is default
    return 30;
  }

  public static async Task App_maybe_delete_old_temporary_files(this HelperBase helper)
  {
    // var olderThan = _delete_temporary_files_older_then();
    // var files = list_files(TEMP_FOLDER);
    // foreach (var file in files)
    // {
    //   var path = TEMP_FOLDER + file;
    //   if (file == "index.html") continue;
    //   if (strpos(file, "-upgrade-version-"))
    //   {
    //     var data = get_last_upgrade_copy_data();
    //     if (data != null)
    //     {
    //       var _basename = data.Path;
    //       if (_basename != file || (DateTime.Now - data.Time).Hours <= olderThan) continue;
    //       unlink(data.Path);
    //       await update_option("last_upgrade_copy_data", "");
    //     }
    //     else
    //     {
    //       // Is probably an old upgrade file because of multiple upgrade tries
    //       // Delete it in all cases
    //       unlink(path);
    //     }
    //   }
    //   else
    //   {
    //     var result = DateTime.Now - filectime(path);
    //     if (result.Hours > olderThan) unlink(path);
    //   }
    // }
    //
    // // In case index.html is removed from the folder, re-create it again
    // if (!file_exists(TEMP_FOLDER + "/index.html"))
    //   fopen(TEMP_FOLDER + "/index.html", "w");
    // var folders = list_folder(TEMP_FOLDER);
    // foreach (var path in folders.Select(folder => TEMP_FOLDER + folder)
    //            .Where(path => (DateTime.Now - filectime(path)).Hours > olderThan))
    //   delete_dir(path);
  }


  public static bool is_dir(this HelperBase helper, string path)
  {
    return Directory.Exists(path);
  }

  public static bool delete_dir(this HelperBase helper, string path)
  {
    Directory.Delete(path, true);
    return true;
  }

  // delete recursive
  public static bool unlink(this HelperBase helper, string path)
  {
    Directory.Delete(path, true);
    return true;
  }

  public static List<string?> list_files(this HelperBase helper, string path)
  {
    return Directory.GetFiles(path).Select(Path.GetFileName).ToList();
  }

  public static bool file_create(this HelperBase helper, string path, bool if_not_exists = false)
  {
    if (if_not_exists && File.Exists(path)) return false;
    File.Create(path);
    return true;
  }

  public static bool file_exists(this HelperBase helperBase, string path)
  {
    return File.Exists(path);
  }

  public static int file_put_contents(this HelperBase self, string path, string data, bool append = false)
  {
    try
    {
      // If append is true, write at the end of the file, otherwise overwrite the file.
      if (append)
        File.AppendAllText(path, data);
      else
        File.WriteAllText(path, data);

      // Return the number of bytes written (like in PHP)
      return data.Length;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
      return -1; // Return -1 in case of an error
    }
  }

  public static string file_name(this HelperBase helper, string filepath)
  {
    return Path.GetFileName(filepath);
  }

  public static string file_extension(this HelperBase helper, string filepath)
  {
    // Get the file extension using Path.GetExtension
    return Path.GetExtension(filepath);
  }


  public static void xcopy(this HelperBase helper, string sourceDirectory, string targetDirectory, bool copySubDirectories = true)
  {
    // Ensure the source directory exists
    if (!Directory.Exists(sourceDirectory)) throw new DirectoryNotFoundException($"Source directory does not exist: {sourceDirectory}");

    // Create the target directory if it doesn't exist
    if (!Directory.Exists(targetDirectory)) Directory.CreateDirectory(targetDirectory);

    // Get the files in the source directory
    var dir = new DirectoryInfo(sourceDirectory);
    var files = dir.GetFiles();

    // Copy each file to the target directory
    foreach (var file in files)
    {
      var targetFilePath = Path.Combine(targetDirectory, file.Name);
      file.CopyTo(targetFilePath, true); // Overwrite if file exists
    }

    // Copy subdirectories if specified
    if (!copySubDirectories) return;
    var subDirectories = dir.GetDirectories().ToList();
    subDirectories.ForEach(subDirectory =>
    {
      var newTargetDirectory = Path.Combine(targetDirectory, subDirectory.Name);
      helper.xcopy(subDirectory.FullName, newTargetDirectory, copySubDirectories); // Recursively copy subdirectories
    });
  }

  public static string stream_get_contents(this HelperBase helper, Stream stream)
  {
    if (stream == null) throw new ArgumentNullException(nameof(stream));
    if (!stream.CanRead) throw new InvalidOperationException("Stream cannot be read.");
    using var reader = new StreamReader(stream);
    return reader.ReadToEnd();
  }

  public static void delete_files_by_extension(this HelperBase helper, string directoryPath, string extension)
  {
    if (!Directory.Exists(directoryPath))
    {
      Console.WriteLine("Directory does not exist.");
      return;
    }

    // Get all files with the specified extension in the directory and subdirectories
    var files = Directory.GetFiles(directoryPath, $"*.{extension}", SearchOption.AllDirectories);

    foreach (var file in files)
      try
      {
        File.Delete(file);
        Console.WriteLine($"Deleted: {file}");
      }
      catch (IOException ex)
      {
        Console.WriteLine($"Error deleting file {file}: {ex.Message}");
      }
  }

  public static bool create_folder_if_not_exists(this HelperBase helper, string path)
  {
    if (Directory.Exists(path)) return false;
    Directory.CreateDirectory(path);
    return true;
  }

  public static bool create_file_if_not_exists(this HelperBase helper, string currentPath)
  {
    try
    {
      var folder = Path.GetDirectoryName(currentPath);
      helper.create_folder_if_not_exists(folder);
      if (!File.Exists(currentPath))
        File.Create(currentPath).Dispose(); // Create the file and immediately release the file handle
      return true;
    }
    catch (Exception ex)
    {
      return false;
    }
  }

  public static bool create_json_file(this HelperBase helper, string path, object? data = null)
  {
    try
    {
      data ??= new { };
      var json = JsonConvert.SerializeObject(data);
      File.WriteAllText(path, json);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred: {ex.Message}");
      return false;
    }
  }
}
