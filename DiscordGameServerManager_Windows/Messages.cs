using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
namespace DiscordGameServerManager_Windows
{
    public class Messages
    {
        private const string dir = "Resources";
        private const string config = "DMs.json";
        public static Dictionary<string, DiscordDmChannel> userDM = new Dictionary<string, DiscordDmChannel>();
        public static void setMessage(string str, message[] m, int index) 
        {
            if (Config.bot.useHeuristics)
            {
                str = Heuristics.produceString(str);
                m[index].message_body = str;
            }
            else 
            {
                m[index].message_body = str;
            }
            Config.write();
        }
        public static void setMessage(string str, message[] m, int index, DateTime date) 
        {
            if (Config.bot.useHeuristics)
            {
                str = Heuristics.produceString(str);
                m[index].message_body = str;
                m[index].Date = date;
            }
            else 
            {
                m[index].message_body = str;
                m[index].Date = date;
            }
            Config.write();
        }
        public static void setMessage(string head, string body, message[] m, int index) 
        {
            if (Config.bot.useHeuristics) 
            {
                head = Heuristics.produceString(head);
                body = Heuristics.produceString(body);
                m[index].message_head = head;
                m[index].message_body = body;
            }
            else
            {
                m[index].message_head = head;
                m[index].message_body = body;
            }
            Config.write();
        }
        public static void setMessage(string head, string body, message[] m, int index, DateTime date) 
        {
            if (Config.bot.useHeuristics) 
            {
                head = Heuristics.produceString(head);
                body = Heuristics.produceString(body);
                m[index].message_head = head;
                m[index].message_body = body;
                m[index].Date = date;
            }
            else
            {
                m[index].message_head = head;
                m[index].message_body = body;
                m[index].Date = date;
            }
            Config.write();
        }
        public static async Task message_send(message m, DiscordChannel discordChannel, DiscordClient discord)
        {
            DateTime current_date = DateTime.Now;
            try
            {
                if (DateTime.Parse(m.Date.Month+"/"+m.Date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month+"/"+current_date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) && m.MessageOn && Config.bot.useHeuristics)
                {
                    DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, Heuristics.produceString(m.message_head + Heuristics.newline + m.message_body), false, null);
                    await discordMessage.AcknowledgeAsync();
                }
                else if (DateTime.Parse(m.Date.Month + "/" + m.Date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) == DateTime.Parse(current_date.Month + "/" + current_date.Day, System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name)) && m.MessageOn) 
                {
                    DiscordMessage discordMessage = await discord.SendMessageAsync(discordChannel, m.message_head + Heuristics.newline + m.message_body, false, null);
                    await discordMessage.AcknowledgeAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Message most likely has null or undefined properties, you can ignore this error for message at index 0");
                Console.WriteLine(e.Message);
            }
        }
        public static void write()
        {
            string json = JsonConvert.SerializeObject(userDM, Formatting.Indented);
            File.WriteAllText(dir + "/" + config, json);
        }
        public static void load()
        {
            string json = File.ReadAllText(dir + "/" + config);
            userDM = JsonConvert.DeserializeObject<Dictionary<string, DiscordDmChannel>>(json);
        }
        public struct message
        {
            public DateTime Date { get; set; }
            public string message_head { get; set; }
            public string message_body { get; set; }
            public bool MessageOn { get; set; }
        }
    }
}
