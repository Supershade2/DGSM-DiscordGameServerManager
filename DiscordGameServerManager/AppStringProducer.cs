using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace DiscordGameServerManager
{
    class AppStringProducer
    {
        public static string GetSystemCompatibleString(string app) 
        {
            if (OSInfo.GetOSPlatform() == OSPlatform.Windows)
            {
                if (app.Contains(".exe",StringComparison.CurrentCultureIgnoreCase)) 
                {
                    return app;
                }
                else 
                {
                    app += ".exe";
                    return app;
                }
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
                    if (OSInfo.GetOSPlatform() == OSPlatform.Windows)
                    {
                        return System.IO.Path.ChangeExtension(app, ".exe"); //System.IO.Path.GetFileNameWithoutExtension(app) + ".exe";
                    }
                    else if (OSInfo.GetOSPlatform() == OSPlatform.Linux || OSInfo.GetOSPlatform() == OSPlatform.OSX) 
                    {
                        return System.IO.Path.ChangeExtension(app, ".deb");//System.IO.Path.GetFileNameWithoutExtension(app) + ".deb";
                    }
                    f = System.IO.Path.ChangeExtension(app, Details.d.default_extension);//System.IO.Path.GetFileNameWithoutExtension(app) + Details.d.default_extension;
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
                            if (OSInfo.GetOSPlatform() == OSPlatform.Windows)
                            {
                                return System.IO.Path.ChangeExtension(app, ".bat");//System.IO.Path.GetFileNameWithoutExtension(app) + ".bat";
                            }
                            else if (OSInfo.GetOSPlatform() == OSPlatform.Linux || OSInfo.GetOSPlatform() == OSPlatform.OSX)
                            {
                                return System.IO.Path.ChangeExtension(app, ".sh"); //System.IO.Path.GetFileNameWithoutExtension(app) + ".sh";
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
