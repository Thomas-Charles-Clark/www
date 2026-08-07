using Braintree;

using DocumentFormat.OpenXml.Wordprocessing;

using iText.Html2pdf.Attach.Impl.Layout.Form.Renderer;

using MertexerV2.Models;
using MertexerV2.SQLFunctions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using U2.Data.ClientTypes;

using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Cache.ProductPages;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.LandingPage;
using wwwDesignersGuild.Models.ModularFurniture;
using wwwDesignersGuild.Models.OrderForm;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels;

using static Antlr.Runtime.Tree.TreeWizard;

namespace wwwDesignersGuild.Controllers
{
    public class ProductPageAPIController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> fetchModuleDetail([FromBody]PassModuleOption module)
        {
            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

            var objProductPage = new cmsOrderForm();

            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;

            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter, $"visitorIdFromDatabase was null!");
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;

            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            int? tradeAccountId = null;

            if (session["memberTradeChecksPassed"] != null)
            {
                if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
                {
                    tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"]);
                }
            }

            var furnitureOrderForm = await objProductPage.loadOrderFormFromCms(
                    visitorDbId,
                    memberId,
                    module.ProductPageID,
                    module.LanguageID,
                    module.CountryID,
                    tradeAccountId,
                    Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                    HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
                    Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                    useDate);

            string render = null;

            if (module.FabricProductPageID.HasValue)
            {
                var fabricOrderForm = await objProductPage.loadOrderFormFromCms(
                        visitorDbId,
                        memberId,
                        module.FabricProductPageID.Value,
                        module.LanguageID,
                        module.CountryID,
                        tradeAccountId,
                        Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]),
                        HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle"),
                        Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]),
                        useDate);


                var objBasket = new basketRoutines();

                var basket = await objBasket.loadBasketDetailQuick(
                    visitorDbId,
                    memberId,
                    module.LanguageID,
                    module.CountryID,
                    useDate,
                    tradeAccountId,
                    tradeAccountId.HasValue,
                    Convert.ToBoolean(HttpContext.Current.Session["memberDisplayTradePrice"]));

                var orderFormItemViewModel = new OrderFormItemViewModel(
                    JObject.FromObject(fabricOrderForm.orderForm).SelectToken("products")?.FirstOrDefault(),
                    module.MasterDefinition,
                    new BaseViewModel(verifiedBot: false)
                    {
                        Basket = basket,
                        MasterDefinition = module.MasterDefinition
                    }
                );

                render = dgFramework.RenderViewToString("NewSection", "~/Views-New/ProductPages/Shared/_OrderFormItem.cshtml", orderFormItemViewModel);
            }

            return Ok(new { 
                furnitureOrderForm,
                render
            });
        }

   //     [HttpPost]
   //     public async Task<IHttpActionResult> FetchPageSectionDefinition([FromBody]ProductPageGrid_PagingDTO dto)
   //     {
			//var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));

			//var session = HttpContext.Current.Session;

			//Int32? memberId = null;

			//if (session["memberIdFromDatabase"] == null)
			//{

			//}
			//else
			//{
			//	memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
			//}

			//int? tradeAccountId = null;

			//if (session["memberTradeChecksPassed"] != null)
			//{
			//	if (Convert.ToBoolean(session["memberTradeChecksPassed"]) == true)
			//	{
			//		tradeAccountId = Convert.ToInt32(session["memberTradeAccountID"].ToString());
			//	}
			//}

   //         var displayTradePrice = Convert.ToBoolean(session["memberDisplayTradePrice"]);
            
   //         var countryMatched = Convert.ToBoolean(HttpContext.Current.Session["countryCheckMatched"]);

			//dto.TradeAccountID = tradeAccountId;
   //         dto.IsTrade = dto.TradeAccountID.HasValue;
   //         dto.DisplayTradePrice = displayTradePrice;
   //         dto.MemberID = memberId;
   //         dto.IsMember = dto.MemberID.HasValue;
   //         dto.UseDate = useDate;
   //         dto.CountryMatch = countryMatched;
   //         dto.DebugProductCode = null;
			//dto.Debug = false;

			//var Definition = await new cmsLandingPage().loadLandingPageFromCms(
			//   memberId: dto.MemberID,
			//   pageId: dto.PageID,
			//   languageId: dto.LanguageID,
			//   countryId: dto.CountryID,
			//   useDate: dto.UseDate,
			//   isMember: dto.IsMember,
			//   isTrade: dto.IsTrade,
			//   countryMatch: dto.CountryMatch,
			//   displayTradePrice: dto.DisplayTradePrice,
			//   tradeAccountId: dto.TradeAccountID);

   //         var ppg = Definition.productPageGridSections.Single(p => p.PageSectionID == dto.PageSectionID);

   //         var pageSection = Definition.pageSectionSelects.Single(p => p.PageSectionID == dto.PageSectionID);

   //         var ps = new PageSectionViewModel(
   //             pageSection: pageSection,
   //             baseViewModel: null,
   //             sectionDefinition: (object)await ProductPageGridPageSection.CreateAsync(
   //                 originalUrlKeywords: Definition.PageDetails.URLKeywords,
   //                 productPage: Definition.productPageGridSections.Single(p => p.PageSectionID == dto.PageSectionID),
   //                 pagingEnabled: pageSection.Paging || pageSection.Filter,
   //                 filterIndexingEnabled: Definition.PageDetails.EnableFilterIndexing,
   //                 isShop: Definition.PageDetails.IsShop,
   //                 isLifestyleGallery: pageSection.IsLifestyleGallery,
   //                 model: dto.BaseViewModel
   //             ),
   //             isShop: Definition.PageDetails.IsShop);

			//return Ok(ps);
   //     }
    }
}