using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;

using wwwDesignersGuild.Models.Database;

namespace wwwDesignersGuild.Models.OrderForm
{
	public class cmsOrderForm
	{
		public async Task<cmsRealTimeStock> loadOrderFormFromCms(long visitorId, int? memberId, int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
		{
			//  Is result already in cache?
			cmsRealTimeStock stockResult = null;

			var cache = new OrderFormCacheProvider();

			stockResult = cache.GetOrCreate(productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate, () => {

				var result = Task.Run(() => loadOrderFormFromDatabase(visitorId, memberId, productPageId, languageId, countryId, tradeAccountId, displayTradePrice, isLifestyle, countryMatch, useDate)).Result;

				return result;

			});

			return stockResult;
		}

		private cmsRealTimeStock loadOrderFormFromDatabase(long visitorId, int? memberId, int productPageId, byte languageId, short countryId, int? tradeAccountId, bool displayTradePrice, bool isLifestyle, bool countryMatch, DateTime useDate)
		{
			var dataContext = new databaseDataContext();

			var result = dataContext.ProductPage_OrderForm_Select(visitorId, memberId, productPageId, languageId, countryId, useDate, tradeAccountId, displayTradePrice, tradeAccountId.HasValue, isLifestyle, countryMatch);

			var realtimeStockStore = result.SingleOrDefault();

			return realtimeStockStore;
		}
	}
}