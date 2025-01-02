using Service.Core.Extensions;
using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Framework.Helpers.Entities;
using File = System.IO.File;

namespace Service.Helpers;

public static class UploadHelper
{
  /**
 * Function that return full path for upload based on passed type
 * @param  string  type
 * @return string
 */
  public static string get_upload_path_by_type(  string type)
  {

    var path = type switch
    {
      "lead" => LEAD_ATTACHMENTS_FOLDER,
      "expense" => EXPENSE_ATTACHMENTS_FOLDER,
      "project" => PROJECT_ATTACHMENTS_FOLDER,
      "proposal" => PROPOSAL_ATTACHMENTS_FOLDER,
      "estimate" => ESTIMATE_ATTACHMENTS_FOLDER,
      "invoice" => INVOICE_ATTACHMENTS_FOLDER,
      "credit_note" => CREDIT_NOTES_ATTACHMENTS_FOLDER,
      "task" => TASKS_ATTACHMENTS_FOLDER,
      "contract" => CONTRACTS_UPLOADS_FOLDER,
      "customer" => CLIENT_ATTACHMENTS_FOLDER,
      "staff" => STAFF_PROFILE_IMAGES_FOLDER,
      "company" => COMPANY_FILES_FOLDER,
      "ticket" => TICKET_ATTACHMENTS_FOLDER,
      "contact_profile_images" => CONTACT_PROFILE_IMAGES_FOLDER,
      "newsfeed" => NEWSFEED_FOLDER,
      "estimate_request" => NEWSFEED_FOLDER,
      _ => string.Empty
    };

    return hooks.apply_filters("get_upload_path_by_type", path, type);
  }

  /**
 * Check for ticket attachment after inserting ticket to database
 * @param  mixed  ticketid
 * @return mixed           false if no attachment || array uploaded attachments
 */
  public static List<TicketAttachment> handle_ticket_attachments(this HelperBase helper, int ticketId, string indexName = "attachments")
  {
    var (self, db) = getInstance();
    var path = helper.get_upload_path_by_type("ticket") + ticketId + "/";
    var uploadedFiles = new List<TicketAttachment>();

    // Assuming `HttpContext.Current.Request.Files` is used to simulate PHP's ` _FILES`
    var files = self.context.Request.Form.Files[indexName];
    if (files == null) return uploadedFiles.Count > 0 ? uploadedFiles : null;
    helper._file_attachments_index_fix(indexName);
    var maxAllowedAttachments = db.get_option<int>("maximum_allowed_ticket_attachments");

    for (var i = 0; i < files.Length; i++)
    {
      hooks.do_action("before_upload_ticket_attachment", ticketId);

      if (i > maxAllowedAttachments) continue;
      // Get the temp file path
      // var file = files.[i];
      var file = files;
      var tmpFilePath = "";
      //file.InputStream != null ? file.FileName : string.Empty;

      // Make sure we have a filepath
      if (string.IsNullOrEmpty(tmpFilePath)) continue;
      // Getting file extension
      var extension = self.helper.file_extension(file.FileName).ToLower();
      var allowedExtensions = db.get_option("ticket_attachments_file_extensions")
        .Split(',')
        .Select(e => e.Trim())
        .ToList();

      // Check if this extension is allowed
      if (!allowedExtensions.Contains(extension)) continue;

      helper.maybe_create_upload_path(path);
      var filename = helper.unique_filename(path, file.FileName);
      var newFilePath = Path.Combine(path, filename);

      // Upload the file into the specified path
      using (var fileStream = File.Create(newFilePath))
      {
        // file.InputStream.CopyTo(fileStream);
      }

      uploadedFiles.Add(new TicketAttachment
      {
        FileName = filename,
        FileType = file.ContentType
      });
    }

    return (uploadedFiles.Count > 0 ? uploadedFiles : null)!;
  }

  /**
 * Check if path exists if not exists will create one
 * This is used when uploading files
 * @param  string  path path to check
 * @return null
 */
  public static void maybe_create_upload_path(this HelperBase helper, string path)
  {
    if (Directory.Exists(path)) return;
    Directory.CreateDirectory(path);

    // Create an empty index.html file in the directory
    var indexFilePath = Path.Combine(path.TrimEnd(Path.DirectorySeparatorChar), "index.html");
    using (var fs = File.Create(indexFilePath))
    {
      // Optionally write something to the file if needed
    }
  }

