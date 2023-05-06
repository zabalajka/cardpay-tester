using System.Globalization;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

namespace TesterWeb.Utils
{
    public class Crypto
    {
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data; 
        }

        public static string ConvertByteArrayToHexString(byte[] buffer)
        {
            string s = BitConverter.ToString(buffer).Replace("-", "").ToLowerInvariant();
            return s;
        }

        public static byte[] CalculateHMAC(string message, byte[] key)
        {
            var fn = new System.Security.Cryptography.HMACSHA256(key);
            byte[] HMAC_output = fn.ComputeHash(Encoding.ASCII.GetBytes(message));
            return HMAC_output;
        }

        public static byte[] CalculateSignature(string message, string PRIVATE_PEM)
        {
            var parameters = new PemReader(new StringReader(PRIVATE_PEM)).ReadObject();
            var signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(true, (ECPrivateKeyParameters)parameters);
            byte[] responseToVerify_Bytes = Encoding.UTF8.GetBytes(message);
            signer.BlockUpdate(responseToVerify_Bytes, 0, responseToVerify_Bytes.Length);
            byte[] signature = signer.GenerateSignature();
            return signature;
        }

        public static bool IsSignatureValid(string message, byte[] signature, string PUBLIC_PEM)
        {
            var paramters = new PemReader(new StringReader(PUBLIC_PEM)).ReadObject();
            var signer = SignerUtilities.GetSigner("SHA256withECDSA");
            signer.Init(false, (ICipherParameters)paramters);
            byte[] responseToVerify_Bytes = Encoding.UTF8.GetBytes(message);
            signer.BlockUpdate(responseToVerify_Bytes, 0, responseToVerify_Bytes.Length);
            bool isSignatureValid = signer.VerifySignature(signature);
            return isSignatureValid;
        }
    }
}