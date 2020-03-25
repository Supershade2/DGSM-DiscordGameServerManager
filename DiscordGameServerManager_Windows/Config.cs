using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace DiscordGameServerManager_Windows
{
    class Config
    {
        private const string dir = "Resources";
        private const string config = "config.json";
        public static BotConfig bot;
        Config()
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
                temp_clust.servers[0].RCONPass = "password";
                temp_clust.servers[0].mainPort = 7777;
                temp_clust.servers[0].useMods = false;
                temp_clust.servers[0].rawPort = 7778;
                temp_clust.servers[0].queryPort = 27015;
                temp_clust.servers[0].RCONPort = 27020;
                bot.cluster = temp_clust;
                bot.useHeuristics = false;
                //initializes default messages for specific dates
                bot.botmessages = new Messages.Message[14];
                bot.botmessages[12].Date = DateTime.Parse("12/25/"+DateTime.Now.Year, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name));
                bot.botmessages[12].messagehead = "Merry Christmas, here's a verse:";
                bot.botmessages[12].messagebody = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life." + Environment.NewLine + "For God did not send his Son into the world to condemn the world, but to save the world through him." + Environment.NewLine + "Whoever believes in him is not condemned, but whoever does not believe stands condemned already because they have not believed in the name of God’s one and only Son."+Environment.NewLine+"This is the verdict: Light has come into the world, but people loved darkness instead of light because their deeds were evil."+Environment.NewLine+"Everyone who does evil hates the light, and will not come into the light for fear that their deeds will be exposed."+Environment.NewLine+"But whoever lives by the truth comes into the light, so that it may be seen plainly that what they have done has been done in the sight of God.";
                bot.botmessages[12].MessageOn = true;
                bot.botmessages[11].Date = DateTime.Parse("03/10/"+DateTime.Now.Year, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name));
                bot.botmessages[11].messagehead = "It's a march:";
                bot.botmessages[11].MessageOn = false;
                bot.botmessages[11].messagebody = "Wahoo!";
                bot.botmessages[0].messagebody = "%user% you are not permitted to post here, please contact a admin or mod for info";
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
        public static void write(object obj, Type type) 
        {
            //var converted = Convert.ChangeType(obj, type);
            if (type == typeof(Messages.Message[])) 
            {
                bot.botmessages = (Messages.Message[])obj;
            }
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(dir + "/" + config, json);
        }
        public static void load()
        {
            string json = File.ReadAllText(dir + "/" + config);
            bot = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
    public struct GameServer : IEquatable<GameServer>
    { 
        public int maxPlayers { get; set; }
        public string map { get; set; }
        public string address { get; set; }
        public int mainPort { get; set; }
        public bool useMods { get; set; }
        public int rawPort { get; set; }
        public int queryPort { get; set; }
        public int RCONPort { get; set; }
        public string RCONPass { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (this == (GameServer)obj)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + maxPlayers.GetHashCode();
            hash = (hash * 3) + map.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + address.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + mainPort.GetHashCode();
            hash = (hash * 3) + useMods.GetHashCode();
            hash = (hash * 3) + rawPort.GetHashCode();
            hash = (hash * 3) + queryPort.GetHashCode();
            hash = (hash * 3) + RCONPort.GetHashCode();
            hash = (hash * 3) + RCONPass.GetHashCode(StringComparison.CurrentCulture);
            return hash;
        }

        public static bool operator ==(GameServer left, GameServer right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }
            return left.maxPlayers == right.maxPlayers && left.map == right.map && left.address == right.address && left.mainPort == right.mainPort && left.useMods == right.useMods && left.rawPort == right.rawPort && left.queryPort == right.queryPort && left.RCONPort == right.RCONPort && left.RCONPass == right.RCONPass;
        }

        public static bool operator !=(GameServer left, GameServer right)
        {
            return !(left == right);
        }

        public bool Equals(GameServer other)
        {
            return this == other;
        }
    }
    public struct Cluster : IEquatable<Cluster>
    { 
        public int portGap { get; set; }
        public GameServer[] servers { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else
            {
                if (this == (Cluster)obj)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + portGap.GetHashCode();
            hash = (hash * 3) + servers.GetHashCode();
            return hash;
        }

        public static bool operator ==(Cluster left, Cluster right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }
            return left.portGap == right.portGap && left.servers == right.servers;
        }

        public static bool operator !=(Cluster left, Cluster right)
        {
            return !(left == right);
        }

        public bool Equals(Cluster other)
        {
            return this == other;
        }
    }
    public struct BotConfig : IEquatable<BotConfig>
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
        public string GametrackingURL { get; set; }
        public bool useHeuristics { get; set; }
        public string registrationkey { get; set; }
        public string invite { get; set; }
        public Cluster cluster { get; set; }
        public Messages.Message[] botmessages { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) 
            {
                return false;
            }
            else 
            {
                if (this == (BotConfig)obj) 
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + token.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + server_guild_id.GetHashCode();
            hash = (hash * 3) + discord_channel.GetHashCode();
            hash = (hash * 3) + message_channel.GetHashCode();
            hash = (hash * 3) + steamcmd_dir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + game_dir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + wsapikey.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + wscollectionid.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + game_launch_args_script.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + prefix.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + backup_dir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + game.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + ID.GetHashCode();
            hash = (hash * 3) + GametrackingURL.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + useHeuristics.GetHashCode();
            hash = (hash * 3) + registrationkey.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + invite.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + cluster.GetHashCode();
            hash = (hash * 3) + botmessages.GetHashCode();
            return hash;
        }

        public static bool operator ==(BotConfig left, BotConfig right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }

            // Return true if the fields match:
            return left.token == right.token && left.server_guild_id == right.server_guild_id && left.discord_channel == right.discord_channel && left.message_channel == right.message_channel && left.steamcmd_dir == right.steamcmd_dir && left.game_dir == right.game_dir && left.wsapikey == right.wsapikey && left.wscollectionid == right.wscollectionid && left.game_launch_args_script == right.game_launch_args_script && left.prefix == right.prefix && left.backup_dir == right.backup_dir && left.game == right.game && left.ID == right.ID && left.GametrackingURL == right.GametrackingURL && left.useHeuristics == right.useHeuristics && left.registrationkey == right.registrationkey && left.invite == right.invite && left.cluster.portGap == right.cluster.portGap && left.cluster.servers == right.cluster.servers && left.botmessages == right.botmessages;
        }

        public static bool operator !=(BotConfig left, BotConfig right)
        {
            return !(left == right);
        }

        public bool Equals(BotConfig other)
        {
            return this == other;
        }
    }
}