  public static void _file_attachments_index_fix(this HelperBase helper, string indexName, Dictionary<string, List<string>> files = default)
  {
    if (!files.ContainsKey(indexName)) return;
    // if (files[indexName].ContainsKey("name") && files[indexName]["name"] is List<string> names) files[indexName]["name"] = names.Where(n => n != null).ToList();
    // if (files[indexName].ContainsKey("type") && files[indexName]["type"] is List<string> types) files[indexName]["type"] = types.Where(t => t != null).ToList();
    // if (files[indexName].ContainsKey("tmp_name") && files[indexName]["tmp_name"] is List<string> tmpNames) files[indexName]["tmp_name"] = tmpNames.Where(tmp => tmp != null).ToList();
    // if (files[indexName].ContainsKey("error") && files[indexName]["error"] is List<string> errors) files[indexName]["error"] = errors.Where(e => e != null).ToList();
    // if (files[indexName].ContainsKey("size") && files[indexName]["size"] is List<string> sizes) files[indexName]["size"] = sizes.Where(s => s != null).ToList();
  }

  /// <summary>
  /// Moves an uploaded file to a new location.
  /// </summary>
  /// <param name="sourceFilePath">The temporary file path of the uploaded file.</param>
  /// <param name="destinationFilePath">The target file path to move the file to.</param>
  /// <returns>True if the file was successfully moved; otherwise, false.</returns>
  public static bool move_uploaded_file(this HelperBase helper, string sourceFilePath, string destinationFilePath)
  {
    try
    {
      if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("Source file does not exist.", sourceFilePath);
      var destinationDirectory = Path.GetDirectoryName(destinationFilePath);
      if (!Directory.Exists(destinationDirectory)) Directory.CreateDirectory(destinationDirectory);
      File.Move(sourceFilePath, destinationFilePath);
      return true;
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error moving file: {ex.Message}");
      return false;
    }
  }

