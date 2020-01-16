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
namespace DiscordGameServerManager_Windows
{
    class Program
    {
        
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
        public static Thread RconThread;
        static DiscordClient discord = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.bot.token,
            TokenType = TokenType.Bot
        });
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
                RconThread = new Thread(() =>
                {
                    Rcon("127.0.0.1", Config.bot.rcon_port, Config.bot.rcon_pass, "enablecheats " + Config.bot.admin_pass);
                    Rcon("127.0.0.1", Config.bot.rcon_port, Config.bot.rcon_pass, "cheats saveworld");
                    Rcon("127.0.0.1", Config.bot.rcon_port, Config.bot.rcon_pass, "cheats quit");
                });
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
            //Discord channel id gets fetched from the id specified in config.json
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
                            DiscordTrustManager.updateChannel(e.Author.Username+e.Author.Discriminator, e.Author.Id, e.Channel);
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
                                DiscordTrustManager.addChannel(e.Author.Username + e.Author.Discriminator,e.Author.Id,discordDm);
                            }
                            string user = e.Author.Username + e.Author.Discriminator;
                            await message_send("Direct Messaging features are a WIP, some planned features are remote connection management directly to the game from here" + Heuristics.newline + "Current features are " + prefix + "_register and for others type "+prefix+"_help", e.Channel).ConfigureAwait(false);
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
            Respond_Message(e.Author.Username+e.Author.Discriminator, e.Message.Content, e.Channel, perms, e);
        }
        public static async void Respond_Message(string Author, string message,DiscordChannel discordChannel, bool[] perm_value, DSharpPlus.EventArgs.MessageCreateEventArgs e) 
        {
            if (perm_value[4]) 
            {
                if (message.ToLower().Equals(prefix + "_help"))
                {
                    await message_send("What would you like help with?" + Environment.NewLine + prefix + "_start: starts the server" + Environment.NewLine + prefix + "_stop: stops the server" + Environment.NewLine + prefix + "_restart: restarts the server" + Environment.NewLine + prefix + "_backup: initiates server backup, this will take the server offline", discordChannel);
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
            psi.WorkingDirectory = Config.bot.steamcmd_dir;
            psi.UseShellExecute = true;
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            psi.CreateNoWindow = true;
            psi.FileName = AppStringProducer.GetSystemCompatibleString("steamcmd.exe");
            psi.Arguments = " +login anonymous +force_install_dir " + Config.bot.game_dir + " +app_update 376030 +app_run 376030 " + Config.bot.game_launch_args;
            thread = new Thread(() =>
            {
                process.StartInfo = psi;
                process.Start();
            });
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
                        switch (option)
                        {
                            case 0:
                                psi.Arguments = Game_Profile._profile.steam_game_args;
                                thread.Start();
                                break;
                            case 1:
                                thread.Abort();
                                break;
                            case 2:
                                thread.Abort();
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
                                thread.Abort();
                                break;
                            case 2:
                                thread.Abort();
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
                    break;
                case 1:
                    try
                    {
                        RconThread.Start();
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
                            RconThread.Start();
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
        static void force_shutdown_ARK(Thread thread)
        {
            if (thread.IsAlive)
            {
                try
                {
                    thread.Abort();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + Environment.NewLine + "Closing Ark server using other methods");
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
        }
        static async Task message_send(string str, DiscordChannel discordChannel)
        {
            DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, str, false, null);
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
    }
}
