using System;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO.Compression;
using System.Globalization;
using System.Net.Http;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.RegularExpressions;

namespace DiscordGameServerManager
{
    class Program
    {
        public static Details _details = new Details();
        public static bool log = false;
        public static string logoutput = "";
        public static bool verboseoutput = false;
        public static bool MemoryStorage = false;
        //public static DiscordFunctions functions = new DiscordFunctions();
        static void Main(string[] args)
        {
            if (args.Length > 0) 
            {
                for (int i = 0; i < args.Length; i++) 
                {
                    args[i] = args[i].ToLower(CultureInfo.CurrentCulture);
                    switch (args[i]) 
                    {
                        case "--log":
                            log = true;
                            string pattern = @"[/]\w+";
                            if (i + 1 < args.Length - 1)
                            {
                                Match m = Regex.Match(args[i + 1], pattern);
                                logoutput = m.Success == true ? args[i + 1] : "";
                            }
                            break;
                        case "--verbose":
                            verboseoutput = true;
                            break;
                        case "-v":
                            verboseoutput = true;
                            break;
                        case "--memory":
                            MemoryStorage = true;
                            break;
                    }

                }
                verboseoutput = args.Contains("--verbose") || args.Contains("-v");
            }
            Setup setup = new Setup();
            for (int i = 0; i < Config.bot.cluster.servers.Length; i++) 
            {
                setup.CreateScript(i);
            }
            setup.Dispose();
            DiscordFunctions.Connect();
            DiscordFunctions.MainDiscord();
            Console.CancelKeyPress += (sender, e) => 
            {
                DiscordFunctions.Disconnect();
            };
        }
        internal class Setup : IDisposable
        {
            private readonly HttpClient client = new HttpClient();
            const string ARK_WORKSHOP_DIR = "./steamcmd/steamapps/workshop/content/346110";
            public void Initialize(Process p, ProcessStartInfo psi) 
            {
                if (!Config.bot.useSSH) 
                {
                    get_steamcmd();
                    steamworkshop.Download(p, psi);
                }
                else 
                {
                    Server.Initialize();
                    Server.SendCommand("wget http://media.steampowered.com/installer/steamcmd_linux.tar.gz");
                }
            }
            public void CreateScript(int server) 
            {
                const string batch_noecho = @"@echo off";
                const string setvarbatch = "SETLOCAL ";
                string map = Config.bot.cluster.servers[server].map;
                const string end_flags = "-nosteamclient -game -server -log";
                switch (string.IsNullOrWhiteSpace(Config.bot.game))
                {
                    case false:
                                if (!File.Exists(Properties.Resources.ResourcesDir + "/" + Config.bot.game + "/" + AppStringProducer.GetSystemCompatibleString("LaunchScript.bat",true,true))) 
                                {
                                    using (var stream = File.CreateText(Properties.Resources.ResourcesDir + "/" + Config.bot.game + "/" + AppStringProducer.GetSystemCompatibleString("LaunchScript.bat",true,true))) 
                                    {
                                        stream.Write(Game_Profile._profile.steam_game_args_script_data);
                                    }
                                }
                        break;
                    default:
                        break;
                }
            }
            //Gets steamcmd if steamcmd directory is not specified
            public void get_steamcmd() 
            {
                const string steamcmd_windows = "https://steamcdn-a.akamaihd.net/client/installer/steamcmd.zip";
                const string steamcmd_linux = "http://media.steampowered.com/installer/steamcmd_linux.tar.gz";
                if (string.IsNullOrEmpty(Config.bot.steamcmddir))
                {
                    switch (Directory.Exists("./steamcmd"))
                    {
                        case true:
                            break;
                        default:
                            Directory.CreateDirectory("./steamcmd");
                            break;
                    }
                    if (OSInfo.GetOSPlatform() == System.Runtime.InteropServices.OSPlatform.Windows)
                    {
                        download(steamcmd_windows, Directory.GetCurrentDirectory()+"/steamcmd/steamcmd.zip");
                        ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "/steamcmd/steamcmd.zip", Directory.GetCurrentDirectory() + "/steamcmd", System.Text.Encoding.UTF8,true);
                    }
                    else 
                    {
                        download(steamcmd_linux, "./steamcmd/steamcmd_linux.tar.gz");
                        Tar.ExtractTarGz("./steamcmd/steamcmd_linux.tar.gz","./steamcmd");
                    }
                }
            }
            public void download(string url, string file)
            {
                if (!File.Exists(file))
                {
                    client.Timeout = TimeSpan.FromHours(1);
                    var url_0 = new Uri(url);
                    var response = client.GetAsync(url_0).ConfigureAwait(false).GetAwaiter().GetResult();
                    var status = response.StatusCode;
                    if (status.ToString().ToLower(CultureInfo.CurrentCulture) == "ok")
                    {
                        var content = response.Content;
                        Console.Out.WriteLineAsync("Creating file: " + file).ConfigureAwait(false).GetAwaiter().GetResult();
                        FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
                        content.CopyToAsync(fs).ConfigureAwait(false).GetAwaiter().GetResult();
                        fs.Flush();
                        fs.Dispose();
                    }
                    else
                    {
                        var content = response.Content;
                        Console.Out.WriteLineAsync("Creating file(may require manual downloading): " + file).ConfigureAwait(false).GetAwaiter().GetResult();
                        FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
                        content.CopyToAsync(fs).ConfigureAwait(false).GetAwaiter().GetResult();
                        fs.Flush();
                        fs.Dispose();
                        File.AppendAllText("./Checklist.txt", "Potential Error: " + file + Environment.NewLine);
                    }
                }
            }
            public void download(Uri uri, string file) 
            {
                if (!File.Exists(file))
                {
                    client.Timeout = TimeSpan.FromHours(1);
                    var response = client.GetAsync(uri).ConfigureAwait(false).GetAwaiter().GetResult();
                    var status = response.StatusCode;
                    if (status.ToString().ToLower(CultureInfo.CurrentCulture) == "ok")
                    {
                        var content = response.Content;
                        Console.Out.WriteLineAsync("Creating file: " + file).ConfigureAwait(false).GetAwaiter().GetResult();
                        FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
                        content.CopyToAsync(fs).ConfigureAwait(false).GetAwaiter().GetResult();
                        fs.Flush();
                        fs.Dispose();
                    }
                    else
                    {
                        var content = response.Content;
                        Console.Out.WriteLineAsync("Creating file(may require manual downloading and uploading to server): " + file).ConfigureAwait(false).GetAwaiter().GetResult();
                        FileStream fs = new FileStream(file, FileMode.CreateNew, FileAccess.Write);
                        content.CopyToAsync(fs).ConfigureAwait(false).GetAwaiter().GetResult();
                        fs.Flush();
                        fs.Dispose();
                        File.AppendAllText("./Checklist.txt", "Potential Error: " + file + Environment.NewLine);
                    }
                }
            }
            bool disposed;

            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        client.Dispose();
                    }
                }
                //dispose unmanaged resources
                disposed = true;
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }  
    }
}