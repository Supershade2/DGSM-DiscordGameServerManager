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
using System.Globalization;

namespace DiscordGameServerManager
{
    public class Messages
    {
        private const string DM = "DMs.json";
        public const string config = "messages.json";
        public static List<Message[]> botmessages { get; set; }
        public static Message[] servermessages { get; set; }
        //initializes default messages for specific dates
        public static void Initialize(ulong id) 
        {
            servermessages = new Message[14];
            servermessages[12].Date = DateTime.Parse("12/25/" + DateTime.Now.Year, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name));
            servermessages[12].messagehead = "Merry Christmas, here's a verse:";
            servermessages[12].messagebody = "For God so loved the world that he gave his one and only Son, that whoever believes in him shall not perish but have eternal life." + Environment.NewLine + "For God did not send his Son into the world to condemn the world, but to save the world through him." + Environment.NewLine + "Whoever believes in him is not condemned, but whoever does not believe stands condemned already because they have not believed in the name of God’s one and only Son." + Environment.NewLine + "This is the verdict: Light has come into the world, but people loved darkness instead of light because their deeds were evil." + Environment.NewLine + "Everyone who does evil hates the light, and will not come into the light for fear that their deeds will be exposed." + Environment.NewLine + "But whoever lives by the truth comes into the light, so that it may be seen plainly that what they have done has been done in the sight of God.";
            servermessages[12].MessageOn = true;
            servermessages[11].Date = DateTime.Parse("03/10/" + DateTime.Now.Year, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name));
            servermessages[11].messagehead = "It's a march:";
            servermessages[11].MessageOn = false;
            servermessages[11].messagebody = "Wahoo!";
            servermessages[0].messagebody = "%user% you are not permitted to post here, please contact a admin or mod for info";
            if (Program.MemoryStorage) 
            {
                botmessages.Add(servermessages);
            }
            Config.write(id, servermessages, typeof(Message[]));
        }
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
        public static Dictionary<ulong, DiscordDmChannel> GetUserDMs() 
        {
            return userDM;
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
        public static Message[] GetMessage(ulong id) 
        { 
           string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + id.ToString(CultureInfo.CurrentCulture) + "/" + config);
            Message[] m = JsonConvert.DeserializeObject<Message[]>(json);
            return m;
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
        public static void setMessage(ulong ID, string head, string body, Message[] m, int index, DateTime date) 
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
            Config.write(ID,m, typeof(Message[]));
        }
        public static async Task MessageSend(Message m, DiscordChannel discordChannel, DiscordClient discord)
        {
            DateTime current_date = DateTime.Now;
                if (!string.IsNullOrEmpty(m.messagehead)) 
                {
                    if (DateTime.Parse(m.Date.Month + "/" + m.Date.Day, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month + "/" + current_date.Day, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name)) && m.MessageOn && Config.bot.useHeuristics)
                    {
                        try
                        {
#pragma warning disable CA1062 // Validate arguments of public methods
                        DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, Heuristics.produceString(m.messagehead + Heuristics.newline + m.messagebody), false, null).ConfigureAwait(false);
#pragma warning restore CA1062 // Validate arguments of public methods
                    }
                        catch (ArgumentNullException ex)
                        {
                            if (Program.verboseoutput) 
                            {
                                Console.Error.WriteLine("MessageSend: Argument Null");
                                Console.Error.WriteLine(ex.Message);
                            }
                        Console.Error.WriteLine("MessageSend: Argument Null");
                        }
                        //await discordMessage.AcknowledgeAsync();
                    }
                    else if (DateTime.Parse(m.Date.Month + "/" + m.Date.Day, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month + "/" + current_date.Day, CultureInfo.GetCultureInfo(CultureInfo.CurrentCulture.Name)) && m.MessageOn)
                    {
                    _ = await discord.SendMessageAsync(discordChannel, m.messagehead + Heuristics.newline + m.messagebody, false, null).ConfigureAwait(false);
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
            File.WriteAllText(Properties.Resources.ResourcesDir + "/" + DM, json);
        }
        public static void load()
        {
            string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + DM);
            userDM = JsonConvert.DeserializeObject<Dictionary<ulong, DiscordDmChannel>>(json);
        }
        public struct Message
        {
            public DateTime Date { get; set; }
            public string messagehead { get; set; }
            public string messagebody { get; set; }
            public bool MessageOn { get; set; }
            public bool Heuristicsused { get; set; }

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
