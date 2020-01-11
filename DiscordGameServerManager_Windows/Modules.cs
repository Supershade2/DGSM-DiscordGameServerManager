using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using System.Text;

namespace DiscordGameServerManager_Windows
{

    class Modules
    {
        public static ProcessModuleCollection ModuleCollection;
        private const string dir = "Resources/Modules";
        public static bool BuildModules() 
        {
            var auto = ProjectRootElement.Create();
            var propertyGroup = auto.AddPropertyGroup();
            var slItemGroup = auto.CreateItemGroupElement();
            var sl1ItemGroup = auto.CreateItemGroupElement();
            var sourceitem = auto.CreateItemGroupElement();
            IEnumerable<string> module_dirs = Directory.EnumerateDirectories(dir);
            foreach (var moduledir in module_dirs)
            {
                IEnumerable<string> modules = Directory.EnumerateFiles(moduledir);
                foreach (var module in modules)
                {
                    propertyGroup.AddProperty("DefaultTargets", "Build");
                    //auto.SetProperty("DefaultTargets","Build");
                    auto.AddProperty("TargetFrameworkVersion", "v4.5");
                    auto.AddProperty("RootNamespace", "AutoJoiner");
                    auto.AddProperty("OutputType", "Exe");
                    auto.InsertAfterChild(slItemGroup, auto.LastChild);
                    slItemGroup.AddItem("Reference", "System");
                    slItemGroup.AddItem("Reference", "System.Core");
                    slItemGroup.AddItem("Reference", "System.Linq");
                    slItemGroup.AddItem("Reference", "System.Reflection");
                    slItemGroup.AddItem("Reference", "System.IO");
                    slItemGroup.AddItem("Reference", "System.Runtime.InteropServices");
                    slItemGroup.AddItem("Reference", "System.Collections.Generic");
                    auto.InsertAfterChild(sl1ItemGroup, auto.LastChild);
                    auto.InsertAfterChild(sourceitem, auto.LastChild);
                    sourceitem.AddItem("Compile", moduledir + @"\\Template.cs");
                    var target = auto.AddTarget("Build");
                    var task = target.AddTask("Csc");
                    task.SetParameter("Sources", "@(Compile)");
                    task.SetParameter("OutputAssembly", moduledir + @"\\AutoJoiners\\" + Path.GetFileNameWithoutExtension(module+".exe"));
                    auto.Save(moduledir + @"\\output.csproj");
                    Project program = new Project(auto.DirectoryPath + @"\\output.csproj");
                    program.Build();
                }
            }
            return false;
        }
    }
    public struct module 
    { 
       public string cs_path { get; set; }
       public List<string> references { get; set; }
       public Dictionary<string,string> properties { get; set; }

    }
}
