using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;
using wwwDesignersGuild.Models.Snippet;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_Paint : ProductPageViewModel
    {
        public OrderFormItemViewModel SamplePot 
        {
            get
            {
                var samplePot = AllProducts.Where(p => p.Attributes != null && p.Attributes.Select(a => a.AttributeID).Contains(605)).SingleOrDefault();

                if (samplePot != null)
                {
                    samplePot.Solitary = true;
                }

                return samplePot;
            }
        }

        public List<AttributeGroup> AttributeGroups { get; set; }

        public IEnumerable<AttributeDetail> Attributes
        {
            get
            {
                return AttributeGroups.SelectMany(p => p.Attributes);
            }
        }

        public List<int> AttributeIDs 
        { 
            get
            {
                return AllProducts.Where(p => p.Attributes != null).SelectMany(p => p.Attributes).Select(a => a.AttributeID).Distinct().ToList();
            } 
        }

        public static async Task<ProductPageViewModel_Paint> CreateAsync(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop)
        {
            var result = new ProductPageViewModel_Paint(objMaster,
                objVisitor,
                objCacheSelect,
                isShop);

            await result.Initialise();

            return result;
        }

        private ProductPageViewModel_Paint(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop) : base(
            objMaster,
            objVisitor,
            objCacheSelect,
            isShop)
        {
            UsePaintOrderForm = true;
            UseOrderFormGrouping = false;
            UseSizeButtons = false;
        }

        new protected async Task Initialise()
        {
            await base.Initialise(injectProductNameIntoProperties: false);

            var objSnippets = new cmsSnippet();

            var objSnippetLoad = objSnippets.loadSnippetFromCms(
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                Visitor.memberTradeAccountID,
                Visitor.memberDisplayTradePrice,
                MasterDefinition.UrlBreakdown.ContentDate);

            var objAttribute = new cmsAttribute();

            var objAttributeLoad = objAttribute.loadAttributeFromCms(MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

            Task.WaitAll(objAttributeLoad, objSnippetLoad);

            var objAttributeDefinitions = await objAttributeLoad;

            var objSnippetDefinitions = await objSnippetLoad;

            var attributeGroupIds = objAttributeDefinitions.attributeDetails
                .Where(a => AttributeIDs.Contains(a.AttributeID))
                .Select(a => a.AttributeGroupID)
                .Distinct()
                .ToList();

            AttributeGroups = objAttributeDefinitions.attributeGroups.Where(p => attributeGroupIds.Contains(p.AttributeGroupID)).ToList();

            AttributeGroups.ForEach(ag => 
            {
                ag.Attributes = objAttributeDefinitions.attributeDetails.Where(a => a.AttributeGroupID == ag.AttributeGroupID).ToList();
            });

            PrefetchedSnippets = objSnippetDefinitions.snippetRows.Where(s => s.SnippetLocationID == 85).ToList();

            var resourceDefinition = new cmsResource().loadResourceFromCms(
                LanguageID,
                CountryID,
                Date);

            var resourcesResult = new List<ResourceResult>();

            var required = new List<string>()
                    {
                        "PleaseProvideWidth",
                        "PleaseProvideHeight",
                        "PleaseProvideLength",
                        "Width",
                        "Length",
                        "Height",
                        "CalculatorMustBeBetween",
                        "FormsErrorEmailFormat",
                        "FormsErrorEmail",
                        "ThankYou",
                        "ThankYouEmailSent"
                    };

            foreach (var r in required)
            {
                resourcesResult.Add(new ResourceResult()
                {
                    Name = r,
                    Value = resourceDefinition.Result.resourceSelects
                                                     .Where(p => p.Name == r)
                                                     .Select(p => p.Value)
                                                     .SingleOrDefault()
                });
            }

            PrefetchedResources.AddRange(resourcesResult);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}