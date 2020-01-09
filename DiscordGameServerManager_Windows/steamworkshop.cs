using System;
using System.Net.Http;
using System.Collections.Generic;
using SteamWebAPI2;
using System.Text;

namespace DiscordGameServerManager_Windows
{
    class steamworkshop
    {
        private static readonly HttpClient client = new HttpClient();
        public static async System.Threading.Tasks.Task<string> get_CollectionDetailsAsync() 
        {
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
        public static async System.Threading.Tasks.Task<string> parse_Details(string details) 
        {
            return "";
        }
    }
}
