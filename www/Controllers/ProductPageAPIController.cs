using iText.Html2pdf.Attach.Impl.Layout.Form.Renderer;

using MertexerV2.SQLFunctions;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
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
                    new BaseViewModel()
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
    }
}