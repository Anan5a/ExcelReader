namespace Utility
{

    public class PasswordManager
    {
        public static string HashPassword(string password, int cost = 14)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: cost);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

}