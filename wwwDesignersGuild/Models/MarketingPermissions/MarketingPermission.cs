using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Classes
{
    public class MarketingPermissionResult
    {
        public List<MarketingPermission> Permissions { get; set; }

        /// <summary>
        /// Is email marketing already permitted for this user? 
        /// </summary>
        public bool EmailMarketingPermitted { get; set; }

        /// <summary>
        /// Is this user already unsubscribed from email marketing?
        /// </summary>
        public bool UnsubscribedFromEmailMarketing { get; set; }

        public MarketingPermissionResult()
        {
            Permissions = new List<MarketingPermission>();
            UnsubscribedFromEmailMarketing = false;
        }
    }

    public class MarketingPermission
    {
        public byte MarketingPermissionID { get; set; }
        public string Name { get; set; }
        public bool Default { get; set; }
        public bool Hidden { get; set; }
        public string Label { get; set; }
        public bool Checked { get; set; }
        public bool? CurrentStatus { get; set; }
    }
}