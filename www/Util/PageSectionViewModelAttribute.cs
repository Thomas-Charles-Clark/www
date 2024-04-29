using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class PageSectionViewModelAttribute : Attribute
    {
        public string PageSectionType { get; set; }

        public PageSectionViewModelAttribute(string pageSectionType)
        {
            PageSectionType = pageSectionType;
        }
    }
}