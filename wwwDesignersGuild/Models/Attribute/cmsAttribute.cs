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
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.Models.Attribute
{


    public class cmsAttribute : ICmsAttributeRepository
    {

        public async Task<cmsAttributeDefinition> loadAttributeFromCms(byte languageId)
        {

            //  Are attributes in cache?
            cmsAttributeDefinition attributeStore;
            AttributeCacheProvider objAttributeCache = new AttributeCacheProvider();
            attributeStore = objAttributeCache.Get(languageId);

            if (attributeStore == null)
            {
                attributeStore = await Task.FromResult<cmsAttributeDefinition>(loadAttributeFromCmsDatabaseAsync(languageId));
                objAttributeCache.Set(languageId, attributeStore);
            }

           return attributeStore;


        }


        private cmsAttributeDefinition loadAttributeFromCmsDatabaseAsync(byte languageId)
        {

            
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.Attribute_GenerateMultiple(languageId);

            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            cmsAttributeDefinition attributeStore = new cmsAttributeDefinition
            {
                attributeGroups = results.GetResult<AttributeGroup>().ToList(),
                attributeGroupProductTypes = results.GetResult<AttributeGroup_ProductType>().ToList(),
                attributeDetails = results.GetResult<AttributeDetail>().ToList(),
                productTypes = results.GetResult<ProductType>().ToList()
            };


            return  attributeStore;


        }

    }

}