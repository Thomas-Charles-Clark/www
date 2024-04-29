using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ViewPathAttribute : Attribute
    {
        public string ViewPath { get; set; }

        public ViewPathAttribute(string viewPath)
        {
            ViewPath = viewPath;
        }
    }
}