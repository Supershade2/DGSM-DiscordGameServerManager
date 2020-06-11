using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.IO.IsolatedStorage;
using System.Runtime.InteropServices;

namespace DiscordGameServerManager
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable
    public class Details
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        private const string config = "details.json";
        public static details d = new details();
        private static System.Globalization.CultureInfo cinfo = System.Globalization.CultureInfo.GetCultureInfo(System.Globalization.CultureInfo.CurrentCulture.Name);
        static Details()
        { 
            d.culture_name = cinfo.Name;
            if (!Directory.Exists(Properties.Resources.ResourcesDir))
                Directory.CreateDirectory(Properties.Resources.ResourcesDir);

            if (!File.Exists(Properties.Resources.ResourcesDir + "/" + config))
            {
                d.default_extension = AppStringProducer.GetSystemCompatibleString("", true);
                d.first_run = true;
                d.platform = OSInfo.GetOSPlatform();
                File.Create(Properties.Resources.ResourcesDir + "/" + config).Close();
                string json = JsonConvert.SerializeObject(d, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + config, json);
            }
            else
            {
                load();
            }
        }
        public static void load()
        {
            FileInfo f_info = new FileInfo(Properties.Resources.ResourcesDir + "/" + config);
            if (f_info.Length > 0) 
            {
                string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + config);
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
            using (var resource = File.Open(Properties.Resources.ResourcesDir + "/" + config, FileMode.Truncate, FileAccess.Write, FileShare.Write))
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
        public OSPlatform platform { get; set; }
        public string culture_name { get; set; }
        public string default_extension { get; set; }
    }
}
