using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace DiscordGameServerManager_Windows
{
    class ModuleHandler
    {
        public static Process process;
        public static ProcessModuleCollection ModuleCollection;
        public static NamedPipeServerStream[] namedPipeServerStreams;
        public static void InitializePipes() 
        {
            namedPipeServerStreams = new NamedPipeServerStream[Modules.module_Collection.modulelist.Count];
        }
        public static void InitializePipes(string[] names) 
        { 
            
        }
    }
}
