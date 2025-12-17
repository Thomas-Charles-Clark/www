using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_AccountBalance_Result
	{
		public string CurrencyHeading { get; set; }
		public IEnumerable<Trade_Enquiry_AccountBalance_Record> RecordsByDocumentDate { get; set; }
		public IEnumerable<Trade_Enquiry_AccountBalance_Record> RecordsByDueDate{ get; set; }
		public Trade_Enquiry_OnAccount_Record OnAccount { get; set; }

		public decimal TotalByDocumentDate
		{
			get
			{
				return RecordsByDocumentDate.Select(r => r.Value).Sum();
			}
		}

		public decimal TotalByDueDate
		{
			get
			{
				return RecordsByDueDate.Select(r => r.Value).Sum();
			}
		}
	}
}