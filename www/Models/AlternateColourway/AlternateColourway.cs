using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.AlternateColourway
{
    public class AlternateColourway
    {
        public int productPageId { get; set; }
        public string code { get; set; }
        public string heading { get; set; }
        public long imageId { get; set; }
        public string url { get; set; }
        public string urlSemantics { get; set; }
    }
}