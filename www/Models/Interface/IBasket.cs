using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface IBasket
    {
        Task<AddToBasketReturn> callAddToBasket(Int64 visitorDbId, Int32? memberId, Byte priceListId, Int32? tradeAccountId, Int32 productId, decimal quantity, Int16 countryId, Byte languageId, DateTime useDate, int? productPageId = null, byte basketLineTypeId = 1);
        Task<Boolean> callSaveForLaterBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, Boolean saveForLater);
        Task<BasketSummary> loadLatestBasket(Int64 visitorDbId, Int32? memberId, Int32? tradeAccountId, bool displayTradePrice, byte languageId, short countryId, DateTime useDate);
        Task<Boolean> callUpdateToBasket(Int64 visitorDbId, Int32? memberId, Int32 productId, byte basketLineTypeId, short countryId, decimal quantity, Boolean savedForLaterFlag);
        Task<BasketProductStockSelect> getProductStockStatus(Int64 visitorDbId, Int32? memberId, Int32 productId, short countryId, Boolean savedForLaterFlag);
    }
}
