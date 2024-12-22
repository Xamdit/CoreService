namespace Service.Framework.Library.Net;

public class Upload(string uploadPath)
{
  public async Task<string> UploadFileAsync(IFormFile file)
  {
    if (file == null || file.Length == 0) return "No file selected.";

    // Ensure the upload directory exists
    Directory.CreateDirectory(uploadPath);

    // Create a unique filename to prevent overwriting
    var fileName = Path.GetFileName(file.FileName);
    var uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
    var filePath = Path.Combine(uploadPath, uniqueFileName);

    // Save the file
    await using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    return uniqueFileName; // Return the unique filename or the full path if needed
  }
}
