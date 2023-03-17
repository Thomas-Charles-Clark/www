using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;
using Newtonsoft.Json;

namespace wwwDesignersGuild.Models.Lifestyle
{


    public class cmsLifestyle : ICmsLifestyleGalleryPopupRepository
    {
        public async Task<cmsLifestyleGalleryPopupDefinition> loadLifestyleGalleryPopupFromCms(int productPageId, byte languageId, short countryId, bool isTrade, DateTime useDate)
        {
            //  Is page in cache?
            cmsLifestyleGalleryPopupDefinition lifestyleStore;
            LifestyleGalleryPopupCacheProvider objLifestyleCache = new LifestyleGalleryPopupCacheProvider();
            lifestyleStore = objLifestyleCache.Get(productPageId, languageId, countryId, useDate);

            if (lifestyleStore == null)
            {
                lifestyleStore = await Task.FromResult<cmsLifestyleGalleryPopupDefinition>(loadLifestyleGalleryPopupFromCmsDatabase(productPageId, languageId, countryId, isTrade, useDate));
                objLifestyleCache.Set(productPageId, languageId, countryId, useDate, lifestyleStore);
            }

            return lifestyleStore;
        }


        private cmsLifestyleGalleryPopupDefinition loadLifestyleGalleryPopupFromCmsDatabase(int productPageId, byte languageId, short countryId, bool isTrade, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.LifestyleGalleryPopup_GenerateMultiple(productPageId, languageId, countryId, isTrade, useDate);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsLifestyleGalleryPopupDefinition lifestyleStore = new cmsLifestyleGalleryPopupDefinition
            {
                productPageHeaders = results.GetResult<productPageHeader>().ToList(),
                productPageImages = results.GetResult<searchResultRenderImage>().ToList(),
                productPageAlternateColourways = results.GetResult<productPageAlternateColourway>().ToList(),
                lifestyleBuyOnlines = results.GetResult<lifestyleBuyOnline>().ToList(),
                productPagePrimarys = results.GetResult<productPageProduct>().ToList(),
                productPageRRPs = results.GetResult<productPageRRP>().ToList()
            };

            return lifestyleStore;
        }
    }
}