using Service.Entities;
using Service.Framework.Core.Engine;
using Service.Framework.Helpers;
using Service.Models.Statements;
using File = System.IO.File;

namespace Service.Helpers.Pdf;

public static class PdfHelper
{
  public static string APPPATH = "./apps/";

  public static string LIBSPATH = "./libraries/";

  public static void load_pdf_language(this HelperBase helper, string clientLanguage)
  {
    var (self, db) = getInstance();
    var language = db.get_option("active_language");

    // When cron or email sending PDF document the PDFs need to be in the client language
    if (helper.is_data_for_customer() || helper.is_cron())
    {
      if (!string.IsNullOrEmpty(clientLanguage)) language = clientLanguage;
    }
    else
    {
      if (!string.IsNullOrEmpty(clientLanguage) && db.get_option("output_client_pdfs_from_admin_area_in_client_language") == "1") language = clientLanguage;
    }

    var languagePath = Path.Combine(APPPATH, "language", language);

    if (!FileExists(languagePath)) return;
    // self.Lang.Load(language + "_lang", language);
    helper.load_custom_lang_file(language);
    // self.Lang.SetLastLoadedLanguage(language);
    self.hooks.do_action("load_pdf_language", new { language });
  }

  public static string pdf_logo_url(this HelperBase helper)
  {
    var (self, db) = getInstance();

    var custom_pdf_logo_url = db.get_option("custom_pdf_logo_image_url");
    var companyUploadPath = helper.get_upload_path_by_type("company");
    var logoUrl = "";
    var width = int.TryParse(db.get_option("pdf_logo_width"), out var w) ? w : 120;

    if (!string.IsNullOrEmpty(custom_pdf_logo_url))
    {
      logoUrl = custom_pdf_logo_url;
    }
    else
    {
      var companyLogoDark = db.get_option("company_logo_dark");
      var companyLogo = db.get_option("company_logo");

      if (!string.IsNullOrEmpty(companyLogoDark) && FileExists(Path.Combine(companyUploadPath, companyLogoDark)))
        logoUrl = Path.Combine(companyUploadPath, companyLogoDark);
      else if (!string.IsNullOrEmpty(companyLogo) && FileExists(Path.Combine(companyUploadPath, companyLogo))) logoUrl = Path.Combine(companyUploadPath, companyLogo);
    }

    return !string.IsNullOrEmpty(logoUrl)
      ? $"<img width='{width}px' src='{logoUrl}'>"
      : self.hooks.apply_filters("pdf_logo_url", logoUrl);
  }

  public static List<string> get_pdf_fonts_list(this HelperBase helper)
  {
    var (self, db) = getInstance();
    var fontList = new List<string>();
    // string fontsDir = TCPDF_FONTS.GetFontPath();
    var fontsDir = string.Empty;

    if (Directory.Exists(fontsDir))
      fontList.AddRange(Directory.EnumerateFiles(fontsDir, "*.json").Select(file => Path.GetFileNameWithoutExtension(file).ToLower()).Where(name => !name.EndsWith("i")));

    return self.hooks.apply_filters("pdf_fonts_list", fontList);
  }


  public static (string orientation, string format) get_pdf_format(this HelperBase helper, string optionName)
  {
    var (self, db) = getInstance();
    var optionValue = db.get_option(optionName).ToUpper();
    var (orientation, format) = ("", "");

    switch (optionValue)
    {
      case "A4-PORTRAIT":
        orientation = "P";
        format = "A4";
        break;
      case "A4-LANDSCAPE":
        orientation = "L";
        format = "A4";
        break;
      case "LETTER-PORTRAIT":
        orientation = "P";
        format = "LETTER";
        break;
      case "LETTER-LANDSCAPE":
        orientation = "L";
        format = "LETTER";
        break;
    }

    return (orientation, format);
  }


  // This method generates PDFs for different types like invoices, contracts, etc.
  public static void generate_pdf(this HelperBase helper, string type, object model)
  {
    var classPath = helper.get_pdf_class_path(type);
    // var pdf = Activator.CreateInstance(Type.GetType(classPath), model);
    var pdf = new PdfDocumentGenerator(classPath);
    // Assuming the pdf object has a method called Prepare
    pdf.Save();
  }

