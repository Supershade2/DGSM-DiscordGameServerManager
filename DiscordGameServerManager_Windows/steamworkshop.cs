using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Security;

namespace DiscordGameServerManager_Windows
{
    class steamworkshop
    {
        private static readonly HttpClient client = new HttpClient();
        private const string Default_appID = "346110";
        //private const string build_workshop = "workshop_build_item ";
        private const string download_workshop = "workshop_download_item ";
        //private static SteamWebInterfaceFactory InterfaceFactory = new SteamWebInterfaceFactory(Config.bot.wsapikey);
        //private static ISteamWebAPIUtil WebAPIUtil;
        private static async Task<string> get_CollectionDetailsAsync() 
        {
            //Steam.Models.SteamServerInfo serverInfo;
            //var steamInterface = InterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
            var values = new Dictionary<string, string>
            {
                { "key", Config.bot.wsapikey},
                { "collectioncount", "1"},
                { "publishedfileids[0]", Config.bot.wscollectionid}
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetCollectionDetails/v1", content);
            var response_string = await response.Content.ReadAsStringAsync();
            return response_string;
        }
        public static async Task<string[]> get_PublishedFileDetails(string[] files) 
        {
            string[] details = new string[files.Length];
            for(int index=0; index<files.Length; index++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", Config.bot.wsapikey},
                        { "itemcount", "1"},
                        { "publishedfileids[0]", files[index]}
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", content);
                details[index] = await response.Content.ReadAsStringAsync();
            }
            return details;
        }
        public static async Task<string[]> get_UGCFileDetails(string[] ugc_ids) 
        {
            string[] details = new string[ugc_ids.Length];
            for (int i = 0; i < ugc_ids.Length; i++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", Config.bot.wsapikey},
                        { "ugcid", ugc_ids[i]},
                        { "appid", Default_appID}
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await client.GetAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/"+content);
                details[i] = await response.Content.ReadAsStringAsync();
            }
            return details;
        }
        public static async Task<string[]> get_UGCFileDetails(string[] ugc_ids, string app_id)
        {
            string[] details = new string[ugc_ids.Length];
            for (int i = 0; i < ugc_ids.Length; i++)
            {
                var values = new Dictionary<string, string>
                    {
                        { "key", Config.bot.wsapikey},
                        { "ugcid", ugc_ids[i]},
                        { "appid", app_id}
                    };
                var content = new FormUrlEncodedContent(values);
                var response = await client.GetAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/"+content);
                details[i] = await response.Content.ReadAsStringAsync();
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
                                keyfound = item.ToLower() == trigger.ToLower() ? true : false;
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
                                keyfound = item.ToLower() == trigger.ToLower() ? true : false;
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
                                if (item.ToLower().Contains(".zip"))
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
        public static async Task<bool> GetFiles() 
        {
            //client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            try
            {
                string[] collection = parse_Details(await get_CollectionDetailsAsync());
                string[] file_details = await get_PublishedFileDetails(collection);
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
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Method: GetFiles"+Environment.NewLine+ex.Message);
                return false;
            }
        }
        public static async void download(System.Diagnostics.Process p, System.Diagnostics.ProcessStartInfo psi)
        {
            string[] collection = parse_Details(await get_CollectionDetailsAsync());
            for (int i = 0; i < collection.Length; i++) 
            {
                psi.Arguments = "+login anonymous "+download_workshop + Default_appID + " " + collection[i]+" +quit";
                p.StartInfo = psi;
                p.Start();
                p.WaitForExit(30000);
            }
        }
    }
}
