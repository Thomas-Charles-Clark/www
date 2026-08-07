using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache.General;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.Scrapbook
{
    public class cmsScrapbook
    {
        public async Task<cmsScrapbookImagesDefinition> loadScrapbookFromCmsAsync(
            long visitorId,
            int? memberId,
            short countryId, 
            byte languageId, 
            DateTime useDate)
        {
            // Is page in cache?
            var cache = new ScrapbookCacheProvider();

            var data = cache.GetOrCreate(visitorId, memberId, countryId, languageId, useDate, () => {

				var result = Task.Run(() => loadScrapbookFromCms(visitorId, memberId, countryId, languageId, useDate)).Result;

                return result;

			});

            return data;
        }

        public void setScrapbook(
            long visitorId,
            int? memberId,
            short countryId,
            byte languageId,
            DateTime useDate,
            cmsScrapbookImagesDefinition objScrapbook)
        {
            var cache = new ScrapbookCacheProvider();
            cache.Set(visitorId, memberId, countryId, languageId, useDate, objScrapbook);
        }

        private async Task<cmsScrapbookImagesDefinition> loadScrapbookFromCms(long visitorId, int? memberId, short countryId, byte languageId, DateTime useDate)
        {
            var imagesResult = await new visitorDb().fetchCompleteScrapbookAsync(visitorId, memberId, countryId, languageId, useDate);

			var result = new cmsScrapbookImagesDefinition()
            {
                Images = imagesResult
            };

            return result;
        }
    }
}