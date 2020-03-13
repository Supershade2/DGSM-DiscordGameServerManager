using System;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;
using System.Linq;

namespace DiscordGameServerManager_Windows
{
    public class DiscordFunctions
    {
        static bool backup_requested = false;
        static string prefix = Config.bot.prefix.ToLower();
        static string user = "";
        static DiscordGuild Guild;
        static bool dmchannel = false;
        public static DiscordDmChannel discordDm = null;
        static bool IsValid = false;
        static ulong botID = Config.bot.ID;
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
        public static void DoCheck(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            bool[] perms = DiscordTrustManager.checkPermission(e.Channel.Id, e.Author.Id);
            Respond_Message(e.Author.Username + e.Author.Discriminator, e.Message.Content, e.Channel, perms, e);
        }
        public static void MainDiscord()
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
        static readonly DiscordClient discord = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.bot.token,
            TokenType = TokenType.Bot
        });
        static DiscordChannel discordChannel = discord.GetChannelAsync(Config.bot.discord_channel).Result;
        static DiscordChannel message_channel = discord.GetChannelAsync(Config.bot.message_channel).Result;
        public DiscordFunctions() 
        {
            {
                //Creates the permissions.json file then prints whether it succeeds or fails to the console
                Console.WriteLine("Creating permissions.json note that this will count as succeeding if file already exists:");
                bool success = DiscordTrustManager.createJSON();
                Console.WriteLine("Result:" + success);
                Console.WriteLine(Config.bot.token);
                OS_Info.GetOSPlatform();
                try
                {
                    GameManager.CreateRcon(true);
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
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Things may not work properly");
                    Console.WriteLine(ex.Message);
                }
            }
        }
        public void startDiscord() 
        {
            while (true)
            {
                MainDiscord();
            }
        }
        public static async Task LogDMs()
        {
            await Console.Out.WriteLineAsync("Total DM Channels is " + Messages.userDM.Values.Count);
            //Sets the current count of user DM's to the config object
            Details.d.user_count = Messages.userDM.Count;
            //updates the config.json with the new value
            Config.write();
            Messages.write();
        }
        public static async Task message_send(string str, DiscordChannel discordChannel)
        {
            DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, str, false, null);
            await discordMessage.RespondAsync();
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
                                                ulong author_id = e.Author.Id;
                                                await message_send(Config.bot.invite, e.Channel);
                                                DiscordTrustManager.register(author_id,dm);
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
                                            ulong author_id = e.Author.Id;
                                            await message_send(Config.bot.invite, e.Channel);
                                            DiscordTrustManager.register(author_id, discordDm);
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
        public static string process_string(string input, DSharpPlus.EventArgs.MessageCreateEventArgs e, string variable)
        {
            if (variable.ToLower() == "user")
            {
                bool IsParsing = false;
                string user = e.Author.Username + "#" + e.Author.Discriminator;
                char[] username = user.ToCharArray();
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
        public static async void Respond_Message(string Author, string message, DiscordChannel discordChannel, bool[] perm_value, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (perm_value[0])
            {
                if (message.ToLower().Equals(prefix + "_help"))
                {
                    await message_send("What would you like help with?" + Environment.NewLine + prefix + "_start: starts the server" + Environment.NewLine + prefix + "_stop: stops the server" + Environment.NewLine + prefix + "_restart: restarts the server" + Environment.NewLine + prefix + "_backup: initiates server backup, this will take the server offline", discordChannel);
                }
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
                    GameManager.Manage_server(0, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_stop"))
                {
                    await message_send("Server is stopping...", discordChannel);
                    GameManager.Manage_server(1, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_restart"))
                {
                    await message_send("Server is restarting...", discordChannel);
                    GameManager.Manage_server(2, discordChannel);
                }
                if (message.ToLower().Equals(prefix + "_backup"))
                {
                    user = Author;
                    await message_send("Server backup initiated would you like to start server when finished yes/no?", discordChannel);
                    backup_requested = true;
                }
                if (message.ToLower().Equals("yes") && backup_requested == true && Author == user)
                {
                    GameManager.server_startup = true;
                    await message_send("Server will be started after backup", discordChannel);
                    GameManager.Manage_server(3, discordChannel);
                    GameManager.server_startup = false;
                    backup_requested = false;
                    user = "";
                }
                else if (backup_requested == true && Author == user && message.ToLower().Equals("no"))
                {
                    await message_send("Server will remain offline after backup", discordChannel);
                    GameManager.Manage_server(3, discordChannel);
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
    }
}
