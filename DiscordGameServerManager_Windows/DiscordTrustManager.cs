using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using System.IO;
using DSharpPlus.Entities;
namespace DiscordGameServerManager_Windows
{
    class DiscordTrustManager
    {
        public static channelinfo channel_dictionary = new channelinfo();
        private static permissions perms = new permissions();
        private static channel channel = new channel();
        public static int usernum = Details.d.user_count;
        public static List<user> users = new List<user>();
        public static user User = new user();
        //private static int user_index = usernum - 1 >=0 ? usernum-1:0;
        //private static int user_index_global = usernum - 1 >= 0 ? usernum - 1 : 0;
        private const string dir = "Resources";
        private const string config = "permissions.json";
        private static bool default_perm_value = false;
        static DiscordTrustManager()
        {
            if (channel_dictionary.channels == null) 
            {
                channel_dictionary.channels = new Dictionary<ulong, channel>();
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
        public static bool[] checkPermission(ulong channelID, ulong id) 
        {
            try
            {
                List<user> users_c = GetUsers(channelID);
                user User = GetUser(users_c, id);
                bool[] perms = new bool[6];
                perms[0] = User.perms.manage_game;
                perms[1] = User.perms.remote_management;
                perms[2] = User.perms.start;
                perms[3] = User.perms.stop;
                perms[4] = User.perms.text;
                perms[5] = User.perms.voice;
                return perms;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiscordTrustManager: Method: checkPermission");
                Console.WriteLine(ex.Message);
                bool[] perms = new bool[6];
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
            usernum = Messages.userDM.Count;
            return usernum;
        }
        public static void setTotalUsers(int usercount) 
        {
            usernum = usercount;
            Details.d.user_count = usercount;
            Details.write();
        }
        public static void getTotalUsers(BotConfig bot) 
        {
            usernum = Details.d.user_count;
        }
        public static void addChannel(string name,ulong id,DiscordDmChannel dmChannel)
        {
            channel.id = dmChannel.Id;
            channel.name = dmChannel.Name;
            channel.category = "Direct Message";
            User.username = name;
            User.userID = id;
            bool[] perms_array = new bool[0];
            setPermissions(perms_array, User);
            users.Add(User);
            channel.users = users;
            channel_dictionary.channels.Add(dmChannel.Id,channel);
            write();
        }
        public static void addChannel(string name, ulong id, DiscordChannel Dchannel) 
        {
            channel.id = Dchannel.Id;
            channel.name = Dchannel.Name;
            channel.category = Dchannel.Guild.Name;
            User.username = name;
            User.userID = id;
            bool[] perms_array = new bool[0];
            setPermissions(perms_array,User);
            users.Add(User);
            channel.users = users;
            channel_dictionary.channels.Add(Dchannel.Id, channel);
            write();
        }
        public static void updateChannel(string name, ulong id, DiscordChannel Dchannel) 
        {
            channel_dictionary.channels.Remove(Dchannel.Id);
            User.username = name;
            User.userID = id;
            channel.users = users;
            channel_dictionary.channels.Add(Dchannel.Id, channel);
            write();
        }
        public static void updateChannel(ulong id, DiscordDmChannel dmChannel) 
        {
            channel_dictionary.channels.Remove(dmChannel.Id);
            User.userID = id;
            channel.users = users;
            channel_dictionary.channels.Add(dmChannel.Id, channel);
            write();
        }
        public static void setChannelID(ulong id, int index)
        {
            channel.id = id;
        }
        public static void setChannelCategory(string category, int index)
        {
            channel.category = category;
        }
        public static void setChannelName(string name, int index)
        {
            channel.name = name;
        }
        public static void setPermissions(bool[] perm_values, user u)
        {
            switch (perm_values.Length)
            {
                case 6:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = perm_values[4];
                    perms.stop = perm_values[5];
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 5:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = perm_values[4];
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 4:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 3:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = default_perm_value;
                    break;
                case 2:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
                    break;
                case 1:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = default_perm_value;
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
                    break;
                default:
                    perms.manage_game = default_perm_value;
                    perms.remote_management = default_perm_value;
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
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
                case 6:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = perm_values[4];
                    perms.stop = perm_values[5];
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 5:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = perm_values[4];
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 4:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = perm_values[3];
                    break;
                case 3:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = perm_values[2];
                    perms.voice = default_perm_value;
                    break;
                case 2:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = perm_values[1];
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
                    break;
                case 1:
                    perms.manage_game = perm_values[0];
                    perms.remote_management = default_perm_value;
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
                    break;
                default:
                    perms.manage_game = default_perm_value;
                    perms.remote_management = default_perm_value;
                    perms.start = default_perm_value;
                    perms.stop = default_perm_value;
                    perms.text = default_perm_value;
                    perms.voice = default_perm_value;
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
                }
            }
        }
        public static bool createJSON()
        {
            try
            {
                if (!File.Exists(dir + "/" + config))
                {
                    string json = JsonConvert.SerializeObject(channel_dictionary, Formatting.Indented);
                    File.WriteAllText(dir + "/" + config, json);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiscordTrustManager: Method: createJSON");
                Console.WriteLine(ex.Message);
                return false;
            }

        }
        public static bool write() 
        {
            try
            {
                string json = JsonConvert.SerializeObject(channel_dictionary, Formatting.Indented);
                File.WriteAllText(dir + "/" + config, json);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiscordTrustManager: Method: write");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public static bool load() 
        {
            try
            {
                string json = File.ReadAllText(dir + "/" + config);
                channel_dictionary = JsonConvert.DeserializeObject<channelinfo>(json);
                default_perm_value = channel_dictionary.default_perm_value;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("DiscordTrustManager: Method: load");
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
    public struct channelinfo
    {
        public Dictionary<ulong, channel> channels{get; set;}
        public bool default_perm_value { get; set; }
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
        public bool remote_management { get; set; }
        public bool start { get; set; }
        public bool stop { get; set; }
        public bool text { get; set; }
        public bool voice { get; set; }
    }
    public struct channel
    {
        public List<user> users { get; set; }
        public ulong id { get; set; }
        public string category { get; set; }
        public string name { get; set; }
    }
}
