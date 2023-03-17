using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Models.ViewedProduct
{


    public class cmsViewedProduct : IViewedProductRepository
    {

        #region Read Routines


        public async Task<passViewedProductPage> buildViewedProduct(Int64 visitorDbId, passViewedProductPage objViewedProductPage, bool isTrade, bool displayTradePrice)
        {
            passViewedProductPage storeResults = await Task.FromResult<passViewedProductPage>(buildViewedProductAsync(visitorDbId, objViewedProductPage, isTrade, displayTradePrice));
            
            var stockStatus = new cmsStockStatus();
            var stockStatuses = await stockStatus.loadStockStatusesFromCms(
                objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.LanguageID, 
                objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.CountryID, 
                objViewedProductPage.masterDefinition.UrlBreakdown.ContentDate);

            storeResults.viewedProductCores.ForEach(c => 
            {
                c.StockStatus = stockStatuses.StockStatuses.Where(p => p.StockStatusID == c.StockStatusID).SingleOrDefault()?.StockStatus;
            });
            
            return storeResults;
        }


        private passViewedProductPage buildViewedProductAsync(Int64 visitorDbId, passViewedProductPage objViewedProductPage, bool isTrade, bool displayTradePrice)
        {

            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.ProductPageHistory_GenerateMultiple(visitorDbId,
                                                                                        objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                                        objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                                        objViewedProductPage.ProductPageID,
                                                                                        objViewedProductPage.masterDefinition.UrlBreakdown.ContentDate,
                                                                                        (Int32?)HttpContext.Current.Session["memberTradeAccountID"],
                                                                                        HttpContext.Current.Session["memberIdFromDatabase"] != null,
                                                                                        isTrade,
                                                                                        displayTradePrice);


            //  Convert from IEnumerable to a List so a collection query can be run multiple times
            objViewedProductPage.viewedProductCores = results.GetResult<searchResultRenderCore>().ToList();
            objViewedProductPage.viewedProductImages = results.GetResult<searchResultRenderImage>().ToList();

            return objViewedProductPage;
            //return objDbViewedProducts.generateViewedProductResults(objViewedProductPage);
           
        }


        #endregion


        #region Merge Routines


        public async Task<Boolean> mergeViewedProduct(Int64 visitorDbId, int? productPageId, short countryId)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(mergeViewedProductAsync(visitorDbId, productPageId, countryId));
            return storeResults;
        }


        private Boolean mergeViewedProductAsync(Int64 visitorDbId, int? productPageId, short countryId)
        {

            viewedProductDb objDbViewedProducts = new viewedProductDb();
            return objDbViewedProducts.productPageHistoryMerge(visitorDbId, productPageId, countryId);

        }


        #endregion


    }


}