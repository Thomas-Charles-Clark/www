using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_FabricWallpaper : ProductPageViewModel
    {
        public static async Task<ProductPageViewModel_FabricWallpaper> CreateAsync(NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop)
        {
            var result = new ProductPageViewModel_FabricWallpaper(objMaster,
                objVisitor,
                objCacheSelect,
                isShop);

            await result.Initialise();

            return result;
        }

        private ProductPageViewModel_FabricWallpaper(NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop) : base(objMaster, objVisitor, objCacheSelect, isShop)
        {

        }

        new protected async Task Initialise()
        {

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

            PrefetchedResources = resourcesResult;

            await base.Initialise();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}