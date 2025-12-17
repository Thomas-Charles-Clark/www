using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_RecentPayments_Record
	{
		public string PaymentReference { get; set; }
		public DateTime PaymentDate { get; set; }
		public decimal OriginalValueInCurrency { get; set; }
	}
}