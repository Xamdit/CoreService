using RestSharp;
using Service.Core.Synchronus;
using Service.Helpers.Tags;
using Service.Libraries;
using Service.Schemas;

namespace Service.Core.Constants;

public static class GlobalConstants
{
  public static string Gateway { get; set; }

  // public static string Gateway => Env == "development" ? "https://localhost:5099/" : "https://api.xamdit.com/";
  public static AppSetting? Setting = new();
  public static string Env = "development";
  public static AppObject App = new();
  public static SyncBuilder syncBuilder = new("https://api.xamdit.com");
  public static AppObjectCache app_object_cache = new();


  // private static IStripeGateway stripeGateway = new StripeGateway();
  // public static StripeCore stripe_core => new(stripeGateway);

  public static string today()
  {
    return DateTime.UtcNow.ToString("Y-m-d H:i:s");
  }


  public static string date(string format = "Y-m-d")
  {
    return DateTime.UtcNow.ToString(format);
  }

  public static string date(DateTime d)
  {
    return d.ToString("Y-m-d H:i:s");
  }


  public static int DO_NOT_SEND_SMS_ON_DATA_OLDER_THEN = 30;


  // upload helper
  public static string LEAD_ATTACHMENTS_FOLDER = "uploads/leads/";
  public static string EXPENSE_ATTACHMENTS_FOLDER = "uploads/expenses/";
  public static string PROJECT_ATTACHMENTS_FOLDER = "uploads/projects/";
  public static string PROPOSAL_ATTACHMENTS_FOLDER = "uploads/proposals/";
  public static string ESTIMATE_ATTACHMENTS_FOLDER = "uploads/estimates/";
  public static string INVOICE_ATTACHMENTS_FOLDER = "uploads/invoices/";
  public static string CREDIT_NOTES_ATTACHMENTS_FOLDER = "uploads/credit_notes/";
  public static string TASKS_ATTACHMENTS_FOLDER = "uploads/tasks/";
  public static string CONTRACTS_UPLOADS_FOLDER = "uploads/contracts/";
  public static string CLIENT_ATTACHMENTS_FOLDER = "uploads/clients/";
  public static string STAFF_PROFILE_IMAGES_FOLDER = "uploads/staff_profile_images/";
  public static string COMPANY_FILES_FOLDER = "uploads/company/";
  public static string TICKET_ATTACHMENTS_FOLDER = "uploads/ticket_attachments/";
  public static string CONTACT_PROFILE_IMAGES_FOLDER = "uploads/client_profile_images/";
  public static string NEWSFEED_FOLDER = "uploads/newsfeed/";
  //end  upload helper

  public static string Token =>
    // if (Setting != null) return Setting.Token;
    string.Empty;

  // public static User CurrentUser { get; set; } = null;
  public static UserSchema CurrentUser { get; set; } = null;
  public static AppObject AppObjectCache = new();
  public static string CurrentDateFormat = "yyyy-MM-dd";

  public static int TimeFormat = 24;

  // public static BaseContext Context = new();
  public static readonly RestClientOptions UtilsUrl = new($"{Gateway}/utils") { MaxTimeout = -1 };

  public static readonly Dictionary<string, string> Mimes = new()
  {
    { "ai", "application/postscript" },
    { "aif", "audio/x-aiff" },
    { "aifc", "audio/x-aiff" },
    { "aiff", "audio/x-aiff" },
    { "avi", "video/x-msvideo" },
    { "bin", "application/octet-stream" },
    { "bmp", "image/bmp" },
    { "class", "application/octet-stream" },
    { "cpt", "application/mac-compactpro" },
    { "css", "text/css" },
    { "dms", "application/octet-stream" },
    { "doc", "application/msword" },
    { "docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
    { "dot", "application/msword" },
    { "dvi", "application/x-dvi" },
    { "exe", "application/octet-stream" },
    { "ez", "application/andrew-inset" },
    { "gif", "image/gif" },
    { "gtar", "application/x-gtar" },
    { "gz", "application/x-gzip" },
    { "gzip", "application/x-gzip" },
    { "hdf", "application/x-hdf" },
    { "hqx", "application/mac-binhex40" },
    { "htm", "text/html" },
    { "html", "text/html" },
    { "jar", "application/java-archive" },
    { "java", "text/plain" },
    { "jpeg", "image/jpeg" },
    { "jpg", "image/jpeg" },
    { "jpe", "image/jpeg" },
    { "latex", "application/x-latex" },
    { "mid", "audio/x-midi" },
    { "midi", "audio/x-midi" },
    { "mov", "video/quicktime" },
    { "movie", "video/x-sgi-movie" },
    { "mp2", "audio/mpeg" },
    { "mp3", "audio/mpeg" },
    { "mpe", "video/mpeg" },
    { "mpeg", "video/mpeg" },
    { "mpg", "video/mpeg" },
    { "mpga", "audio/mpeg" },
    { "oda", "application/oda" },
    { "pdf", "application/pdf" },
    { "png", "image/png" },
    { "ppt", "application/vnd.ms-powerpoint" },
    { "pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
    { "ps", "application/postscript" },
    { "qt", "video/quicktime" },
    { "ra", "audio/x-realaudio" },
    { "ram", "audio/x-pn-realaudio" },
    { "ras", "image/x-cmu-raster" },
    { "rgb", "image/x-rgb" },
    { "rtf", "application/rtf" },
    { "rtx", "text/richtext" },
    { "sgm", "text/sgml" },
    { "sgml", "text/sgml" },
    { "sit", "application/x-stuffit" },
    { "tar", "application/x-tar" },
    { "tiff", "image/tiff" },
    { "tif", "image/tiff" },
    { "tsv", "text/tab-separated-values" },
    { "txt", "text/plain" },
    { "wav", "audio/x-wav" },
    { "webm", "video/webm" },
    { "xhtml", "application/xhtml+xml" },
    { "xlb", "application/vnd.ms-excel" },
    { "xls", "application/vnd.ms-excel" },
    { "xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
    { "xml", "text/xml" },
    { "zip", "application/zip" }
  };

  public static string Tempfolder = "temp/";
  public static string Clientattachmentsfolder = "uploads/clients/";
  public static string Ticketattachmentsfolder = "uploads/ticket_attachments/";
  public static string Companyfilesfolder = "uploads/company/";
  public static string Staffprofileimagesfolder = "uploads/staff_profile_images/";
  public static string Contactprofileimagesfolder = "uploads/client_profile_images/";
  public static string Newsfeedfolder = "uploads/newsfeed/";
  public static string Contractsuploadsfolder = "uploads/contracts/";
  public static string Tasksattachmentsfolder = "uploads/tasks/";
  public static string Invoiceattachmentsfolder = "uploads/invoices/";
  public static string Estimateattachmentsfolder = "uploads/estimates/";
  public static string Proposalattachmentsfolder = "uploads/proposals/";
  public static string Expenseattachmentsfolder = "uploads/expenses/";
  public static string Leadattachmentsfolder = "uploads/leads/";
  public static string Projectattachmentsfolder = "uploads/projects/";
  public static string Projectdiscussionattachmentfolder = "uploads/discussions/";
  public static string Creditnotesattachmentsfolder = "uploads/credit_notes/";
  public static string Appmodulespath = "modules/";


  public static AppObjectCache _cacheService = new();
}
