using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class LoginRequiredAttribute : Attribute
    {
        public bool LoginRequired { get; set; }

        public bool TradeRequired { get; set; }

        public LoginRequiredAttribute(bool loginRequired, bool tradeRequired = false)
        {
            LoginRequired = loginRequired;
            TradeRequired = tradeRequired;
        }
    }
}