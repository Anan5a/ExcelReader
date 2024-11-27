using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class UploadDTO
    {
        [MaxLength(32)]
        [MinLength(2)]
        [RegularExpression(@"^[a-zA-Z0-9-\(\)\._]*$", ErrorMessage = "The field must contain only alphanumeric characters.")]
        public string? FileName { get; set; }

        [Required]
        public IFormFile ExcelFile { get; set; }
    }
}