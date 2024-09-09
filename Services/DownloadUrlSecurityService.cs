using System.Security.Cryptography;
using System.Text;

namespace Services
{
    public class DownloadUrlSecurityService
    {
        public static string? SignData(string dataToSign, string privateKeyB64)
        {
            try
            {

                string txtPrivateKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(privateKeyB64));
                var privateKey = txtPrivateKey.ToCharArray();

                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromPem(privateKey);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToSign);
                    return BitConverter.ToString(
                        rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
                        ).Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return null;
            }
        }


        public static bool VerifySignature(string dataToVerify, string signatureHex, string publicKeyB64, bool computeMd5 = false)
        {
            try
            {

                string txtPublicKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(publicKeyB64));
                var publicKey = txtPublicKey.ToCharArray();
                var signature = HexToBytes(signatureHex);
                using (RSA rsa = RSA.Create())
                {
                    rsa.ImportFromPem(publicKey);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(dataToVerify);
                    try
                    {
                        return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                    }
                    catch (CryptographicException)
                    {
                        // Verification failed
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorConsole.Log(ex.Message);
                return false;
            }
        }

        public static byte[] HexToBytes(string hex)
        {
            int numberOfChars = hex.Length;
            byte[] bytes = new byte[numberOfChars / 2];
            for (int i = 0; i < numberOfChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        public static string ComputeMd5Hash(string input) => BitConverter.ToString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();


    }
}
