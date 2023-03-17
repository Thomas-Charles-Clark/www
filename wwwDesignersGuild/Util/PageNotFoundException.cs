using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    public class PageNotFoundException : Exception
    {
        public string SiteCulture { get; set; }

        public PageNotFoundException(string siteCulture) : base()
        {
            SiteCulture = siteCulture;
        }
    }
}