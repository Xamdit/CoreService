using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Service.Helpers.Pdf;

public class PdfDocumentGenerator(string _fileName)
{
  private readonly string _fileName;
  private PdfDocument _document = new();


  // Method to generate a new page
  public void AddPage(string title, string content)
  {
    var page = _document.AddPage();
    var gfx = XGraphics.FromPdfPage(page);

    // Define the font
    var font = new XFont("Verdana", 20, XFontStyleEx.Bold);

    // Draw the title
    gfx.DrawString(title, font, XBrushes.Black,
      new XRect(0, 0, page.Width, 50),
      XStringFormats.TopCenter);

    // Draw content text
    font = new XFont("Verdana", 12, XFontStyleEx.Regular);
    gfx.DrawString(content, font, XBrushes.Black,
      new XRect(40, 100, page.Width - 80, page.Height - 150),
      XStringFormats.TopLeft);
  }

  // Method to add a logo to the PDF (example with a centered image)
  public void AddLogo(string imagePath)
  {
    var page = _document.AddPage();
    var gfx = XGraphics.FromPdfPage(page);

    // Load the image from file
    var logo = XImage.FromFile(imagePath);

    // Draw the logo at the center
    gfx.DrawImage(logo, (page.Width - logo.PixelWidth) / 2, (page.Height - logo.PixelHeight) / 2);
  }

  // Save the document to a file
  public void Save()
  {
    try
    {
      _document.Save(_fileName);
      Console.WriteLine($"PDF saved to {_fileName}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error saving PDF: {ex.Message}");
    }
  }

  public bool Output(string path)
  {
    try
    {
      _document.Save(path);
      Console.WriteLine($"PDF saved to {path}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Error saving PDF: {ex.Message}");
      return false;
    }

    return true;
  }


  // Method to return the PDF as a memory stream (useful for web apps)
  public MemoryStream GetPdfStream()
  {
    var stream = new MemoryStream();
    _document.Save(stream, false);
    return stream;
  }
}
