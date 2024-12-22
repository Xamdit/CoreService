namespace Service.Helpers.Pdf;

public class TestUnit
{
  [Test]
  public void Test()
  {
    // Initialize the PDF generator
    var pdfGenerator = new PdfDocumentGenerator("Invoice.pdf");
    // Add a title and some content
    var invoiceTitle = "Invoice #123";
    var invoiceContent = "This is the content of the invoice...";
    pdfGenerator.AddPage(invoiceTitle, invoiceContent);
    // Add a logo (optional)
    var logoPath = "path/to/logo.png";
    // if (WebRequestMethods.File.Exists(logoPath)) pdfGenerator.AddLogo(logoPath);
    // Save the PDF to disk
    pdfGenerator.Save();
  }
}
