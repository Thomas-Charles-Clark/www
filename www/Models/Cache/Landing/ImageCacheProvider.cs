using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Caching;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using System.Web.Configuration;

namespace wwwDesignersGuild.Models.Cache
{
    public class ImageCacheProvider : ICacheImageProvider
    {


        private ObjectCache Cache { get { return MemoryCache.Default; } }


        public byte[] Get(String imageType, String width, String imageId)
        {
            String key = buildKey(imageType, width, imageId);
            return (byte[])Cache[key];
        }


        public void Set(String imageType, String width, String imageId, byte[] data)
        {
            Boolean.TryParse(WebConfigurationManager.AppSettings["landingCacheEnabled"], out bool cacheEnabled);

            if (cacheEnabled)
            {
                var policy = new CacheItemPolicy();
                if (Int16.TryParse(WebConfigurationManager.AppSettings["landingCacheDuration"], out short cacheDurationMinutes))
                {
                    policy.AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheDurationMinutes);
                }
                else
                {
                    policy.AbsoluteExpiration = DateTime.Today.AddDays(1).Date;
                }

                var key = buildKey(imageType, width, imageId);

                Cache.Add(new CacheItem(key, data), policy);
            }
        }


        public bool IsSet(String imageType, String width, String imageId)
        {
            String key = buildKey(imageType, width, imageId);
            return (Cache[key] != null);
        }


        public void Invalidate(String imageType, String width, String imageId)
        {
            String key = buildKey(imageType, width, imageId);
            Cache.Remove(key);
        }


        private string buildKey(String imageType, String width, String imageId)
        {
            return String.Format("{0}~{1}~{2}", imageType, width, imageId);
        }


    }
}