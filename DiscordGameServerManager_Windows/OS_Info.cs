using System.Runtime.InteropServices;
/*!
 *  \file OS_Info.cs
 *  File that contains the code for getting the current OS Platform
 *  This file contains the source code for retrieving the current platform the application is running on
 */
namespace DiscordGameServerManager_Windows
{
    public class OS_Info
    {
        //! OSPlatform info gets set by 
        private static OSPlatform platform;
        //@{
        /*! Returns current platform application is running on and sets platform to it */
        public static OSPlatform GetOSPlatform() 
        {
            if (platform != null)
            {
                return platform;
            }
            else 
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
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    platform = OSPlatform.OSX;
                    return OSPlatform.OSX;
                }
                platform = OSPlatform.Create("Custom");
                return platform;
            }
        }
        //@}
    }
}
