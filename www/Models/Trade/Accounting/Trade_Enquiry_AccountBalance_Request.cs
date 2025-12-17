using Org.BouncyCastle.Asn1.Mozilla;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Trade.Accounting
{
	public class Trade_Enquiry_AccountBalance_Request
	{
		public int TradeAccountID { get; set; }

		public short CountryID { get; set; }

		public byte LanguageID { get; set; }
	}
}