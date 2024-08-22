using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models
{
    public class Upload
    {
        [Required]
        public IFormFile excelFile { get; set; }
    }
}
