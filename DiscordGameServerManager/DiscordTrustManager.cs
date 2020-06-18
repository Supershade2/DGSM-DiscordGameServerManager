using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using DSharpPlus.Entities;
using System.Globalization;

namespace DiscordGameServerManager
{
    class DiscordTrustManager
    {
        public static List<Guild> guildinfo = new List<Guild>();
        private static Guild guild = new Guild();
        private static channelinfo channel_dictionary = new channelinfo();
        private static permissions perms = new permissions();
        private static channel channel = new channel();
        public static int usernum = Details.d.GuildInfo.usercount;
        public static List<user> users = new List<user>();
        public static user User = new user();
        //private static int user_index = usernum - 1 >=0 ? usernum-1:0;
        //private static int user_index_global = usernum - 1 >= 0 ? usernum - 1 : 0;
        private const string config = "permissions.json";
        private static bool default_perm_value = false;
        static DiscordTrustManager()
        {
            if (guild.chinfo.channels == null) 
            {
                channel_dictionary.channels = new Dictionary<ulong, channel>();
                guild.chinfo = channel_dictionary;
            }
            switch (load()) 
            {
                case false:
                    createJSON();
                    break;
                default:
                    break;
            }
        }
        public static void AddGuild(ulong id) 
        {
            guild.Uid = id;
            guildinfo.Add(guild);
        }
        public static void SetGuildID(ulong id) 
        {
            guild.Uid = id;
        }
        public static void UpdateGuild(ulong id) 
        {
            for (int i = 0; i < guildinfo.Count; i++)
            {
                if (guildinfo[i].Uid == id) 
                {
                    guildinfo[i] = guild;
                }
            }
        }
        public static bool[] checkPermission(ulong channelID, ulong id) 
        {
            try
            {
                List<user> users_c = GetUsers(channelID);
                user User = GetUser(users_c, id);
                bool[] perms = new bool[4];
                perms[0] = User.perms.manage_game;
                perms[1] = User.perms.dm_remote_management;
                perms[2] = User.perms.stop;
                perms[3] = User.perms.backup;
                return perms;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine(Properties.Resources.DiscordTrustManagerFailure);
                Console.Error.WriteLine(ex.Message);
                bool[] perms = new bool[4];
                for (int i = 0; i < perms.Length; i++) 
                {
                    perms[i] = false;
                }
                return perms;
            }
        }
        //Gets users by channel id
        public static List<user> GetUsers(ulong id) 
        {
            channel c = new channel();
            channel_dictionary.channels.TryGetValue(id, out c);
            return c.users;
        }
        private static user GetUser(List<user> users,ulong id) 
        {
            user u = new user();
            foreach (var usr in users)
            {
                if (usr.userID == id)
                {
                    u = usr;
                    return u;
                }
            }
            return u;
        }
        public static int getTotalUsers() 
        {
            usernum = Messages.GetUserDMCount();
            return usernum;
        }
        public static void setTotalUsers(ulong id, int usercount) 
        {
            usernum = usercount;
            ginfo g = Details.d.GuildInfo;
            g.usercount = usercount;
            Details.d.GuildInfo = g;
            Details.write();
        }
        public static void getTotalUsers(BotConfig bot) 
        {
            usernum = Details.d.GuildInfo.usercount;
        }
        public static void AddChannel(string name,ulong id,DiscordDmChannel dmChannel)
        {
            channel.id = dmChannel.Id;
            channel.name = dmChannel.Name;
            channel.category = "Direct Message";
			channel.PromptSent = true;
            User.username = name;
            User.userID = id;
            bool[] perms_array = Array.Empty<bool>();
            setPermissions(perms_array, User);
            users.Add(User);
            channel.users = users;
            channel_dictionary.channels.Add(dmChannel.Id,channel);
            guild.chinfo = channel_dictionary;
            UpdateGuild(guild.Uid);
            write();
        }
        public static void AddChannel(string name, ulong id, DiscordChannel Dchannel) 
        {
            channel.id = Dchannel.Id;
            channel.name = Dchannel.Name;
            channel.category = Dchannel.Guild.Name;
            User.username = name;
            User.userID = id;
            bool[] perms_array = Array.Empty<bool>();
            setPermissions(perms_array,User);
            users.Add(User);
            channel.users = users;
            channel_dictionary.channels.Add(Dchannel.Id, channel);
            guild.chinfo = channel_dictionary;
            UpdateGuild(guild.Uid);
            write();
        }
        public static void updateChannel(string name, ulong id, DiscordChannel Dchannel) 
        {
            channel_dictionary.channels.Remove(Dchannel.Id);
            User.username = name;
            User.userID = id;
            channel.users = users;
            channel_dictionary.channels.Add(Dchannel.Id, channel);
            guild.chinfo = channel_dictionary;
            UpdateGuild(guild.Uid);
            write();
        }
        public static void updateChannel(ulong id, DiscordDmChannel dmChannel) 
        {
            channel_dictionary.channels.Remove(dmChannel.Id);
            User.userID = id;
            channel.users = users;
            channel_dictionary.channels.Add(dmChannel.Id, channel);
            guild.chinfo = channel_dictionary;
            UpdateGuild(guild.Uid);
            write();
        }
        public static void setChannelID(ulong id)
        {
            channel.id = id;
        }
        public static void setChannelCategory(string category)
        {
            channel.category = category;
        }
        public static void setChannelName(string name)
        {
            channel.name = name;
        }
        public static void setPermissions(bool[] perm_values, user u)
        {
            switch (perm_values.Length)
            {
                case 4:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = perm_values[2];
                    perms.backup = perm_values[3];
                    break;
                case 3:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = perm_values[2];
                    perms.backup = default_perm_value;
                    break;
                case 2:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
                case 1:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
                default:
                    perms.manage_game = default_perm_value;
                    perms.dm_remote_management = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
            }
            int indx = users.IndexOf(u);
            user usr = new user();
            usr.perms = new permissions();
            usr.perms = perms;
            users[indx] = usr;
        }
        public static void setPermissions(ulong channel_id, ulong user_id,bool[] perm_values) 
        {
            switch (perm_values.Length)
            {
                case 4:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = perm_values[2];
                    perms.backup = perm_values[3];
                    break;
                case 3:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = perm_values[2];
                    perms.backup = default_perm_value;
                    break;
                case 2:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = perm_values[1];
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
                case 1:
                    perms.manage_game = perm_values[0];
                    perms.dm_remote_management = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
                default:
                    perms.manage_game = default_perm_value;
                    perms.dm_remote_management = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.backup = default_perm_value;
                    break;
            }
            user u;
            u = GetUser(GetUsers(channel_id), user_id);
            updateUserPerms(channel_id,u);
        }
        public static void register(ulong id, DiscordDmChannel dmChannel) 
        {
            channel ch = new channel();
            channel_dictionary.channels.TryGetValue(dmChannel.Id, out channel);
            user u = new user();
            foreach (var usr in ch.users)
            {
                if (usr.userID == id) 
                {
                    int index = ch.users.IndexOf(usr);
                    u = ch.users.ElementAt(index);
                    u.registered = true;
                    ch.users[index] = u;
                    channel_dictionary.channels.Remove(dmChannel.Id);
                    channel_dictionary.channels.Add(dmChannel.Id,ch);
                    guild.chinfo = channel_dictionary;
                }
            }
        }
        public static void updateUserPerms(ulong channel_id,user _user)
        {
            channel ch = new channel();
            channel_dictionary.channels.TryGetValue(channel_id, out ch);
            user u = new user();
            foreach (var usr in ch.users)
            {
                if (usr.userID == _user.userID) 
                {
                    int index = ch.users.IndexOf(usr);
                    u = ch.users.ElementAt(index);
                    u.perms = perms;
                    ch.users[index] = u;
                    channel_dictionary.channels.Remove(channel_id);
                    channel_dictionary.channels.Add(channel_id, ch);
                    guild.chinfo = channel_dictionary;
                }
            }
        }
        public static bool createJSON()
        {
            try
            {
                if (!File.Exists(Properties.Resources.ResourcesDir + "/" + config))
                {
                    string json = JsonConvert.SerializeObject(guildinfo, Formatting.Indented);
                    File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
                }
                return true;
            }
            catch (JsonSerializationException ex)
            {
                Console.WriteLine("DiscordTrustManager: Method: createJSON");
                Console.WriteLine(ex.InnerException.Source);
                Console.WriteLine(ex.Message);
                return false;
            }
            catch (IOException ex) 
            {
                Console.WriteLine("DiscordTrustManager: Method: createJSON");
                Console.WriteLine(ex.InnerException.Source);
                Console.WriteLine(ex.Message);
                return false;
            }

        }
        public static bool write() 
        {
                try
                {
                    string json = JsonConvert.SerializeObject(guildinfo, Formatting.Indented);
                    File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
                    return true;
                }
                catch (JsonSerializationException ex)
                {
                    Console.Error.WriteLine(Properties.Resources.DiscordTrustManagerWriteFailure);
                    Console.Error.WriteLine(ex.Message);
                    return false;
                }
        }
        public static bool load() 
        {
            try
            {
                string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + config);
                guildinfo = JsonConvert.DeserializeObject<List<Guild>>(json);
                default_perm_value = guild.default_perm_value;
                return true;
            }
            catch (FileNotFoundException ex)
            {
                Console.Error.WriteLine(Properties.Resources.DiscordTrustManagerWriteFailure);
                Console.Error.WriteLine(ex.Message);
                return false;
            }
        }
        public static string[] ReadConfig(ulong id) 
        {
            List<string> items = new List<string>();
            using (StreamReader reader = new StreamReader(File.OpenRead(Properties.Resources.ResourcesDir + "/" + config)))
            {
                char[] ca = reader.ReadLine().ToCharArray();
                do
                {
                    bool Is_open = false;
                    bool value = false;
                    bool Idfound = false;
                    bool keyfound = false;
                    string trigger = "Uid";
                    string item = "";
                    for (int i = 0; i < ca.Length; i++)
                    {
                        if (ca[i] == ',' || ca[i] == '}' || ca[i] == ']' || ca[i] == '{' || ca[i] == '[')
                        {
                            value = false;
                            if (item.Contains(id.ToString(CultureInfo.CurrentCulture),StringComparison.CurrentCulture)) 
                            {
                                Idfound = true;
                            }
                        }
                        if (ca[i] == '"')
                        {
                            Is_open = Is_open == true ? false : true;
                            if (!keyfound)
                            {
                                switch (Is_open)
                                {
                                    case true:
                                        break;
                                    default:
                                        keyfound = item.ToLower(System.Globalization.CultureInfo.CurrentCulture) == trigger.ToLower(System.Globalization.CultureInfo.CurrentCulture) ? true : false;
                                        item = "";
                                        break;
                                }
                            }
                            else
                            {
                                switch (Is_open)
                                {
                                    case true:
                                        break;
                                    default:
                                        if (Idfound) 
                                        {
                                            items.Add(item);
                                            item = "";
                                            keyfound = false;
                                        }
                                        break;
                                }
                            }
                        }
                        else if (value)
                        {
                            item += ca[i];
                        }
                        if (ca[i] == ':')
                        {
                            value = true;
                        }
                    }
                    ca = reader.ReadLine().ToCharArray();
                } while (ca != null);
            }
                return items.ToArray();
        }
    }
    public struct Guild
    {
        public ulong Uid { get; set; }
        public bool default_perm_value { get; set; }
        public channelinfo chinfo { get; set; }
        public ulong DiscordChannel { get; set; }
    }
    public struct channelinfo
    {
        public Dictionary<ulong, channel> channels{get; set;}
    }
    public struct user
    {
        public string username { get; set; }
        public ulong userID { get; set; }
        public string discriminator { get; set; }
        public string role { get; set; }
        public int trust_level { get; set; }
        public bool registered { get; set; }
        public permissions perms { get; set; }
    }
    public struct permissions
    {
        public bool manage_game { get; set; }
        public bool dm_remote_management { get; set; }
        public bool stop { get; set; }
        public bool backup { get; set; }
    }
    public struct channel
    {
        public List<user> users { get; set; }
        public ulong id { get; set; }
        public string category { get; set; }
        public string name { get; set; }
		public bool PromptSent{get; set;}
    }
}
