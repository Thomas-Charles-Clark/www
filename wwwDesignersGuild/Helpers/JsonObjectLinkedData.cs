using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Helpers
{
    public static class JsonObjectLDHelper
    {
        /// <summary>
        /// JSON-LD writer.
        /// It will need to be told what type of object it's receiving,
        /// then cast the incoming object to a type... and if it fails fail quietly!
        /// 
        /// </summary>
        /// <param name="html"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JsonObjectLD JsonObjectLD(this HtmlHelper html, string type, object objData)
        {
            return new JsonObjectLD(type, objData);
        }

        public static JsonObjectLD JsonObjectLD(this HtmlHelper html, string type, object objData, string currencyCode)
        {
            return new JsonObjectLD(type, objData, currencyCode);
        }
    }

    public class JsonObjectLD : IHtmlString
    {
        private readonly string type;

        private readonly object objData;

        private readonly string currencyCode;

        public JsonObjectLD(string text, object objData)
        {
            this.type = text;
            this.objData = objData;
        }

        public JsonObjectLD(string text, object objData, string currency)
        {
            this.type = text;
            this.objData = objData;
            this.currencyCode = currency;
        }

        public override string ToString()
        {
            return ToHtmlString();
        }

        public string ToHtmlString()
        {

            var result = "";

            switch (this.type)
            {
                #region Base
//                case "Base":
//                    //Provide: WebPage, Organisation, WebSite
//                    result = "<script type='application/ld+json'>";

//                    result += @"
//[{
//    ""@context"": ""http://schema.org"",
//    ""@type"": ""Organization"",
//    ""name"": ""Designers Guild"",
//    ""url"": ""https://www.designersguild.com"",
//    ""logo"": {
//        ""@type"": ""ImageObject"",
//        ""url"": ""https://www.designersguild.com/Content/images/logo/designers_guild_logo.png"",
//        ""width"": ""350"",
//        ""height"": ""25""
//    },
//    ""sameAs"": [""//www.instagram.com/designersguild"", ""//www.facebook.com/designersguildgroup"", ""//www.youtube.com/user/DesignersGuildGroup""]
//}, {
//    ""@context"": ""http://schema.org"",
//    ""@type"": ""WebSite"",
//    ""name"": ""Designers Guild"",
//    ""url"": ""https://www.designersguild.com""
//}]
//";

//                    result += "</script>";
//                    break;
                #endregion

                #region Products
                case "products":

                    result = "";

                    var primaries = objData as List<productPageProduct>;
                    if (primaries != null)
                    {
                        var build = @"
[";

                        foreach (var p in primaries)
                        {
                            //https://developers.google.com/search/docs/data-types/products
                            // so... there are parts of the product schema which we might reasonably not express
                            // depending on varying data available. 

                            // soooo... the review data we deliver to the page is all wossnamed. it's all reviews for the primary products on the page.
                            // so I need to pull from that for each product... and that can circumvent needing to join in the procedure. Sure.

                            build += @"
{";
                            build += String.Format(@"
    ""@schema"": ""http://www.schema.org"",
    ""@type"": ""Product"",
    ""name"": ""{0}"",
    ""image"": ""https://www.designersguild.com/image/1024/{1}"",
    ""description"": ""{2}"",
    ""brand"": ""{3}"",
    ""sku"": ""{4}""",
                                (p.OrderFormName ?? "").Replace("\"", @"\"""),
                                p.ImageID,
                                (""),
                                p.Brand,
                                p.Code);

                            if (p.ExampleReviewRating.HasValue) //hasReview 
                            {
                                build += String.Format(@",
    ""review"": {{
        ""@type"": ""Review"",
        ""body"": ""{0}"",
        ""author"": {{
            ""@type"": ""Organization"",
            ""name"": ""Feefo""
        }},
        ""datePublished"": ""{1}"",
        ""reviewRating"": {{
            ""@type"": ""Rating"",
            ""ratingValue"": ""{2}""
        }}, 
        ""publisher"": {{
            ""@type"": ""Organization"",
            ""name"": ""Feefo"",
            ""url"": ""https://en.business.feefo.com/"",
            ""logo"": {{
                ""@type"": ""ImageObject"",
                ""url"": ""https://en.business.feefo.com/wp-content/themes/feefo/assets/images/logo.png"",
                ""width"": ""173"",
                ""height"": ""40""
            }}
        }},
        ""url"": ""{3}""
    }}",
                                HtmlHelperExtra.EscapeForJS(p.ExampleReviewBody.Replace("\"", @"\""")),
                                p.ExampleReviewDate.Value.ToString("yyyy-MM-dd"),
                                p.ExampleReviewRating,
                                p.ExampleReviewURL);
                            }

                            if (p.RatingCount > 0) //hasAggregateRating
                            {
                                build += String.Format(@",
    ""aggregateRating"": {{
        ""@type"": ""AggregateRating"",
        ""ratingValue"": ""{0}"",
        ""reviewCount"": ""{1}""
    }}",
                                p.AverageRatingValue.Value.ToString("F"),
                                p.RatingCount);
                            }

                            if (p.DisplayPrice.HasValue) // availableToBuy
                            {
                                if (p.DisplaySalePrice.HasValue && p.SalePriceAvailableUntil.HasValue)
                                {
                                    build += String.Format(@",
    ""offers"": {{
        ""@type"": ""Offer"",
        ""price"": ""{0}"",
        ""priceCurrency"": ""{1}"",
        ""priceAvailableUntil"": ""{2}"",
        ""availability"": ""{3}"",
        ""url"": ""{4}""
    }}",
                                    p.DisplaySalePrice.Value.ToString("F"),
                                    currencyCode,
                                    p.SalePriceAvailableUntil.Value.ToString("yyyy-MM-dd"),
                                    p.ItemAvailability,
                                    HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
                                }
                                else
                                {
                                    build += String.Format(@",
    ""offers"": {{
        ""@type"": ""Offer"",
        ""price"": ""{0}"",
        ""priceCurrency"": ""{1}"",
        ""availability"": ""{2}"",
        ""url"": ""{3}""
    }}",
                                    p.DisplayPrice.Value.ToString("F"),
                                    currencyCode,
                                    p.ItemAvailability,
                                    HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Path));
                                }
                            }

                            build += @"
}, ";

                            if (p == primaries.Last())
                                build = build.TrimEnd(new char[] { ',', ' ' });

                        }

                        build = build + @"]";

                        result = result + build;
                    }

                    break;
                #endregion
            }

            return result;
        }
    }
}