using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models
{
    public class GiftVoucherPDF
    {
        public short GiftVoucherID { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencyCulture { get; set; }
        public string CurrencySymbol { get; set; }
        public string Code { get; set; }
        public decimal? ValueInc { get; set; }
        public decimal? ValueEx { get; set; }
        public DateTime ExpiryDT { get; set; }
        public DateTime? CancelledDT { get; set; }
    }
}