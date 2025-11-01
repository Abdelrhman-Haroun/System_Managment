using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BLL.Services.IService
{
    public interface IFileUploader
    {
        /// <summary>
        /// Saves an image to a specified path.
        /// </summary>
        /// <param name="image">The image file to upload.</param>
        /// <param name="basePath">The absolute base path where the 'uploads' folder is (e.g., wwwroot).</param>
        /// <returns>The relative URL path to the saved image (e.g., /uploads/image.jpg).</returns>
        Task<string> UploadImageAsync(IFormFile image, string basePath);

        /// <summary>
        /// Deletes an image from a specified path.
        /// </summary>
        /// <param name="relativePath">The relative URL path of the image to delete (e.g., /uploads/image.jpg).</param>
        /// <param name="basePath">The absolute base path where the 'uploads' folder is (e.g., wwwroot).</param>
        /// <returns>True if the deletion was successful.</returns>
        Task<bool> DeleteImageAsync(string relativePath, string basePath);
    }
}