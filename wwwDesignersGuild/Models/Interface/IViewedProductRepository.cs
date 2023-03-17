using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface IViewedProductRepository
    {
        Task<passViewedProductPage> buildViewedProduct(Int64 visitorDbId, passViewedProductPage objViewedProductPage, bool isTrade, bool displayTradePrice);
        Task<Boolean> mergeViewedProduct(Int64 visitorDbId, int? productPageId, short countryId);
    }
}
