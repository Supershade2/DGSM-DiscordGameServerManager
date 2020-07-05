using System;
using DSharpPlus;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.IO.Compression;
using System.Globalization;
using System.Net.Http;
using System.IO;
using Microsoft.EntityFrameworkCore.Internal;
using System.Text.RegularExpressions;

namespace DiscordGameServerManager
{
    class Program
    {
        public static Details _details = new Details();
        public static bool log = false;
        public static string logoutput = "";
        public static bool verboseoutput = false;
        public static bool MemoryStorage = false;
        public static bool DisableTrustManagement = false;
        //public static DiscordFunctions functions = new DiscordFunctions();
        static void Main(string[] args)
        {
            if (args.Length > 0) 
            {
                for (int i = 0; i < args.Length; i++) 
                {
                    args[i] = args[i].ToLower(CultureInfo.CurrentCulture);
                    switch (args[i]) 
                    {
                        case "--log":
                            log = true;
                            string pattern = @"[/]\w+";
                            if (i + 1 < args.Length - 1)
                            {
                                Match m = Regex.Match(args[i + 1], pattern);
                                logoutput = m.Success == true ? args[i + 1] : "";
                            }
                            break;
                        case "--verbose":
                            verboseoutput = true;
                            break;
                        case "-v":
                            verboseoutput = true;
                            break;
                        case "--memory":
                            MemoryStorage = true;
                            break;
                        case "-m":
                            MemoryStorage = true;
                            break;
                        case "--notrust":
                            DisableTrustManagement = true;
                            break;

                    }

                }
                //verboseoutput = args.Contains("--verbose") || args.Contains("-v");
            }
            DiscordFunctions.Connect();
            DiscordFunctions.MainDiscord();
            Console.CancelKeyPress += (sender, e) => 
            {
                DiscordFunctions.Disconnect();
                Logging.ExitLog();
            };
        }  
    }
}