using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_InvoiceSearch_Record
	{
		public string InvoiceHead { get; set; }
		public DateTime InvoiceDT { get; set; }
		public DateTime? DueDT { get; set; }
		public DateTime? DespatchedDT { get; set; }
		public string OrderID { get; set; }
		public string CustomerOrderReference { get; set; }
		public decimal OriginalValueInCurrency { get; set; }
		public decimal OutstandingValueInCurrency { get; set; }
		public bool Disputed { get; set; }
		public string InvoiceURI { get; set; }
	}
}