using System;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using SteamKit2;
using SteamWebAPI2.Utilities;
using SteamWebAPI2.Interfaces;

namespace DiscordGameServerManager_Windows
{
    class steamworkshop
    {
        private static readonly HttpClient client = new HttpClient();
        private const string Default_appID = "376030";
        //private static SteamWebInterfaceFactory InterfaceFactory = new SteamWebInterfaceFactory(Config.bot.wsapikey);
        //private static ISteamWebAPIUtil WebAPIUtil;
        public static async Task<string> get_CollectionDetailsAsync() 
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
                var response = await client.GetAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", content);
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
                var response = await client.GetAsync("https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/", content);
                details[i] = await response.Content.ReadAsStringAsync();
            }
            return details;
        }
        private static async Task<string[]> parse_Details(string details) 
        {
            string[] items = new string[2];
            return items;
        }
        public static async Task<bool> GetFiles(string files) 
        {
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return false;
        }
    }
}
