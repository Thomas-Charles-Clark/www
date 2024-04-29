using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface IPromotion
    {
        Task<passPromotionCheck> callCheckPromotionCode(String promotionCode, DateTime useDate, short countryId, byte languageId, Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice);
        Task<Boolean> callDeletePromotionCode(Int64 visitorDbId, Int16 promotionCodeId, short countryId);
    }
}
