using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.ClientError
{

    public class PassClientError
    {
        public string message { get; set; }
        public string source { get; set; }
        public string lineno { get; set; }
        public string colno { get; set; }
        public string error { get; set; }
        public string url { get; set; }
    }
}