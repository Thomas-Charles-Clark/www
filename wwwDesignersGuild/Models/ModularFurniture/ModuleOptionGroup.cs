using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.ModularFurniture
{
    public class ModuleOptionGroup
    {
        public int ModuleOptionGroupID { get; set; }

        [JsonProperty("ModuleOptionGroupName")]
        public string Name { get; set; }

        public string Description { get; set; }

        public IEnumerable<ModuleOption> Options { get; set; }

        public ModuleOptionGroup(int moduleOptionGroupId, string name, string description, IEnumerable<ModuleOption> options)
        {
            ModuleOptionGroupID = moduleOptionGroupId;
            Name = name;
            Description = description;
            Options = options;
        }
    }
}