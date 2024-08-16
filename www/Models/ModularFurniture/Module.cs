using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.ModularFurniture
{
    public class Module
    {
        public short ModuleID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ConfirmText { get; set; }

        public string FitGuide { get; set; }

        public long? DimensionsImageID { get; set; }

        [JsonProperty("moduleOptions")]
        public List<ModuleOption> Options { get; set; }

        public IEnumerable<ModuleOptionGroup> GroupedOptions
        {
            get
            {
                if (Options.Where(o => o.ModuleOptionGroupName != null).Any())
                {
                    return Options
                        .GroupBy(o => new { o.ModuleOptionGroupID, o.ModuleOptionGroupName, o.ModuleOptionGroupDescription })
                        .Select(g => new ModuleOptionGroup(g.Key.ModuleOptionGroupID, g.Key.ModuleOptionGroupName, g.Key.ModuleOptionGroupDescription, g));
                }
                else
                {
                    return new List<ModuleOptionGroup>();
                }
            }
        }

        public ModuleOption Current { get; set; }

        public int OptionsCount
        {
            get
            {
                if (GroupedOptions.Any())
                    return GroupedOptions.Select(p => p.Options.Count()).Sum();
                else
                    return Options.Count();
            }
        }

        [JsonIgnore]
        public IEnumerable<ModuleOption> OptionsExceptCurrent
        {
            get
            {
                return Options.Where(o => o.ModuleOptionID != Current.ModuleOptionID);
            }
        }
    }
}