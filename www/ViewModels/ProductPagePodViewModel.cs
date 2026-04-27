using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels
{
    public class ProductPagePodViewModel
    {
        public ImageSize ImageSize { get; set; }
        public NewMasterDefinition MasterDefinition { get; set; }
        public ProductPageFilterablePod ProductPage { get; set; }
        public bool UseLifestyleGalleryBehaviour { get; set; }

        public int ProductPageID
        {
            get
            {
                return ProductPage.ProductPageID;
            }
        }
        
        public bool DisplayParagraph
        {
            get
            {
                return (!String.IsNullOrEmpty(ProductPage.ProductPagePrice?.CodeMessage)) ||
                       (!String.IsNullOrEmpty(ProductPage.ProductPagePrice?.DiscountMessage)) ||
                       (!String.IsNullOrEmpty(ProductPage.ProductPagePrice?.PriceMessage)) ||
                       (!String.IsNullOrEmpty(ProductPage.ProductPagePrice?.RRPMessage));
            }
        }

        public bool DisplayRating
        {
            get
            {
                return ProductPage.ReviewCount > 0;
            }
        }

        public bool NoFollow { get; set; }
        public bool NoIndex { get; set; }
        public string Rel
        {
            get
            {
                var strings = new List<string>();

                if (NoFollow)
                {
                    strings.Add("nofollow");
                }

                if (NoIndex) 
                {
                    strings.Add("noindex");
                }

                if (strings.Any())
                    return String.Join(", ", strings);
                else
                    return null;
            }
        }

        public ProductPagePodViewModel(NewMasterDefinition objMaster,
            ProductPageFilterablePod objProductPage,
            bool useLifestyleGalleryBehaviour,
            ImageSize? imageSize = null)
        {
            MasterDefinition = objMaster;
            ProductPage = objProductPage;
            UseLifestyleGalleryBehaviour = useLifestyleGalleryBehaviour;
            ImageSize = imageSize ?? (dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Three);
            NoFollow = objProductPage.NoFollow;
            NoIndex = objProductPage.NoIndex;
        }
    }
}