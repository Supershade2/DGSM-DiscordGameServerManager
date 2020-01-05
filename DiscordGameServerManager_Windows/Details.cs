using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
namespace DiscordGameServerManager_Windows
{
    class Details
    {
        private const string dir = "Resources";
        private const string config = "details.json";
        public static details d = new details();
        private static System.Globalization.CultureInfo cinfo = System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name);
        static Details()
        { 
            d.culture_name = cinfo.Name;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(dir + "/" + config))
            {
                File.Create(dir + "/" + config).Close();
                string json = JsonConvert.SerializeObject(d, Formatting.Indented);
                File.WriteAllText(dir + "/" + config, json);
            }
            else
            {
                load();
            }
        }
        public static void load()
        {
            string json = File.ReadAllText(dir + "/" + config);
            d = JsonConvert.DeserializeObject<details>(json);
        }
    }
    public struct details 
    {
        public int user_count { get; set; }
        public string culture_name { get; set; }
    }
}
