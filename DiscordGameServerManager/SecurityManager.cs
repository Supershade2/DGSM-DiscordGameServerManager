using System;
using System.Collections.Generic;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace DiscordGameServerManager
{
    public static class SecurityManager
    {
        private static readonly SHA256 encrypt = SHA256.Create();
        public static string GetHash(string s) 
        { 
            s = encrypt.ComputeHash(Encoding.UTF8.GetBytes(s.ToCharArray())).ToString();
            return s;
        }
        public static bool HashCheck(string input, string hash) 
        {
            input = GetHash(input);
            return hash == input;
        }
    }
}
