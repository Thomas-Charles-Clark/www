using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.ModularFurniture
{
    public class ModuleOption
    {
        public int ProductPageID { get; set; }

        public string URLKeywords { get; set; }

        public string URL { get; set; }

        public long ProductPageImageID { get; set; }

        public int ModuleOptionID { get; set; }

        public int ModuleOptionGroupID { get; set; }

        public string ModuleOptionGroupName { get; set; }

        public string ModuleOptionGroupDescription { get; set; }


        [JsonProperty("ModuleOptionName")]
        public string Name { get; set; }

        public long ModuleOptionImageID { get; set; }

        public string Code { get; set; }

        public int? SampleProductID { get; set; }
    }
}