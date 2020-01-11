﻿using System.Runtime.InteropServices;

namespace DiscordGameServerManager_Windows
{
    public class OS_Info
    {
        public static OSPlatform platform;
        public static OSPlatform GetOSPlatform() 
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                platform = OSPlatform.Windows;
                return OSPlatform.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) 
            {
                platform = OSPlatform.Linux;
                return OSPlatform.Linux;
            }
            platform = OSPlatform.OSX;
            return OSPlatform.OSX;
        }
    }
}
