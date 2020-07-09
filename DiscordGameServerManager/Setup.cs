using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;

namespace DiscordGameServerManager
{
    public class Setup : IDisposable
    {
        private readonly HttpClient client = new HttpClient();
        const string ARKWORKSHOPDIR = "/steamcmd/steamapps/workshop/content/346110";
        public void Initialize(Process p, ProcessStartInfo psi,Globalvars g)
        {
            if (!Config.bot.useSSH)
            {
                get_steamcmd();
                steamworkshop.Download(p, psi,g);
            }
            else
            {
                Server.Initialize();
                Server.SendCommand("uname -a");
                Server.SendCommand("wget http://media.steampowered.com/installer/steamcmd_linux.tar.gz");
            }
        }
        public void CreateScript(int server, profile pfile,Globalvars globalvars)
        {
            string[] steamcmdscriptdata = { "login","app_update",pfile.steam_app_id.ToString(CultureInfo.CurrentCulture),"quit" };
            const string batchnoecho = @"@echo off";
            const string batchstart = "start";
            const string batchwait = " /w ";
            const string setvarbatch = "SETLOCAL ";
            string map = globalvars.cluster.servers[server].map;
            const string end_flags = "-nosteamclient -game -server -log";
            switch (string.IsNullOrWhiteSpace(globalvars.game))
            {
                case false:
                    if (!File.Exists(Properties.Resources.ResourcesDir + "/" + globalvars.game + "/" + AppStringProducer.GetSystemCompatibleString("LaunchScript.bat", true, true)))
                    {
                        using (var stream = File.CreateText(Properties.Resources.ResourcesDir + "/" + globalvars.game + "/" + AppStringProducer.GetSystemCompatibleString("LaunchScript.bat", true, true)))
                        {
                            stream.Write(Game_Profile._profile.steam_game_args_script_data);
                        }
                    }
                    break;
                default:

                    break;
            }
        }
        //Method will be used to create the script to handle and launch multiple independent server sessions not in a cluster or seperate clusters
        public void CreateScript(int server, profile pfile, Globalvars globalvars,int servers) { }
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
                    download(new Uri(steamcmd_windows), Directory.GetCurrentDirectory() + "/steamcmd/steamcmd.zip");
                    ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "/steamcmd/steamcmd.zip", Directory.GetCurrentDirectory() + "/steamcmd", System.Text.Encoding.UTF8, true);
                }
                else
                {
                    download(new Uri(steamcmd_linux), "./steamcmd/steamcmd_linux.tar.gz");
                    Tar.ExtractTarGz("./steamcmd/steamcmd_linux.tar.gz", "./steamcmd");
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
