using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace DiscordGameServerManager_Windows
{
    public class Messages
    {
        private const string dir = "Resources";
        private const string config = "DMs.json";
        private static Dictionary<ulong, DiscordDmChannel> userDM = new Dictionary<ulong, DiscordDmChannel>();
        public static void setMessage(string str, Message[] m, int index) 
        {
            if (Config.bot.useHeuristics)
            {
                str = Heuristics.produceString(str);
                m[index].messagebody = str;
            }
            else 
            {
                m[index].messagebody = str;
            }
            Config.write();
        }
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
        public static Dictionary<ulong,DiscordDmChannel>.ValueCollection GetValues() 
        {
            return userDM.Values;
        }
        public static void setMessage(string str, Message[] m, int index, DateTime date) 
        {
            if (Config.bot.useHeuristics)
            {
                str = Heuristics.produceString(str);
                m[index].messagebody = str;
                m[index].Date = date;
            }
            else 
            {
                m[index].messagebody = str;
                m[index].Date = date;
            }
            Config.write();
        }
        public static void setMessage(string head, string body, Message[] m, int index) 
        {
            if (Config.bot.useHeuristics) 
            {
                head = Heuristics.produceString(head);
                body = Heuristics.produceString(body);
                m[index].messagehead = head;
                m[index].messagebody = body;
            }
            else
            {
                m[index].messagehead = head;
                m[index].messagebody = body;
            }
            Config.write();
        }
        public static void setMessage(string head, string body, Message[] m, int index, DateTime date) 
        {
            if (Config.bot.useHeuristics) 
            {
                head = Heuristics.produceString(head);
                body = Heuristics.produceString(body);
                m[index].messagehead = head;
                m[index].messagebody = body;
                m[index].Date = date;
            }
            else
            {
                m[index].messagehead = head;
                m[index].messagebody = body;
                m[index].Date = date;
            }
            Config.write(m, typeof(Message[]));
        }
        public static async Task MessageSend(Message m, DiscordChannel discordChannel, DiscordClient discord)
        {
            DateTime current_date = DateTime.Now;
                if (!string.IsNullOrEmpty(m.messagehead)) 
                {
                    if (DateTime.Parse(m.Date.Month + "/" + m.Date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month + "/" + current_date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) && m.MessageOn && Config.bot.useHeuristics)
                    {
                        DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, Heuristics.produceString(m.messagehead + Heuristics.newline + m.messagebody), false, null).ConfigureAwait(false);
                        //await discordMessage.AcknowledgeAsync();
                    }
                    else if (DateTime.Parse(m.Date.Month + "/" + m.Date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month + "/" + current_date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) && m.MessageOn)
                    {
                        DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, m.messagehead + Heuristics.newline + m.messagebody, false, null).ConfigureAwait(false);
                        //await discordMessage.AcknowledgeAsync();
                    }
                }
            /*catch (Exception e)
            {
                Console.WriteLine("Message most likely has null or undefined properties, you can ignore this error for message at index 0");
                Console.WriteLine(e.Message);
            }*/
        }
        public static void write()
        {
            string json = JsonConvert.SerializeObject(userDM, Formatting.Indented);
            File.WriteAllText(dir + "/" + config, json);
        }
        public static void load()
        {
            string json = File.ReadAllText(dir + "/" + config);
            userDM = JsonConvert.DeserializeObject<Dictionary<ulong, DiscordDmChannel>>(json);
        }
        public struct Message : IEquatable<Message>
        {
            public DateTime Date { get; set; }
            public string messagehead { get; set; }
            public string messagebody { get; set; }
            public bool MessageOn { get; set; }
            public bool Heuristicsused { get; set; }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                else
                {
                    if (this == (Message)obj)
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
                hash = (hash * 3) + Date.GetHashCode();
                hash = (hash * 3) + messagehead.GetHashCode(StringComparison.CurrentCulture);
                hash = (hash * 3) + messagebody.GetHashCode(StringComparison.CurrentCulture);
                hash = (hash * 3) + MessageOn.GetHashCode();
                hash = (hash * 3) + Heuristicsused.GetHashCode();
                return hash;
            }

            public static bool operator ==(Message left, Message right)
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
                return left.Date == right.Date && left.messagehead == right.messagehead && left.messagebody == right.messagebody && left.MessageOn == right.MessageOn && left.Heuristicsused == right.Heuristicsused;
            }

            public static bool operator !=(Message left, Message right)
            {
                return !(left == right);
            }

            public bool Equals(Message other)
            {
                return this == other;
            }
        }
    }
}
