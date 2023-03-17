using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Models.BasketClasses
{
    public class BasketSummary
    {
        public List<BasketTotal> basketTotals { get; set; }
        public List<BasketDetail> basketDetails { get; set; }
    }

    public class Basket
    {
        public BasketTotal Total
        {
            get
            {
                return Totals.FirstOrDefault();
            }
        }
        public List<BasketTotal> Totals { get; set; }
        public List<BasketDetail> BasketItems { get; set; }
        public List<BasketDeliveryOption> ProductDeliveryOptions { get; set; }
        public List<BasketPromotionalMessage> PromotionalMessages { get; set; }
        public BasketErrorMessage Errors { get; set; }
        public string Notices { get; set; }
        public decimal? SummaryTotal { get; set; }
        public PromotionCodeDetails PromotionCodeDetails { get; set; }

        public bool HasPromotionCode
        {
            get
            {
                return PromotionCodeDetails.PromotionCodeDetailsJSON != null;
            }
            private set
            {

            }
        }
    }

    public class BasketTotal
    {
        public decimal? TotalQuantity { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? TotalGiftVoucher { get; set; }
        public string PromotionalMessage { get; set; }
    }

    public class BasketDetail
    {
        public byte BasketLineTypeID { get; set; }
        public string BasketLineType { get; set; }
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public string BasketName { get; set; }
        public string Brand { get; set; }
        public string Code { get; set; }
        public long ImageID { get; set; }
        public string Size { get; set; }
        public byte SortOrder { get; set; }
        public string PriceMessage { get; set; }
        public bool SalePriceTypeID { get; set; }
        public decimal TotalStock { get; set; }
        public byte? StockStatusID { get; set; }
        public bool Treatable { get; set; }
        public bool GiftWrappable { get; set; }
        public bool SaveForLater { get; set; }
        public string URLKeywords { get; set; }
        public int ProductPageID { get; set; }
        public decimal Price { get; set; }

        public string ImageUrl
        {
            set
            {

            }
            get
            {
                var url = new UrlHelper(HttpContext.Current.Request.RequestContext);

                return url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = ImageID.ToString() });
            }
        }
        public string DisplayLine
        {
            get
            {
                if (Size == null || Size == "" || Size == "-")
                {
                    return BasketName;
                }

                return String.Format("{0} - {1}", BasketName, Size);
            }
            private set { }
        }
        public string ProductPageURL
        {
            get
            {
                return dgFramework.buildUrl(dgContentType.productPage, ProductPageID, new countryItem() { URL = URLCulture }, null, null, null, URLKeywords);
            }
            private set { }
        }

        public byte ProductTypeID { get; set; }
        public string StockMessage { get; set; }
        public StockMessageWrapper StockMessaging
        {
            get
            {
                return JsonConvert.DeserializeObject<StockMessageWrapper>(StockMessage);
            }
            private set { }
        }

        public string BasketMessage { get; set; }
        public object basketMessage
        {
            get
            {
                if (!String.IsNullOrEmpty(BasketMessage))
                    return JsonConvert.DeserializeObject(BasketMessage);
                else
                    return null;
            }
            private set { }
        }
        public string URLCulture { get; set; }
        public bool New { get; set; }

        public List<BasketDeliveryOption> DeliveryOptions { get; set; }
    }

    public class StockMessageWrapper
    {
        public byte ProductTypeID { get; set; }
        public decimal QuantityMinimum { get; set; }
        public decimal QuantityMaximum { get; set; }
        public decimal? ProductTypeQuantityMaximum { get; set; }
        public decimal QuantityIncrement { get; set; }
        public decimal ReserveQuantityMaximum { get; set; }
        public string Unit { get; set; }
        public byte StockUnitID { get; set; }
        public bool AllowStockCheck { get; set; }
        public decimal? PieceBreakdownMinimum { get; set; }
        public decimal? PieceBreakdownMaximum { get; set; }
        public bool AllowBackInStockReminder { get; set; }
        public List<BasketQuantityThreshold> BasketQuantityThresholds { get; set; }
    }

    public class BasketQuantityThreshold
    {
        public decimal Minimum { get; set; }
        public decimal Maximum { get; set; }
        public List<StockMessageEntry> StockMessages { get; set; }
    }

    public class StockMessageEntry
    {
        public string StockMessage { get; set; }
        public string Class { get; set; }
        public int StockBreakdownTab { get; set; }
    }

    public class BasketDeliveryOption
    {
        public int ProductID { get; set; }
        public byte DeliveryOptionID { get; set; }
        public string Name { get; set; }

        public string ThumbnailURL
        {
            get
            {
                switch (DeliveryOptionID)
                {
                    case 1:
                        return $"/content/images/basket/truck{(Framework.ServeWebP ? ".webp" : ".jpg")}";
                    case 2:
                        return $"/content/images/basket/mouse{(Framework.ServeWebP ? ".webp" : ".jpg")}";
                    case 3:
                        return $"/content/images/basket/ups{(Framework.ServeWebP ? ".webp" : ".jpg")}";
                    default:
                        return null;
                }
            }
            private set
            {

            }
        }

        public string ResourceName
        {
            get
            {
                switch (DeliveryOptionID)
                {
                    case 1:
                        return "BasketHomeDelivery";
                    case 2:
                        return "BasketClickCollect";
                    case 3:
                        return "BasketUPSAccessPoint";
                    default:
                        return null;
                }
            }
            private set
            {

            }
        }
    }

    public class BasketPromotionalMessage
    {
        public string Message { get; set; }
    }

    public class BasketErrorMessage
    {
        public string ErrorJSON { get; set; }
        public object errors
        {
            set
            {

            }
            get
            {
                return JsonConvert.DeserializeObject(String.IsNullOrEmpty(ErrorJSON) ? "[]" : ErrorJSON);
            }
        }
    }

    public class PromotionCodeDetails
    {
        public object json
        {
            get
            {
                return JsonConvert.DeserializeObject(String.IsNullOrEmpty(PromotionCodeDetailsJSON) ? "[]" : PromotionCodeDetailsJSON);
            }
            private set
            {

            }
        }

        public string Code
        {
            get
            {
                return JArray.FromObject(json).FirstOrDefault()?.SelectToken("promotionCode").Value<string>();
            }
            private set
            {

            }
        }

        public string Description
        {
            get
            {
                return JArray.FromObject(json).FirstOrDefault()?.SelectToken("description").Value<string>();
            }
            private set
            {

            }
        }

        public decimal? EffectiveDiscount
        {
            get
            {
                return JArray.FromObject(json).FirstOrDefault()?.SelectToken("result")?.SelectToken("effectiveDiscount").Value<decimal?>();
            }
            private set
            {

            }
        }

        public string PromotionCodeDetailsJSON { get; set; }
    }

    public class BasketTotalQuantity
    {
        public decimal TotalQuantity { get; set; }
    }

    public class BasketLineType
    {
        public byte BasketLineTypeID { get; set; }
        public string Name { get; set; }
    }

    public class BasketSummaryTotalQuantity
    {
        public decimal TotalQuantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalGiftVoucher { get; set; }
    }
    
    public class BasketLatestItem
    {
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public string BasketName { get; set; }
        public string Size { get; set; }
        public long? ImageID { get; set; }
        public int ProductPageID { get; set; }
        public string URLKeywords { get; set; }
        public string ImageUrl { get; set; }
        public string ProductPageUrl { get; set; }
        public string DisplayLine { get; set; }
    }

    public class BasketLineDetails
    {
        public List<BasketDetail> basketDetails { get; set; }
    }

    public class BasketGenerateQuick
    {
        public List<BasketTotal> basketTotals { get; set; }
        public List<BasketDetail> basketDetails { get; set; }
    }

    public class BasketProductStockSelect
    {
        public List<StockStatusSelect> basketStockStatusLines { get; set; }
        public List<BasketProductTotal> basketProductTotals { get; set; }
    }

    public class BasketProductTotal
    {
        public decimal TotalStock { get; set; }
    }

    public class PassBasketAdd
    {
        public int productId { get; set; }
        public decimal quantity { get; set; }
        public string basketCountry { get; set; }
        public byte priceListId { get; set; }
        public int? productPageId { get; set; }
        public byte? basketLineTypeId { get; set; }
    }


    public class PassBasketUpdate
    {
        public NewMasterDefinition objMaster { get; set; }
        public byte? basketLineTypeId { get; set; }
        public int? productId { get; set; }
        public decimal? quantity { get; set; }
        public string basketCountry { get; set; }
        public bool? savedForLaterFlag { get; set; }
    }


    public class PassBasketSummary
    {
        public string basketCountry { get; set; }
    }


    public class PassSaveForLater
    {
        public byte basketLineTypeId { get; set; }
        public int productCode { get; set; }
        public string basketCountry { get; set; }
        public bool savedForLaterFlag { get; set; }
    }


    public class PassBasket
    {
        public decimal totalQuantity { get; set; }
        public decimal totalPrice { get; set; }
        public List<PassBasketLine> basketLines { get; set; }
        public string country { get; set; }
    }

    public class PassBasketLine
    {
        public byte basketLineTypeId { get; set; }
        public int productId { get; set; }
        public decimal quantity { get; set; }
    }

    public class AddToBasketReturn
    {
        public decimal? totalDisplayQuantity { get; set; }
        public decimal? quantity { get; set; }
        public string basketName { get; set; }
        public string code { get; set; }
        public string quantityDisplay { get; set; }
        public string basketLineText { get; set; }
        public decimal? price { get; set; }
        public string currency { get; set; }
        public string stockUnit { get; set; }
        public byte stockUnitId { get; set; }
    }


    public class brontoJsonPostReturn
    {
        public string cartUrl { get; set; }
        public decimal? grandTotal { get; set; }
        public string currency { get; set; }
        public decimal? subtotal { get; set; }
        public decimal? discountAmount { get; set; }
        public decimal? taxAmount { get; set; }
        public string email { get; set; }
        public List<brontoJsonPostReturnBasketLine> lineItems { get; set; }
    }

    public class brontoJsonPostReturnBasketLine
    {
        public string sku { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string category { get; set; }
        public decimal? unitPrice { get; set; }
        public decimal? salePrice { get; set; }
        public decimal quantity { get; set; }
        public decimal? totalPrice { get; set; }
        public string imageUrl { get; set; }
        public string productUrl { get; set; }
    }


    public class basketTotalQuantityReturn
    {
        public string quantityDisplay { get; set; }
        public string basketLineText { get; set; }
    }


    public class basketSummaryReturn
    {
        public string promotionalMessage { get; set; }
        public string itemsCountMessage { get; set; }
        public string quantityDisplay { get; set; }
        public BasketSummary basketSummaryData { get; set; }
    }

    public class basketUpdateReturn
    {
        public string promotionalMessage { get; set; }
        public string itemsCountMessage { get; set; }
        public string quantityDisplay { get; set; }
        public BasketDetail lineDetail { get; set; }
        public List<BasketTotal> basketTotals { get; set; }
    }
}