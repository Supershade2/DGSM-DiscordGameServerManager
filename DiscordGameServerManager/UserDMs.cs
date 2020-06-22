using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordGameServerManager
{
    public class UserDMs
    {
        private static Dictionary<ulong, DiscordDmChannel> userDM = new Dictionary<ulong, DiscordDmChannel>();
        public static void AddDM(ulong id, DiscordDmChannel discordDm)
        {
            if (!userDM.ContainsKey(id))
            {
                userDM.Add(id, discordDm);
            }
        }
        public static int GetUserDMCount()
        {
            return userDM.Count;
        }
        public static bool HasID(ulong id)
        {
            return userDM.Keys.ToArray().Contains(id);
        }
        public static Dictionary<ulong, DiscordDmChannel> GetUserDMs()
        {
            return userDM;
        }
        public static Dictionary<ulong, DiscordDmChannel>.ValueCollection GetValues()
        {
            return userDM.Values;
        }
    }
}
