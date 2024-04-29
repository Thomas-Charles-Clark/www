using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.Models.Helper
{
    public class TradeAccountContactHelper
    {
        public static IHtmlString CreditControlEmail(int tradeAccountId, byte languageId)
        {
            var detailFetcher = new cmsTradeAccountContactDetails();

            var contactResult = detailFetcher.loadTradeAccountContactDetailsFromCms(tradeAccountId, languageId, "CC").Result;

            var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

            return new HtmlString(email);
        }

        public static IHtmlString CustomerServicesEmail(int tradeAccountId, byte languageId)
        {
            var detailFetcher = new cmsTradeAccountContactDetails();

            var contactResult = detailFetcher.loadTradeAccountContactDetailsFromCms(tradeAccountId, languageId, "CS").Result;

            var email = JObject.FromObject(contactResult.json).SelectToken("$.contactDetails[0].email").Value<string>();

            return new HtmlString(email);
        }
    }
}