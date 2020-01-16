using System;
using System.Collections.Generic;
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
        private const string dir = "Resources/Modules";
        private const string module_list = "modules.json";
        public static Modulecollection module_Collection;
        public Modules()
        {
            Module sample = new Module();
            sample.main_cs = "example.cs";
            sample.module_resources = "/path to module resources";
            sample.references = new List<string>();
            sample.references.Add("System");
            sample.references.Add("System.Core");
            sample.references.Add("System.IO.Pipes");
            sample.references.Add("System.Collections.Generic");
            sample.properties = new Dictionary<string, string>();
            sample.properties.Add("TargetFramework", "netcoreapp2.2");
            sample.properties.Add("RootNamespace", "Module_Namespace");
            sample.properties.Add("OutputType", "Exe");
            List<Module> modulelist = new List<Module>();
            modulelist.Add(sample);
            module_Collection = new Modulecollection(modulelist);
            string json;
            if (!File.Exists(dir + "/" + module_list))
            {
                switch (Directory.Exists(dir))
                {
                    case true:
                        json = JsonConvert.SerializeObject(modulelist, Formatting.Indented);
                        File.WriteAllText(dir + "/" + module_list, json);
                        break;
                    default:
                        Directory.CreateDirectory(dir);
                        json = JsonConvert.SerializeObject(modulelist, Formatting.Indented);
                        File.WriteAllText(dir + "/" + module_list, json);
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
        public bool prepackaged { get; set; }
    }
    public class Modulecollection
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
                        var pr = ProjectRootElement.Create();
                        var propertyGroup = pr.AddPropertyGroup();
                        var slItemGroup = pr.CreateItemGroupElement();
                        var sl1ItemGroup = pr.CreateItemGroupElement();
                        var sourceitem = pr.CreateItemGroupElement();
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
                            pr.AddProperty(property_keys[index], property_values[index]);
                        }
                        pr.InsertAfterChild(slItemGroup, pr.LastChild);
                        foreach (var reference in module.references)
                        {
                            slItemGroup.AddItem("Reference", reference);
                        }
                        pr.InsertAfterChild(sl1ItemGroup, pr.LastChild);
                        pr.InsertAfterChild(sourceitem, pr.LastChild);
                        sourceitem.AddItem("Compile", moduledir + module.main_cs);
                        /** For reference to how to point to a file with a literal string
                         * sourceitem.AddItem("Compile", moduledir + @"\\Template.cs");*/
                        var target = pr.AddTarget("Build");
                        var task = target.AddTask("Csc");
                        task.SetParameter("Sources", "@(Compile)");
                        var namekey = (from string key in property_keys
                                       where (key.Contains("RootNamespace"))
                                       select key);
                        string name = "";
                        module.properties.TryGetValue(namekey.Single(), out name);
                        task.SetParameter("OutputAssembly", moduledir + Path.GetFileNameWithoutExtension(name + ".exe"));
                        pr.Save(moduledir + @"\\output.csproj");
                        Project program = new Project(pr.DirectoryPath + @"\\output.csproj");
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
        public List<Module> modulelist { get; set; }
    }
    public class extensions
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
}