using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_InvoiceSearch_Result
	{
		public IEnumerable<Trade_Enquiry_InvoiceSearch_Record> Records { get; set; }
		public Trade_Enquiry_OnAccount_Record OnAccount { get; set; }
	}
}