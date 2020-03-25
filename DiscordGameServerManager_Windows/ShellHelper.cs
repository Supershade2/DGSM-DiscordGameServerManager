using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Globalization;

namespace DiscordGameServerManager_Windows
{
    public static class ShellHelper
    {
        public static string Bash(this string cmd)
        {
            string result;
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            using (var stream = new MemoryStream())
            {
                process.StandardOutput.BaseStream.CopyToAsync(stream).ConfigureAwait(false).GetAwaiter().GetResult();
                result = stream.Read(stream.ToArray(),0,stream.ToArray().Length-1).ToString(CultureInfo.CurrentCulture);
            }
            process.WaitForExit();
            return result;
        }
    }
}
