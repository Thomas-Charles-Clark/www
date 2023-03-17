using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_Standard : ProductPageViewModel
    {
        public static async Task<ProductPageViewModel_Standard> CreateAsync(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop)
        {
            var result = new ProductPageViewModel_Standard(objMaster,
                objVisitor,
                objCacheSelect,
                isShop);

            await result.Initialise();

            if (!result.AllProductsUnavailable && !result.OtherProducts.Any())
            {
                result.MainProduct.Solitary = true;
            }

            return result;
        }

        private ProductPageViewModel_Standard(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop) : base(
            objMaster,
            objVisitor,
            objCacheSelect,
            isShop)
        {
            UseOrderFormGrouping = false;
            UseSizeButtons = false;
        }

        new protected async Task Initialise()
        {
            await base.Initialise();
        }

        //public override string ToString()
        //{
        //    return JsonConvert.SerializeObject(this, Formatting.Indented);
        //}
    }
}