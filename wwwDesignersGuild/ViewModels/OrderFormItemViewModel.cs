using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.OrderForm;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.ViewModels
{
    public class OrderFormItemViewModel
    {

        [JsonIgnore]
        public BaseViewModel Base { get; set; }

        [JsonIgnore]
        public NewMasterDefinition MasterDefinition { get; set; }

        public bool Solitary { get; set; }

        public JObject ProductBase { get; set; }

        public JObject Product { get; set; }

        public CarouselViewModel Carousel { get; set; }

        public int ProductID
        {
            get
            {
                return Product.SelectToken("productId").Value<int>();
            }
        }

        public byte ProductTypeID
        {
            get
            {
                return Product.SelectToken("productTypeId").Value<byte>();
            }
        }

        public int ProductPageID
        {
            get
            {
                return Product.SelectToken("productPageId").Value<int>();
            }
        }

        public string ProductCode
        {
            get
            {
                return Product.SelectToken("code").Value<string>();
            }
        }

        public string Brand
        {
            get
            {
                return Product.SelectToken("brand").Value<string>();
            }
        }

        public string OrderFormID
        {
            get
            {
                return $"#product-{ProductID}";
            }
        }

        public string OrderFormCarouselID
        {
            get
            {
                return $"#product-{ProductID}-slide";
            }
        }

        public bool PrimaryProduct
        {
            get
            {
                return Product.SelectToken("primaryProduct")?.Value<bool?>() ?? false;
            }
        }

        public int? OrderFormGroupID
        {
            get
            {
                return Product.SelectToken("orderFormGroupId")?.Value<int?>();
            }
        }

        public string OrderFormGroup
        {
            get
            {
                return Product.SelectToken("orderFormGroup").Value<string>();
            }
        }

        public string Size
        {
            get
            {
                return Product.SelectToken("size").Value<string>();
            }
        }

        public string OrderFormName
        {
            get
            {
                return Regex.Replace(Product.SelectToken("orderFormName")?.Value<string>(), "(.)\\/(.)", "$1 / $2");
            }
        }

        public string BasketName
        {
            get
            {
                return Regex.Replace(Product.SelectToken("name")?.Value<string>(), "(.)\\/(.)", "$1 / $2");
            }
        }

        public int SortOrder
        {
            get
            {
                return Product.SelectToken("sortOrder").Value<int>();
            }
        }

        public IEnumerable<Models.Classes.Attribute> Attributes
        {
            get
            {
                return Product.SelectToken("attributes").ToObject<List<Models.Classes.Attribute>>();
            }
        }

        public productPageImage Image
        {
            get
            {
                return new productPageImage(
                    imageId: Product.SelectToken("imageId").Value<long>(),
                    templateImageId: (byte)2,
                    altText: Product.SelectToken("imageAltText").Value<string>(),
                    titleText: Product.SelectToken("imageTitleText").Value<string>());
            } 
        }

        public List<KeyValuePair<string, string>> OrderFormProperties
        {
            get
            {
               return JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(Product.SelectToken("orderFormProperties").ToString());
            }
        }

        public bool AllowSample
        {
            get
            {
                return Product.SelectToken("controls").SelectToken("allowSample").Value<bool>();
            }
        }

        public bool AllowLargeSample
        {
            get
            {
                return Product.SelectToken("controls").SelectToken("allowLargeSample").Value<bool>(); ;
            }
        }

        public bool AllowOrder
        {
            get
            {
                return Product.SelectToken("controls").SelectToken("allowOrder").Value<bool>();
            }
        }

        public bool AllowReserve
        {
            get
            {
                return Product.SelectToken("controls").SelectToken("allowReserve").Value<bool>();
            }
        }

        public bool AllowReserveWithBatchCutting
        {
            get
            {
                return Product.SelectToken("controls").SelectToken("allowReserveWithBatchSample").Value<bool>();
            }
        }

        public string ClientProductViewModels
        {
            get
            {
                var result = new List<string>();

                if (AllowSample)
                {
                    result.Add($@"productViewModel({Product["sample"].SelectToken("productId").Value<int>()}, {OrderFormItemType.Sample.GetAttributeOfType<BasketLineTypeAttribute>().BasketLineTypeID})");
                }

                if (AllowLargeSample)
                {
                    result.Add($@"productViewModel({Product["largeSample"].SelectToken("productId").Value<int>()}, {OrderFormItemType.LargeSample.GetAttributeOfType<BasketLineTypeAttribute>().BasketLineTypeID})");
                }

                if (AllowOrder)
                {
                    result.Add($@"productViewModel({Product["productId"].Value<int>()}, {OrderFormItemType.Order.GetAttributeOfType<BasketLineTypeAttribute>().BasketLineTypeID})");
                }

                if (AllowReserve)
                {
                    result.Add($@"productViewModel({Product["productId"].Value<int>()}, {OrderFormItemType.Reserve.GetAttributeOfType<BasketLineTypeAttribute>().BasketLineTypeID})");
                }

                if (AllowReserveWithBatchCutting)
                {
                    result.Add($@"productViewModel({Product["productId"].Value<int>()}, {OrderFormItemType.ReserveWithBatchCutting.GetAttributeOfType<BasketLineTypeAttribute>().BasketLineTypeID})");
                }

                return String.Join(", ", result);
            }
        }

        public string URLKeywords
        {
            get
            {
                return $"{Product["urlSemantics"]}";
            }
        }

        public bool ImageLinkToProductPage { get; set; }

        public bool IsLifestyle
        {
            get
            {
                return HttpContext.Current.Request.QueryString.AllKeys.Contains("lifestyle");
            }
        }

        public string AlreadyInBasketString(BasketDetail extant, OrderFormItemType orderFormType)
        {
            var itemString = HtmlHelperExtra.Resource(extant.Quantity == 1 ? "item" : "items", Base.LanguageID, Base.CountryID, Base.Date).ToString();

            if (extant.StockMessaging.Unit == "" ||
                orderFormType == OrderFormItemType.Sample ||
                orderFormType == OrderFormItemType.LargeSample)
            {
                extant.StockMessaging.Unit = itemString;
            }

            var currentlyInYourBasketString = HtmlHelperExtra.Resource(extant.Quantity == 1 ? "ShoppingProductInBasket" : "ShoppingProductsInBasket", Base.LanguageID, Base.CountryID, Base.Date).ToString();

            var trimmedUnit = extant.StockMessaging.Unit.Trim();

            if (trimmedUnit == "roll(s)" ||
                trimmedUnit == "rouleaux" ||
                trimmedUnit == "rollen" ||
                trimmedUnit == "rotoli" ||
                trimmedUnit == "рулонов")
            {
                trimmedUnit = HtmlHelperExtra.Resource(extant.Quantity == 1 ? "RollLowerCase" : "RollsLowerCase", Base.LanguageID, Base.CountryID, Base.Date).ToString();
            }
            
            var spacer = " ";

            if (extant.StockMessaging.Unit == "m" ||
                extant.StockMessaging.Unit == "m²")
            {
                spacer = "";
            }

            return $"{extant.Quantity:0.##}{spacer}{trimmedUnit} {currentlyInYourBasketString}";
        }

        [JsonIgnore]
        public BasketGenerateQuick Basket { get; set; }

        public bool IsHotspot { get; set; }

        public OrderFormItemViewModel(
            object productBase,
            NewMasterDefinition objMaster,
            BaseViewModel baseViewModel,
            bool imageLinkToProductPage = false,
            ImageSize imageSize = ImageSize.One, 
            bool injectProductNameIntoProperties = false)
        {
            ProductBase = JObject.FromObject(productBase);

            Base = baseViewModel;

            Basket = baseViewModel.Basket;

            Solitary = false;

            Product = JObject.FromObject(ProductBase.SelectToken("product"));

            MasterDefinition = objMaster;
            ImageLinkToProductPage = imageLinkToProductPage;

            Carousel = new CarouselViewModel
            (
                carouselId: $"#product-{ProductID}-carsouel",
                wrapperCssClass: null,
                aspectRatio: 1.0m,
                heading: OrderFormName,
                images: new[] { new CarouselImage(Image.imageId, Image.sortOrder, Image.altText, ImageSize.One) },
                baseViewModel: baseViewModel
            );

            if (injectProductNameIntoProperties)
            {
                var nameResource = HtmlHelperExtra.Resource("Name", objMaster.UrlBreakdown.countryDetails.LanguageID,
                    objMaster.UrlBreakdown.countryDetails.CountryID,
                    objMaster.UrlBreakdown.ContentDate).ToString();

                ((JArray)Product["orderFormProperties"]).Insert(0, JToken.FromObject(new { key = nameResource, value = BasketName }));
            }

            IsHotspot = false;
        }
    }
}