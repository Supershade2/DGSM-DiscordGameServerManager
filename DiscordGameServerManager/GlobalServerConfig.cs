using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public static void load(ulong id) 
        {
            string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + id.ToString(System.Globalization.CultureInfo.CurrentCulture) + "/" + vars);
            gvars = JsonConvert.DeserializeObject<Globalvars>(json);
        }
    }
    public struct Globalvars : IEquatable<Globalvars>
    {
        public ulong id { get; set; }
        public ulong DiscordChannel { get; set; }
        public ulong MessageChannel { get; set; }

        public override bool Equals(object obj) =>
            obj is Globalvars gv && this == gv;

        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + id.GetHashCode();
            hash = (hash * 3) + DiscordChannel.GetHashCode();
            hash = (hash * 3) + MessageChannel.GetHashCode();
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
            return left.id == right.id && left.DiscordChannel == right.DiscordChannel && left.MessageChannel == right.MessageChannel;
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
