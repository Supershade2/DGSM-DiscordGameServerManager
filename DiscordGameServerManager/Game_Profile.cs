using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;
using System.Security.Cryptography;
namespace DiscordGameServerManager
{
    class Game_Profile
    {
        private const string config = "gameprofile.json";
        private const string key = "";
        public static profile _profile;
        static Game_Profile()
        {
            
            if (!File.Exists(Properties.Resources.ResourcesDir + "/" + Config.bot.game + "/" + config))
            {
                File.Create(Properties.Resources.ResourcesDir + "/" + Config.bot.game + "/" + config).Close();
                _profile = new profile();
                _profile.game = Config.bot.game;
                _profile.user_and_pass = new Dictionary<string,string>();
                _profile.user_and_pass.Add("anonymous","123");
                string json = JsonConvert.SerializeObject(_profile, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" +Config.bot.game+ "/" + config, json);
                //byte[] json_data = Encoding.ASCII.GetBytes(json);
                //File.Open(Properties.Resources.ResourcesDir + "/" + config, FileMode.Open, FileAccess.Write, FileShare.Write).Write(json_data,0,json_data.Length-1);
            }
            else
            {
                string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + Config.bot.game + "/" + config);
                _profile = JsonConvert.DeserializeObject<profile>(json);
            }
        }
    }
    public struct profile
    {
        public bool Is_Steam { get; set; }
        public bool useSSH { get; set; }
        public string game { get; set; }
        public string file_location { get; set; }
        public string mod_dir { get; set; }
        public string rcon_address { get; set; }
        public int RCONPort { get; set; }
        public string RCONPass { get; set; }
        public string[] rcon_commands { get; set; }
        //In the future will be encrypted into the file and then stored in the dictionary
        public Dictionary<string, string> user_and_pass { get; set; }
        public long steam_app_id { get; set; }
        public string steam_game_args_script { get; set; }
        public string steam_install_dir { get; set; }
        public string start_command { get; set; }
        public string stop_command { get; set; }
        /* Not currently implemented, currently process is killed and restarted
        public string restart_command { get; set; }
        */
    }
}
