using MertexerV2.Models.Pricing;
using MertexerV2.Models;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.GiftVoucher
{
    public class GiftVoucher_SelectResult
    {
        [Key]
        public short GiftVoucherID { get; set; }

        public byte GiftVoucherTypeID { get; set; }

        public string Code { get; set; }

        public byte PriceListID { get; set; }

        public decimal ValueInc { get; set; }

        public decimal ValueEx { get; set; }

        public DateTime CreatedDT { get; set; }

        public DateTime? AuthorisedDT { get; set; }

        public DateTime? ExpiryDT { get; set; }

        public bool Expired { get; set; }

        public string ExpiredMessage { get; set; }

        public DateTime? UsedDT { get; set; }

        public bool Used { get; set; }

        public string UsedMessage { get; set; }

        public DateTime? CancelledDT { get; set; }

        public bool Cancelled { get; set; }

        public string CancelledMessage { get; set; }

        public int? SaleTransactionID { get; set; }

        public string CreatorUserID { get; set; }

        public int? CreditTransactionID { get; set; }

        public string Reason { get; set; }

        public int? CreatorMemberID { get; set; }

        public string SaleOrderID { get; set; }

        public string Message { get; set; }

        public string CreditOrderID { get; set; }

        public string CurrencySymbol { get; set; }

        public string VisitorCulture { get; set; }

        public string GiftForYouText { get; set; }

        public string GiftVoucherLabelText { get; set; }

        public string GiftVoucherValidUntilText { get; set; }

        public string UseInStoreOrOnlineText { get; set; }

        public string BeforeText { get; set; }

        public string eGiftCardHeading { get; set; }

        public string eGiftCardInStoreSubHeading { get; set; }

        public string eGiftCardInStoreText { get; set; }

        public string eGiftCardOnlineHeading { get; set; }

        public string eGiftCardOnlineText1 { get; set; }

        public string eGiftCardOnlineText2 { get; set; }

        public string eGiftCardOnlineText3 { get; set; }

        public string TermsAndConditionsHeading { get; set; }

        public string TermsAndConditionsText1 { get; set; }

        public string TermsAndConditionsText2 { get; set; }

        public string TermsAndConditionsText3 { get; set; }

        public string TermsAndConditionsText4 { get; set; }
    }
}