using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Resource;
using wwwDesignersGuild.Models.ResourceFetch;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_404 : BaseViewModel
    {
        public string RedirectURL { get; set; }

        public static async Task<ProductPageViewModel_404> CreateAsync(
            string redirectUrl,
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            bool isShop,
            bool verifiedBot)
        {
            objMaster.TitleTag = "404 - Designers Guild";

            var result = new ProductPageViewModel_404(
                redirectUrl,
                objMaster,
                objVisitor,
                isShop,
                verifiedBot);

            await result.Initialise();

            return result;
        }

        private ProductPageViewModel_404(
            string redirectUrl, 
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            bool isShop,
            bool verifiedBot) : base(objMaster, objVisitor, verifiedBot)
        {
            RedirectURL = redirectUrl;
        }

        protected async Task Initialise()
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

            PrefetchedResources.AddRange(resourcesResult);

            await base.Initialise(VerifiedBot);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class ProductPage404Definition
    {
        public object json { get; set; }

        public ProductPage404Definition()
        {
            json = new object();
        }
    }
}