  private static string get_pdf_class_path(this HelperBase helper, string type)
  {
    return type switch
    {
      "invoice" => "Path.To.InvoicePdfClass",
      "estimate" => "Path.To.EstimatePdfClass",
      "contract" => "Path.To.ContractPdfClass",
      // Additional cases here
      _ => throw new NotImplementedException()
    };
  }

  /**
  * Set constant for sending mail template
  * Used to identify if the custom fields should be shown and loading the PDF language
  */
  public static bool set_mailing_constant(this HelperBase helper)
  {
    // if (!defined('SEND_MAIL_TEMPLATE')) {
    //   define('SEND_MAIL_TEMPLATE', true);
    // }
    return true;
  }

  /**
 * Prepare general invoice pdf
 * @param  object $invoice Invoice as object with all necessary fields
 * @param  string $tag     tag for bulk pdf exporter
 * @return mixed object
 */
  public static PdfDocumentGenerator invoice_pdf(this HelperBase helper, Invoice invoice, string tag = "")
  {
    return helper.app_pdf("invoice", $"{LIBSPATH}pdf/Invoice_pdf", invoice, tag);
  }

  /**
 * Generate payment pdf
 * @param  mixed $payment payment from database
 * @param  string $tag     tag for bulk pdf exporter
 * @return object
 */
  public static PdfDocumentGenerator payment_pdf(this HelperBase helper, InvoicePaymentRecord payment, string tag = "")
  {
    return helper.app_pdf("payment", $"{LIBSPATH}pdf/Payment_pdf", payment, tag);
  }

  /// <summary>
  /// General function for PDF documents logic.
  /// </summary>
  /// <param name="type">Document type e.g. payment, statement, invoice</param>
  /// <param name="path">Full class path</param>
  /// <param name="parameters">Parameters to pass in class constructor</param>
  /// <returns>Instance of the document object after preparation</returns>
  public static PdfDocumentGenerator app_pdf(this HelperBase helper, string type, string path, params object[] parameters)
  {
    var (self, db) = getInstance();
    // Initialize the PDF generator
    var pdfGenerator = new PdfDocumentGenerator(path);
    // Add a title and some content
    var invoiceTitle = "Invoice #123";
    var invoiceContent = "This is the content of the invoice...";
    pdfGenerator.AddPage(invoiceTitle, invoiceContent);
    // Add a logo (optional)
    var logoPath = "path/to/logo.png";
    if (File.Exists(logoPath)) pdfGenerator.AddLogo(logoPath);
    // Save the PDF to disk
    pdfGenerator.Save();


    const string fileExtension = ".cs"; // Assuming we're dealing with C# source files

    // Get class name by capitalizing the base name of the path
    var className = Path.GetFileNameWithoutExtension(path)?.First().ToString().ToUpper() + Path.GetFileNameWithoutExtension(path)?.Substring(1);

    // If the path doesn't end with the expected extension, add it
    if (!path.EndsWith(fileExtension)) path += fileExtension;

    // Apply hooks or filters to the path (assuming hook functionality is implemented)
    // path = self.hooks.apply_filters($"{type}_pdf_class_path", path, parameters);
    path = self.hooks.apply_filters($"{type}_pdf_class_path", path);

    // Load the class via reflection
    Type classType;
    try
    {
      // Ensure the file exists before loading
      if (!File.Exists(path)) throw new FileNotFoundException($"The specified path '{path}' does not exist.");

      // Dynamically load the assembly if needed (if the class resides in a separate assembly)
      // For simplicity, let's assume the class is within the same project/namespace
      classType = Type.GetType(className) ?? throw new InvalidOperationException($"Class '{className}' not found.");
    }
    catch (Exception ex)
    {
      throw new Exception($"Failed to load the class at path '{path}': {ex.Message}");
    }

    // Instantiate the class with the provided parameters
    object instance;
    try
    {
      instance = Activator.CreateInstance(classType, parameters);
    }
    catch (Exception ex)
    {
      throw new Exception($"Failed to instantiate the class '{className}' with the provided parameters: {ex.Message}");
    }

    // Ensure the class has a 'Prepare' method
    var prepareMethod = classType.GetMethod("Prepare");
    if (prepareMethod == null) throw new MissingMethodException($"The class '{className}' does not contain a method named 'Prepare'.");

    // Call the 'Prepare' method
    // return prepareMethod.Invoke(instance, null);
    return pdfGenerator;
  }


