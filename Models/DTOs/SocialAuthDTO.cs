using System.ComponentModel.DataAnnotations;

namespace Models.DTOs
{
    public class SocialAuthDTO
    {
        [Required(ErrorMessage = "Id Token is required.")]
        public string IdToken { get; set; }

        // Default constructor
        public SocialAuthDTO() { }

        // Parameterized constructor
        public SocialAuthDTO(string idtoken)
        {
            IdToken = idtoken;
        }
    }
}

