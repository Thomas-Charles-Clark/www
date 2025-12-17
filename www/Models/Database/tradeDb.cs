using Dapper;

using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Trade.Accounting;
using System.Data;
using WebGrease.Css.Extensions;

namespace wwwDesignersGuild.Models.Database
{
	public partial class databaseDataContext
	{
		public Trade_Enquiry_AccountBalance_Result Trade_Enquiry_AccountBalance(Trade_Enquiry_AccountBalance_Request dto)
		{
			var db = new databaseDataContext();

			var response = db.Connection.QueryMultiple(@"pub.Trade_Enquiry_AccountBalance", dto, null, 30, CommandType.StoredProcedure);

			return new Trade_Enquiry_AccountBalance_Result()
			{
				CurrencyHeading = response.ReadSingle<string>(),
				RecordsByDocumentDate = response.Read<Trade_Enquiry_AccountBalance_Record>(),
				RecordsByDueDate = response.Read<Trade_Enquiry_AccountBalance_Record>(),
				OnAccount = response.ReadSingle<Trade_Enquiry_OnAccount_Record>()
			};
		}

		public Trade_Enquiry_OnAccountDrilldown_Result Trade_Enquiry_OnAccountDrilldown(Trade_Enquiry_InvoiceSearch_Request dto)
		{
			var db = new databaseDataContext();

			var response = db.Connection.QueryMultiple("pub.Trade_Enquiry_OnAccountDrilldown", dto, null, null, CommandType.StoredProcedure);

			return new Trade_Enquiry_OnAccountDrilldown_Result()
			{
				Records = response.Read<Trade_Enquiry_OnAccountDrilldown_Record>()
			};
		}

		public Trade_Enquiry_InvoiceSearch_Result Trade_Enquiry_InvoiceSearch(Trade_Enquiry_InvoiceSearch_Request dto)
		{
			var db = new databaseDataContext();

			var response = db.Connection.QueryMultiple("pub.Trade_Enquiry_InvoiceSearch", new
			{
				dto.TradeAccountID,
				dto.DateQualifier,
				dto.CountryID,
				dto.LanguageID,
				DateRangeStart = dto.DateRangeStart?.Date,
				DateRangeEnd = dto.DateRangeEnd?.Date,
				dto.SearchTerm,
				dto.Outstanding
			}, null, null, CommandType.StoredProcedure);

			return new Trade_Enquiry_InvoiceSearch_Result()
			{
				Records = response.Read<Trade_Enquiry_InvoiceSearch_Record>().ToList(),
				OnAccount = response.ReadSingle<Trade_Enquiry_OnAccount_Record>()
			};
		}

		public Trade_Enquiry_InvoiceSearch_Result Trade_Enquiry_AccountBalance_Drilldown(Trade_Enquiry_InvoiceSearch_Request dto)
		{
			var db = new databaseDataContext();

			var response = db.Connection.QueryMultiple("pub.Trade_Enquiry_AccountBalance_Drilldown", new
			{
				dto.TradeAccountID,
				dto.DateQualifier,
				dto.CountryID,
				dto.LanguageID,
				DateRangeStart = dto.DateRangeStart?.Date,
				DateRangeEnd = dto.DateRangeEnd?.Date
			}, null, null, CommandType.StoredProcedure);

			return new Trade_Enquiry_InvoiceSearch_Result()
			{
				Records = response.Read<Trade_Enquiry_InvoiceSearch_Record>().ToList()
			};
		}

		public Trade_Enquiry_RecentPayments_Result Trade_Enquiry_RecentPayments(Trade_Enquiry_RecentPayments_Request dto)
		{
			var db = new databaseDataContext();

			var response = db.Connection.QueryMultiple("pub.Trade_Enquiry_RecentPayments", dto, null, null, CommandType.StoredProcedure);

			return new Trade_Enquiry_RecentPayments_Result()
			{
				Records = response.Read<Trade_Enquiry_RecentPayments_Record>().ToList()
			};
		}
	}
}