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
    public class cmsAttribute
    {
        public async Task<cmsAttributeDefinition> loadAttributeFromCms(byte languageId)
        {
            cmsAttributeDefinition attributeStore;

            var objAttributeCache = new AttributeCacheProvider();

            attributeStore = objAttributeCache.GetOrCreate(languageId, () => {

                var result = Task.Run(() => loadAttributeFromCmsDatabaseAsync(languageId)).Result;

                return result;

            });

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
                productTypes = results.GetResult<ProductType>().ToList(),
                attributeGroupGroups = results.GetResult<AttributeGroupGroup>().ToList(),
            };


            return  attributeStore;


        }

    }

}