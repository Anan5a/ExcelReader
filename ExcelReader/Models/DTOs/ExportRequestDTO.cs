using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models.DTOs
{
    public class ExportRequestDTO
    {
        [Required(ErrorMessage = "File id is required.")]
        public long FileId { get; set; }

    }
}
