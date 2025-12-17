using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_RecentPayments_Result
	{
		public IEnumerable<Trade_Enquiry_RecentPayments_Record> Records { get; set; }
	}
}