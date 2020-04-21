using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;

namespace DiscordGameServerManager
{
    public class Details
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
                d.default_extension = AppStringProducer.GetSystemCompatibleString("", true);
                d.first_run = true;
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
                if (d.first_run) 
                {
                    d.first_run = false;
                    write();
                }
            }
            else 
            {
                write();
            }
        }
        public static void write() 
        {
            string json = JsonConvert.SerializeObject(d, Formatting.Indented);
            byte[] json_data = Encoding.ASCII.GetBytes(json);
            using (var resource = File.Open(dir + "/" + config, FileMode.Truncate, FileAccess.Write, FileShare.Write))
            {
                resource.Write(json_data, 0, json_data.Length - 1);
                resource.Flush();
                resource.Dispose();
            }
        }
    }
    public struct details 
    {
        public int user_count { get; set; } 
        public bool first_run { get; set; }
        public string culture_name { get; set; }
        public string default_extension { get; set; }
    }
}
