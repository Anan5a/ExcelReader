using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models.DTOs
{
    public class UploadDTO
    {
        [MinLength(8)]
        [MaxLength(32)]
        [RegularExpression(@"^[a-zA-Z0-9-]*$", ErrorMessage = "The field must contain only alphanumeric characters.")]
        public string? FileName { get; set; }

        [Required]
        public IFormFile excelFile { get; set; }
    }
}
