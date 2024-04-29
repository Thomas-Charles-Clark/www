using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ActionAttribute : Attribute
    {
        public string Action { get; set; }
        public string Controller { get; set; }

        public bool RequiresBase { get; set; }

        public ActionAttribute(string action, string controller = "NewSection", bool passBaseViewModel = true)
        {
            Action = action;
            Controller = controller;
            RequiresBase = passBaseViewModel;
        }
    }
}