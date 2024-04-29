using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class PasswordResetModel
    {
        public bool TokenValid { get; set; }
        public string Token { get; set; }

        public PasswordResetModel(bool tokenValid, string token)
        {
            TokenValid = tokenValid;
            Token = token;
        }
    }

    public class MemberUnsubscribe
    {
        public Guid? guid { get; set; }
        public byte? reasonId { get; set; }
        public string emailName { get; set; }
        public string email { get; set; }
    }

    public class MemberResubscribe
    {
        public string email { get; set; }
    }
}