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
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace DiscordGameServerManager
{
    public static class DiscordFunctions
    {
        static bool steamcode_requested = false;
        static bool backup_requested = false;
        static bool guildidrequested = false;
        static string user = "";
        static readonly string prefix = Config.bot.prefix;
        static DiscordGuild Guild;
        //static bool dmchannel = false;
        private static ulong userID;
        private static ulong GID = 0;
        private static DiscordDmChannel discordDm = null;
        private static DiscordChannel discordChannel;
        static bool IsValid = false;
        static readonly ulong botID = Config.bot.ID;
        public static readonly Thread TimerThread = new Thread(async () =>
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            TimeSpan ts = timer.Elapsed;
            List<Messages.Message[]> threadbotmessages;
            if (Program.MemoryStorage) 
            {
                threadbotmessages = Messages.botmessages;
                while (true)
                {
                    if (ts.Hours >= 12)
                    {
                        DiscordChannel message_channel = discord.GetChannelAsync(GetMessageChannel()).Result;
                        foreach (var m in threadbotmessages)
                        {
                            for (int i = 0; i < m.Length; i++)
                            {
                                await Messages.MessageSend(m[i], message_channel, discord);
                                timer.Restart();
                            }
                        }
                    }
                }
            }
            else 
            {
                while (true) 
                {
                    if(ts.Hours >= 12) 
                    {
                        ulong guild = GetGuildID();
                        DiscordChannel message_channel = discord.GetChannelAsync(GetMessageChannel()).Result;
                        Messages.Message[] messagesthreadedvar = Messages.GetMessage(guild);
                        for(int i=0; i<messagesthreadedvar.Length; i++) 
                        {
                            await Messages.MessageSend(messagesthreadedvar[i], message_channel, discord);
                        }
                    }
                }
            }
        });
        public static ulong GetGuildID() 
        {
            return GID;
        }
        public static void DoCheck(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (!Program.DisableTrustManagement) 
            {
                Contract.Requires(e != null);
                bool[] perms = DiscordTrustManager.checkPermission(e.Channel.Id, e.Author.Id);
                RespondMessage(e.Author.Username + e.Author.Discriminator, e.Message.Content, e.Channel, perms, e, GlobalServerConfig.gvars);
            }
        }
        public static void Connect() 
        {
            try
            {
                discord.ConnectAsync().ConfigureAwait(true);
            }
            catch (ArgumentNullException ex)
            {
                Console.WriteLine("Error:"+ex.ParamName+"was null.");
                throw;
            }    
        }
        public static void Disconnect() 
        {
            discord.DisconnectAsync().ConfigureAwait(true);
        }
        public static void MainDiscord()
        {
            while (true) 
            {
                //Discord guild gets fetched from the id specified in config.json
                if (DiscordTrustManager.users.Count == 0)
                {
                    for(int i=0; i < discordChannels.Count; i++) 
                    {
                        if (discordChannels[i].Guild == null)
                        {
                            Guild = discord.GetGuildAsync(GetGuildID()).ConfigureAwait(false).GetAwaiter().GetResult();
                            DiscordTrustManager.setTotalUsers(GetGuildID(),Guild.MemberCount);
                        }
                        else
                        {
                            DiscordTrustManager.setTotalUsers(discordChannels[i].GuildId,discordChannels[i].Guild.MemberCount);
                        }
                    }
                }
                discord.GuildCreated += async e => 
                {
                    DiscordTrustManager.AddGuild(e.Guild.Id);
                    Setup setup = new Setup();
                    GlobalServerConfig.Initialize(e.Guild.Id);
                    if (GlobalServerConfig.gvars.settingup) 
                    {
                        //Make a call to the server to request info be supplied into the webui
                    }
                    else 
                    {
                        for (int i = 0; i < GlobalServerConfig.gvars.cluster.servers.Length; i++)
                        {
                            setup.CreateScript(i, Game_Profile._profile, GlobalServerConfig.gvars);
                        }
                        setup.Dispose();
                    }
                };
                discord.GuildAvailable += async e => 
                {
                    bool matchfound = false;
                    for (int i = 0; i < DiscordTrustManager.guildinfo.Count; i++) 
                    {
                        if (DiscordTrustManager.guildinfo[i].Uid == e.Guild.Id)
                        {
                            matchfound = true;
                        }
                    }
                    if (!matchfound) 
                    {
                        DiscordTrustManager.AddGuild(e.Guild.Id);
                    }
                    //Make a call to the server to request info be supplied into the webui
                };
                //This will execute if a user dm's the bot, the bot then will add the dm as a list of logged dm's
                discord.DmChannelCreated += async e =>
                {
                    if (!Program.DisableTrustManagement) 
                    {
                        //e.Client.CurrentUser.Presence.Guild
                        discordDm = e.Channel;
                        userID = e.Client.CurrentUser.Id;
                        await messageSend("" + Heuristics.newline + "Current Direct Message features are " + prefix + "_register and for others type " + prefix + "_help", e.Channel).ConfigureAwait(false);
                        await messageSend("Please Enter Guild ID: ", e.Channel).ConfigureAwait(false);
                        guildidrequested = true;
                        //Logs Direct messages in memory
                        await LogDMs().ConfigureAwait(false);
                    }
                    else 
                    {
                        await messageSend("Direct Messaging features are disabled for this hosted bot instance.",e.Channel).ConfigureAwait(false);
                    }
                };
                discord.GuildMemberAdded += async e =>
                {
                    if (!Program.DisableTrustManagement) 
                    {
                        DiscordTrustManager.setTotalUsers(e.Guild.Id, e.Guild.MemberCount);
                    }
                };
                discord.GuildMemberRemoved += async e =>
                {
                    if (!Program.DisableTrustManagement) 
                    {
                        await DiscordTrustManager.RemoveUser(e.Guild.Id, e.Member.Id).ConfigureAwait(true);
                    }
                };
                discord.MessageCreated += async e =>
                {
                    await ProcessMessage(e).ConfigureAwait(false);
                };
            }
        }
        public static void Requeststeamcode() 
        {
            steamcode_requested = true;
            messageSend("Please provide steamguard code below", discordChannels.First()).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        static readonly DiscordClient discord = new DiscordClient(new DiscordConfiguration
        {
            Token = Config.bot.token,
            TokenType = TokenType.Bot
        });
        public static readonly List<DiscordDmChannel> discordChannels = GetDiscordDmChannels(Messages.GetValues());
        public static List<DiscordDmChannel> GetDiscordDmChannels(Dictionary<ulong, DiscordDmChannel>.ValueCollection discordDms) 
        {
            List<DiscordDmChannel> dmChannels = new List<DiscordDmChannel>();
            dmChannels.AddRange(discordDms);
            return dmChannels;
        }
        public static ulong GetMessageChannel() 
        {
            GlobalServerConfig.load(GID);
            return GlobalServerConfig.gvars.MessageChannel;
        }
        public static async Task LogDMs()
        {
            await Console.Out.WriteLineAsync("Total DM Channels is " + Messages.GetUserDMCount()).ConfigureAwait(false);
            Messages.AddDM(userID,discordDm);
            //Sets the current count of user DM's to the config object
            ginfo g = Details.d.GuildInfo;
            g.usercount = Messages.GetUserDMCount();
            Details.d.GuildInfo = g;
            //updates the config.json with the new value
            Config.write();
            Messages.write();
        }
        public static async Task messageSend(string str, DiscordChannel discordChannel)
        {
            _ = await discord.SendMessageAsync(discordChannel, str, false, null).ConfigureAwait(false);
            //await discordMessage.RespondAsync();
        }
        public static async Task ProcessMessage(DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            Contract.Requires(e != null);
            GlobalServerConfig.load(e.Guild.Id);
            discordChannel = await discord.GetChannelAsync(GlobalServerConfig.gvars.DiscordChannel).ConfigureAwait(true);
            GID = e.Guild.Id;
            DiscordDmChannel discordDm = null;
            IsValid = false;
            bool isBot = e.Author.Id == botID;
            switch (isBot)
            {
                case false:
                    if (!Program.DisableTrustManagement) 
                    {
                        Guild guild = (from item in DiscordTrustManager.guildinfo where (item.Uid == e.Guild.Id) select item).Single();
                        if (guild.chinfo.channels.Count == 0)
                        {
                            DiscordTrustManager.SetGuildID(guild.Uid);
                            DiscordTrustManager.AddChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                            DiscordTrustManager.UpdateGuild(guild.Uid);
                        }
                        if (!guild.chinfo.channels.ContainsKey(e.Channel.Id))
                        {
                            DiscordTrustManager.SetGuildID(guild.Uid);
                            DiscordTrustManager.AddChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                            DiscordTrustManager.UpdateGuild(guild.Uid);
                        }
                        foreach (var chnl in guild.chinfo.channels)
                        {
                            if (chnl.Key != e.Channel.Id)
                            {
                            }
                            else
                            {
                                DiscordTrustManager.SetGuildID(guild.Uid);
                                DiscordTrustManager.updateChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, e.Channel);
                                DiscordTrustManager.UpdateGuild(guild.Uid);
                            }
                        }
                    }
                    if(e.Channel == discordChannel) 
                    {
                            DoCheck(e);
                            if (e.Message.Author.IsCurrent == false && backup_requested == false)
                            {
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_help") { IsValid = true; }
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_start") { IsValid = true; }
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_stop") { IsValid = true; }
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_restart") { IsValid = true; }
                                if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_backup") { IsValid = true; }
                                if (steamcode_requested) { System.IO.File.WriteAllText("steamcode.txt", e.Message.Content, Encoding.UTF8); steamcode_requested = false; }
                                else if (IsValid == false) { await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help").ConfigureAwait(false); }
                            }
                            else
                            {
                                if (e.Message.Author.Username != user && string.IsNullOrEmpty(user))
                                {
                                    if (e.Message.Author.Id != botID)
                                    {
                                        await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help" + Environment.NewLine + "awaiting response from user: " + user + " for backup prompt.").ConfigureAwait(false);
                                    }
                                }
                            }
                    }
                    else 
                    {
                        foreach (var ch in discordChannels)
                        {
                            if (e.Channel.Id == ch.Id)
                            {
                                if (!Messages.HasID(e.Author.Id))
                                {
                                    Messages.AddDM(e.Author.Id, discordDm);
                                    if (!Program.DisableTrustManagement) 
                                    {
                                        DiscordTrustManager.AddChannel(e.Author.Username + e.Author.Discriminator, e.Author.Id, discordDm);
                                    }
                                }
                                //string user = e.Author.Username + e.Author.Discriminator;
                                if (!Program.DisableTrustManagement) 
                                {
                                    if (guildidrequested) 
                                    { 
                                        
                                    }
                                    if (e.Message.Content.ToLower(CultureInfo.CurrentCulture).Contains(prefix.ToLower(CultureInfo.CurrentCulture) + "_register", StringComparison.CurrentCulture))
                                    {
                                        string[] strarray = e.Message.Content.Split(' ');
                                        foreach (var s in strarray)
                                        {
                                            switch (s == GlobalServerConfig.gvars.registrationkey)
                                            {
                                                case true:
                                                    ulong author_id = e.Author.Id;
                                                    await messageSend(GlobalServerConfig.gvars.invite, e.Channel).ConfigureAwait(false);
                                                    DiscordTrustManager.register(author_id, ch);
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                }
                                    DoCheck(e);
                                    if (e.Message.Author.IsCurrent == false && backup_requested == false)
                                    {
                                        if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_help") { IsValid = true; }
                                        if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_start") { IsValid = true; }
                                        if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_stop") { IsValid = true; }
                                        if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_restart") { IsValid = true; }
                                        if (e.Message.Content.ToLower(CultureInfo.CurrentCulture) == prefix + "_backup") { IsValid = true; }
                                        if (steamcode_requested) { System.IO.File.WriteAllText("steamcode.txt", e.Message.Content, Encoding.UTF8); steamcode_requested = false; }
                                        else if (IsValid == false) { await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help").ConfigureAwait(false); }
                                    }
                                    else
                                    {
                                        if (e.Message.Author.Username != user && string.IsNullOrEmpty(user))
                                        {
                                            if (e.Message.Author.Id != botID)
                                            {
                                                await e.Message.RespondAsync("I'm not programmed to do anything with that if you want a list of commands type " + prefix + "_help" + Environment.NewLine + "awaiting response from user: " + user + " for backup prompt.").ConfigureAwait(false);
                                            }
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
        public static string processString(string input, DSharpPlus.EventArgs.MessageCreateEventArgs e, string variable)
        {
            bool NullPresent = e == null || string.IsNullOrEmpty(input) || string.IsNullOrEmpty(variable);
            Contract.Requires(e != null);
            Contract.Requires(!string.IsNullOrEmpty(variable));
            Contract.Requires(!string.IsNullOrEmpty(input));
            if (NullPresent) 
            {
                return "";
            }
            if (variable.ToLower(CultureInfo.CurrentCulture) == "user")
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
        public static async void RespondMessage(string Author, string message, DiscordChannel discordChannel, bool[] permValue, DSharpPlus.EventArgs.MessageCreateEventArgs e, Globalvars globalvars)
        {
            Contract.Requires(message != null);
            Contract.Requires(permValue != null);
            Contract.Requires(e != null);
            Contract.Requires(discordChannel != null);
            if (!Program.DisableTrustManagement) 
            {
                if (permValue[0])
                {
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_help", StringComparison.CurrentCulture))
                    {
                        await messageSend("What would you like help with?" + Environment.NewLine + prefix + "_start: starts the server" + Environment.NewLine + prefix + "_stop: stops the server" + Environment.NewLine + prefix + "_restart: restarts the server" + Environment.NewLine + prefix + "_backup: initiates server backup, this will take the server offline", discordChannel).ConfigureAwait(false);
                    }
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_start", StringComparison.CurrentCulture))
                    {
                        await messageSend("Server is starting..." + Environment.NewLine + "check server status at the link below", discordChannel).ConfigureAwait(false);
                        if (globalvars.GametrackingURL != null)
                        {
                            await messageSend(globalvars.GametrackingURL, discordChannel).ConfigureAwait(false);
                        }
                        else
                        {
                            await messageSend("gametracking url not defined", discordChannel).ConfigureAwait(false);
                        }
                        GameManager.Manage_server(0, discordChannel,GlobalServerConfig.gvars);
                    }
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_stop", StringComparison.CurrentCulture))
                    {
                        await messageSend("Server is stopping...", discordChannel).ConfigureAwait(false);
                        GameManager.Manage_server(1, discordChannel, GlobalServerConfig.gvars);
                    }
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_restart", StringComparison.CurrentCulture))
                    {
                        await messageSend("Server is restarting...", discordChannel).ConfigureAwait(false);
                        GameManager.Manage_server(2, discordChannel, GlobalServerConfig.gvars);
                    }
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_backup", StringComparison.CurrentCulture))
                    {
                        user = Author;
                        await messageSend("Server backup initiated would you like to start server when finished yes/no?", discordChannel).ConfigureAwait(false);
                        backup_requested = true;
                    }
                    if (message.ToLower(CultureInfo.CurrentCulture).Equals("yes", StringComparison.CurrentCulture) && backup_requested == true && Author == user)
                    {
                        GameManager.server_startup = true;
                        await messageSend("Server will be started after backup", discordChannel).ConfigureAwait(false);
                        GameManager.Manage_server(3, discordChannel, GlobalServerConfig.gvars);
                        GameManager.server_startup = false;
                        backup_requested = false;
                        user = "";
                    }
                    else if (backup_requested == true && Author == user && message.ToLower(CultureInfo.CurrentCulture).Equals("no", StringComparison.CurrentCulture))
                    {
                        await messageSend("Server will remain offline after backup", discordChannel).ConfigureAwait(false);
                        GameManager.Manage_server(3, discordChannel, GlobalServerConfig.gvars);
                        backup_requested = false;
                        user = "";
                    }
                }
                else
                {
                    try
                    {
                        string output = processString(Messages.servermessages[0].messagebody, e, "user");
                        await e.Message.DeleteAsync(output).ConfigureAwait(false);
                    }
                    catch (DSharpPlus.Exceptions.NotFoundException ex)
                    {
                        Console.Error.WriteLine(Properties.Resources.MessageDeletionNotFound);
                        Console.Error.WriteLine(ex.Message);
                        Logging.Log("Error: " + Properties.Resources.MessageDeletionNotFound + Environment.NewLine + ex.Message);
                    }
                    catch (DSharpPlus.Exceptions.UnauthorizedException ex)
                    {
                        Console.Error.WriteLine(Properties.Resources.Message_Deletion_Failed);
                        Console.Error.WriteLine(ex.Message);
                        Logging.Log("Error: " + Properties.Resources.Message_Deletion_Failed + Environment.NewLine + ex.Message);
                    }
                }
            }
            else 
            {
                if (Program.MemoryStorage) 
                { 
                
                }
                else 
                {
                    GlobalServerConfig.load(e.Guild.Id);
                    if (discordChannel.Id == GlobalServerConfig.gvars.DiscordChannel)
                    {
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_help", StringComparison.CurrentCulture))
                        {
                            await messageSend("What would you like help with?" + Environment.NewLine + prefix + "_start: starts the server" + Environment.NewLine + prefix + "_stop: stops the server" + Environment.NewLine + prefix + "_restart: restarts the server" + Environment.NewLine + prefix + "_backup: initiates server backup, this will take the server offline", discordChannel).ConfigureAwait(false);
                        }
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_start", StringComparison.CurrentCulture))
                        {
                            await messageSend("Server is starting..." + Environment.NewLine + "check server status at the link below", discordChannel).ConfigureAwait(false);
                            if (globalvars.GametrackingURL != null)
                            {
                                await messageSend(globalvars.GametrackingURL, discordChannel).ConfigureAwait(false);
                            }
                            else
                            {
                                await messageSend("gametracking url not defined", discordChannel).ConfigureAwait(false);
                            }
                            GameManager.Manage_server(0, discordChannel, GlobalServerConfig.gvars);
                        }
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_stop", StringComparison.CurrentCulture))
                        {
                            await messageSend("Server is stopping...", discordChannel).ConfigureAwait(false);
                            GameManager.Manage_server(1, discordChannel, GlobalServerConfig.gvars);
                        }
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_restart", StringComparison.CurrentCulture))
                        {
                            await messageSend("Server is restarting...", discordChannel).ConfigureAwait(false);
                            GameManager.Manage_server(2, discordChannel, GlobalServerConfig.gvars);
                        }
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals(prefix + "_backup", StringComparison.CurrentCulture))
                        {
                            user = Author;
                            await messageSend("Server backup initiated would you like to start server when finished yes/no?", discordChannel).ConfigureAwait(false);
                            backup_requested = true;
                        }
                        if (message.ToLower(CultureInfo.CurrentCulture).Equals("yes", StringComparison.CurrentCulture) && backup_requested == true && Author == user)
                        {
                            GameManager.server_startup = true;
                            await messageSend("Server will be started after backup", discordChannel).ConfigureAwait(false);
                            GameManager.Manage_server(3, discordChannel, GlobalServerConfig.gvars);
                            GameManager.server_startup = false;
                            backup_requested = false;
                            user = "";
                        }
                        else if (backup_requested == true && Author == user && message.ToLower(CultureInfo.CurrentCulture).Equals("no", StringComparison.CurrentCulture))
                        {
                            await messageSend("Server will remain offline after backup", discordChannel).ConfigureAwait(false);
                            GameManager.Manage_server(3, discordChannel, GlobalServerConfig.gvars);
                            backup_requested = false;
                            user = "";
                        }
                    }
                }
            }
        }
    }
}
