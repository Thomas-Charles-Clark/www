using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.ModularFurniture
{
    public class PassModuleOption : ModuleOption
    {
        public byte LanguageID { get; set; }
        public short CountryID { get; set; }
        public int? FabricProductPageID { get; set; }
        public NewMasterDefinition MasterDefinition { get; set; }
    }
}