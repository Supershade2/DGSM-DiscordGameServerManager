using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace DiscordGameServerManager
{
    //To be Implemented logic for user methods will be migrated to here
    //Repurposed into a global vars config class for different guilds
    public class GlobalServerConfig
    {
        public const string vars = "globalvars.json";
        public static List<Globalvars> globalvars = new List<Globalvars>();
        public static Globalvars gvars = new Globalvars();
        public static void Initialize(ulong id) 
        {
            if(!File.Exists(Properties.Resources.ResourcesDir + "/" + id.ToString(CultureInfo.CurrentCulture) + "/" + vars)) 
            { 
                if(!Directory.Exists(Properties.Resources.ResourcesDir + "/" + id.ToString(CultureInfo.CurrentCulture)))
                { 
                    Directory.CreateDirectory(Properties.Resources.ResourcesDir + "/" + id.ToString(CultureInfo.CurrentCulture));
                }
                if (Program.MemoryStorage)
                {
                    Globalvars g = new Globalvars();
                    g.id = id;
                    globalvars.Add(g);
                }
                else
                {
                    gvars.id = id;
                }
            }
            else 
            {
                load(id);
            }
        }
        public static void SetID(ulong id) 
        {
            gvars.id = id;
        }
        public static void ExtendVarsList(ulong id) 
        {
            bool match = false;
            if(globalvars.Count > 0) 
            {
                foreach (var item in globalvars)
                {
                    if(item.id == id) 
                    {
                        match = true;
                    }
                }
            }
            switch (match)
            {
                case true:
                    break;
                default:
                    Globalvars g = new Globalvars();
                    g.id = id;
                    globalvars.Add(g);
                    break;
            }
        }
        public static void write(ulong id) 
        {
            object obj = gvars;
            Config.write(id, obj, typeof(Globalvars));
        }
        public static void load(ulong id) 
        {
            string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + id.ToString(CultureInfo.CurrentCulture) + "/" + vars);
            gvars = JsonConvert.DeserializeObject<Globalvars>(json);
        }
    }
    /*public struct GameServer
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
    }*/
    public struct Globalvars : IEquatable<Globalvars>
    {
        public ulong id { get; set; }
        public ulong DiscordChannel { get; set; }
        public ulong MessageChannel { get; set; }
        public string registrationkey { get; set; }
        public string invite { get; set; }
        /*public string gamedir { get; set; }
        public string wsapikey { get; set; }
        public string wscollectionid { get; set; }
        public string GamelaunchARGSscript { get; set; }
        public string game { get; set; }
        public string GametrackingURL { get; set; }
         */

        public override bool Equals(object obj) =>
            obj is Globalvars gv && this == gv;

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + id.GetHashCode();
            hash = (hash * 3) + DiscordChannel.GetHashCode();
            hash = (hash * 3) + MessageChannel.GetHashCode();
            hash = (hash * 3) + registrationkey.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + invite.GetHashCode(StringComparison.CurrentCulture);
            return hash;
        }

        public static bool operator ==(Globalvars left, Globalvars right)
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
            return left.id == right.id && left.DiscordChannel == right.DiscordChannel && left.MessageChannel == right.MessageChannel && left.registrationkey == right.registrationkey && left.invite == right.invite;
        }

        public static bool operator !=(Globalvars left, Globalvars right)
        {
            return !(left == right);
        }

        public bool Equals(Globalvars other)
        {
            return this == other;
        }
    }
   /*public struct userdata
    {
        public string username { get; set; }
        public ulong userID { get; set; }
        public string discriminator { get; set; }
        public string role { get; set; }
        public int trust_level { get; set; }
        public bool registered { get; set; }
        public permissions perms { get; set; }
    }*/
}
