using BLL.Services.IService;
using Microsoft.AspNetCore.Http;

namespace BL.Services.Service
{
    public class FileUploader : IFileUploader
    {
        // REMOVED: No longer needs a reference to the web hosting environment.
        // private readonly IWebHostEnvironment _hostingEnvironment;

        // The constructor is now parameterless.
        public FileUploader()
        {
        }

        public async Task<string> UploadImageAsync(IFormFile image, string basePath)
        {
            if (image == null || image.Length == 0)
                return null;

            // Use the provided basePath instead of _hostingEnvironment.WebRootPath
            var uploadsFolder = Path.Combine(basePath, "Files/uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(image.FileName);
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Return the relative path to the image, using forward slashes for web compatibility.
            return ("/uploads/" + uniqueFileName).Replace('\\', '/');
        }

        public Task<bool> DeleteImageAsync(string relativePath, string basePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return Task.FromResult(false);

            // Sanitize the input to get just the file name from the relative path
            var fileName = Path.GetFileName(relativePath);

            // Use the provided basePath instead of _hostingEnvironment.WebRootPath
            var filePath = Path.Combine(basePath, "uploads", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}