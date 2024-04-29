using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.Models
{
    public enum BootstrapBreakpoint
    {
        [BreakpointMaxWidth("575px")]
        xs = 0,

        [BreakpointMaxWidth("767px")]
        sm = 1,

        [BreakpointMaxWidth("991px")]
        md = 2,

        [BreakpointMaxWidth("1319px")]
        lg = 3,

        [BreakpointMaxWidth("1599px")]
        xl = 4,

        [BreakpointMaxWidth("1919px")]
        xxl = 5,

        [BreakpointMaxWidth("")]
        xxxl = 6
    }

    public class BreakpointMaxWidthAttribute : System.Attribute
    {
        public string MaxWidth { get; set; }

        public BreakpointMaxWidthAttribute(string maxWidth)
        {
            MaxWidth = maxWidth;
        }
    }

    public static class BreakpointMaxWidthAttributeExtensions 
    {
        public static BreakpointMaxWidthAttribute GetMaxWidth(BootstrapBreakpoint bb)
        {
            var type = bb.GetType();

            var fi = type.GetField(bb.ToString());

            return fi.GetCustomAttributes(typeof(BreakpointMaxWidthAttribute), false)
                .FirstOrDefault()
                as BreakpointMaxWidthAttribute;
        }
    }

    public class BootstrapBreakpointImageSize
    {
        public BootstrapBreakpoint Breakpoint { get; set; }
        public ImageSize ImageSize { get; set; }

        public BootstrapBreakpointImageSize(BootstrapBreakpoint bb, ImageSize i)
        {
            Breakpoint = bb;
            ImageSize = i;
        }
    }
}