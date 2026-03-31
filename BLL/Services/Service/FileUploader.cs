using BLL.Services.IService;
using Microsoft.AspNetCore.Http;

namespace BLL.Services.Service
{
    public class FileUploader : IFileUploader
    {
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

            return ("/Files/uploads/" + uniqueFileName).Replace('\\', '/');
        }

        public Task<bool> DeleteImageAsync(string relativePath, string basePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return Task.FromResult(false);

            var fileName = Path.GetFileName(relativePath);
            if (string.IsNullOrEmpty(fileName))
                return Task.FromResult(false);

            var candidatePaths = new[]
            {
                Path.Combine(basePath, "Files", "uploads", fileName),
                Path.Combine(basePath, "uploads", fileName)
            };

            foreach (var filePath in candidatePaths)
            {
                if (!File.Exists(filePath))
                    continue;

                File.Delete(filePath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
