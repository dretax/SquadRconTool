using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SquadRconServer.Tokens
{
    internal static class TokenHandler
    {
        private static readonly Dictionary<string, TokenHolder> Tokens = new Dictionary<string, TokenHolder>();
        private static readonly char[] chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        
        internal static string GetUniqueKey(int size)
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

        internal static string AddNewToken(string username)
        {
            string key = GetUniqueKey(15);
            Tokens.Add(username, new TokenHolder(key, DateTime.Now));
            return key;
        }

        internal static void RemoveToken(string username)
        {
            if (Tokens.ContainsKey(username))
            {
                Tokens.Remove(username);
            }
        }

        internal static void ClearTokens()
        {
            Tokens.Clear();
        }

        internal static bool HasValidToken(string username)
        {
            if (Tokens.ContainsKey(username))
            {
                var token = Tokens[username];
                var currentdate = DateTime.Now;
                bool b = (currentdate - token.TokenDate).TotalHours <= Server.TokenValidTime;
                if (!b)
                {
                    Tokens.Remove(username);
                }

                return b;
            }
            return false;
        }
    }
}