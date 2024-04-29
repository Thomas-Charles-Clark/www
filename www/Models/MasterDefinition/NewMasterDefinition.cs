using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.LandingPage;

namespace wwwDesignersGuild.Models.Classes
{
    public class NewMasterDefinition
    {
        public byte? SiteSectionID { get; set; }
        public string TitleTag { get; set; }
        public string MetaDescription { get; set; }
        public int? BackInStockReminder { get; set; }
        public decimal ItemsInBasket { get; set; }
        public urlBreakdown UrlBreakdown { get; set; }
        public List<googleHref> GoogleHrefs { get; set; }

        [JsonIgnore]
        public cmsMenuDefinition Menu { get; set; }

        public cmsCountryDropDownDefinition CountryDropDown { get; set; }
        public cmsSearchProductTypeDropDownDefinition ProductTypeDropDown { get; set; }

        public bool TradeMember { get; set; }

        public bool TradeAllowed
        {
            get
            {
                return
                    TradeMember
                    && Convert.ToBoolean(System.Web.HttpContext.Current.Session["tradeOrderingAllowed"])
                    && Convert.ToBoolean(System.Web.HttpContext.Current.Session["memberTradeChecksPassed"]);
            }
        }

        public bool CanViewRRPs
        {
            get
            {
                return Convert.ToBoolean(System.Web.HttpContext.Current.Session["canViewRRPs"]);
            }
        }

        public dgLoginStatus LoginStatus
        {
            get
            {
                return (dgLoginStatus)Enum.Parse(typeof(dgLoginStatus), (string)HttpContext.Current.Session["loginStatus"]);
            }
        }

        public string UserName
        {
            get
            {
                return (string)HttpContext.Current.Session["memberName"];
            }
        }

        public NewMasterDefinition()
        {

        }

        public static async Task<NewMasterDefinition> CreateAsync(
            string titleTag,
            string metaDescription,
            int? backInStockReminder,
            decimal itemsInBasket,
            urlBreakdown urlBreakdown,
            List<googleHref> googleHrefs,
            bool tradeMember)
        {
            var result = new NewMasterDefinition(titleTag,
                metaDescription,
                backInStockReminder,
                itemsInBasket,
                urlBreakdown,
                googleHrefs,
                tradeMember);

            await result.Initialise();

            return result;
        }

        public static NewMasterDefinition Create(
            string titleTag,
            string metaDescription,
            int? backInStockReminder,
            decimal itemsInBasket,
            urlBreakdown urlBreakdown,
            List<googleHref> googleHrefs,
            bool tradeMember)
        {
            var result = new NewMasterDefinition(titleTag,
                metaDescription,
                backInStockReminder,
                itemsInBasket,
                urlBreakdown,
                googleHrefs,
                tradeMember);

            result.InitialiseSync();

            return result;
        }

        private NewMasterDefinition(
            string titleTag,
            string metaDescription,
            int? backInStockReminder,
            decimal itemsInBasket,
            urlBreakdown urlBreakdown,
            List<googleHref> googleHrefs,
            bool tradeMember)
        {
            TitleTag = titleTag;
            MetaDescription = metaDescription;
            BackInStockReminder = backInStockReminder;
            ItemsInBasket = itemsInBasket;
            UrlBreakdown = urlBreakdown;
            GoogleHrefs = googleHrefs;
            TradeMember = tradeMember;
        }

        private async Task Initialise()
        {
            var menuTask = new Menu.cmsMenu().loadMenuFromCms(
                UrlBreakdown.countryDetails.LanguageID,
                UrlBreakdown.countryDetails.CountryID,
                UrlBreakdown.ContentDate,
                TradeMember);

            var productTypeDropDownTask = new Search.cmsSearchProductType().loadSearchProductTypeDropDownFromCms(
                UrlBreakdown.countryDetails.CountryID,
                UrlBreakdown.countryDetails.LanguageID,
                UrlBreakdown.ContentDate);

            var countryDropDownTask = new cmsCountry().countryDropDownSelectList(
                UrlBreakdown.ContentDate,
                UrlBreakdown.countryDetails.LanguageID);

            var siteSectionTask = Task.Run(() => {
                return (byte?)null;
            });

            // No template banners on product pages
            switch (UrlBreakdown.ContentType)
            {
                case dgContentType.landingPage:
                        siteSectionTask = new cmsLandingPage().loadSiteSectionFromCms(
                            UrlBreakdown.ContentId,
                            UrlBreakdown.ContentDate
                        );
                    break;
                case dgContentType.articlePage:
                    SiteSectionID = 13;
                    break;
                case dgContentType.productPage:
                case dgContentType.collectionPage:
                case dgContentType.checkoutPart:
                case dgContentType.invalid:
                    SiteSectionID = null;
                    break;
            }

            await Task.WhenAll(menuTask, productTypeDropDownTask, countryDropDownTask, siteSectionTask);

            Menu = await menuTask;

            ProductTypeDropDown = await productTypeDropDownTask;

            CountryDropDown = await countryDropDownTask;

            SiteSectionID = await siteSectionTask;
        }

        private void InitialiseSync()
        {
            Menu = new Menu.cmsMenu().loadMenuFromCms(
                UrlBreakdown.countryDetails.LanguageID,
                UrlBreakdown.countryDetails.CountryID,
                UrlBreakdown.ContentDate,
                TradeMember).Result;

            ProductTypeDropDown = new Search.cmsSearchProductType().loadSearchProductTypeDropDownFromCms(
                UrlBreakdown.countryDetails.CountryID,
                UrlBreakdown.countryDetails.LanguageID,
                UrlBreakdown.ContentDate).Result;

            CountryDropDown = new cmsCountry().countryDropDownSelectList(
                UrlBreakdown.ContentDate,
                UrlBreakdown.countryDetails.LanguageID).Result;
        }
    }
}