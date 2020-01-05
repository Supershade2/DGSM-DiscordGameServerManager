using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Newtonsoft.Json;
using System.Text;

namespace DiscordGameServerManager_Windows
{

    class Modules
    {
        static ProjectRootElement auto = ProjectRootElement.Create();
        static ProjectPropertyGroupElement propertyGroup = auto.AddPropertyGroup();
        static ProjectItemGroupElement slItemGroup = auto.CreateItemGroupElement();
        static ProjectItemGroupElement sl1ItemGroup = auto.CreateItemGroupElement();
        static ProjectItemGroupElement sourceitem = auto.CreateItemGroupElement();
    }
    public struct module 
    { 
       public string cs_path { get; set; }
       public List<string> references { get; set; }
       public Dictionary<string,string> properties { get; set; }

    }
}
