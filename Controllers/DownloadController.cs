using Microsoft.AspNetCore.Mvc;
using Service.Controllers.Core;
using Service.Entities;
using Service.Framework;
using Service.Helpers;

namespace Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DownloadController(ILogger<DownloadController> logger, MyInstance self,MyContext db) : ClientControllerBase(logger, self,db)
{
  [HttpPost("preview_video")]
  public IActionResult PreviewVideo([FromForm] string path, [FromForm] string fileType)
  {
    var allowedExtensions = self.helper.get_html5_video_extensions();
    if (!file_exists(path) ||
        string.IsNullOrEmpty(Path.GetExtension(path)) ||
        !allowedExtensions.Contains(Path.GetExtension(path)))
    {
      fileType = "image/jpg";
      path = "assets/images/preview-not-available.jpg";
    }

    if (!System.IO.File.Exists(path)) return NotFound();
    var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
    return File(fileStream, fileType, Path.GetFileName(path));
  }

  [HttpGet("preview_image")]
  public IActionResult PreviewImage([FromForm] string path, [FromForm] string fileType)
  {
    var allowedExtensions = new[] { "jpg", "jpeg", "png", "bmp", "gif", "tif" };

    if (!file_exists(path) ||
        string.IsNullOrEmpty(Path.GetExtension(path)) ||
        !allowedExtensions.Contains(Path.GetExtension(path)))
    {
      fileType = "image/jpg";
      path = "assets/images/preview-not-available.jpg";
    }

    if (!System.IO.File.Exists(path)) return NotFound();

    var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
    return File(fileStream, fileType, Path.GetFileName(path));
  }

  [HttpPost("file")]
  public IActionResult FileDownload([FromForm] string folderIndicator, [FromForm] int attachmentId = 0)
  {
    var db = new MyContext();
    var path = ResolvePath(self, db, folderIndicator, attachmentId);
    if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return NotFound();
    var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
    return File(fileStream, "application/octet-stream", Path.GetFileName(path));
  }

  private string ResolvePath(dynamic self, MyContext db, string folderIndicator, int attachmentId)
  {
    var path = string.Empty;
    switch (folderIndicator)
    {
      case "ticket":
        if (self.helper.is_logged_in())
        {
          var attachment = db.TicketAttachments.FirstOrDefault(x => x.Id == attachmentId);
          if (attachment != null)
          {
            var ticket = self.tickets_model().get_ticket_by_id(attachment.TicketId);
            if (ticket != null && (ticket.ticket.UserId == self.helper.get_client_user_id() || db.is_staff_logged_in())) path = get_upload_path_by_type("ticket") + attachment.TicketId + "/" + attachment.FileName;
          }
        }

        break;

      case "newsfeed":
        if (db.is_staff_logged_in())
        {
          var attachment = db.Files.FirstOrDefault(x => x.Id == attachmentId);
          if (attachment != null) path = get_upload_path_by_type("newsfeed") + attachment.RelId + "/" + attachment.FileName;
        }

        break;

      case "contract":
        var contractAttachment = db.Files.FirstOrDefault(x => x.AttachmentKey == attachmentId.ToString());
        if (contractAttachment != null && (!db.is_staff_logged_in() || contractAttachment.RelType == "contract")) path = get_upload_path_by_type("contract") + contractAttachment.RelId + "/" + contractAttachment.FileName;
        break;

      case "taskattachment":
        if (self.helper.is_logged_in())
        {
          var taskAttachment = db.Files.FirstOrDefault(x => x.AttachmentKey == attachmentId.ToString());
          if (taskAttachment != null) path = get_upload_path_by_type("task") + taskAttachment.RelId + "/" + taskAttachment.FileName;
        }

        break;

      case "sales_attachment":
        var salesAttachment = db.Files.FirstOrDefault(x => x.AttachmentKey == attachmentId.ToString());
        if (salesAttachment != null) path = get_upload_path_by_type(salesAttachment.RelType) + salesAttachment.RelId + "/" + salesAttachment.FileName;
        break;

      case "expense":
        var expenseAttachment = db.Files.FirstOrDefault(x => x.RelId == attachmentId && x.RelType == "expense");
        if (expenseAttachment != null) path = get_upload_path_by_type("expense") + expenseAttachment.RelId + "/" + expenseAttachment.FileName;
        break;

      case "lead_attachment":
        var leadAttachment = db.Files.FirstOrDefault(x => x.Id == attachmentId);
        if (leadAttachment != null) path = get_upload_path_by_type("lead") + leadAttachment.RelId + "/" + leadAttachment.FileName;
        break;

      case "client":
        var clientAttachment = db.Files.FirstOrDefault(x => x.AttachmentKey == attachmentId.ToString());
        if (clientAttachment != null && (self.helper.has_permission("customers", "", "view") || self.helper.is_customer_admin(clientAttachment.RelId) || self.db.is_client_logged_in())) path = get_upload_path_by_type("customer") + clientAttachment.RelId + "/" + clientAttachment.FileName;
        break;
    }

    return path;
  }
}
