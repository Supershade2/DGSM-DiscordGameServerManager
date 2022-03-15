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
    public struct BotConfig
    {
        public string token { get; set; }
        public bool useSSH { get; set; }
        public string steamcmddir { get; set; }
        public string prefix { get; set; }
        public ulong ID { get; set; }
        public bool useHeuristics { get; set; }

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + token.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + steamcmddir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + prefix.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + ID.GetHashCode();
            hash = (hash * 3) + useHeuristics.GetHashCode();
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
            return left.token == right.token && left.steamcmddir == right.steamcmddir && left.prefix == right.prefix  && left.ID == right.ID && left.useHeuristics == right.useHeuristics;
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
