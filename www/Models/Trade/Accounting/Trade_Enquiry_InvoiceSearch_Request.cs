using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_InvoiceSearch_Request
	{
		public int TradeAccountID { get; set; }
		public short CountryID { get; set; }
		public byte LanguageID { get; set; }
		public string DateQualifier { get; set; }
		public DateTime? DateRangeStart { get; set; }
		public DateTime? DateRangeEnd { get; set; }
		public string SearchTerm { get; set; }
		public bool? Outstanding { get; set; }
	}
	
	public class Trade_Enquiry_InvoiceSearch_DetailedRequest : Trade_Enquiry_InvoiceSearch_Request
	{
		public sbyte? DateRange { get; set; }
	}
}