  /**
* Check whether the data is intended to be shown for the customer
* For example this function is used for custom fields, pdf language loading etc...
* @return boolean
*/
  public static bool is_data_for_customer(this HelperBase helper)
  {
    return helper.is_client_logged_in()
           || (!helper.is_staff_logged_in() && !helper.is_client_logged_in())
           || helper.defined("SEND_MAIL_TEMPLATE")
           || helper.defined("CLIENTS_AREA")
           || helper.defined("GDPR_EXPORT");
  }


  public static void load_custom_lang_file(this HelperBase helper, string language)
  {
    if (!helper.file_exists($"language/{language}/custom_lang.json")) return;
    // if (array_key_exists('custom_lang.php',  $CI->lang->is_loaded)) {
    //   unset($CI->lang->is_loaded['custom_lang.php']);
    // }
    // self.Lang.Load("custom_lang", language);
  }

  private static bool FileExists(string path)
  {
    return File.Exists(path);
  }


  public static PdfDocumentGenerator invoice_pdf(this HelperBase helper, object invoice, string tag = "")
  {
    return helper.app_pdf("invoice", $"{LIBSPATH}pdf/invoice_pdf", invoice, tag);
  }

  public static PdfDocumentGenerator credit_note_pdf(this HelperBase helper, object creditNote, string tag = "")
  {
    return helper.app_pdf("credit_note", $"{LIBSPATH}pdf/credit_note_pdf", creditNote, tag);
  }

  public static PdfDocumentGenerator EstimatePdf(this HelperBase helper, object estimate, string tag = "")
  {
    return helper.app_pdf("estimate", $"{LIBSPATH}pdf/estimate_pdf", estimate, tag);
  }

  public static PdfDocumentGenerator ProposalPdf(this HelperBase helper, object proposal, string tag = "")
  {
    return helper.app_pdf("proposal", $"{LIBSPATH}pdf/proposal_pdf", proposal, tag);
  }

  public static PdfDocumentGenerator contract_pdf(this HelperBase helper, object contract)
  {
    return helper.app_pdf("contract", $"{LIBSPATH}pdf/contract_pdf", contract);
  }

  public static PdfDocumentGenerator payment_pdf(this HelperBase helper, object payment, string tag = "")
  {
    return helper.app_pdf("payment", $"{LIBSPATH}pdf/Payment_pdf", payment, tag);
  }

  public static void BulkPdfExportMaybeTag(this HelperBase helper, string tag, ref object pdf)
  {
    var (self, db) = getInstance();
    if (string.IsNullOrEmpty(tag)) return;
    var fontName = db.get_option("pdf_font");
    var fontSize = int.TryParse(db.get_option("pdf_font_size"), out var fs) ? fs : 10;

    // Implement the logic for handling the PDF tag
    pdf.GetType().GetMethod("SetFillColor").Invoke(pdf, new object[] { 240, 240, 240 });
    pdf.GetType().GetMethod("SetDrawColor").Invoke(pdf, new object[] { 245, 245, 245 });
    pdf.GetType().GetMethod("SetXY").Invoke(pdf, new object[] { 0, 0 });
    pdf.GetType().GetMethod("SetFont").Invoke(pdf, new object[] { fontName, 'B', 15 });
    pdf.GetType().GetMethod("SetTextColor").Invoke(pdf, new object[] { 0 });
    pdf.GetType().GetMethod("SetLineWidth").Invoke(pdf, new object[] { 0.75 });
    pdf.GetType().GetMethod("StartTransform").Invoke(pdf, null);
    pdf.GetType().GetMethod("Rotate").Invoke(pdf, new object[] { -35, 109, 235 });
    pdf.GetType().GetMethod("Cell").Invoke(pdf, new object[] { 100, 1, tag.ToUpper(), "TB", 0, "C", true });
    pdf.GetType().GetMethod("StopTransform").Invoke(pdf, null);
    pdf.GetType().GetMethod("SetFont").Invoke(pdf, new object[] { fontName, "", fontSize });
    pdf.GetType().GetMethod("SetX").Invoke(pdf, new object[] { 10 });
    pdf.GetType().GetMethod("SetY").Invoke(pdf, new object[] { 10 });
  }

