using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SquadRconServer.Tokens
{
    internal static class TokenHandler
    {
        private static readonly Dictionary<string, DateTime> Tokens = new Dictionary<string, DateTime>();
        private static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        
        private static string GetUniqueKey(int size)
        {            
            byte[] data = new byte[4 * size];
            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }

        internal static string AddNewToken(int size)
        {
            string key = GetUniqueKey(15);
            Tokens.Add(key, DateTime.Now);
            return key;
        }

        internal static void ClearTokens()
        {
            Tokens.Clear();
        }

        internal static bool IsValidToken(string token)
        {
            if (Tokens.ContainsKey(token))
            {
                var date = Tokens[token];
                var currentdate = DateTime.Now;
                bool b = (currentdate - date).TotalHours >= 24;
                if (b)
                {
                    Tokens.Remove(token);
                }

                return b;
            }
            return false;
        }
    }
}