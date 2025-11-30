using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class EditUserVM
{
    public string Id { get; set; }

    [Required(ErrorMessage = "الاسم مطلوب")]
    public string FullName { get; set; }

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صحيح")]
    public string Email { get; set; }

    public string PhoneNumber { get; set; }
    public string Role { get; set; }

    public string? ProfilePicture { get; set; }
    public IFormFile? ProfilePictureFile { get; set; }

}