using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.ResourceFetch
{
    public class PassResourceFetch
    {
        public string[] resources { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public DateTime useDate { get; set; }
    }

    public class ResourceResult
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}