  /// <summary>
  /// Handles file upload for project discussion comments.
  /// </summary>
  /// <param name="discussionId">The discussion ID.</param>
  /// <param name="postData">Additional post data from the comment.</param>
  /// <param name="insertData">Data to be updated after processing the file.</param>
  /// <returns>Updated insert data with file information.</returns>
  public static DataSet<ProjectDiscussionComment> handle_project_discussion_comment_attachments(
    int discussionId,
    DataSet<ProjectDiscussionComment> postData,
    DataSet<ProjectDiscussionComment> insertData)
  {
    var (self, db) = getInstance();
    var file = self.context.Request.Form.Files["file"];
    if (file == null) return insertData; // No file uploaded
    if (file.Length > 0 && !IsUploadValid(file))
    {
      self.context.Response.StatusCode = 400; // Bad Request
      self.context.Response.WriteAsJsonAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
      {
        message = GetUploadError(file)
      }));
      self.context.Response.CompleteAsync();
    }

    var path = Path.Combine(self.globals("PROJECT_DISCUSSION_ATTACHMENT_FOLDER"), $"{discussionId}");

    // Check if the file extension is allowed
    if (!upload_extension_allowed(file.FileName))
    {
      self.context.Response.StatusCode = 400; // Bad Request
      self.context.Response.WriteAsJsonAsync(new { message = "File extension is blocked." });
      self.context.Response.CompleteAsync();
    }

    var tmpFilePath = file.FileName; // Temporary file path
    if (string.IsNullOrWhiteSpace(tmpFilePath)) return insertData;
    CreateUploadPathIfNeeded(path);

    var filename = GenerateUniqueFileName(path, file.FileName);
    var newFilePath = Path.Combine(path, filename);
    // Save the file
    // file.SaveAs(newFilePath);
    insertData["file_name"] = filename;
    insertData["file_mime_type"] = !string.IsNullOrWhiteSpace(file.ContentType)
      ? file.ContentType
      : self.helper.get_mime_by_extension(filename);

    return insertData;
  }

  private static bool IsUploadValid(IFormFile file)
  {
    return file.Length is > 0 and <= 10 * 1024 * 1024;
  }


  private static bool upload_extension_allowed(string filename)
  {
    var (self, db) = getInstance();
    var extension = self.helper.file_extension(filename);
    var allowed_extensions = db.get_option("allowed_files").Split(",").ToList();
    if (allowed_extensions.Contains("jpg")
        && !allowed_extensions.Contains(".jpeg")
       )
      allowed_extensions.Add("jpeg");
    return allowed_extensions.Contains(extension);
  }

  private static void CreateUploadPathIfNeeded(string path)
  {
    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
  }

  private static string GenerateUniqueFileName(string path, string originalFileName)
  {
    var fileName = Path.GetFileNameWithoutExtension(originalFileName);
    var extension = Path.GetExtension(originalFileName);

    string uniqueFileName;
    var counter = 1;
    do
    {
      uniqueFileName = $"{fileName}_{counter}{extension}";
      counter++;
    } while (File.Exists(Path.Combine(path, uniqueFileName)));

    return uniqueFileName;
  }

  private static string GetUploadError(IFormFile file)
  {
    return "An error occurred during the upload.";
  }

  public static List<Service.Entities.File> handle_task_attachments_array(this HelperBase helper, int taskId, string indexName = "attachments")
  {
    var (self, db) = getInstance();
    var context = self.input.context;
    var uploadedFiles = new List<Service.Entities.File>();
    var path = Path.Combine(get_upload_path_by_type("task"), taskId.ToString());

    if (context.Request.Form.Files.Count <= 0) return uploadedFiles.Count > 0 ? uploadedFiles : [];
    foreach (var file in context.Request.Form.Files)
    {
      if (file == null || file.Length == 0 || !is_upload_extension_allowed(file.FileName))
        continue;

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);


      var filename = generate_unique_filename(path, file.FileName);
      var newFilePath = Path.Combine(path, filename);

      using (var stream = new FileStream(newFilePath, FileMode.Create))
      {
        file.CopyTo(stream);
      }


      uploadedFiles.Add(new Service.Entities.File
      {
        FileName = filename,
        FileType = file.ContentType
      });
      if (is_image(newFilePath)) create_image_thumbnail(path, filename);
    }

    return uploadedFiles.Count > 0 ? uploadedFiles : [];
  }




  private static string generate_unique_filename(string path, string fileName)
  {
    var uniqueName = Path.GetFileNameWithoutExtension(fileName) + "_" + Guid.NewGuid() + Path.GetExtension(fileName);
    return uniqueName;
  }

  private static bool is_upload_extension_allowed(string fileName)
  {
    var allowedExtensions = new[] { ".jpg", ".png", ".pdf", ".docx" };
    var fileExtension = Path.GetExtension(fileName).ToLower();
    return allowedExtensions.Contains(fileExtension);
  }

  private static bool is_image(string filePath)
  {
    var allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var fileExtension = Path.GetExtension(filePath).ToLower();
    return allowedImageExtensions.Contains(fileExtension);
  }

  private static void create_image_thumbnail(string path, string fileName)
  {
    var filePath = Path.Combine(path, fileName);
  }

  /**
 * Handle lead attachments if any
 * @param  mixed leadid
 * @return boolean
 */
  public static bool handle_lead_attachments(this HelperBase helper, int leadId, string indexName = "file", bool formActivity = false)
  {
    var (self, db) = getInstance();
    var files = self.input.context.Request.Form.Files;

    // Check if the file exists in the form and handle form activity
    if (!files.Any(f => f.Name == indexName) && formActivity) return false;

    var file = files.FirstOrDefault(f => f.Name == indexName);
    if (file == null) return false;

    // Check for upload errors
    if (file.Length == 0 || !is_upload_extension_allowed(file.FileName))
    {
      self.input.context.Response.StatusCode = 400;
      self.input.context.Response.WriteAsJsonAsync(new { message = "An error occurred during the upload." });
      return false;
    }

    // Execute hooks before upload
    hooks.do_action("before_upload_lead_attachment", leadId);

    // Define the upload path
    var path = Path.Combine(get_upload_path_by_type("lead"), leadId.ToString());
    Directory.CreateDirectory(path);

    // Generate unique filename
    var filename = generate_unique_filename(path, file.FileName);
    var newFilePath = Path.Combine(path, filename);

    // Save the file
    using (var stream = new FileStream(newFilePath, FileMode.Create))
    {
      file.CopyTo(stream);
    }

    // Add attachment details to the database
    var leads_model = self.leads_model(db);
    var dataset = new Service.Entities.File
    {
      FileName = filename,
      FileType = file.ContentType
    };
    leads_model.add_attachment_to_database(leadId, dataset, null, formActivity);
    return true;
  }
}
