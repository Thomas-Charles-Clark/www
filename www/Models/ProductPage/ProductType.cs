using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.ProductPage
{
    public class ProductType
    {
        public byte ProductTypeID { get; set; }
        public string Name { get; set; }
        public string NamePlural { get; set; }
        public string URLKeywords { get; set; }
        public byte SortOrder { get; set; }
        
        public IEnumerable<int> ProductPageIDs { get; set; }

        public string FormValue
        {
            get
            {
                if (ProductTypeID == 0)
                    return "";
                else
                    return HttpUtility.UrlEncode(Name.ToLowerInvariant().Replace(" ", "-"));
            }
            set
            {

            }
        }

        public string FormValuePlural
        {
            get
            {
                if (ProductTypeID == 0)
                    return "";
                else
                    return HttpUtility.UrlEncode(NamePlural?.ToLowerInvariant().Replace(" ", "-"));
            }
            set
            {

            }
        }

        public ProductType()
        {
            ProductPageIDs = new List<int>();
        }

        public ProductType(ProductType productType, IEnumerable<int> productPageIds)
        {
            ProductTypeID = productType.ProductTypeID;
            Name = productType.Name;
            SortOrder = productType.SortOrder;
            ProductPageIDs = productPageIds;
        }

        // Product Types have sets of Attribute Groups

        // Some Attribute Groups are common across multiple Product Types
    }

    public class AttributeGroupProductType : ProductType
    {
        public byte? LexicographicalSortOrder { get; set; }

        public AttributeGroupProductType(ProductType pt, byte? lx)
        {
            ProductTypeID = pt.ProductTypeID;
            Name = pt.Name;
            SortOrder = pt.SortOrder;
            ProductPageIDs = pt.ProductPageIDs;
            LexicographicalSortOrder = lx;
        }
    }

    public class ProductTypeHint
    {
        public string URLKeywords { get; set; }
        public byte ProductTypeID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }

        public string Heading
        {
            get
            {
                return Name;
            }
            set
            {

            }
        }

        public string FormValue
        {
            get
            {
                if (ProductTypeID == 0)
                    return "";
                else
                    return HttpUtility.UrlEncode(Name.ToLowerInvariant().Replace(" ", "-"));
            }
            set
            {

            }
        }
    }
}