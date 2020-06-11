using System.Runtime.InteropServices;
/*!
 *  \file OS_Info.cs
 *  File that contains the code for getting the current OS Platform
 *  This file contains the source code for retrieving the current platform the application is running on
 */
namespace DiscordGameServerManager
{
    public static class OSInfo
    {
        //! private platform variable updated by first use of GetOSPlatform 
        private static OSPlatform platform;
        //@{
        /*! Returns current platform application is running on and sets platform to it*/
        public static OSPlatform GetOSPlatform() 
        {
            //@{
            //! return platform if it's value is not null
            if (platform != null)
            {
                return platform;
            }
            //@}
            else 
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    platform = OSPlatform.Windows;
                    return platform;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    platform = OSPlatform.Linux;
                    return platform;
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    platform = OSPlatform.OSX;
                    return platform;
                }
                platform = OSPlatform.Create("Custom");
                return platform;
            }
        }
        //@}
    }
}
