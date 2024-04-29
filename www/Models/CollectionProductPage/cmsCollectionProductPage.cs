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


namespace wwwDesignersGuild.Models.CollectionProductPage
{
    //public class cmsCollectionProductPage : ICmsCollectionProductPageRepository
    //{


    //    public async Task<cmsCollectionProductPageDefinition> loadCollectionProductPageFromCms(short collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort, Int16 CacheTimeMinutes)
    //    {

    //        //  Is collection product in cache?
    //        cmsCollectionProductPageDefinition collectionProductPageStore;
    //        CollectionProductPageCacheProvider objCollectionProductPageCache = new CollectionProductPageCacheProvider();
    //        collectionProductPageStore = objCollectionProductPageCache.Get(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);
            
    //        if (collectionProductPageStore == null)
    //        {
    //            collectionProductPageStore = await Task.FromResult<cmsCollectionProductPageDefinition>(loadCollectionProductPageFromCmsDatabase(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort));
    //            objCollectionProductPageCache.Set(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort, collectionProductPageStore, CacheTimeMinutes);
    //        }

    //        return collectionProductPageStore;

    //    }


    //    private cmsCollectionProductPageDefinition loadCollectionProductPageFromCmsDatabase(short collectionId, byte languageId, short countryId, DateTime useDate, byte? brandId, byte? productTypeId, string sort)
    //    {

            
    //        var dataContext = new databaseDataContext();


    //        IMultipleResults results = dataContext.CollectionProduct_GenerateMultiple(collectionId, languageId, countryId, useDate, brandId, productTypeId, sort);


    //        //  Convert from IEnumerable to a List so a collection query can be run multiple times
    //        cmsCollectionProductPageDefinition collectionProductStore = new cmsCollectionProductPageDefinition();
            
    //        collectionProductStore.collectionAlls = results.GetResult<collectionAll>().ToList<collectionAll>();
    //        collectionProductStore.collectionHeaders = results.GetResult<collectionHeader>().ToList<collectionHeader>();
    //        collectionProductStore.collectionProducts = results.GetResult<collectionProduct>().ToList<collectionProduct>();
    //        collectionProductStore.collectionProductTypes = results.GetResult<collectionProductType>().ToList<collectionProductType>();
    //        collectionProductStore.collectionBrands = results.GetResult<collectionBrand>().ToList<collectionBrand>();
    //        collectionProductStore.pageSectionBanners = results.GetResult<pageSectionBanner>().ToList<pageSectionBanner>();

            
    //        //  Always ensure at least one entry
    //        //if (collectionProductStore.collectionHeaders.Count == 0)
    //        //{
    //        //    collectionHeader objCollection = new collectionHeader();
    //        //    objCollection.Heading = "unknown";
    //        //    objCollection.SubHeading = "unknown";
    //        //    objCollection.TitleTag = "unknown";
    //        //    objCollection.MetaDescription = "unknown";
    //        //    collectionProductStore.collectionHeaders.Add(objCollection);
    //        //}


    //        return collectionProductStore;


    //    }

    //}
}