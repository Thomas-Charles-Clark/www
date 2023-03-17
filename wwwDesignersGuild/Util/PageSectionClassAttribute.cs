using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PageSectionClassAttribute : Attribute
    {
        public string ClassName { get; set; }

        public PageSectionClassAttribute(string className)
        {
            ClassName = className;
        }
    }
}