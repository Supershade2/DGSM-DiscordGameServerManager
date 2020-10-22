using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using System.Text;

namespace DiscordGameServerManager
{

    class Modules
    {
        private const string dir = "Resources/Modules";
        private const string module_list = "modules.json";
        public static Modulecollection module_Collection;
        public static Modules m = new Modules();
        public Modules()
        {
            ModuleData sample = new ModuleData();
            sample.mainCS = "example.cs";
            sample.moduleResources = "/path to module resources";
            sample.references = new List<string>();
            sample.references.Add("System");
            sample.references.Add("System.Core");
            sample.references.Add("System.IO.Pipes");
            sample.references.Add("System.Collections.Generic");
            sample.properties = new Dictionary<string, string>();
            sample.properties.Add("TargetFramework", "netcoreapp2.2");
            sample.properties.Add("RootNamespace", "Module_Namespace");
            sample.properties.Add("OutputType", "Exe");
            List<ModuleData> modulelist = new List<ModuleData>();
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
            List<ModuleData> modules = JsonConvert.DeserializeObject<List<ModuleData>>(json);
            module_Collection = new Modulecollection(modules);
        }
        public static void BuildModules()
        {
            var data = module_Collection.BuildModules(dir);
            List<bool> results = data.GetResults();
            if (Program.verboseoutput) 
            {
                for (int i = 0; i < results.Count; i++)
                {
                    string end = Heuristics.newline + "succeeded:" + results[i] + ".";
                    Console.Write("Module number:" + i + Heuristics.newline + "Module Info {" + Heuristics.newline + "   Main .cs file:" + data.Subprograms[i].mainCS + Heuristics.newline + " Resources:" + data.Subprograms[i].moduleResources + Heuristics.newline + "   References {" + Heuristics.newline);
                    foreach (var reference in data.Subprograms[i].references)
                    {
                        Console.Write("     " + reference + Heuristics.newline);
                    }
                    Console.Write(" }" + Heuristics.newline+"   Package References {"+Heuristics.newline);
                    foreach (var package in data.Subprograms[i].packageReferences.Keys)
                    {
                        string version = "";
                        data.Subprograms[i].packageReferences.TryGetValue(package, out version);
                        Console.Write("     Package: " +package+", version: "+version+Heuristics.newline);
                    }
                    Console.Write(" }" + Heuristics.newline + "   Properties {" + Heuristics.newline);
                }
            }
        }
    }
    public struct ModuleData : IEquatable<ModuleData>
    {
        public string mainCS { get; set; }
        public string moduleResources { get; set; }
        public List<string> references { get; set; }
        public Dictionary<string,string> packageReferences { get; set; }
        public Dictionary<string, string> properties { get; set; }
        public bool prepackaged { get; set; }

        public override bool Equals(object obj) =>
            obj is ModuleData MD && this == MD;
        public override int GetHashCode()
        {
            int hash = 0;
            hash = (hash * 3) + mainCS.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + moduleResources.GetHashCode(StringComparison.CurrentCulture);
            hash = (hash * 3) + references.GetHashCode();
            hash = (hash * 3) + packageReferences.GetHashCode();
            hash = (hash * 3) + properties.GetHashCode();
            hash = (hash * 3) + prepackaged.GetHashCode();
            return hash;
        }

        public static bool operator ==(ModuleData left, ModuleData right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)left == null) || ((object)right == null))
            {
                return false;
            }
            return left.mainCS == right.mainCS && left.moduleResources == right.moduleResources && left.references == right.references && left.packageReferences == right.packageReferences && left.properties == right.properties && left.prepackaged == right.prepackaged;
        }

        public static bool operator !=(ModuleData left, ModuleData right)
        {
            return !(left == right);
        }

        public bool Equals(ModuleData other)
        {
            return this == other;
        }
    }
    public class Modulecollection
    {
        public Modulecollection(List<ModuleData> modulelist)
        {
            this.modulelist = modulelist;
        }
        /*! Use with Modulecollection.modulelist or  Modulecollection(old Modulecollectionobject.modulelist, bool PreservePrevious) 
         * to preserve the old list up to one prior reinstantiation or = new Modulecollection */
        public extensions BuildModules(string dir)
        {
            extensions exts = new extensions();
            List<string> property_keys = new List<string>();
            List<string> property_values = new List<string>();
            try
            {
                IEnumerable<string> module_dirs = Directory.EnumerateDirectories(dir);
                exts.Subprograms = modulelist;
                foreach (var moduledir in module_dirs)
                {
                    foreach (var module in modulelist)
                    {
                        if (!module.prepackaged) 
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
                            foreach (var package in module.packageReferences.Keys) 
                            {
                                string version = "";
                                module.packageReferences.TryGetValue(package, out version);
                                sl1ItemGroup.AddItem("PackageReference",package).AddMetadata("Version",version);
                            }
                            pr.InsertAfterChild(sourceitem, pr.LastChild);
                            sourceitem.AddItem("Compile", moduledir + module.mainCS);
                            /** For reference to how to point to a file with a literal string
                             * sourceitem.AddItem("Compile", moduledir + @"\\Template.cs");*/
                            var target = pr.AddTarget("Build");
                            var task = target.AddTask("Csc");
                            task.SetParameter("Sources", "@(Compile)");
                            var namekey = (from string key in property_keys
                                           where (key.Contains("RootNamespace",StringComparison.CurrentCulture))
                                           select key);
                            string name = "";
                            module.properties.TryGetValue(namekey.Single(), out name);
                            task.SetParameter("OutputAssembly", moduledir + Path.GetFileNameWithoutExtension(name + ".exe"));
                            pr.Save(moduledir + @"\\output.csproj");
                            Project program = new Project(pr.DirectoryPath + @"\\output.csproj");
                            exts.SetResult(program.Build());
                        }
                    }
                }
                return exts;
            }
            catch (Exception ex)
            {
                var ex0 = ex.InnerException;
                Console.WriteLine(ex.Message);
                exts.SetResult(false);
                return exts;
                throw ex0;
            }
        }
        public List<ModuleData> modulelist { get; set; }
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
        public List<ModuleData> Subprograms { get; set; }
    }
}