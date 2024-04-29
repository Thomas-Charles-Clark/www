using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Interface;

namespace wwwDesignersGuild.Models.Banner
{
    public class cmsBanner : ICmsBannerRepository
    {
        public async Task<cmsBannerDefinition> loadBannersFromCms(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            var objBannerCache = new BannerCacheProvider();
            var bannerStore = objBannerCache.Get(countryId, languageId, useDate, isTrade, platformSwitch);

            if (bannerStore == null)
            {
                bannerStore = await Task.FromResult(loadBannerFromCmsDatabase(countryId, languageId, useDate, isTrade, platformSwitch));

                objBannerCache.Set(bannerStore, countryId, languageId, useDate, isTrade, platformSwitch);
            }

            return bannerStore;
        }

        private cmsBannerDefinition loadBannerFromCmsDatabase(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            return new bannerDb().LoadBanners(countryId, languageId, useDate, isTrade, platformSwitch);
        }
    }
}