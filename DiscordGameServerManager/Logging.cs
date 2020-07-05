using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DiscordGameServerManager
{
    class Logging
    {
        private static List<string> queue = new List<string>();
        private static bool logging = false;
        public static void Log(string content) 
        {
            if (!logging) 
            {
                if(queue.Count == 0) 
                {
                    PerformLog(content);
                }
                else 
                {
                    PerformLog(queue[0]);
                    queue.RemoveAt(0);
                }
            }
            else 
            {
                queue.Add(content);
            }
        }
        public static void ExitLog() 
        {
            PerformLog();
        }
        private static void PerformLog() 
        {
            if (Program.log) 
            { 
                while(queue.Count != 0) 
                {
                    using (var logfile = File.Open(Program.logoutput + DateTime.Today.Date.Month + DateTime.Today.Date.Day + DateTime.Today.Date.Year + ".log", FileMode.Open, FileAccess.Write, FileShare.Write))
                    {
                        foreach (var ch in queue[0].ToCharArray())
                        {
                            byte[] data = BitConverter.GetBytes(ch);
                            logfile.Write(data, 0, data.Length - 1);
                        }
                        logfile.Flush();
                    }
                    queue.RemoveAt(0);
                }
            }
        }
        private static void PerformLog(string content) 
        {
            if (Program.log) 
            {
                    if (!string.IsNullOrWhiteSpace(Program.logoutput))
                    {
                        logging = true;
                        if (!File.Exists(Program.logoutput + DateTime.Today.Date.Month + DateTime.Today.Date.Day + DateTime.Today.Date.Year + ".log"))
                        {
                            using (var logfile = File.CreateText(Program.logoutput + DateTime.Today.Date.Month + DateTime.Today.Date.Day + DateTime.Today.Date.Year + ".log"))
                            {
                                logfile.Write(content);
                                logfile.Flush();
                                logfile.Close();
                                logging = false;
                            }
                        }
                        else
                        {
                            using (var logfile = File.Open(Program.logoutput + DateTime.Today.Date.Month + DateTime.Today.Date.Day + DateTime.Today.Date.Year + ".log", FileMode.Open, FileAccess.Write,FileShare.Write))
                            {
                                foreach (var ch in content.ToCharArray())
                                {
                                    byte[] data = BitConverter.GetBytes(ch);
                                    logfile.Write(data, 0, data.Length - 1);
                                    logfile.Flush();
                                }
                                logfile.Close();
                                logging = false;
                            }
                        }
                    }
            }
        }
    }
}
