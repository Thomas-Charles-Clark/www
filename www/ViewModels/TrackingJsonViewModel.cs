using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class TrackingJsonViewModel
    {
        public DotDigitalTrackingBehaviourProfile DotDigitalBehaviourProfile { get; set; }
        public masterDefinition objMaster { get; set; }
        public brontoJSONBasketHeader Header { get; set; }
        public List<brontoJSONBasketLine> Lines { get; set; }
    }
}