namespace Models
{

    public class AuthResponse
    {
        public required string Token { get; set; }
        public required User user { get; set; }
    }
}