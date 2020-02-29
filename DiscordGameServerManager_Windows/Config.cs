using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace DiscordGameServerManager_Windows
{
    class Config
    {
        private const string dir = "Resources";
        private const string config = "config.json";
        public static BotConfig bot;
        static Config()
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(dir + "/" + config))
            {
                File.Create(dir + "/" + config).Close();
                bot = new BotConfig();
                Cluster temp_clust = new Cluster();
                temp_clust.servers = new GameServer[1];
                temp_clust.servers[0].address = "127.0.0.1";
                temp_clust.servers[0].RCON_pass = "password";
                temp_clust.servers[0].main_port = 7777;
                temp_clust.servers[0].use_mods = false;
                temp_clust.servers[0].raw_port = 7778;
                temp_clust.servers[0].query_port = 27015;
                temp_clust.servers[0].RCON_port = 27020;
                bot.cluster = temp_clust;
                bot.useHeuristics = false;
                //initializes default messages for specific dates
                bot._messages = new Messages.message[14];
                bot._messages[12].Date = DateTime.Parse("12/24/2020");
                bot._messages[12].message_head = "Merry Christmas, here's a verse:";
                bot._messages[12].message_body = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life." + Environment.NewLine + "For God did not send his Son into the world to condemn the world, but to save the world through him." + Environment.NewLine + "Whoever believes in him is not condemned, but whoever does not believe stands condemned already because they have not believed in the name of God’s one and only Son."+Environment.NewLine+"This is the verdict: Light has come into the world, but people loved darkness instead of light because their deeds were evil."+Environment.NewLine+"Everyone who does evil hates the light, and will not come into the light for fear that their deeds will be exposed."+Environment.NewLine+"But whoever lives by the truth comes into the light, so that it may be seen plainly that what they have done has been done in the sight of God.";
                bot._messages[12].MessageOn = true;
                bot._messages[11].Date = DateTime.Parse("03/10/2020");
                bot._messages[11].message_head = "It's a march:";
                bot._messages[11].MessageOn = false;
                bot._messages[11].message_body = "Wahoo!";
                bot._messages[0].message_body = "%user% you are not permitted to post here, please contact a admin or mod for info";
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(dir + "/" + config, json);
                //byte[] json_data = Encoding.ASCII.GetBytes(json);
                //File.Open(dir + "/" + config, FileMode.Open, FileAccess.Write, FileShare.Write).Write(json_data,0,json_data.Length-1);
            }
            load();
        }
        public static void write() 
        {
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(dir + "/" + config, json);
        }
        public static void load()
        {
            string json = File.ReadAllText(dir + "/" + config);
            bot = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
    public struct GameServer 
    { 
        public int max_players { get; set; }
        public string map { get; set; }
        public string address { get; set; }
        public int main_port { get; set; }
        public bool use_mods { get; set; }
        public int raw_port { get; set; }
        public int query_port { get; set; }
        public int RCON_port { get; set; }
        public string RCON_pass { get; set; }
    }
    public struct Cluster 
    { 
        public int port_gap { get; set; }
        public GameServer[] servers { get; set; }
    }
    public struct BotConfig
    {
        public string token { get; set; }
        public ulong server_guild_id { get; set; }
        public ulong discord_channel { get; set; }
        public ulong message_channel { get; set; }
        public string steamcmd_dir { get; set; }
        public string game_dir { get; set; }
        public string wsapikey { get; set; }
        public string wscollectionid { get; set; }
        public string game_launch_args_script { get; set; }
        public string prefix { get; set; }
        public string backup_dir { get; set; }
        public string game { get; set; }
        public ulong ID { get; set; }
        public string gametracking_url { get; set; }
        public bool useHeuristics { get; set; }
        public string registration_key { get; set; }
        public string invite { get; set; }
        public Cluster cluster { get; set; }
        public Messages.message[] _messages { get; set; }
    }
}
