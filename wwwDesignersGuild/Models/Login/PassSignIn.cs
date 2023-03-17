using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class PassSignIn
    {
        public string email { get; set; }
        public string password { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
        public webapiSignInAccountType accountType { get; set; }
    }
}