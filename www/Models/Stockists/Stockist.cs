using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Models.Stockists
{
    public class Stockist : IStockist
    {

        #region Stockist Country


        public async Task<StockistCountrySelect> getStockistCountries(DateTime useDate)
        {
            StockistCountrySelect stockistSearchStore = await Task.FromResult<StockistCountrySelect>(getStockistCountriesAsync(useDate));
            return stockistSearchStore;
        }


        private StockistCountrySelect getStockistCountriesAsync(DateTime useDate)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.StockistCountryList_GenerateMultiple(useDate);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            StockistCountrySelect stockistSearchStore = new StockistCountrySelect
            {
                stockistCountries = results.GetResult<StockistCountry>().ToList<StockistCountry>()
            };


            return stockistSearchStore;


        }


        #endregion


        #region Stockist Search


        public async Task<StockistSearchSelect> stockistSearch(Int64 visitorDbId, short countryId, DateTime useDate, PassStockistSearch objStockistSearch)
        {
            StockistSearchSelect stockistSearchStore = await Task.FromResult<StockistSearchSelect>(stockistSearchAsync(visitorDbId, countryId, useDate, objStockistSearch));
            return stockistSearchStore;
        }


        private StockistSearchSelect stockistSearchAsync(Int64 visitorDbId, short countryId, DateTime useDate, PassStockistSearch objStockistSearch)
        {
            var dataContext = new databaseDataContext();

            Boolean isMobile = dgFramework.IsMobileDevice();
            IMultipleResults results = dataContext.StockistSearch_GenerateMultiple(
                visitorDbId,
                countryId,
                useDate,
                objStockistSearch.latitude,
                objStockistSearch.longitude,
                objStockistSearch.selectedBrands.Contains("dg"),
                objStockistSearch.selectedBrands.Contains("dgu"),
                objStockistSearch.selectedBrands.Contains("wy"),
                objStockistSearch.selectedBrands.Contains("rc"),
                objStockistSearch.selectedBrands.Contains("cl"),
                objStockistSearch.selectedBrands.Contains("rl"),
                objStockistSearch.selectedBrands.Contains("jd"),
                objStockistSearch.selectedBrands.Contains("eh"),
                objStockistSearch.selectedProductTypes.Contains("fabric"),
                objStockistSearch.selectedProductTypes.Contains("accessories"),
                objStockistSearch.selectedProductTypes.Contains("rugs"),
                objStockistSearch.selectedProductTypes.Contains("furniture"),
                objStockistSearch.selectedProductTypes.Contains("wallpaper"),
                objStockistSearch.selectedProductTypes.Contains("bed-and-bath"),
                objStockistSearch.selectedProductTypes.Contains("paint"),
                isMobile,
                objStockistSearch.premiumPartner,
                objStockistSearch.qr);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            StockistSearchSelect stockistSearchStore = new StockistSearchSelect
            {
                featuredStockists = results.GetResult<FeaturedStockist>().ToList<FeaturedStockist>(),
                stockistResults = results.GetResult<StockistResult>().ToList<StockistResult>()
            };


            return stockistSearchStore;


        }


        #endregion


        #region Stockist Detail


        public async Task<StockistDetail> stockistDetail(Int32 stockistId, DateTime useDate)
        {
            StockistDetail stockistSearchStore = await Task.FromResult<StockistDetail>(stockistDetailAsync(stockistId, useDate));
            return stockistSearchStore;
        }


        private StockistDetail stockistDetailAsync(Int32 stockistId, DateTime useDate)
        {
            return new databaseDataContext().StockistDetail_GenerateMultiple(stockistId, useDate).Single();
        }

        #endregion
    }

    public class StockistDetailSelect
    {
        public List<StockistDetail> stockistDetails { get; set; }
    }

    public class StockistDetail
    {
        public int StockistID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Image { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string GoogleMap { get; set; }
        public string ServiceOffer { get; set; }
        public string Info { get; set; }
        public bool ShowOpeningTimes { get; set; }
        public string OpenMonday { get; set; }
        public string ClosedMonday { get; set; }
        public string OpenTuesday { get; set; }
        public string ClosedTuesday { get; set; }
        public string OpenWednesday { get; set; }
        public string ClosedWednesday { get; set; }
        public string OpenThursday { get; set; }
        public string ClosedThursday { get; set; }
        public string OpenFriday { get; set; }
        public string ClosedFriday { get; set; }
        public string OpenSaturday { get; set; }
        public string ClosedSaturday { get; set; }
        public string OpenSunday { get; set; }
        public string ClosedSunday { get; set; }
        public bool PremiumPartner { get; set; }
    }


    public class StockistCountrySelect
    {
        public List<StockistCountry> stockistCountries { get; set; }
    }


    public class StockistCountry
    {
        public short CountryID { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
    }


    public class StockistSearchSelect
    {
        public List<FeaturedStockist> featuredStockists { get; set; }
        public List<StockistResult> stockistResults { get; set; }
    }


    public class FeaturedStockist
    {
        public long SortOrder { get; set; }
        public int StockistID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Image { get; set; }

        public bool PremiumPartner { get; set; }

        public string AddressDisplay
        {
            get
            {
                var arr = new List<string>();

                if (!String.IsNullOrEmpty(Address))
                    arr.Add(Regex.Replace(Address, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(City))
                    arr.Add(Regex.Replace(City, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(County))
                    arr.Add(Regex.Replace(County, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(Postcode))
                    arr.Add(Regex.Replace(Postcode, "/\\s+", " ").Replace(",", "").Trim());

                return String.Join(", ", arr);
            }
            set
            {

            }
        }
    }


    public class StockistResult
    {
        public long SortOrder { get; set; }
        public decimal Distance { get; set; }
        public int StockistID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public string Telephone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Facebook { get; set; }
        public string Twitter { get; set; }
        public string Instagram { get; set; }
        public string Image { get; set; }

        public bool PremiumPartner { get; set; }

        public bool DG { get; set; }
        public bool CL { get; set; }
        public bool RC { get; set; }
        public bool RL { get; set; }
        public bool WY { get; set; }
        public bool JD { get; set; }
        public bool EH { get; set; }

        public string AddressDisplay 
        { 
            get
            {
                var arr = new List<string>();

                if (!String.IsNullOrEmpty(Address))
                    arr.Add(Regex.Replace(Address, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(City))
                    arr.Add(Regex.Replace(City, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(County))
                    arr.Add(Regex.Replace(County, "/\\s+", " ").Replace(",", "").Trim());

                if (!String.IsNullOrEmpty(Postcode))
                    arr.Add(Regex.Replace(Postcode, "/\\s+", " ").Replace(",", "").Trim());

                return String.Join(", ", arr);
            }
            set
            {

            }
        }
    }

    public class PassStockistSearch
    {
        public bool qr { get; set; }
        public bool premiumPartner { get; set; }
        public string countryCode { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public List<string> selectedBrands { get; set; }
        public List<string> selectedProductTypes { get; set; }

        public PassStockistSearch()
        {
            selectedBrands = new List<string>();
            selectedProductTypes = new List<string>();
        }
    }

    public class passStockistsContractDetail
    {
        public NewMasterDefinition MasterDefinition { get; set; }
        public StockistCountrySelect StockistCountrySelect { get; set; }
        public stockistsContractSearchSelect StockistsContractSearchSelect { get; set; }
        public short DefaultCountryForDropDown { get; set; }
    }


    public class PassStockistDetail
    {
        public int stockistId { get; set; }
    }
}