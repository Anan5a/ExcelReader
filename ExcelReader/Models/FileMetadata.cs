using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ExcelReader.Models
{
    public class FileMetadata
    {
        [Key]
        [BindNever] 
        public long Id { get; set; }

        [Required]
        [MaxLength(32)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(32)]
        public string FileNameSystem { get; set; }

        [Required]
        //todo: rename to user_id
        public long FileOwner { get; set; }

        [Required]
        [BindNever]
        public DateTime CreatedAt { get; set; }
        
        [BindNever]
        public DateTime? UpdatedAt { get; set; }
        
        [BindNever]
        public DateTime? DeletedAt { get; set; }

        public User? User { get; set; }

        public FileMetadata() { }

        public FileMetadata(long id, string fileName, string fileNameSystem, long fileOwner,
                            DateTime createdAt, DateTime? updatedAt, DateTime? deletedAt,
                            User user, Role role)
        {
            Id = id;
            FileName = fileName;
            FileNameSystem = fileNameSystem;
            FileOwner = fileOwner;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            DeletedAt = deletedAt;
            User = user;
            
        }
    }

}
