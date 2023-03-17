using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ModularFurniture;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_Furniture_Modular : ProductPageViewModel
    {
        public bool SingleProduct
        {
            get
            {
                return OrderFormGroups?.Count() == 1 && OrderFormGroups?.First().Items.Count() == 1;
            }
        }

        public List<Module> Modules { get; set; }

        public static async Task<ProductPageViewModel_Furniture_Modular> CreateAsync(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop)
        {
            var result = new ProductPageViewModel_Furniture_Modular(objMaster,
                objVisitor,
                objCacheSelect,
                isShop);

            await result.Initialise();

            return result;
        }

        private ProductPageViewModel_Furniture_Modular(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop) : base(
            objMaster,
            objVisitor,
            objCacheSelect,
            isShop)
        {
            UseOrderFormGrouping = true;
            UseSizeButtons = true;
        }

        new protected async Task Initialise()
        {
            var ppcms = new cmsProductPage();

            var modularInfo = await ppcms.loadModularInfoFromCms(Definition.productPageId,
                MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                MasterDefinition.UrlBreakdown.ContentDate,
                Convert.ToInt16(System.Web.Configuration.WebConfigurationManager.AppSettings["keepCacheInMinutes"]));

            if (modularInfo.json != null)
            {
                Modules = JObject.FromObject(modularInfo.json).SelectToken("modules").ToObject<List<Module>>();

                Modules.ForEach(m =>
                {
                    m.Current = m.Options.Where(o => o.ProductPageID == Definition.productPageId).Single();
                });
            }

            await base.Initialise();

            if (SingleProduct)
            {
                UseOrderFormGrouping = false;
                UseSizeButtons = false;
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}