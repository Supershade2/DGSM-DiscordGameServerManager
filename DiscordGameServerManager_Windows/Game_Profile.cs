using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace DiscordGameServerManager_Windows
{
    class Game_Profile
    {
        private const string dir = "Resources";
        private const string config = "gameprofile.json";

        public static profile _profile;
        static Game_Profile()
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (!File.Exists(dir + "/" + Config.bot.game + "/" + config))
            {
                File.Create(dir + "/" + Config.bot.game + "/" + config).Close();
                _profile = new profile();
                string json = JsonConvert.SerializeObject(_profile, Formatting.Indented);
                File.WriteAllText(dir + "/" +Config.bot.game+ "/" + config, json);
                //byte[] json_data = Encoding.ASCII.GetBytes(json);
                //File.Open(dir + "/" + config, FileMode.Open, FileAccess.Write, FileShare.Write).Write(json_data,0,json_data.Length-1);
            }
            else
            {
                string json = File.ReadAllText(dir + "/" + Config.bot.game + "/" + config);
                _profile = JsonConvert.DeserializeObject<profile>(json);
            }
        }
    }
    public struct profile
    {
        public bool Is_Steam { get; set; }
        public string file_location { get; set; }
        public string rcon_address { get; set; }
        public int rcon_port { get; set; }
        public string rcon_pass { get; set; }
        public string[] rcon_commands { get; set; }
        public long steam_app_id { get; set; }
        public string steam_game_args { get; set; }
        public string steam_install_dir { get; set; }
        public string start_command { get; set; }
        /* Not currently implemented, currently thread is just aborted
        public string stop_command { get; set; }
        public string restart_command { get; set; }
        */
    }
}
