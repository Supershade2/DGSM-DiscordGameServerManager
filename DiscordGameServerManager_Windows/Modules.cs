using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using System.Text;

namespace DiscordGameServerManager_Windows
{

    class Modules
    {
        public static Process process;
        public static ProcessModuleCollection ModuleCollection;
        private const string dir = "Resources/Modules";
        private const string module_list = "modules.json";
        public static Modulecollection module_Collection;
        public Modules()
        {
            Module sample = new Module();
            sample.main_cs = "example.cs";
            sample.module_resources = @"\\path to module resources\\";
            sample.references = new List<string>();
            sample.references.Add("System");
            sample.references.Add("System.Core");
            sample.references.Add("System.IO.Pipes");
            sample.references.Add("System.Collections.Generic");
            sample.properties = new Dictionary<string, string>();
            sample.properties.Add("TargetFrameworkVersion", "v4.5");
            sample.properties.Add("RootNamespace", "Module_Namespace");
            sample.properties.Add("OutputType", "Exe");
            List<Module> modules = new List<Module>();
            modules.Add(sample);
            string json;
            if (!File.Exists(dir + "/" + module_list))
            {
                switch (Directory.Exists(dir))
                {
                    case true:
                        json = JsonConvert.SerializeObject(modules, Formatting.Indented);
                        File.WriteAllText(dir + "/" + module_list, json);
                        break;
                    default:
                        Directory.CreateDirectory(dir);
                        json = JsonConvert.SerializeObject(modules, Formatting.Indented);
                        File.ReadAllText(dir + "/" + module_list);
                        break;
                }
            }
        }
        public static void Initialize_Collection()
        {
            string json = File.ReadAllText(dir + "/" + module_list);
            List<Module> modules = JsonConvert.DeserializeObject<List<Module>>(json);
            module_Collection = new Modulecollection(modules);
        }
        public static void BuildModules()
        {
            List<bool> results = module_Collection.BuildModules(dir).GetResults();
            for (int i = 0; i < results.Count; i++)
            {
                Console.WriteLine("Module number:" + i + Heuristics.newline + "succeeded:" + results[i] + ".");
            }
        }
    }
    public struct Module
    {
        public string main_cs { get; set; }
        public string module_resources { get; set; }
        public List<string> references { get; set; }
        public Dictionary<string, string> properties { get; set; }
        public bool precompiled { get; set; }
    }
    internal class Modulecollection
    {
        public Modulecollection(List<Module> modulelist)
        {
            this.modulelist = modulelist;
        }
        public extensions BuildModules(string dir)
        {
            extensions exts = new extensions();
            List<string> property_keys = new List<string>();
            List<string> property_values = new List<string>();
            try
            {
                IEnumerable<string> module_dirs = Directory.EnumerateDirectories(dir);
                foreach (var moduledir in module_dirs)
                {
                    foreach (var module in modulelist)
                    {
                        var auto = ProjectRootElement.Create();
                        var propertyGroup = auto.AddPropertyGroup();
                        var slItemGroup = auto.CreateItemGroupElement();
                        var sl1ItemGroup = auto.CreateItemGroupElement();
                        var sourceitem = auto.CreateItemGroupElement();
                        foreach (var prop in module.properties.Keys)
                        {
                            property_keys.Add(prop);
                        }
                        foreach (var value in module.properties.Values)
                        {
                            property_values.Add(value);
                        }
                        propertyGroup.AddProperty("DefaultTargets", "Build");
                        for (int index = 0; index < module.properties.Keys.Count; index++)
                        {
                            auto.AddProperty(property_keys[index], property_values[index]);
                        }
                        auto.InsertAfterChild(slItemGroup, auto.LastChild);
                        foreach (var reference in module.references)
                        {
                            slItemGroup.AddItem("Reference", reference);
                        }
                        auto.InsertAfterChild(sl1ItemGroup, auto.LastChild);
                        auto.InsertAfterChild(sourceitem, auto.LastChild);
                        sourceitem.AddItem("Compile", moduledir + module.main_cs);
                        /** For reference to how to point to a file with a literal string
                         * sourceitem.AddItem("Compile", moduledir + @"\\Template.cs");*/
                        var target = auto.AddTarget("Build");
                        var task = target.AddTask("Csc");
                        task.SetParameter("Sources", "@(Compile)");
                        var namekey = (from string key in property_keys
                                       where (key.Contains("RootNamespace"))
                                       select key);
                        string name = "";
                        module.properties.TryGetValue(namekey.First(), out name);
                        task.SetParameter("OutputAssembly", moduledir + Path.GetFileNameWithoutExtension(name + ".exe"));
                        auto.Save(moduledir + @"\\output.csproj");
                        Project program = new Project(auto.DirectoryPath + @"\\output.csproj");
                        exts.SetResult(program.Build());
                    }
                }
                return exts;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                exts.SetResult(false);
                return exts;
            }
        }
        internal class extensions
        {
            public ref List<bool> GetResults()
            {
                return ref result;
            }
            public void SetResult(bool r)
            {
                result.Add(r);
            }
            private List<bool> result;
        }
        public List<Module> modulelist { get; set; }
    }
}