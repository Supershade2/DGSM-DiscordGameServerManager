using System;
using System.Collections.Generic;
using System.Diagnostics;
using DSharpPlus.Entities;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Runtime.InteropServices;
using System.Globalization;
namespace DiscordGameServerManager
{
    class GameManager
    {
        static LinkedList<int> process_ids = new LinkedList<int>();
        public static Thread[] RconThread;
        public static bool server_startup = false;
        public static void CreateRcon(bool MainConfig)
        {
            switch (MainConfig)
            {
                case true:
                    RconThread = new Thread[Config.bot.cluster.servers.Length];
                    for (int i = 0; i < Config.bot.cluster.servers.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Config.bot.cluster.servers[i].RCONPass))
                        {
                            if (!string.IsNullOrEmpty(Config.bot.cluster.servers[i].address))
                            {
                                RconThread[i] = new Thread(() =>
                                {
                                    Rcon(Config.bot.cluster.servers[i].address, Config.bot.cluster.servers[i].RCONPort, Config.bot.cluster.servers[i].RCONPass, "cheats saveworld");
                                    Rcon(Config.bot.cluster.servers[i].address, Config.bot.cluster.servers[i].RCONPort, Config.bot.cluster.servers[i].RCONPass, "cheats quit");
                                });
                            }
                            else
                            {
                                RconThread[i] = new Thread(() =>
                                {
                                    Rcon(Config.bot.cluster.servers[0].address, Config.bot.cluster.servers[0].RCONPort, Config.bot.cluster.servers[0].RCONPass, "cheats saveworld");
                                    Rcon(Config.bot.cluster.servers[0].address, Config.bot.cluster.servers[0].RCONPort, Config.bot.cluster.servers[0].RCONPass, "cheats quit");
                                });
                            }
                        }
                    }
                    break;
                default:
                    if (!string.IsNullOrEmpty(Game_Profile._profile.rcon_address))
                    {
                        RconThread[0] = new Thread(() =>
                        {
                            string[] args = Game_Profile._profile.rcon_commands;
                            for (int i = 0; i < args.Length; i++)
                            {
                                Rcon(Game_Profile._profile.rcon_address, Game_Profile._profile.RCONPort, Game_Profile._profile.RCONPass, args[i]);
                            }
                        });
                    }
                    break;
            }
        }
        public static void force_shutdown_ARK(Thread thread)
        {
            if (thread.IsAlive)
            {
                try
                {
                    Process[] proc = Process.GetProcessesByName("ShooterGameServer");
                    proc[0].Kill();
                }
                catch (Exception ex_1)
                {
                    Console.WriteLine(ex_1.Message);
                }
                try
                {
                    Process[] proc = Process.GetProcessesByName("steamcmd");
                    proc[0].Kill();
                }
                catch (Exception ex_2)
                {
                    Console.WriteLine(ex_2.Message);
                }
            }
        }
        public static void Manage_server(int option, DiscordChannel discordChannel)
        {
            if (!Config.bot.useSSH) 
            {
                string output;
                Process process = new Process();
                ProcessStartInfo psi = new ProcessStartInfo();
                Thread thread;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardInput = true;
                psi.RedirectStandardError = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = string.IsNullOrEmpty(Config.bot.steamcmddir) == false ? Config.bot.steamcmddir : Directory.GetCurrentDirectory() + "/steamcmd";
                psi.FileName = AppStringProducer.GetSystemCompatibleString("steamcmd.exe");
                if (Details.d.first_run)
                {
                    psi.Arguments = " +login anonymous +force_install_dir " + Config.bot.gamedir + " +app_update 376030 validate +quit";
                }
                else
                {
                    psi.Arguments = "+login anonymous +run_script " + Config.bot.GamelaunchARGSscript + " +quit";
                }
                process_ids.AddLast(process.Id);
                thread = new Thread(() =>
                {
                    Process p = process;
                    p.StartInfo = psi;
                    p.Start();
                    Thread sub = new Thread(() =>
                    {
                        string line = "";
                        do
                        {
                            line += p.StandardOutput.ReadLine();
                            line = line.ToLower(CultureInfo.CurrentCulture);
                        } while (!line.Contains("two-factor code:", StringComparison.CurrentCulture));
                        Console.Out.WriteAsync(line).ConfigureAwait(true).GetAwaiter().GetResult();
                        DiscordFunctions.Requeststeamcode();
                        p.StandardInput.WriteLine(File.ReadAllText("steamcode.txt", Encoding.UTF8));
                        if (OSInfo.GetOSPlatform() == OSPlatform.Windows)
                        {
                            p.StandardInput.Write((char)13);
                        }
                        else
                        {
                            p.StandardInput.Write((char)10);
                        }
                        p.StandardInput.Flush();
                    });
                    p.WaitForExit();
                    p.Close();
                })
                {
                    IsBackground = true
                };
                Console.WriteLine(psi.FileName);
                switch (Config.bot.game)
                {
                    case null:
                        break;
                    default:
                        Console.WriteLine(Properties.Resources.CreatingGameProfile);
                        var temp = Game_Profile._profile.file_location;
                        temp = null;
                        if (Game_Profile._profile.Is_Steam == true)
                        {
                            CreateRcon(false);
                            var user = Game_Profile._profile.user_and_pass.Keys;
                            var pass = Game_Profile._profile.user_and_pass.Values;
                            switch (option)
                            {
                                case 0:
                                    psi.Arguments = "+login " + Game_Profile._profile.steam_game_args_script_data;
                                    thread.Start();
                                    break;
                                case 1:
                                    break;
                                case 2:
                                    thread.Start();
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            switch (option)
                            {
                                case 0:
                                    psi.Arguments = Game_Profile._profile.start_command;
                                    thread.Start();
                                    break;
                                case 1:
                                    foreach (int id in process_ids)
                                    {
                                        Process.GetProcessById(id).Kill();
                                    }
                                    break;
                                case 2:
                                    foreach (int id in process_ids)
                                    {
                                        Process.GetProcessById(id).Kill();
                                    }
                                    thread.Start();
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                }
                switch (option)
                {
                    case 0:
                        if (!thread.IsAlive)
                            try
                            {
                                thread.Start();
                            }
                            catch (ThreadStartException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        Console.WriteLine(process.StartInfo.WorkingDirectory);
                        Console.WriteLine(process.StartInfo.Arguments);
                        var input = process.StandardInput;
                        break;
                    case 1:
                        try
                        {
                            for (int i = 0; i < RconThread.Length; i++)
                            {
                                RconThread[i].Start();
                            }
                        }
                        catch (ThreadStartException ex)
                        {
                            Console.WriteLine(Properties.Resources.RCONStartFailure);
                            Console.WriteLine(ex.Message);
                            Console.WriteLine("Exception source:" + ex.InnerException.Source);
                        }
                        break;
                    case 2:
                        if (thread.IsAlive)
                        {
                            try
                            {
                                for (int i = 0; i < RconThread.Length; i++)
                                {
                                    RconThread[i].Start();
                                }
                            }
                            catch (ThreadStartException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }
                        force_shutdown_ARK(thread);
                        thread.Start();
                        Console.WriteLine(process.StartInfo.WorkingDirectory);
                        Console.WriteLine(process.StartInfo.Arguments);
                        process.Start();
                        output = process.StandardOutput.ReadToEnd();
                        try
                        {
                            DiscordFunctions.messageSend(output, discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                        }
                        catch (AggregateException ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                        break;
                    case 3:
                        Manage_server(1, discordChannel);
                        ZipFile.CreateFromDirectory(Config.bot.gamedir, "backup_" + DateTime.UtcNow.Month + "_" + DateTime.UtcNow.Day + "_" + DateTime.UtcNow.Year + ".zip", CompressionLevel.Optimal, true);

                        if (server_startup == true) { Manage_server(0, discordChannel); }
                        DiscordFunctions.messageSend("Backup complete", discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                        break;
                }
            }
            else 
            { 
                
            }
        }
        public static void Rcon(string ipaddress, int port, string password, string command)
        {

            SourceRcon Sr = new SourceRcon();
            Sr.Errors += new StringOutput(ErrorOutput);
            Sr.ServerOutput += new StringOutput(ConsoleOutput);

            if (Sr.Connect(new IPEndPoint(IPAddress.Parse(ipaddress), port), password))
            {
                while (!Sr.Connected)
                {
                    Thread.Sleep(10);
                }
                Thread.Sleep(1000);
                Sr.ServerCommand(command);
            }
            else
            {
                Console.WriteLine("No connection!");
                Thread.Sleep(1000);
            }
        }
        static void ErrorOutput(string input)
        {
            Console.WriteLine("Error: {0}", input);
        }

        static void ConsoleOutput(string input)
        {
            Console.WriteLine("Console: {0}", input);
        }
    }
}
