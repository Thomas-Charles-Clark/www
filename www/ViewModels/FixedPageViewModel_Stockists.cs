using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Stockists;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_Stockists : FixedPageViewModel<StockistCountrySelect>
    {
        public string SearchTerm { get; set; }

        public string CountryCode { get; set; }

        public decimal? Latitude { get; set; }
        
        public decimal? Longitude { get; set; }

        public List<string> SelectedBrands { get; set; }

        public List<string> SelectedProductTypes { get; set; }

        public StockistSearchSelect StockistSearchSelect { get; set; }

        public bool AnyResults
        {
            get
            {
                return StockistSearchSelect.stockistResults.Any();
            }
        }

        public static async Task<FixedPageViewModel_Stockists> CreateAsync(
            StockistCountrySelect objDefinition,
            BaseViewModel objBase)
        {
            var result = new FixedPageViewModel_Stockists(objDefinition,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        protected FixedPageViewModel_Stockists (
            StockistCountrySelect objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {

        }

        private async Task InitialiseAsync()
        {
            #region Preamble

            var objStockists = new Stockist();

            var objCountry = new cmsCountry();

            CountryCode = HttpContext.Current.Request.QueryString["country"] ?? BaseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryCode;

            #endregion

            #region The Bit in the Middle

            var objCountryData = await objCountry.countryLanguageSelect(BaseViewModel.Date);

            var objCountryItem = objCountryData.countryItems.Where(ci => ci.CountryCode == CountryCode && ci.LanguageID == BaseViewModel.LanguageID).SingleOrDefault();

            if (objCountryItem == null)
            {
                objCountryItem = objCountryData.countryItems.Where(ci => ci.CountryID == BaseViewModel.CountryID).Single();
            }

            var useDate = Framework.GetUseDate(new HttpRequestWrapper(HttpContext.Current.Request));


            if (HttpContext.Current.Request.QueryString["brand"] == null)
            {
                SelectedBrands = new List<string>();
            }
            else
            {
                SelectedBrands = HttpContext.Current.Request.QueryString["brand"].Split(new string[] { "," }, StringSplitOptions.None).ToList();
            }

            if (HttpContext.Current.Request.QueryString["product-type"] == null)
            {
                SelectedProductTypes = new List<string>();

                // This was removed 28/01/2026, whatever the reasoning behind this has been forgotten
                // and a Contracts user was dismayed to not find their Palavela Home result in Scottsdale at the top of the list!
                //if (CountryID == 238)
                //{
                //    SelectedProductTypes.Add("fabric");
                //}
            }
            else
            {
                SelectedProductTypes = HttpContext.Current.Request.QueryString["product-type"].Split(new string[] { "," }, StringSplitOptions.None).ToList();
            }

            if (Decimal.TryParse(HttpContext.Current.Request.QueryString["lat"], out decimal lat))
            {
                Latitude = lat;
            }

            if (Decimal.TryParse(HttpContext.Current.Request.QueryString["lng"], out decimal lng))
            {
                Longitude = lng;
            }

            SearchTerm = HttpContext.Current.Request.QueryString["search-term"]?.Trim();

            #endregion

            #region Postamble

            StockistSearchSelect = await objStockists.stockistSearch(BaseViewModel.Visitor.visitorDbId,
                objCountryItem.CountryID,
                useDate,
                new PassStockistSearch()
                {
                    qr = HttpContext.Current.Request.QueryString["qr"] == "1",
                    premiumPartner = HttpContext.Current.Request.QueryString["pp"] == "1",
                    countryCode = CountryCode,
                    latitude = Latitude.HasValue ? (float)Latitude.Value : 0.0f,
                    longitude = Longitude.HasValue ? (float)Longitude.Value : 0.0f
                });

            StockistSearchSelect.stockistResults = new List<StockistResult>();

            #endregion
        }
    }
}