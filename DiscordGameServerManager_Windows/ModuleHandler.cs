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
        public static List<int> pipe_indexes = new List<int>();
        public static int available_threads;
        public static int available_async_threads;
        public static Process process = new Process();
        public static ProcessStartInfo startInfo;
        public static NamedPipeServerStream[] namedPipeServerStreams;
        public static Thread[] pipe_threads;
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
            pipe_threads = new Thread[namedPipeServerStreams.Length];
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
        public static void StartProcessModule(string name) 
        {
            startInfo = new ProcessStartInfo("dotnet");
            startInfo.Arguments = "run "+dir+name+" -- "+pipenames[current_pipe];
            process.StartInfo = startInfo;
            pipe_threads[current_pipe] = new Thread(() => 
            {
                string current_name = pipenames[current_pipe];
                string directory = dir;
                string mName = name;
                int current_index = current_pipe;
                Process module = process;
                module.Start();
                ProcessModule pModule;
                ProcessModuleCollection processModuleC;
                processModuleC = module.Modules;
                System.IO.TextWriter text = new System.IO.StreamWriter(directory+mName+"_output.txt");
                for (int i = 0; i < processModuleC.Count; i++)
                {
                    pModule = processModuleC[i];
                    text.WriteLine("Properties of the modules  associated "
        + "with " + name + " process are:");
                    text.WriteLine("The moduleName is "
                        + pModule.ModuleName);
                    text.WriteLine("The " + pModule.ModuleName + "'s base address is: "
                        + pModule.BaseAddress);
                    text.WriteLine("The " + pModule.ModuleName + "'s Entry point address is: "
                        + pModule.EntryPointAddress);
                    text.WriteLine("The " + pModule.ModuleName + "'s File name is: "
                        + pModule.FileName);
                }
                text.Flush();
                text.Close();
            });
            ThreadPool.GetAvailableThreads(out available_threads, out available_async_threads);
            if (current_pipe+1 < available_threads)
            {
                pipe_threads[current_pipe].Start();
                ReadPipe(current_pipe);
            }
            pipe_indexes.Add(current_pipe);
            current_pipe = current_pipe < pipenames.Count ? current_pipe+1:current_pipe;
            // Display the properties of each of the modules.
        }
        public static void ReadPipe(int index) 
        {
            List<string> pipedata = new List<string>();

        }
    }
}
