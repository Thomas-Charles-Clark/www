using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_AccountBalance_Record
	{
		public string Label { get; set; }

		public decimal Value { get; set; }

		public int SortOrder { get; set; }

		public DateTime? RangeStartDate { get; set; }

		public DateTime? RangeEndDate { get; set; }
	}
}