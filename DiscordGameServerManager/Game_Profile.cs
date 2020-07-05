﻿using System;
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
            
            if (!File.Exists(Properties.Resources.ResourcesDir + "/" + GlobalServerConfig.gvars.id + "/" + GlobalServerConfig.gvars.game + "/" + config))
            {
                File.Create(Properties.Resources.ResourcesDir + "/" + GlobalServerConfig.gvars.id + "/" + GlobalServerConfig.gvars.game + "/" + config).Close();
                _profile = new profile();
                _profile.game = GlobalServerConfig.gvars.game;
                _profile.user_and_pass = new Dictionary<string,string>();
                _profile.user_and_pass.Add("anonymous","123");
                string json = JsonConvert.SerializeObject(_profile, Formatting.Indented);
                File.WriteAllText(Properties.Resources.ResourcesDir + "/" + GlobalServerConfig.gvars.game+ "/" + config, json);
                //byte[] json_data = Encoding.ASCII.GetBytes(json);
                //File.Open(Properties.Resources.ResourcesDir + "/" + config, FileMode.Open, FileAccess.Write, FileShare.Write).Write(json_data,0,json_data.Length-1);
            }
            else
            {
                string json = File.ReadAllText(Properties.Resources.ResourcesDir + "/" + GlobalServerConfig.gvars.game + "/" + config);
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
        //Steam user and pass for games which require a subscription to the game on an account
        public Dictionary<string, string> user_and_pass { get; set; }
        public long steam_app_id { get; set; }
        public string steam_game_args_script_data { get; set; }
        public string steam_install_dir { get; set; }
        public string start_command { get; set; }
        public string stop_command { get; set; }


        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + Is_Steam.GetHashCode();
            hash = (hash * 3) + useSSH.GetHashCode();
            hash = (hash * 3) + game.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + file_location.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + mod_dir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + rcon_address.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + RCONPort.GetHashCode();
            hash = (hash * 3) + RCONPass.GetHashCode(StringComparison.CurrentCulture);
            foreach (var item in rcon_commands)
            {
                hash = (hash * 3) + item.GetHashCode(StringComparison.CurrentCulture);
            }
            foreach (var item in user_and_pass.Keys)
            {
                hash = (hash * 3) + item.GetHashCode(StringComparison.CurrentCulture);
            }
            foreach (var item in user_and_pass.Values)
            {
                hash = (hash * 3) + item.GetHashCode(StringComparison.CurrentCulture);
            }
            hash = (hash * 3) + steam_app_id.GetHashCode();
            hash = (hash * 3) + steam_game_args_script_data.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + steam_install_dir.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + start_command.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + stop_command.GetHashCode(StringComparison.CurrentCulture);
            return hash;
        }

        public static bool operator ==(profile left, profile right)
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
            return left.Is_Steam == right.Is_Steam && left.useSSH == right.useSSH && left.game == right.game && left.file_location == right.file_location && left.mod_dir == right.mod_dir && left.rcon_address == right.rcon_address && left.RCONPort == right.RCONPort && left.RCONPass == right.RCONPass &&  left.rcon_commands == right.rcon_commands && left.user_and_pass == right.user_and_pass && left.steam_app_id == right.steam_app_id && left.steam_game_args_script_data == right.steam_game_args_script_data && left.steam_install_dir == right.steam_install_dir && left.start_command == right.start_command && left.stop_command == right.stop_command;
        }

        public static bool operator !=(profile left, profile right)
        {
            return !(left == right);
        }

        public bool Equals(profile other)
        {
            return this == other;
        }
        /* Not currently implemented, currently process is killed and restarted
public string restart_command { get; set; }
*/
    }
}
