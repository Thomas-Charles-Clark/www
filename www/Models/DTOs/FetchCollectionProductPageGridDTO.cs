using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.DTOs
{
	public class FetchCollectionProductPageGridDTO
	{
		public short collectionId { get; set; }
		public int? memberId { get; set; }
		public byte languageId { get; set; }
		public short countryId { get; set; }
		public int? tradeAccountId { get; set; }
		public bool displayTradePrice { get; set; }
		public DateTime useDate { get; set; }
		public bool countryCheckMatched { get; set; }
	}
}