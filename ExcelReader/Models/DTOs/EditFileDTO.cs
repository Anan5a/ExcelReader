using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models.DTOs
{
    public class EditFileDTO
    {
        [Required]
        [MaxLength(32)]
        [MinLength(2)]
        [RegularExpression(@"^[a-zA-Z0-9-\(\)\._]*$", ErrorMessage = "The field must contain only alphanumeric characters.")]
        public string FileName { get; set; }

        [Required]
        public long fileId { get; set; }

    }
}
