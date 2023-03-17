using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class ImageController : Controller
    {
        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageResult CallImage(string width, string imageid)
        {
            return new ImageResult(width,
                                    imageid,
                                    System.Web.Configuration.WebConfigurationManager.AppSettings["imagePath"],
                                    Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]),
                                    true);
        }

        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageResult CallImageNonWebP(string width, string imageid)
        {
            return new ImageResult(width,
                                    imageid,
                                    System.Web.Configuration.WebConfigurationManager.AppSettings["imagePath"],
                                    Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]),
                                    false);
        }

        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageResult CallSiteSelectorImage(string platform, string size)
        {
            return new ImageResult(platform,
                                   size,
                                   System.Web.Configuration.WebConfigurationManager.AppSettings["siteSelectorBackgroundImagePath"],
                                   Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]),
                                   true);
        }

        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageResult CallSiteSelectorImageNonWebP(string width, string imageid)
        {
            return new ImageResult(width,
                                   imageid,
                                   System.Web.Configuration.WebConfigurationManager.AppSettings["siteSelectorBackgroundImagePath"],
                                   Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]),
                                   false);
        }

        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageBannerResult CallImageBanner(string width, string imageid)
        {
            return new ImageBannerResult(width,
                                        imageid,
                                        System.Web.Configuration.WebConfigurationManager.AppSettings["imageBannerPath"],
                                        true);
        }

        [AccessControlAllowOrigin("*")]
        [OutputCache(Duration = 31536000, Location = System.Web.UI.OutputCacheLocation.Downstream)] // Cached for 1 year
        public ImageBannerResult CallImageBannerNonWebP(string width, string imageid)
        {
            return new ImageBannerResult(width,
                                        imageid,
                                        System.Web.Configuration.WebConfigurationManager.AppSettings["imageBannerPath"],
                                        false);
        }
    }
}