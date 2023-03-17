using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models
{
    public class Breadcrumb
    {
        public int? ContentID { get; set; }
        public string ContentType { get; set; }
        public int? ParentPageID { get; set; }
        public string URLKeywords { get; set; }
        public string Heading { get; set; }
    }
}