using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Renci.SshNet;
using System.Security;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Build.Logging;

namespace DiscordGameServerManager
{
    class Server
    {
        public static SSHInfo info = new SSHInfo();
        public static SshClient client;
        private static readonly SHA256 encrypt = SHA256.Create();
        private static bool initialized = false;
        public static void Initialize(string address, string username, string pass, string path)
        {
            SetAddress(address);
            if (!string.IsNullOrWhiteSpace(pass))
            {
                SetPass(pass);
                client = new SshClient(address,username,pass);
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                SetKeypath(path);
                PrivateKeyFile privateKey = new PrivateKeyFile(path);
                PrivateKeyFile[] keyFiles = new[] { privateKey };
                client = new SshClient(address, username, keyFiles);
            }
            initialized = true;
        }
        public static void Initialize(string address, int port, string username, string pass, string path) 
        {
            SetAddress(address);
            if (!string.IsNullOrWhiteSpace(pass)) 
            {
                SetPass(pass);
                client = new SshClient(address,port,username,pass);
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                SetKeypath(path);
                PrivateKeyFile privateKey = new PrivateKeyFile(path);
                PrivateKeyFile[] keyFiles = new[] { privateKey };
                client = new SshClient(address, port, username, keyFiles);
            }
            initialized = true;
        }
        public static void Initialize(string address, int port, string username, string pass, string path, string challenge)
        {
            SetAddress(address);
            if (!string.IsNullOrWhiteSpace(pass))
            {
                SetPass(pass);
                client = new SshClient(address, port, username, pass);
            }
            if (!string.IsNullOrWhiteSpace(path))
            {
                SetKeypath(path);
                PrivateKeyFile privateKey = new PrivateKeyFile(path);
                PrivateKeyFile[] keyFiles = new[] { privateKey };
                if (string.IsNullOrWhiteSpace(challenge))
                {
                    client = new SshClient(address, port, username, keyFiles);
                }
                else 
                { 
                    //TODO
                }
            }
            initialized = true;
        }
        public static void SendCommand(string command) 
        {
            if (initialized) 
            {
                client.Connect();
                if (client.IsConnected) 
                {
                    var commandtext = client.CreateCommand(command);
                    client.RunCommand(commandtext.CommandText);
                    var result = commandtext.Result;
                    Console.Out.WriteLineAsync(result).ConfigureAwait(false);
                    Console.Out.FlushAsync();
                    client.Disconnect();
                }
            }
        }
        public static void SendCommands(string[] commands) 
        {
            if (initialized) 
            {
                client.Connect();
                int current = 0;
                int max = commands.Length-1;
                while (client.IsConnected) 
                {
                    var commandtext = client.CreateCommand(commands[current]);
                    client.RunCommand(commandtext.CommandText);
                    var result = commandtext.Result;
                    Console.Out.WriteLineAsync(result).ConfigureAwait(false);
                    current += 1;
                    if(current == max) 
                    {
                        Console.Out.FlushAsync();
                        client.Disconnect();
                    }
                }
            }
        }
        private static void SetAddress(string address) 
        {
            if (string.IsNullOrWhiteSpace(address)) 
            {
                info.Address = "127.0.0.1";
            }
            else 
            {
                info.Address = address;
            }
        }
        private static void SetPass(string pass) 
        {
            info.Pass = encrypt.ComputeHash(Encoding.UTF8.GetBytes(pass.ToCharArray())).ToString();
        }
        private static void SetKeypath(string path) 
        {
            if (string.IsNullOrWhiteSpace(path)) 
            {
                info.Keypath = info.Keypath;
            }
            else 
            {
                info.Keypath = path;
            }
        }

    }
    struct SSHInfo 
    { 
        public string Address { get; set; }
        public string Pass { get; set; }
        public int Port { get; set; }
        public byte[] fingerprint { get; set; }
        public string Keypath
        {
            get 
            {
                if (!string.IsNullOrWhiteSpace(Keypath)) 
                {
                    return Keypath;
                }
                else 
                {
                    if (System.Runtime.InteropServices.OSPlatform.Linux == OS_Info.GetOSPlatform()) 
                    {
                        return "/home/" + Environment.UserName + "/.ssh";
                    }
                    else 
                    {
                        return Environment.GetEnvironmentVariable("SYSTEMDRIVE")+"/Users/"+Environment.UserName+"/.ssh";
                    }
                }
            }
            set 
            {
                Keypath = value;
            }
        }
        public string Challenge { get; set; }
    }
}
