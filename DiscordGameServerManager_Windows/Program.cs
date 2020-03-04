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
        public static Thread TimerThread = new Thread(async () =>
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            TimeSpan ts = timer.Elapsed;
            Messages.message[] thread_messages = Config.bot._messages;
            while (true)
            {
                if (ts.Hours >= 12)
                {
                    foreach (var m in thread_messages)
                    {
                        await Messages.message_send(m, message_channel, discord);
                        timer.Restart();
                    }
                }
            }
        });
        public static Thread[] RconThread;
        static DiscordClient discord = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.bot.token,
            TokenType = TokenType.Bot
        });
        static LinkedList<int> process_ids = new LinkedList<int>(); 
        static DiscordChannel discordChannel = discord.GetChannelAsync(Config.bot.discord_channel).Result;
        static DiscordChannel message_channel = discord.GetChannelAsync(Config.bot.message_channel).Result;
        static DiscordGuild Guild;
        static string prefix = Config.bot.prefix.ToLower();
        static bool backup_requested = false;
        static bool server_startup = false;
        static string user = "";
        public static Dictionary<string, string> prompts = new Dictionary<string, string>();
        static bool IsValid = false;
        static ulong botID = Config.bot.ID;
        static bool dmchannel = false;
        public static DiscordDmChannel discordDm = null;
        static void Main(string[] args)
        {
            //Creates the permissions.json file then prints whether it succeeds or fails to the console
            Console.WriteLine("Creating permissions.json note that this will count as succeeding if file already exists:");
            bool success = DiscordTrustManager.createJSON();
            Console.WriteLine("Result:" + success);
            Console.WriteLine(Config.bot.token);
            OS_Info.GetOSPlatform();
            try
            {
                CreateRcon(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create RCON thread");
                Console.WriteLine(ex.Message);
            }
            try
            {
                Messages.load();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to load DMs.json");
                Console.WriteLine(ex.Message);
            }
            //Starts the bot through a try catch statement, if it fails it will print to console the error message
            try
            {
                discord.InitializeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                discord.ConnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (!TimerThread.IsAlive)
                {
                    TimerThread.Start();
                }
                while (true)
                {
                    MainDiscord(args);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Things may not work properly");
                Console.WriteLine(ex.Message);
            }
        }
        static void MainDiscord(string[] args)
        {
            //Discord guild gets fetched from the id specified in config.json
            if (DiscordTrustManager.users.Count == 0)
            {
                if (discordChannel.Guild == null)
                {
                    Guild = discord.GetGuildAsync(Config.bot.server_guild_id).ConfigureAwait(false).GetAwaiter().GetResult();
                    DiscordTrustManager.setTotalUsers(Guild.MemberCount);
                }
                else
                {
                    DiscordTrustManager.setTotalUsers(discordChannel.Guild.MemberCount);
                }
            }
            //This will execute if a user dm's the bot, the bot then will add the dm as a list of logged dm's
            discord.DmChannelCreated += async e =>
            {
                dmchannel = true;
                discordDm = e.Channel;
                    //Logs Direct messages in memory
                    await LogDMs();
            };
            discord.MessageCreated += async e =>
            {
                await ProcessMessage(e);
            };
        }
        public static async Task ProcessMessage(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            DiscordDmChannel discordDm = null;
            IsValid = false;
            bool isBot = e.Author.Id == botID;
            switch (isBot)
            {
                case false:
                    if (DiscordTrustManager.channel_dictionary.channels.Count == 0)
                    {
                        DiscordTrustManager.addChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                    }
                    if (!DiscordTrustManager.channel_dictionary.channels.ContainsKey(e.Channel.Id))
                    {
                        DiscordTrustManager.addChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                    }
                    foreach (var chnl in DiscordTrustManager.channel_dictionary.channels)
                    {
                        if (chnl.Key != e.Channel.Id)
                        {
                        }
                        else
                        {
                            DiscordTrustManager.updateChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                        }
                    }
                    if (e.Channel == discordChannel)
                    {
                        DoCheck(e);
                        if (e.Message.Author.IsCurrent == false && backup_requested == false)
                        {
                            if (e.Message.Content.ToLower() == prefix + "_help") { IsValid = true; }
                            if (e.Message.Content.ToLower() == prefix + "_start") { IsValid = true; }
                            if (e.Message.Content.ToLower() == prefix + "_stop") { IsValid = true; }
                            if (e.Message.Content.ToLower() == prefix + "_restart") { IsValid = true; }
                            if (e.Message.Content.ToLower() == prefix + "_backup") { IsValid = true; }
                            else if (IsValid == false) { await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help"); }
                        }
                        else
                        {
                            if (e.Message.Author.Username != user && user != "")
                            {
                                if (e.Message.Author.Id != botID)
                                {
                                    await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help" + Environment.NewLine + "awaiting response from user: " + user + " for backup prompt.");
                                }
                            }
                        }
                    }
                    else if (dmchannel != true)
                    {

                        foreach (var dm in Messages.userDM.Values)
                        {
                            if (dm.Id == e.Channel.Id)
                            {
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).Contains(prefix.ToLower(CultureInfo.CurrentCulture) + "_register"))
                                {
                                    string[] strarray = e.Message.Content.Split(' ');
                                    foreach (var s in strarray)
                                    {
                                        switch (s == Config.bot.registration_key)
                                        {
                                            case true:
                                                await message_send(Config.bot.invite, e.Channel);
                                                //IReadOnlyList<DiscordMember> discordMembers = discordChannel.Guild.Members;
                                                //var Denumerator = discordMembers.GetEnumerator();
                                                //string user = Denumerator.Current.Presence.User.Username + Denumerator.Current.Presence.User.Discriminator;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    DoCheck(e);
                                }
                            }
                        }
                    }
                    if (dmchannel == true)
                    {
                        if (e.Channel == discordDm)
                        {
                            if (!Messages.userDM.Keys.ToArray().Contains(e.Author.Username + e.Author.Discriminator))
                            {
                                Messages.userDM.Add(e.Author.Username + e.Author.Discriminator, discordDm);
                                DiscordTrustManager.addChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, discordDm);
                            }
                            string user = e.Author.Username + e.Author.Discriminator;
                            await message_send("Direct Messaging features are a WIP, some planned features are remote connection management directly to the game from here" + Heuristics.newline + "Current features are " + prefix + "_register and for others type " + prefix + "_help", e.Channel).ConfigureAwait(false);
                            dmchannel = false;
                            if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).Contains(prefix.ToLower(CultureInfo.CurrentCulture) + "_register"))
                            {
                                string[] strarray = e.Message.Content.Split(' ');
                                foreach (var s in strarray)
                                {
                                    switch (s == Config.bot.registration_key)
                                    {
                                        case true:
                                            await message_send(Config.bot.invite, e.Channel);
                                            //IReadOnlyList<DiscordMember> discordMembers = discordChannel.Guild.Members;
                                            //var Denumerator = discordMembers.GetEnumerator();
                                            //string user = Denumerator.Current.Presence.User.Username + Denumerator.Current.Presence.User.Discriminator;
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }
        public static void DoCheck(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            bool[] perms = DiscordTrustManager.checkPermission(e.Channel.Id, e.Author.Id);
            Respond_Message(e.Author.Username + e.Author.Discriminator, e.Message.Content, e.Channel, perms, e);
        }
        public static async void Respond_Message(string Author, string message, DiscordChannel discordChannel, bool[] perm_value, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (perm_value[2]) 
            {
                if (message.ToLower().Equals(prefix + "_help"))
                {
                    await message_send("What would you like help with?" + Environment.NewLine + prefix + "_start: starts the server" + Environment.NewLine + prefix + "_stop: stops the server" + Environment.NewLine + prefix + "_restart: restarts the server" + Environment.NewLine + prefix + "_backup: initiates server backup, this will take the server offline", discordChannel);
                }
            }
            if (perm_value[0])
            {
                if (message.ToLower().Equals(prefix + "_start"))
                {
                    await message_send("Server is starting..." + Environment.NewLine + "check server status at the link below", discordChannel);
                    if (Config.bot.gametracking_url != null)
                    {
                      await message_send(Config.bot.gametracking_url, discordChannel);
                    }
                    else
                    {
                      await message_send("gametracking url not defined", discordChannel);
                    }
                    Manage_server(0, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_stop"))
                {
                    await message_send("Server is stopping...", discordChannel);
                    Manage_server(1, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_restart"))
                {
                    await message_send("Server is restarting...", discordChannel);
                    Manage_server(2, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_backup"))
                {
                    user = Author;
                    await message_send("Server backup initiated would you like to start server when finished yes/no?", discordChannel);
                    backup_requested = true;
                }
                if (message.ToLower().Equals("yes") && backup_requested == true && Author == user)
                {
                    server_startup = true;
                    await message_send("Server will be started after backup", discordChannel);
                    Manage_server(3, discordChannel);
                    server_startup = false;
                    backup_requested = false;
                    user = "";
                }
                else if (backup_requested == true && Author == user && message.ToLower().Equals("no"))
                {
                    await message_send("Server will remain offline after backup", discordChannel);
                    Manage_server(3, discordChannel);
                    backup_requested = false;
                    user = "";
                }
            }
            else
            {
                try
                {
                    string output = process_string(Config.bot._messages[0].message_body, e, "user");
                    await e.Message.DeleteAsync(output);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error occured, is the bot a admin or granted the correct permissions?");
                    Console.WriteLine(ex.Message);
                }
            }


        }
        public static string process_string(string input, DSharpPlus.EventArgs.MessageCreateEventArgs e, string variable)
        {
            if (variable.ToLower() == "user")
            {
                bool IsParsing = false;
                char[] username = e.Author.Username.ToCharArray();
                string output = "";
                foreach (var item in input)
                {
                    if (item == '%')
                    {
                        IsParsing = IsParsing != true ? true : false;
                        if (IsParsing)
                        {
                            foreach (var i in username)
                            {
                                output += i;
                            }
                        }
                    }
                    if (!IsParsing)
                    {
                        output += item;
                    }
                }
                return output;
            }
            else
            {
                return input;
            }
        }
        public static void Manage_server(int option, DiscordChannel discordChannel)
        {
            string output;
            //string error;
            Process process = new Process();
            ProcessStartInfo psi = new ProcessStartInfo();
            Thread thread;
            psi.UseShellExecute = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.WorkingDirectory = string.IsNullOrEmpty(Config.bot.steamcmd_dir) == false ? Config.bot.steamcmd_dir:Directory.GetCurrentDirectory()+"/steamcmd";
            psi.FileName = AppStringProducer.GetSystemCompatibleString("steamcmd.exe");
            if (Details.d.first_run) 
            {
                psi.Arguments = " +login anonymous +force_install_dir " + Config.bot.game_dir + " +app_update 376030 validate +quit";
            }
            else 
            {
                psi.Arguments = "+login anonymous +run_script " + Config.bot.game_launch_args_script+" +quit";
            }
            process_ids.AddLast(process.Id);
            thread = new Thread(() =>
            {
                Process p = process;
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit();
                p.Close();
            });
            thread.IsBackground = true;
            Console.WriteLine(psi.FileName);
            switch (Config.bot.game)
            {
                case null:
                    break;
                default:
                    Console.WriteLine("Game specified creating gameprofile.json if it doesn't already exist");
                    var temp = Game_Profile._profile.file_location;
                    temp = null;
                    if (Game_Profile._profile.Is_Steam == true)
                    {
                        CreateRcon(false);
                        switch (option)
                        {
                            case 0:
                                psi.Arguments = "+login "+Game_Profile._profile.steam_game_args_script;
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
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    Console.WriteLine(process.StartInfo.WorkingDirectory);
                    Console.WriteLine(process.StartInfo.Arguments);
                    //output = process.StandardOutput.ReadToEnd();
                    //error = process.StandardError.ReadToEnd();
                    /*try
                    {
                        message_send(output, discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }*/
                    break;
                case 1:
                    try
                    {
                        for (int i = 0; i < RconThread.Length; i++)
                        {
                            RconThread[i].Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to start RCON thread");
                        Console.WriteLine(ex.Message);
                    }
                    /*psi.Arguments = " stop";
                    process.StartInfo = psi;
                    process.Start();*/
                    //output = process.StandardOutput.ReadToEnd();
                    //error = process.StandardError.ReadToEnd();
                    /*try
                    {
                        await message_send(output, discordChannel);
                        await message_send(error, discordChannel);
                    }
                    catch (Exception)
                    {
                        throw;
                    }*/
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
                        catch (Exception ex)
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
                    //error = process.StandardError.ReadToEnd();
                    try
                    {
                        message_send(output, discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    /*psi.Arguments = " restart";
                    process.StartInfo = psi;
                    process.Start();*/
                    //output = process.StandardOutput.ReadToEnd();
                    //error = process.StandardError.ReadToEnd();
                    /*try
                    {
                        await message_send(output, discordChannel);
                        await message_send(error, discordChannel);
                    }
                    catch (Exception)
                    {
                        throw;
                    }*/
                    break;
                case 3:
                    Manage_server(1, discordChannel);
                    ZipFile.CreateFromDirectory(Config.bot.game_dir, "backup_" + DateTime.UtcNow.Month + "_" + DateTime.UtcNow.Day + "_" + DateTime.UtcNow.Year + ".zip", CompressionLevel.Optimal, true);
                    /*ShellHelper.Bash("exec /home/shade/GSM/LinuxGSM/arkserver stop");
                    ShellHelper.Bash("exec /home/shade/GSM/LinuxGSM/arkserver backup");
                    bool printed = false;
                    while (System.IO.File.Exists("/home/shade/GSM/LinuxGSM/lgsm/tmp/.backup.lock"))
                    {
                        switch (printed)
                        {
                            case false:
                                Console.WriteLine("Backup in progress");
                                message_send("Backup in progress", discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                                printed = true;
                                break;
                        }
                    }
                    Console.WriteLine("Backup finished");
                    message_send("Backup finished", discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();*/
                    if (server_startup == true) { Manage_server(0, discordChannel); }
                    message_send("Backup complete", discordChannel).ConfigureAwait(false).GetAwaiter().GetResult();
                    break;
            }
            //output = "";
            //error = "";
        }
        //Iinitializes RCON thread with either main config arguments or Game_Profile Arguments
        static void CreateRcon(bool MainConfig)
        {
            switch (MainConfig)
            {
                case true:
                    RconThread = new Thread[Config.bot.cluster.servers.Length];
                    for (int i = 0; i < Config.bot.cluster.servers.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Config.bot.cluster.servers[i].RCON_pass))
                        {
                            if (!string.IsNullOrEmpty(Config.bot.cluster.servers[i].address))
                            {
                                RconThread[i] = new Thread(() =>
                                {
                                    //Rcon(Config.bot.rcon_address, Config.bot.rcon_port, Config.bot.rcon_pass, "enablecheats " + Config.bot.admin_pass);
                                    Rcon(Config.bot.cluster.servers[i].address, Config.bot.cluster.servers[i].RCON_port, Config.bot.cluster.servers[i].RCON_pass, "cheats saveworld");
                                    Rcon(Config.bot.cluster.servers[i].address, Config.bot.cluster.servers[i].RCON_port, Config.bot.cluster.servers[i].RCON_pass, "cheats quit");
                                });
                            }
                            else
                            {
                                RconThread[i] = new Thread(() =>
                                {
                                    //Rcon(Config.bot.rcon_address, Config.bot.rcon_port, Config.bot.rcon_pass, "enablecheats " + Config.bot.admin_pass);
                                    Rcon(Config.bot.cluster.servers[0].address, Config.bot.cluster.servers[i].RCON_port, Config.bot.cluster.servers[i].RCON_pass, "cheats saveworld");
                                    Rcon(Config.bot.cluster.servers[0].address, Config.bot.cluster.servers[i].RCON_port, Config.bot.cluster.servers[i].RCON_pass, "cheats quit");
                                });
                            }
                        }
                    }
                    break;
                default:
                    if (Game_Profile._profile.rcon_address != null || Game_Profile._profile.rcon_address != "")
                    {
                        RconThread[0] = new Thread(() =>
                        {
                            //Rcon(Config.bot.rcon_address, Config.bot.rcon_port, Config.bot.rcon_pass, "enablecheats " + Config.bot.admin_pass);
                            string[] args = Game_Profile._profile.rcon_commands;
                            for (int i = 0; i < args.Length; i++)
                            {
                                Rcon(Game_Profile._profile.rcon_address, Game_Profile._profile.rcon_port, Game_Profile._profile.rcon_pass, args[i]);
                            }
                        });
                    }
                    break;
            }
        }
        //Forces Ark server shutdown by Terminating the process
        static void force_shutdown_ARK(Thread thread)
        {
            if (thread.IsAlive)
            {
                try
                {
                    Process[] proc = Process.GetProcessesByName("ShooterGameServer");
                    /*for (int i = 0; i < proc.Length; i++) 
                    {
                        proc[i].Kill();
                    }*/
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
        static async Task message_send(string str, DiscordChannel discordChannel)
        {
            DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, str, false, null);
            await discordMessage.RespondAsync();
        }
        static async Task LogDMs()
        {
            await Console.Out.WriteLineAsync("Total DM Channels is " + Messages.userDM.Values.Count);
            //Sets the current count of user DM's to the config object
            Details.d.user_count = Messages.userDM.Count;
            //updates the config.json with the new value
            Config.write();
            Messages.write();
        }

        private static void Rcon(string ipaddress, int port, string password, string command)
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
        internal class Setup
        {
            private static readonly HttpClient client = new HttpClient();
            string ARK_WORKSHOP_DIR = "./steamcmd/steamapps/workshop/content/346110";
            public static void Initialize(Process p, ProcessStartInfo psi) 
            {
                steamworkshop.download(p, psi);
            }
            public static void CreateScript(int server) 
            {
                const string batch_noecho = @"@echo off";
                const string setvarbatch = "SETLOCAL ";
                string map = Config.bot.cluster.servers[server].map;
                const string end_flags = "-nosteamclient -game -server -log";
                switch (string.IsNullOrEmpty(Config.bot.game))
                {
                    case false:

                        break;
                    default:
                        break;
                }
            }
            //Gets steamcmd if steamcmd directory is not specified
            public static void get_steamcmd() 
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
                        download(steamcmd_windows, "./steamcmd/steamcmd.zip");
                        ZipFile.ExtractToDirectory("./steamcmd/steamcmd.zip", "./steamcmd", System.Text.Encoding.ASCII,true);
                    }
                    else 
                    {
                        download(steamcmd_linux, "./steamcmd/steamcmd_linux.tar.gz");
                        Tar.ExtractTarGz("./steamcmd/steamcmd_linux.tar.gz","./steamcmd");
                    }
                }
            }
            public static void download(string url, string file)
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
