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
using System.Configuration;
using Newtonsoft.Json;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.BasketClasses
{


    public class basketRoutines : IBasket
    {

        #region Add To Basket


        public async Task<AddToBasketReturn> callAddToBasket(Int64 visitorDbId, Int32? memberId, Byte priceListId, Int32? tradeAccountId, Int32 productId, decimal quantity, Int16 countryId, Byte languageId, DateTime useDate, int? productPageId = null, byte basketLineTypeId = 1)
        {
            AddToBasketReturn storeResults = await Task.FromResult<AddToBasketReturn>(callAddToBasketAsync(visitorDbId, memberId, priceListId, tradeAccountId, productId, quantity, countryId, languageId, useDate, productPageId, basketLineTypeId));
            return storeResults;
        }


        private AddToBasketReturn callAddToBasketAsync(Int64 visitorDbId, Int32? memberId, Byte priceListId, Int32? tradeAccountId, Int32 productId, decimal quantity, Int16 countryId, Byte languageId, DateTime useDate, int? productPageId = null, byte basketLineTypeId = 1)
        {
            basketDb objDbBasket = new basketDb();
            return objDbBasket.callAddToBasket(visitorDbId, memberId, priceListId, tradeAccountId, productId, quantity, countryId, languageId, useDate, productPageId, basketLineTypeId);
        }


        #endregion


        #region Update To Basket


        public async Task<Boolean> callUpdateToBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, decimal quantity, Boolean savedForLaterFlag)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(callUpdateToBasketAsync(visitorDbId, memberId, productId, basketLineTypeId, countryId, quantity, savedForLaterFlag));
            return storeResults;
        }


        private Boolean callUpdateToBasketAsync(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, decimal quantity, Boolean savedForLaterFlag)
        {

            basketDb objDbBasket = new basketDb();
            return objDbBasket.callUpdateToBasket(visitorDbId, memberId, productId, basketLineTypeId, countryId, quantity, savedForLaterFlag);

        }


        #endregion


        #region Basket - Save For Later


        public async Task<Boolean> callSaveForLaterBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, Boolean saveForLater)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(callSaveForLaterBasketAsync(visitorDbId, memberId, productId, basketLineTypeId, countryId, saveForLater));
            return storeResults;
        }


        private Boolean callSaveForLaterBasketAsync(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, Boolean saveForLater)
        {

            basketDb objDbBasket = new basketDb();
            return objDbBasket.callSaveForLaterBasket(visitorDbId, memberId, productId, basketLineTypeId, countryId, saveForLater);

        }


        #endregion

        #region Get Bronto JSON
        public async Task<TrackingJsonViewModel> callGetBrontoJSON(Int64 visitorDbId, Int32? memberId, Int32? pageId, Int64? checkoutId, Int16 countryId, Byte languageId, Int32? tradeAccountId, DateTime date)
        {
            TrackingJsonViewModel results = await Task.FromResult<TrackingJsonViewModel>(callGetBrontoJSONAsync(visitorDbId, memberId, pageId, checkoutId, countryId, languageId, tradeAccountId, date));
            return results;
        }

        private TrackingJsonViewModel callGetBrontoJSONAsync(Int64 visitorDbId, Int32? memberId, Int32? pageId, Int64? checkoutId, Int16 countryId, Byte languageId, Int32? tradeAccountId, DateTime date)
        {
            basketDb objDbBasket = new basketDb();
            return objDbBasket.callGetBasketBronto(visitorDbId, memberId, pageId, checkoutId, countryId, languageId, tradeAccountId, date);
        }

        #endregion


        #region Get DotDigital JSON
        public async Task<DotDigitalJSON> callGetDotDigitalJSON(Int64 visitorDbId, Int32? memberId, Int16 countryId, Byte languageId, bool checkoutConfirmation)
        {
            var results = await Task.FromResult<DotDigitalJSON>(callGetDotDigitalJSONAsync(visitorDbId, memberId, countryId, languageId, checkoutConfirmation));
            return results;
        }

        private DotDigitalJSON callGetDotDigitalJSONAsync(Int64 visitorDbId, Int32? memberId, Int16 countryId, Byte languageId, bool checkoutConfirmation)
        {
            basketDb objDbBasket = new basketDb();
            return objDbBasket.callGetBasketDotDigital(visitorDbId, memberId, countryId, languageId, checkoutConfirmation);
        }

        #endregion


        #region Get Basket Total Quantity
        public async Task<BasketTotalQuantity> loadBasketTotalQuantity(Int64 visitorDbId, Int32? memberId, short countryId)
        {
            var basketTotalQuantity = await Task.FromResult<BasketTotalQuantity>(loadBasketTotalQuantityAsync(visitorDbId, memberId, countryId));
            return basketTotalQuantity;
        }

        public BasketTotalQuantity loadBasketTotalQuantityAsync(Int64 visitorDbId, Int32? memberId, short countryId)
        {
            var dataContext = new databaseDataContext();

            var result = dataContext.Basket_TotalQuantity_Select(visitorDbId, memberId, countryId).Single();

            if (result == null)
                result = new BasketTotalQuantity();

            return result;
        }

        #endregion

        #region Get Basket Summary


        public async Task<BasketSummary> loadLatestBasket(Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice, byte languageId, short countryId, DateTime useDate)
        {
            BasketSummary basketStore = await Task.FromResult<BasketSummary>(loadLatestBasketAsync(visitorDbId, memberId, tradeAccountId, displayTradePrice, languageId, countryId, useDate));
            return basketStore;
        }


        private BasketSummary loadLatestBasketAsync(Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice, byte languageId, short countryId, DateTime useDate)
        {


            var dataContext = new databaseDataContext();

            // Totally the wrong procedure to call here; you'll only get the first five results!
            // SoooOOOooo... call the Basket Generate procedure? How much wastage is there going to be 
            // in calling that?
            IMultipleResults results = dataContext.BasketSummary_GenerateMultiple(visitorDbId, memberId, tradeAccountId, countryId, languageId, useDate, displayTradePrice, tradeAccountId.HasValue);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            BasketSummary basketStore = new BasketSummary
            {
                basketTotals = results.GetResult<BasketTotal>().ToList(),
                basketDetails = results.GetResult<BasketDetail>().ToList()
            };

            return basketStore;
        }


        #endregion


        #region Get Basket Line Details 

        public async Task<BasketLineDetails> loadBasketLineDetails(Int64 visitorDbId, Int32? memberId, Int32 productId, byte languageId, short countryId, DateTime useDate)
        {
            BasketLineDetails detailStore = await Task.FromResult<BasketLineDetails>(loadBasketLineDetailsAsync(visitorDbId, memberId, productId, languageId, countryId, useDate));
            return detailStore;
        }

        private BasketLineDetails loadBasketLineDetailsAsync(Int64 visitorDbId, Int32? memberId, Int32 productId, byte languageId, short countryId, DateTime useDate)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.BasketLineDetail_GenerateMultiple(visitorDbId, memberId, productId, countryId, languageId, useDate);

            BasketLineDetails detailStore = new BasketLineDetails
            {
                basketDetails = results.GetResult<BasketDetail>().ToList<BasketDetail>()
            };

            return detailStore;
        }

        #endregion


        #region Get Basket Details


        public async Task<Basket> loadBasketDetail(Int64 visitorDbId, Int32? memberId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool isTrade, bool displayTradePrice)
        {
            Basket basketStore = await Task.FromResult<Basket>(loadBasketDetailAsync(visitorDbId, memberId, languageId, countryId, useDate, memberTradeAccountID, isTrade, displayTradePrice));

            var ptorStore = new cmsProductTypeOrderRestriction();
            _ = await ptorStore.loadProductTypeOrderRestrictionFromCms(memberTradeAccountID, languageId, countryId, useDate);

            return basketStore;
        }


        private Basket loadBasketDetailAsync(Int64 visitorDbId, Int32? memberId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool isTrade, bool displayTradePrice)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.BasketDetail_GenerateMultiple(visitorDbId, memberId, countryId, languageId, useDate, memberTradeAccountID, isTrade, displayTradePrice);

            Basket basketStore = new Basket
            {
                Totals = results.GetResult<BasketTotal>().ToList(),
                BasketItems = results.GetResult<BasketDetail>().ToList(),
                ProductDeliveryOptions = results.GetResult<BasketDeliveryOption>().GroupBy(p => p.DeliveryOptionID).SelectMany(p => p).ToList(),
                Errors = results.GetResult<BasketErrorMessage>().Single(),
                PromotionCodeDetails = results.GetResult<PromotionCodeDetails>().Single(),
                Notices = results.GetResult<string>().SingleOrDefault()
            };

            return basketStore;
        }

        public async Task<BasketGenerateQuick> loadBasketDetailQuick(Int64 visitorDbId, Int32? memberId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool isTrade, bool displayTradePrice)
        {
            var basketStore = await Task.FromResult(loadBasketDetailQuickAsync(visitorDbId, memberId, languageId, countryId, useDate, memberTradeAccountID, isTrade, displayTradePrice));

            return basketStore;
        }


        private BasketGenerateQuick loadBasketDetailQuickAsync(Int64 visitorDbId, Int32? memberId, byte languageId, short countryId, DateTime useDate, int? memberTradeAccountID, bool isTrade, bool displayTradePrice)
        {
            var dataContext = new databaseDataContext();

            IMultipleResults results = dataContext.BasketDetail_GenerateQuickMultiple(visitorDbId, memberId, countryId, languageId, useDate, memberTradeAccountID, isTrade, displayTradePrice);

            var basketStore = new BasketGenerateQuick
            {
                basketTotals = results.GetResult<BasketTotal>().ToList(),
                basketDetails = results.GetResult<BasketDetail>().ToList()
            };

            return basketStore;
        }

        #endregion

        #region Get Promotional Message


        public async Task<storePromotionalMessage> getPromotionalMessage(bool isTrade, Int64 visitorDbId, byte languageId, short countryId, DateTime useDate)
        {
            storePromotionalMessage promotionalMessageStore = await Task.FromResult<storePromotionalMessage>(getPromotionalMessageAsync(isTrade, visitorDbId, languageId, countryId, useDate));
            return promotionalMessageStore;
        }


        private storePromotionalMessage getPromotionalMessageAsync(bool isTrade, Int64 visitorDbId, byte languageId, short countryId, DateTime useDate)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.PromotionalMessages_GenerateMultiple(isTrade, visitorDbId, countryId, languageId, useDate);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            storePromotionalMessage promotionalMessageStore = new storePromotionalMessage
            {
                promotionalMessages = results.GetResult<promotionalMessage>().ToList<promotionalMessage>()
            };
            return promotionalMessageStore;


        }


        #endregion
        
        #region Get Product Stock Status


        public async Task<BasketProductStockSelect> getProductStockStatus(Int64 visitorDbId, Int32? memberId, Int32 productId, short countryId, Boolean savedForLaterFlag)
        {
            BasketProductStockSelect productStock = await Task.FromResult<BasketProductStockSelect>(getProductStockStatusAsync(visitorDbId, memberId, productId, countryId, savedForLaterFlag));
            return productStock;
        }


        private BasketProductStockSelect getProductStockStatusAsync(Int64 visitorDbId, Int32? memberId, Int32 productId, short countryId, Boolean savedForLaterFlag)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.ProductStockSelect_GenerateMultiple(visitorDbId, memberId, productId, countryId, savedForLaterFlag);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            BasketProductStockSelect productStock = new BasketProductStockSelect
            {
                basketProductTotals = results.GetResult<BasketProductTotal>().ToList<BasketProductTotal>(),
                basketStockStatusLines = results.GetResult<StockStatusSelect>().ToList<StockStatusSelect>()
            };
            return productStock;


        }


        #endregion

        #region

        public async Task<bool> memberBasketClone(int sourceMemberId, int destinationMemberId, short countryId)
        {
            return await Task.FromResult<bool>(memberBasketCloneAsync(sourceMemberId, destinationMemberId, countryId));
        }
        private bool memberBasketCloneAsync(int sourceMemberId, int destinationMemberId, short countryId)
        {
            basketDb basketDb = new basketDb();
            return basketDb.callMemberBasketClone(sourceMemberId, destinationMemberId, countryId);
        }
        #endregion
    }
}