using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Org.BouncyCastle.OpenSsl;

namespace TesterWeb.Utils
{
    public static class Keys
    {
        // no cache
        public static JsonNode All => Load();
        public static Func<string, string> KEY = (MID) => All["merchants"][MID]["KEY"].GetValue<string>();
        public static Func<string, byte[]> KEYo = (MID) => Crypto.ConvertHexStringToByteArray(KEY(MID));
        public static Func<string, string> PRIVATE_PEM = (ECDSA_KEY) => All["bank-keys"][ECDSA_KEY]["PRIVATE_PEM"]?.GetValue<string>();
        public static Func<string, object> PRIVATE_PEMo = (ECDSA_KEY) => new PemReader(new StringReader(PRIVATE_PEM(ECDSA_KEY))).ReadObject();
        public static Func<string, string> PUBLIC_PEM = (ECDSA_KEY) => All["bank-keys"][ECDSA_KEY]["PUBLIC_PEM"].GetValue<string>();
        public static Func<string, object> PUBLIC_PEMo = (ECDSA_KEY) => new PemReader(new StringReader(PUBLIC_PEM(ECDSA_KEY))).ReadObject();

        private static JsonNode Load()
        {
            string path = "../keys.json";
            var keys = JsonSerializer.Deserialize<JsonNode>(File.ReadAllText(path));
            return keys;
        }
    }
}