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
            d.default_extension = AppStringProducer.GetSystemCompatibleString("", true);
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
            FileInfo f_info = new FileInfo(dir + "/" + config);
            if (f_info.Length > 0) 
            {
                string json = File.ReadAllText(dir + "/" + config);
                d = JsonConvert.DeserializeObject<details>(json);
            }
            else 
            {
                write();
            }
        }
        public static void write() 
        {
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);
            File.WriteAllText(dir + "/" + config, json);
        }
    }
    public struct details 
    {
        public int user_count { get; set; }
        public string culture_name { get; set; }
        public string default_extension { get; set; }
    }
}
