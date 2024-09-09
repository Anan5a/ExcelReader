using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Models
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
        public long UserId { get; set; }

        [Required]
        [BindNever]
        public DateTime CreatedAt { get; set; }

        [BindNever]
        public DateTime? UpdatedAt { get; set; }

        [BindNever]
        public DateTime? DeletedAt { get; set; }

        [BindNever]
        public long FilesizeBytes { get; set; }



        public Role? User { get; set; }

        public FileMetadata() { }

        public FileMetadata(long id, string fileName, string fileNameSystem, long fileOwner,
                            DateTime createdAt, DateTime? updatedAt, DateTime? deletedAt,
                            Role user, Role role, long fsb)
        {
            Id = id;
            FileName = fileName;
            FileNameSystem = fileNameSystem;
            UserId = fileOwner;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
            DeletedAt = deletedAt;
            User = user;
            FilesizeBytes = fsb;

        }
    }

}
