using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLL.ViewModels.Account
{
    public class ProductVM
    {
        public int ID { get; set; }
        [Required]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Required]
        public double Price { get; set; }
        public string? ProductImgName { get; set; }
        public IFormFile Image { get; set; }
        public string Note { get; set; }

    }
}
