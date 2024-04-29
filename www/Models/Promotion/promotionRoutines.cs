using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Linq;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.Promotion
{


    public class promotionRoutines : IPromotion
    {

        #region Check Promotion Code


        public async Task<passPromotionCheck> callCheckPromotionCode(String promotionCode, DateTime useDate, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice)
        {
            passPromotionCheck storeResults = await Task.FromResult<passPromotionCheck>(callCheckPromotionCodeAsync(promotionCode, useDate, countryId, languageId, visitorDbId, memberId, tradeAccountId, displayTradePrice));
            return storeResults;
        }


        private passPromotionCheck callCheckPromotionCodeAsync(String promotionCode, DateTime useDate, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice)
        {

            promotionDb objDbPromotion = new promotionDb();
            return objDbPromotion.callCheckPromotionCode(promotionCode, useDate, countryId, languageId, visitorDbId, memberId, tradeAccountId, displayTradePrice);

        }

        #endregion

        #region Delete Promotion Code


        public async Task<Boolean> callDeletePromotionCode(Int64 visitorDbId, Int16 promotionCodeId, short countryId)
        {
            Boolean storeResults = await Task.FromResult<Boolean>(callDeletePromotionCodeAsync(visitorDbId, promotionCodeId, countryId));
            return storeResults;
        }


        private Boolean callDeletePromotionCodeAsync(Int64 visitorDbId, Int16 promotionCodeId, short countryId)
        {

            promotionDb objDbPromotion = new promotionDb();
            return objDbPromotion.callDeletePromotionCode(visitorDbId, promotionCodeId, countryId);

        }


        #endregion

    }
}