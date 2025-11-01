using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.ViewModels.Account
{
    public class UserDetailsVM
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public IFormFile? ProfilePicture { get; set; }
        [Required(ErrorMessage = "درجة المستخدم مطلوبة")]
        public string UserRole { get; set; }
    }
}
