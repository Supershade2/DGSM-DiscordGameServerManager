using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DiscordGameServerManager_Windows
{
    class AppStringProducer
    {
        public static string GetSystemCompatibleString(string app) 
        {
            if (OS_Info.platform == OSPlatform.Windows)
            {
                return app;
            }
            else
            {
                return System.IO.Path.GetFileNameWithoutExtension(app);
            }
        }
        public static string GetSystemCompatibleString(string app, bool needs_extension) 
        {
            switch (needs_extension)
            {
                case true:

                    break;
                default:
                    break;
            }
            return "";
        }
    }
}
