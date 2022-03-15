using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
        public static bool checkServerPrint(byte[] expected, byte[] print) 
        {
            return expected == print;
        }
        //Writes ssh server data to authorized_hosts.txt
        public static void appendAuthorizedHosts(string[] info,byte[] fingerprint) 
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + "/" + Properties.Resources.ResourcesDir + "/authorized_hosts.txt")) 
            {
                File.CreateText(Directory.GetCurrentDirectory() + "/" + Properties.Resources.ResourcesDir + "/authorized_hosts.txt").Dispose();
            }
            using (var stream = File.Open(Directory.GetCurrentDirectory() + "/" + Properties.Resources.ResourcesDir + "/authorized_hosts.txt", FileMode.Append, FileAccess.Write, FileShare.Write)) 
            {
                List<byte[]> data = new List<byte[]>();
                var newline = InsertNewline().ConfigureAwait(false);
                foreach (var item in info)
                {
                    string line = item + Environment.NewLine;
                    char[] str = line.ToCharArray();
                    for (int i = 0; i < str.Length; i++) 
                    {
                        byte[] chr = BitConverter.GetBytes(str[i]);
                        data.Add(chr);
                    }
                }
                data.Add(fingerprint);
                data.Add(fingerprint);
                for(int i=0; i<data.Count; i++) 
                {
                    stream.Write(data[i], 0, data[i].Length - 1);
                }
                stream.Flush();
            }
        }
        public static async Task<List<byte[]>> InsertNewline() 
        {
            List<byte[]> newline = new List<byte[]>();
            char[] str = Environment.NewLine.ToCharArray();
            for(int i=0; i<str.Length; i++)
            {
                newline.Add(BitConverter.GetBytes(str[i]));
            }
            return newline;
        }
    }
}
