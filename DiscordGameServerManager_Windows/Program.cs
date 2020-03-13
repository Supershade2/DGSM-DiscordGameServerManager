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
using System.Runtime.InteropServices;
using System.Net.Http;
using System.IO;

namespace DiscordGameServerManager_Windows
{
    class Program
    {
        public static Details _details = new Details();
        public static DiscordFunctions functions = new DiscordFunctions();
        static void Main(string[] args)
        {
            Setup setup = new Setup();
            setup.get_steamcmd();
            for (int i = 0; i < Config.bot.cluster.servers.Length; i++) 
            {
                setup.CreateScript(i);
            }
            functions.startDiscord();
        }
        internal class Setup
        {
            private readonly HttpClient client = new HttpClient();
            string ARK_WORKSHOP_DIR = "./steamcmd/steamapps/workshop/content/346110";
            public void Initialize(Process p, ProcessStartInfo psi) 
            {
                steamworkshop.download(p, psi);
            }
            public void CreateScript(int server) 
            {
                const string batch_noecho = @"@echo off";
                const string setvarbatch = "SETLOCAL ";
                string map = Config.bot.cluster.servers[server].map;
                const string end_flags = "-nosteamclient -game -server -log";
                switch (string.IsNullOrEmpty(Config.bot.game))
                {
                    case false:
                        switch (OS_Info.GetOSPlatform() == OSPlatform.Windows) 
                        {
                            case true:
                                break;
                            default:
                                break;
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
                if (string.IsNullOrEmpty(Config.bot.steamcmd_dir)) 
                {
                    switch (Directory.Exists("./steamcmd"))
                    {
                        case true:
                            break;
                        default:
                            Directory.CreateDirectory("./steamcmd");
                            break;
                    }
                    if (OS_Info.GetOSPlatform() == OSPlatform.Windows)
                    {
                        download(steamcmd_windows, Directory.GetCurrentDirectory()+"/steamcmd/steamcmd.zip");
                        ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "/steamcmd/steamcmd.zip", Directory.GetCurrentDirectory() + "/steamcmd", System.Text.Encoding.ASCII,true);
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
                    client.Timeout = TimeSpan.FromSeconds(45);
                    var response = client.GetAsync(url).ConfigureAwait(false).GetAwaiter().GetResult();
                    var status = response.StatusCode;
                    if (status.ToString().ToLower() == "ok")
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
        }  
    }
}