using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Renci.SshNet;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Build.Logging;
using Newtonsoft.Json;
using System.Globalization;

namespace DiscordGameServerManager
{
    class Server
    {
        private const string config = "ssh.json";
        public static SSHInfo info = new SSHInfo();
        private static SshClient client;
        private static SshClient falseclient;
        private static Shell shell;
        private static bool initialized = false;
        private static MemoryStream input = new MemoryStream();
        private static MemoryStream output = new MemoryStream();
        private static MemoryStream extendedOutput = new MemoryStream();
        public static void Initialize() 
        {
            if(!File.Exists(Properties.Resources.ResourcesDir + "/" + config))
            {
                info.Address = "127.0.0.1";
                info.Pass = "password";
                info.Port = 22;
                info.fingerprint = new[] { 0x00,0x01,0x02 };
                info.Keypath = info.Keypath;
                info.Challenge = "Challenge type";
                string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
            }
        }
        public static void Initialize(Globalvars g) 
        {
            if (!File.Exists(Properties.Resources.ResourcesDir + "/"+ g.id + "/" + config)) 
            {
                info.Address = "127.0.0.1";
                info.Pass = "password";
                info.Port = 22;
                info.fingerprint = new[] { 0x00, 0x01, 0x02 };
                info.Keypath = info.Keypath;
                info.Challenge = "Challenge type";
                string json = JsonConvert.SerializeObject(info, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
            }
        }
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
            shell = client.CreateShell(input, output, extendedOutput);
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
            shell = client.CreateShell(input, output, extendedOutput);
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
            shell = client.CreateShell(input, output, extendedOutput);
            initialized = true;
        }
        public static string SendCommand(string cmd) 
        {
            string output = "";
            if (initialized) 
            {

                client.Connect();
                do
                {
                    if (client.IsConnected)
                    {
                        var command = client.CreateCommand(cmd);
                        command.Execute();
                        //client.RunCommand(command);
                        var result = command.Result;
                        Console.Out.WriteLineAsync(result).ConfigureAwait(false);
                        Console.Out.FlushAsync();
                        output = result.ToLower(CultureInfo.CurrentCulture);
                    }
                } while (!client.IsConnected);
                client.Disconnect();
                return output;
            }
            else 
            {
                return output;
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
                    if(current == max) 
                    {
                        Console.Out.FlushAsync();
                        client.Disconnect();
                    }
                    current += 1;
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
            info.Pass = SecurityManager.GetHash(pass);
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
        private string keypath;
        public string Address { get; set; }
        public string Username { get; set; }
        public string Pass { get; set; }
        public int Port { get; set; }
        public int[] fingerprint { get; set; }
        public string Keypath
        {
            get 
            {
                if (!string.IsNullOrWhiteSpace(Keypath)) 
                {
                    return keypath;
                }
                else 
                {
                    if (System.Runtime.InteropServices.OSPlatform.Linux == OSInfo.GetOSPlatform()) 
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
                keypath = value;
            }
        }
        public string Challenge { get; set; }
    }
}
