﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.IO.Pipelines;
using System.Runtime;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using System.Linq;

namespace DiscordGameServerManager
{
    public class ModuleHandler
    {
        private static EventWaitHandle clearCount =
        new EventWaitHandle(false, EventResetMode.AutoReset);
        static int activethreads = 0;
        public static int available_threads;
        public static int available_async_threads;
        public static Process process = new Process();
        public static ProcessStartInfo startInfo;
        public static Socket info = new Socket(AddressFamily.Unix,SocketType.Rdm,ProtocolType.Tcp);
        public static UnixDomainSocketEndPoint unix_socket = new UnixDomainSocketEndPoint("Resources/Modules/Socket/module.sock");
        public static Pipe pipe = new Pipe();
        public static NamedPipeServerStream[] namedPipeServerStreams;
        public static Thread[] pipe_threads;
        static Random random = new Random();
        static string dir;
        static List<string> pipenames = new List<string>();
        static int current_pipe = 0;
        public ModuleHandler(string d)
        {
            dir = d;
            ThreadPool.GetAvailableThreads(out available_threads, out available_async_threads);
        }
        public static void InitializeSocket() 
        {
            if (!Directory.Exists("Resources/Modules/Socket")) 
            {
                Directory.CreateDirectory("Resources/Modules/Socket");
                File.Create("Resources/Modules/Socket/module.sock").Dispose();
            }
            ThreadPool.GetAvailableThreads(out available_threads, out available_async_threads);
            info.Bind(unix_socket);
            info.Listen(available_threads);
        }
        public static void InitializePipes() 
        {
            namedPipeServerStreams = new NamedPipeServerStream[Modules.module_Collection.modulelist.Count];
            pipe_threads = new Thread[namedPipeServerStreams.Length];
            string name = System.Reflection.Assembly.GetEntryAssembly().FullName;
            char[] name_parts = name.ToCharArray();
            string pipename = "";
            for (int i = 0; i < namedPipeServerStreams.Length; i++)
            {
                foreach (char c in name_parts)
                {
                    pipename += (char)random.Next(65,90);
                }
                if (pipenames.Count > 0) 
                {
                    foreach (string s in pipenames)
                    {
                        while (s.ToLower(CultureInfo.CurrentCulture) == pipename.ToLower(CultureInfo.CurrentCulture))
                        {
                            pipename = "";
                            random = new Random(DateTime.UtcNow.Millisecond);
                            foreach (char c in name_parts)
                            {
                                int num = random.Next(1,10);
                                if (num > 5) 
                                {
                                    char ch = (char)random.Next(97, 122);
                                    pipename += ch;
                                }
                                else 
                                {
                                    pipename += (char)random.Next(65, 90);
                                }
                            }
                        }
                    }
                }
                namedPipeServerStreams[i] = new NamedPipeServerStream(pipename,PipeDirection.InOut);
                namedPipeServerStreams[i].ReadMode = PipeTransmissionMode.Message;
                pipenames.Add(pipename);
                pipename = "";
            }
        }
        public static void InitializePipes(string[] names) 
        {
            if (names.Length == Modules.module_Collection.modulelist.Count)
            {
                for (int i = 0; i < names.Length; i++)
                {
                    namedPipeServerStreams[i] = new NamedPipeServerStream(names[i],PipeDirection.InOut);
                    namedPipeServerStreams[i].ReadMode = PipeTransmissionMode.Message;
                    pipenames.Add(names[i]);
                }
            }
            else if (names.Length > Modules.module_Collection.modulelist.Count)
            {
                Console.WriteLine(Properties.Resources.ExcessModuleNames);
            }
            else 
            {
                Console.WriteLine(Properties.Resources.InsufficientModuleNames);
            }
        }
        public static void StartProcessModule(string name) 
        {
            startInfo = new ProcessStartInfo("dotnet");
            startInfo.Arguments = "run "+dir+name+" -- "+pipenames[current_pipe];
            process.StartInfo = startInfo;
            if (pipe_threads[current_pipe] != null) 
            {
                pipe_threads[current_pipe] = new Thread((object data) =>
                {
                    string current_name = pipenames[(int) data];
                    string directory = dir;
                    string mName = name;
                    int current_index = (int) data;
                    Process module = process;
                    module.Start();
                    ProcessModule pModule;
                    ProcessModuleCollection processModuleC;
                    processModuleC = module.Modules;
                    StreamWriter text = new StreamWriter(directory + mName + "_output.txt");
                    // Display the properties of each of the modules.
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
                    text.Dispose();
                    ReadPipesCallback(data);
                });
            }
            ThreadPool.GetAvailableThreads(out available_threads, out available_async_threads);
            if (current_pipe+1 < (available_threads/2)-2 && pipe_threads[current_pipe].Name != name)
            {
                pipe_threads[current_pipe].Name = name;
                pipe_threads[current_pipe].IsBackground = true;
                pipe_threads[current_pipe].Start();
                current_pipe = current_pipe < pipenames.Count ? current_pipe + 1 : current_pipe;
            }
        }
        public static async Task<string> ReadPipe(int pipe) 
        {
            List<byte> data_bytes = PerformReads(pipe);
            //byte[] str_bytes = Encoding.Convert(Encoding.UTF8, Encoding.ASCII, data_bytes.ToArray());
            string data = BitConverter.ToString(data_bytes.ToArray());
            return data;
        }
        public static List<byte> PerformReads(int current) 
        {
            List<byte> data_bytes = new List<byte>();
            byte[] message = new byte[1024];
            bool connected;
                if (!namedPipeServerStreams[current].IsConnected)
                {
                namedPipeServerStreams[current].WaitForConnection();
                }
            connected = namedPipeServerStreams[current].IsConnected;
            if (connected)
            {
                using (var ms = new MemoryStream())
                {
                    do
                    {
                        //Length to be determined in testing for how large the smallest message will be or switch to BeginRead and EndRead
                        namedPipeServerStreams[current].Read(message, 0, 1024);
                        ms.Write(message, 0, message.Length);
                    } while (!namedPipeServerStreams[current].IsMessageComplete);
                    data_bytes.AddRange(ms.ToArray());
                }
                string reply = "processing";
                //Alert 
                namedPipeServerStreams[current].Write(Encoding.GetEncoding(reply).GetBytes(reply));
            }
            return data_bytes;
        }
        public static void Read(int current) 
        {
            ReInitializePipe(current);
        }
        private static void ExitRequested(int pipe) 
        {
            namedPipeServerStreams[pipe].Disconnect();
            namedPipeServerStreams[pipe].Dispose();
        }
        private static void ReInitializePipe(int current)
        {
            namedPipeServerStreams[current].Disconnect();
            namedPipeServerStreams[current].WaitForConnection();
        }

        public static void ReadPipesCallback(object data) 
        {
            ReadPipesCall rpipes = ReadPipes;
            Action<object> action = new Action<object>(rpipes);
            action?.Invoke((int) data);
        }

        private static void ReadPipes(object pi)
        {
            while (true) 
            {
                if (!File.Exists(dir + "/input.txt"))
                {
                    File.CreateText(dir + "/" + (int)pi + "_input.txt").Dispose();
                }
                string output = ReadPipe((int)pi).ConfigureAwait(true).GetAwaiter().GetResult();
                ProcessData(output,(int)pi);
            }
        }
        private static void ProcessData(string data,int pipe_index) 
        {
            if (data.ToLower(CultureInfo.CurrentCulture).Contains("discordsendmessage:",StringComparison.CurrentCulture)) 
            { 
                
            }
            if (data.ToLower(CultureInfo.CurrentCulture).Contains("discord:", StringComparison.CurrentCulture)) 
            { 
            
            }
            if (data.ToLower(CultureInfo.CurrentCulture).Equals("exit",StringComparison.CurrentCulture)) 
            {
                ExitRequested(pipe_index);
            }
        }
        internal delegate void ReadPipesCall(object p);
    }
}