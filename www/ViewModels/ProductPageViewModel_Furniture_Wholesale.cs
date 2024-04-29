using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using System.Linq;
using System.Threading.Tasks;

using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPageViewModel_Furniture_Wholesale : ProductPageViewModel
    {
        public string InteractiveBrochureURL { get; set; }

        public string FurniturePDFURL { get; set; }

        public CarouselViewModel SecondaryCarousel { get; set; }

        public static async Task<ProductPageViewModel_Furniture_Wholesale> CreateAsync(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop)
        {
            var result = new ProductPageViewModel_Furniture_Wholesale(objMaster,
                objVisitor,
                objCacheSelect,
                isShop);

            await result.Initialise();

            if (!result.AllProductsUnavailable && !result.OtherProducts.Any())
            {
                result.MainProduct.Solitary = true;
            }

            return result;
        }

        private ProductPageViewModel_Furniture_Wholesale(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            productPageCacheSelect objCacheSelect,
            bool isShop) : base(
            objMaster,
            objVisitor,
            objCacheSelect,
            isShop)
        {
            UseOrderFormGrouping = false;
            UseSizeButtons = false;

            var links = JsonConvert.DeserializeObject<WholesaleFurnitureLinks>(JObject.FromObject(Definition.json).SelectToken("wholesaleFurniture").ToString());

            InteractiveBrochureURL = links.InteractiveBrochureURL;
            FurniturePDFURL = links.FurniturePDFURL;
        }

        new protected async Task Initialise()
        {
            await base.Initialise();

            Carousel.PermitLargeImageModal = false;

            SecondaryCarousel = new CarouselViewModel(
                carouselId: $"#product-page-{MasterDefinition.UrlBreakdown.ContentId}-carousel",
                wrapperCssClass: null,
                aspectRatio: 1.0m,
                heading: Definition.heading,
                images: Definition.Images
                                  .Where(i => i.templateImageId == 3)
                                  .OrderBy(i => i.sortOrder)
                                  .Select(i => new CarouselImage(i.imageId, i.sortOrder, i.altText, ImageSize.Six)),
                this
            )
            {
                DisplayThumbnails = false
            };
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class WholesaleFurnitureLinks
    {
        public string InteractiveBrochureURL { get; set; }
        public string FurniturePDFURL { get; set; }
    }
}