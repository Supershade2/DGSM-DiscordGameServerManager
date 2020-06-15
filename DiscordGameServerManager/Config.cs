using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.InteropServices;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace DiscordGameServerManager
{
    public class Config
    {
        private const string config = "config.json";
        public static BotConfig bot;
        public Config()
        {
            if (!Directory.Exists(Properties.Resources.ResourcesDir))
                Directory.CreateDirectory(Properties.Resources.ResourcesDir);

            if (!File.Exists(Properties.Resources.ResourcesDir + "/" + config))
            {
                File.Create(Properties.Resources.ResourcesDir + "/" + config).Close();
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
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
                //byte[] json_data = Encoding.ASCII.GetBytes(json);
                //File.Open(dir + "/" + config, FileMode.Open, FileAccess.Write, FileShare.Write).Write(json_data,0,json_data.Length-1);
            }
            load();
        }
        public static void write() 
        {
            string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
            File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
        }
        public static void write(ulong ID, object obj, Type type)
        {
            if (!Directory.Exists(Properties.Resources.ResourcesDir + "/" + ID.ToString(CultureInfo.CurrentCulture)))
            {
                Directory.CreateDirectory(Properties.Resources.ResourcesDir + "/" + ID.ToString(CultureInfo.CurrentCulture));
            }
            //var converted = Convert.ChangeType(obj, type);
            if (type == typeof(Messages.Message[])) 
            {
                string json = JsonConvert.SerializeObject((Messages.Message[])obj, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + ID.ToString(CultureInfo.CurrentCulture) + "/" + Messages.config, json);
            }
            if(type == typeof(Globalvars)) 
            {
                string json = JsonConvert.SerializeObject((Globalvars)obj, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + ID.ToString(CultureInfo.CurrentCulture) + "/" + GlobalServerConfig.vars,json);
            }
        }
        public static void load()
        {
            string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + config);
            bot = JsonConvert.DeserializeObject<BotConfig>(json);
        }
    }
    public struct GameServer
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
    public struct Cluster
    { 
        public int portGap { get; set; }
        public GameServer[] servers { get; set; }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + portGap.GetHashCode();
            foreach (var item in servers) 
            {
                hash = (hash * 3) + item.GetHashCode();
            }
            //hash = (hash * 3) + servers.GetHashCode();
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
    public struct BotConfig
    {
        public string token { get; set; }
        public bool useSSH { get; set; }
        public string steamcmddir { get; set; }
        public string gamedir { get; set; }
        public string wsapikey { get; set; }
        public string wscollectionid { get; set; }
        public string GamelaunchARGSscript { get; set; }
        public string prefix { get; set; }
        public string backupdir { get; set; }
        public string game { get; set; }
        public ulong ID { get; set; }
        public string GametrackingURL { get; set; }
        public bool useHeuristics { get; set; }
        public string registrationkey { get; set; }
        public string invite { get; set; }
        public Cluster cluster { get; set; }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + token.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + steamcmddir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + gamedir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + wsapikey.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + wscollectionid.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + GamelaunchARGSscript.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + prefix.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + backupdir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + game.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + ID.GetHashCode();
            hash = (hash * 3) + GametrackingURL.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + useHeuristics.GetHashCode();
            hash = (hash * 3) + registrationkey.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + invite.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + cluster.GetHashCode();
            //hash = (hash * 3) + botmessages.GetHashCode();
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
            return left.token == right.token && left.steamcmddir == right.steamcmddir && left.gamedir == right.gamedir && left.wsapikey == right.wsapikey && left.wscollectionid == right.wscollectionid && left.GamelaunchARGSscript == right.GamelaunchARGSscript && left.prefix == right.prefix && left.backupdir == right.backupdir && left.game == right.game && left.ID == right.ID && left.GametrackingURL == right.GametrackingURL && left.useHeuristics == right.useHeuristics && left.registrationkey == right.registrationkey && left.invite == right.invite && left.cluster.portGap == right.cluster.portGap && left.cluster.servers == right.cluster.servers;
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