  public static void PdfMultiRow(string left, string right, object pdf, int leftWidth = 40)
  {
    var pageStart = pdf.GetType().GetMethod("GetPage").Invoke(pdf, null);
    var yStart = pdf.GetType().GetMethod("GetY").Invoke(pdf, null);

    // Write the left cell
    pdf.GetType().GetMethod("MultiCell").Invoke(pdf, new object[] { leftWidth, 0, left, 0, 'L', false, 2, null, null, true, 0, true });
    var pageEnd1 = pdf.GetType().GetMethod("GetPage").Invoke(pdf, null);
    var yEnd1 = pdf.GetType().GetMethod("GetY").Invoke(pdf, null);

    pdf.GetType().GetMethod("SetPage").Invoke(pdf, new object[] { pageStart });

    // Write the right cell
    pdf.GetType().GetMethod("MultiCell").Invoke(pdf, new object[] { 0, 0, right, 0, 'R', false, 1, pdf.GetType().GetMethod("GetX").Invoke(pdf, null), yStart, true, 0, true });
    var pageEnd2 = pdf.GetType().GetMethod("GetPage").Invoke(pdf, null);
    var yEnd2 = pdf.GetType().GetMethod("GetY").Invoke(pdf, null);

    var yNew = Math.Max((int)yEnd1, (int)yEnd2);
    pdf.GetType().GetMethod("SetXY").Invoke(pdf, new object[] { pdf.GetType().GetMethod("GetX").Invoke(pdf, null), yNew });
  }

  /**
 * Prepare general credit note pdf
 * @param  object $credit_note Credit note as object with all necessary fields
 * @param  string $tag tag for bulk pdf exported
 * @return mixed object
 */
  public static PdfDocumentGenerator credit_note_pdf(this HelperBase helper, CreditNote credit_note, string tag = "")
  {
    // var lib = helper.libs.pdf();
    return helper.app_pdf("credit_note", $"{LIBSPATH}pdf/credit_note_pdf", credit_note, tag);
  }

  /**
 * Prepare customer statement pdf
 * @param  object $statement statement
 * @return mixed
 */
  public static PdfDocumentGenerator statement_pdf(this HelperBase helper, StatementResult statement)
  {
    return helper.app_pdf("statement", $"{LIBSPATH}pdf/statement_pdf", statement);
  }

  /**
 * Prepare general estimate pdf
 * @since  Version 1.0.2
 * @param  object $estimate estimate as object with all necessary fields
 * @param  string $tag tag for bulk pdf exporter
 * @return mixed object
 */
  public static PdfDocumentGenerator estimate_pdf(this LibraryBase libs, Estimate estimate, string tag = "")
  {
    var (self, db) = getInstance();
    return self.helper.app_pdf("estimate", LIBSPATH + "pdf/Estimate_pdf", estimate, tag);
  }

  /**
 * Function that generates proposal pdf for admin and clients area
 * @param  object $proposal
 * @param  string $tag      tag for bulk pdf exporter
 * @return object
 */
  public static PdfDocumentGenerator proposal_pdf(this LibraryBase libs, Proposal proposal, string tag = "")
  {
    var (self, db) = getInstance();
    return self.helper.app_pdf("proposal", LIBSPATH + "pdf/Proposal_pdf", proposal, tag);
  }
}
