using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace DiscordGameServerManager_Windows
{
    class ModuleHandler
    {
        public static Process process = new Process();
        public static ProcessStartInfo startInfo;
        public static ProcessModule ProcessModule;
        public static ProcessModuleCollection ModuleCollection;
        public static NamedPipeServerStream[] namedPipeServerStreams;
        static Random random = new Random();
        static string dir;
        static List<string> pipenames = new List<string>();
        static int current_pipe = 0;
        public ModuleHandler(string d)
        {
            dir = d;
        }
        public static void InitializePipes() 
        {
            namedPipeServerStreams = new NamedPipeServerStream[Modules.module_Collection.modulelist.Count];
            string name = System.Reflection.Assembly.GetEntryAssembly().FullName;
            char[] name_parts = name.ToCharArray();
            string pipename = "";
            char last = ' ';
            for (int i = 0; i < namedPipeServerStreams.Length; i++)
            {

                foreach (char c in name_parts)
                {
                    pipename += (char)random.Next(last + c);
                    last = c;
                }
                if (pipenames.Count > 0) 
                {
                    foreach (string s in pipenames)
                    {
                        while (s.ToLower() == pipename.ToLower())
                        {
                            pipename = "";
                            random = new Random();
                            foreach (char c in name_parts)
                            {
                                pipename += (char)random.Next(last + c);
                                last = c;
                            }
                        }
                    }
                }
                namedPipeServerStreams[i] = new NamedPipeServerStream(pipename);
                pipenames.Add(pipename);
                pipename = "";
            }
        }
        public static void InitializePipes(string[] names) 
        { 
            
        }
        public static void getProcessModules(string name) 
        {
            startInfo = new ProcessStartInfo("dotnet");
            startInfo.Arguments = "run "+dir+name+" -- "+pipenames[current_pipe];
            current_pipe = current_pipe < pipenames.Count ? current_pipe+1:current_pipe;
            process.StartInfo = startInfo;
            process.Start();
            Console.WriteLine("Properties of the modules  associated "
        + "with "+name+" process are:");
            ModuleCollection = process.Modules;
            // Display the properties of each of the modules.
            for (int i = 0; i < ModuleCollection.Count; i++)
            {
                ProcessModule = ModuleCollection[i];
                Console.WriteLine("The moduleName is "
                    + ProcessModule.ModuleName);
                Console.WriteLine("The " + ProcessModule.ModuleName + "'s base address is: "
                    + ProcessModule.BaseAddress);
                Console.WriteLine("The " + ProcessModule.ModuleName + "'s Entry point address is: "
                    + ProcessModule.EntryPointAddress);
                Console.WriteLine("The " + ProcessModule.ModuleName + "'s File name is: "
                    + ProcessModule.FileName);
            }
        }
    }
}
