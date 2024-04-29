using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class PassNewMember
    {
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public string gRecaptcha { get; set; }
        public bool marketingOptIn { get; set; }
    }
}