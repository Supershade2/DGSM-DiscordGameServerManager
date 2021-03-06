﻿using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
/*!
*  \file steamworkshop.cs
*  File that contains the code for steamworkshop actions
*  This file contains the source code for how steamcmd and workshop collection api will be utilized
*/
namespace DiscordGameServerManager
{
    class steamworkshop
    {
        private static readonly HttpClient client = new HttpClient();
        private const string Default_appID = "346110";
        //private const string build_workshop = "workshop_build_item ";
        private const string download_workshop = "workshop_download_item ";
        //private static SteamWebInterfaceFactory InterfaceFactory = new SteamWebInterfaceFactory(Config.bot.wsapikey);
        //private static ISteamWebAPIUtil WebAPIUtil;
        private static async Task<string> get_CollectionDetailsAsync(Globalvars g) 
        {
            //Steam.Models.SteamServerInfo serverInfo;
            //var steamInterface = InterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
            var values = new Dictionary<string, string>
            {
                { "key", g.wsapikey},
                { "collectioncount", "1"},
                { "publishedfileids[0]", g.wscollectionid}
            };
            var content = new FormUrlEncodedContent(values);
            var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetCollectionDetails/v1");
            var response = await client.PostAsync(url, content).ConfigureAwait(false);
            //var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetCollectionDetails/v1", content);
            var response_string = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            content.Dispose();
            return response_string;
        }
        //Installs app using steamcmd into the specified directory
        public static async Task<bool> InstallApp(string dir) 
        {
            return true;
        }
        //Installs app using steamcmd installing an app other than Ark into the specified directory
        public static async Task<bool> InstallApp(string appid,string dir) 
        {
            return true;
        }
        public static async Task<string[]> get_PublishedFileDetails(string[] files, Globalvars g) 
        {
            string[] details = new string[files.Length];
            for(int index=0; index<files.Length; index++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", g.wsapikey},
                        { "itemcount", "1"},
                        { "publishedfileids[0]", files[index]}
                    };
                var content = new FormUrlEncodedContent(values);
                var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/");
                var response = await client.PostAsync(url, content).ConfigureAwait(false);
                //var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", content).ConfigureAwait(false);
                details[index] = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
                content.Dispose();
            }
            if (!File.Exists("./Details.txt"))
            {
                for (int i = 0; i < details.Length; i++)
                {
                    Console.WriteLine(details[i]);
                    File.AppendAllText("./Details.txt", string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}. {1}" + Environment.NewLine, i + 1, details[i]));
                }
            }
            return details;
        }
        public static async Task<string[]> get_UGCFileDetails(string[] ugc_ids,Globalvars g) 
        {
            string[] details = new string[ugc_ids.Length];
            for (int i = 0; i < ugc_ids.Length; i++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", g.wsapikey},
                        { "ugcid", ugc_ids[i]},
                        { "appid", Default_appID}
                    };
                var content = new FormUrlEncodedContent(values);
                var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/" + content);
                var response = await client.GetAsync(url).ConfigureAwait(false);
                details[i] = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                content.Dispose();
            }
            return details;
        }
        public static async Task<string[]> get_UGCFileDetails(string[] ugc_ids, string app_id, Globalvars g)
        {
            string[] details = new string[ugc_ids.Length];
            for (int i = 0; i < ugc_ids.Length; i++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", g.wsapikey},
                        { "ugcid", ugc_ids[i]},
                        { "appid", app_id}
                    };
                var content = new FormUrlEncodedContent(values);
                var url = new Uri("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/" + content);
                var response = await client.GetAsync(url).ConfigureAwait(false);
                //var response = await client.GetAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/"+content).ConfigureAwait(false);
                details[i] = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                content.Dispose();
            }
            return details;
        }
        private static string[] parse_Details(string details)
        {
            List<string> items = new List<string>();
            char[] ca = details.ToCharArray();
            bool Is_open = false;
            bool keyfound = false;
            string trigger = "publishedfileid";
            string item = "";
            for (int i = 0; i < ca.Length; i++)
            {
                if (ca[i] == '"')
                {
                    Is_open = Is_open == true ? false : true;
                    if (!keyfound)
                    {
                        switch (Is_open)
                        {
                            case true:
                                break;
                            default:
                                keyfound = item.ToLower(System.Globalization.CultureInfo.CurrentCulture) == trigger.ToLower(System.Globalization.CultureInfo.CurrentCulture) ? true : false;
                                item = "";
                                break;
                        }
                    }
                    else
                    {
                        switch (Is_open)
                        {
                            case true:
                                break;
                            default:
                                items.Add(item);
                                item = "";
                                keyfound = false;
                                break;
                        }
                    }
                }
                else if (Is_open)
                {
                    item += ca[i];
                }
            }
            return items.ToArray();
        }
        private static string[] get_Downloads(string details)
        {
            List<string> items = new List<string>();
            char[] ca = details.ToCharArray();
            bool Is_open = false;
            bool keyfound = false;
            string trigger = "file_url";
            string item = "";
            for (int i = 0; i < ca.Length; i++)
            {
                if (ca[i] == '"')
                {
                    Is_open = Is_open == true ? false : true;
                    if (!keyfound)
                    {
                        switch (Is_open)
                        {
                            case true:
                                break;
                            default:
                                keyfound = item.ToLower(System.Globalization.CultureInfo.CurrentCulture) == trigger.ToLower(System.Globalization.CultureInfo.CurrentCulture) ? true : false;
                                item = "";
                                break;
                        }
                    }
                    else
                    {
                        switch (Is_open)
                        {
                            case true:
                                break;
                            default:
                                if (item.ToLower(System.Globalization.CultureInfo.CurrentCulture).Contains(".zip",StringComparison.CurrentCulture))
                                {
                                    items.Add(item);
                                }
                                item = "";
                                keyfound = false;
                                break;
                        }
                    }
                }
                else if (Is_open)
                {
                    item += ca[i];
                }
            }
            return items.ToArray();
        }
        public static async Task<bool> GetFiles(Globalvars g) 
        {
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                string[] collection = parse_Details(await get_CollectionDetailsAsync(g).ConfigureAwait(false));
                string[] file_details = await get_PublishedFileDetails(collection,g).ConfigureAwait(false);
                string combined = "";
                for (int i = 0; i < file_details.Length; i++)
                {
                    combined += file_details[i];
                }
                string[] downloads = get_Downloads(combined);
                for (int i = 0; i < downloads.Length; i++)
                {
                    string id = collection.Length == downloads.Length + 1 ? collection[i + 1] : collection[i];
                    if (!Directory.Exists("./mods"))
                    {
                        Directory.CreateDirectory("./mods");
                    }
                }
                return true;
            }
            catch (AggregateException ex)
            {
                await Console.Out.WriteLineAsync("Method: GetFiles"+Environment.NewLine+ex.Message).ConfigureAwait(false);
                return false;
            }
        }
        public static async void Download(System.Diagnostics.Process p, System.Diagnostics.ProcessStartInfo psi, Globalvars g)
        {
            if (!Config.bot.useSSH) 
            {
                string[] collection = parse_Details(await get_CollectionDetailsAsync(g).ConfigureAwait(false));
                if (p == null)
                {
                    await Console.Out.WriteLineAsync(Properties.Resources.NullProcessVariable).ConfigureAwait(false);
                }
                else
                {
                    if (string.IsNullOrEmpty(psi.Arguments))
                    {
                        for (int i = 0; i < collection.Length; i++)
                        {
                            psi.Arguments = "+login anonymous " + download_workshop + Default_appID + " " + collection[i] + " +quit";
                            p.StartInfo = psi;
                            p.Start();
                        }
                    }
                    else
                    {
                        p.StartInfo = psi;
                        p.Start();
                    }
                }
            }
        }
        public static async void Download(System.Diagnostics.Process p, Globalvars g) 
        {
            string[] collection = parse_Details(await get_CollectionDetailsAsync(g).ConfigureAwait(false));
            for (int i = 0; i < collection.Length; i++)
            {
                System.Diagnostics.ProcessStartInfo psi;
                FillSteamCMDArgs(out psi, collection[i]);
                psi.WorkingDirectory = Directory.GetCurrentDirectory() + "/steamcmd";
                p.StartInfo = psi;
                p.Start();
            }
        }
        public static void Download() 
        {
            if (string.IsNullOrEmpty(Config.bot.steamcmddir))
            {
                System.Diagnostics.Process p = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo();
                SetSteamCMDProcessFilename(out info, info);
                info.WorkingDirectory = Directory.GetCurrentDirectory() + "/steamcmd";
                p.StartInfo = info;
                p.Start();
                p.Dispose();
            }
        }
        public static void SetSteamCMDProcessFilename(out System.Diagnostics.ProcessStartInfo psi, System.Diagnostics.ProcessStartInfo psinfo) 
        {
            psi = psinfo;
            psi.FileName = AppStringProducer.GetSystemCompatibleString("steamcmd");
        }
        public static void FillSteamCMDArgs(out System.Diagnostics.ProcessStartInfo psi, string collectionitem) 
        {
            psi = new System.Diagnostics.ProcessStartInfo(AppStringProducer.GetSystemCompatibleString("steamcmd.exe"),"+login anonymous " + download_workshop + Default_appID + " " + collectionitem + " +quit");
            psi.UseShellExecute = false;
            if (string.IsNullOrEmpty(Config.bot.steamcmddir))
            {
                    psi.WorkingDirectory = Directory.GetCurrentDirectory() + "/steamcmd";
            }
            else 
            {
                psi.WorkingDirectory = Config.bot.steamcmddir;
            }
        }
    }
}
