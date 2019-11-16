using System;
using System.Security.Cryptography;
using System.Text;

namespace SquadRconServer.Tokens
{
    internal static class Crypto
    {
        public static string SHA256Hash(string value)
        {
            StringBuilder Sb = new StringBuilder();
            
            using (var hash = SHA256.Create())            
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (byte b in result)
                {
                    Sb.Append(b.ToString("x2"));
                }
            }

            return Sb.ToString();
        }
    }
}