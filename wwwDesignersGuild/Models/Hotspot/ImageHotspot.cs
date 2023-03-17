using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Hotspot
{
    public class ImageHotspot
    {
        public long ImageID { get; set; }
        public IEnumerable<Hotspot> Hotspots { get; set; }
    }
}