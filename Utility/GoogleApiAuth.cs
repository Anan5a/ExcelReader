using Google.Apis.Auth;

namespace Utility
{
    public class GoogleApiAuth
    {
        public static async Task<GoogleJsonWebSignature.Payload> VerifyIdTokenAsync(string idToken, string clientId)
        {
            // Validate the ID token
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId },
                IssuedAtClockTolerance = TimeSpan.FromMinutes(8),
                ExpirationTimeClockTolerance = TimeSpan.FromMinutes(2),

            };

            // Validate the token and get the payload
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return payload;
        }
    }
}