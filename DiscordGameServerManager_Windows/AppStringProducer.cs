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
            if (OS_Info.GetOSPlatform() == OSPlatform.Windows)
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
            string f;
            switch (needs_extension)
            {
                case true:
                    if (OS_Info.GetOSPlatform() == OSPlatform.Windows)
                    {
                        return System.IO.Path.GetFileNameWithoutExtension(app) + ".exe";
                    }
                    else if (OS_Info.GetOSPlatform() == OSPlatform.Linux || OS_Info.GetOSPlatform() == OSPlatform.OSX) 
                    {
                        return System.IO.Path.GetFileNameWithoutExtension(app) + ".deb";
                    }
                    f = System.IO.Path.GetFileNameWithoutExtension(app) + Details.d.default_extension;
                    break;
                default:
                    f = GetSystemCompatibleString(app);
                    break;
            }
            return f;
        }
        public static string GetSystemCompatibleString(string app, bool needs_extension, bool isScript) 
        {
            string f = "";
            switch (needs_extension)
            {
                case true:
                    switch (isScript)
                    {
                        case true:
                            if (OS_Info.GetOSPlatform() == OSPlatform.Windows)
                            {
                                return System.IO.Path.GetFileNameWithoutExtension(app) + ".bat";
                            }
                            else if (OS_Info.GetOSPlatform() == OSPlatform.Linux || OS_Info.GetOSPlatform() == OSPlatform.OSX)
                            {
                                return System.IO.Path.GetFileNameWithoutExtension(app) + ".sh";
                            }
                            break;
                        default:
                            f = GetSystemCompatibleString(app, needs_extension);
                            break;
                    }
                    break;
                default:
                    f = GetSystemCompatibleString(app);
                    break;
            }
            return f;
        }
    }
}
