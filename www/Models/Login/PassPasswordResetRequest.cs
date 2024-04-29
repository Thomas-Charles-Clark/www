using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class PassPasswordResetRequest
    {
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public string email { get; set; }
    }
}