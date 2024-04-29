using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.TechInfo
{
    public class TechInfoCollection
    {
        public short CollectionID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string URLSemantics { get; set; }
        public string BrochureURL { get; set; }
    }

    public class TechInfoAftercare
    {
        public short AftercareID { get; set; }
        public string Aftercare { get; set; }
    }

    public class TechInfoUsage
    {
        public short UsageID { get; set; }
        public string Usage { get; set; }
    }

    public class TechInfoTest
    {
        public string Test { get; set; }
    }
}