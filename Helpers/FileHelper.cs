using System.Text.RegularExpressions;
using Service.Framework.Core.Engine;

namespace Service.Helpers;

public static class FileHelper
{
  /**
  * Delete Files
  *
  * Deletes all files contained in the supplied directory path.
  * Files must be writable or owned by the system in order to be deleted.
  * If the second parameter is set to TRUE, any directories contained
  * within the supplied base directory will be nuked as well.
  *
  * @param	string		path		File path
  * @param	bool		delDir		Whether to delete any directories found in the path
  * @param	bool		htdocs		Whether to skip deleting .htaccess and index page files
  * @param	int			level		Current directory depth level (default: 0; internal use only)
  * @return	bool
  */
  public static bool delete_files(string path, bool delDir = false, bool htdocs = false, int level = 0)
  {
    // Trim the trailing slash
    path = path.TrimEnd('/', '\\');
    if (!Directory.Exists(path)) return false;
    var currentDir = new DirectoryInfo(path);
    try
    {
      foreach (var fileSystemInfo in currentDir.GetFileSystemInfos())
      {
        var filePath = fileSystemInfo.FullName;

        if (fileSystemInfo is DirectoryInfo directoryInfo)
        {
          // Skip hidden directories (starting with ".") or symbolic links
          if (!fileSystemInfo.Name.StartsWith(".") && !IsSymbolicLink(directoryInfo))
            // Recursively delete directory content
            delete_files(filePath, delDir, htdocs, level + 1);
        }
        else
        {
          // Check if we should skip .htaccess, index files, and web.config files
          if (htdocs && Regex.IsMatch(fileSystemInfo.Name, @"^(\.htaccess|index\.(html|htm|php)|web\.config)$", RegexOptions.IgnoreCase)) continue;
          try
          {
            // WebRequestMethods.File.Delete(filePath);
          }
          catch (Exception ex)
          {
            Console.WriteLine($"Unable to delete file {filePath}: {ex.Message}");
          }
        }
      }

      if (delDir && level > 0)
        try
        {
          currentDir.Delete();
          return true;
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Unable to delete directory {path}: {ex.Message}");
        }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error accessing directory {path}: {ex.Message}");
      return false;
    }

    return true;
  }

  // Helper function to check if a directory is a symbolic link
  private static bool IsSymbolicLink(DirectoryInfo directory)
  {
    try
    {
      // If the directory is a reparse point, it's likely a symbolic link
      return (directory.Attributes & FileAttributes.ReparsePoint) != 0;
    }
    catch
    {
      return false;
    }
  }

  /**
 * Supported html5 video extensions
 * @return array
 */
  public static List<string> get_html5_video_extensions(this HelperBase helper)
  {
    var output = new List<string>() { "mp4", "m4v", "webm", "ogv", "ogg", "flv" };
    output = hooks.apply_filters<List<string>>("html5_video_extensions", output);
    return output;
  }
}
