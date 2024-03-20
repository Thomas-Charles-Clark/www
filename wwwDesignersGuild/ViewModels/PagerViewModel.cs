using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class PagerViewModel
    {
        [JsonIgnore]
        public NewMasterDefinition MasterDefinition { get; set; }

        // This is for anything which doesn't have attribute groups / filtering:
        // An opportunity to display a count of whatever we're paging through, on the left where otherwise we would have
        // the filter / clear buttons
        public string DisplayCount { get; set; }

        // Rendering index: are we rendering the first or second lot of paging controls?
        // The first lot should including filtering buttons, sorting options and paging
        // the second lot should include just sorting and paging?
        public int Index { get; set; }

        // Does this pager AJAX for result rendering?
        public bool Ajax { get; set; }

        // If so, at what endpoint
        public string AjaxEndpoint { get; set; }

        public bool IsShop { get; set; }

        public long PageSectionID { get; set; }

        public bool PagingEnabled { get; set; }

        public string Sort { get; set; }

        public int PagerBreadth { get; set; }

        public int PagerBreadthNegative { get; set; }

        public int PagerBreadthPositive { get; set; }

        public bool DisplayRightEllipsis
        {
            get
            {
                if (PageCount == Page)
                    return false;
                return PageCount > Page + PagerBreadthPositive;
            }
        }

        public bool DisplayLeftEllipsis
        {
            get
            {
                return Page - PagerBreadthNegative > 1;
            }
        }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TakeCount { get; set; }

        public string ProductType { get; set; }

        public List<Models.ProductPage.ProductType> ProductTypes { get; set; }

        public List<AttributeGroup> CommonAttributeGroups { get; set; }

        public List<AttributeGroup> AttributeGroups { get; set; }

        public List<AttributeGroupGroup> AttributeGroupGroups { get; set; }

        public bool AnyFiltersSelected
        {
            get
            {
                return AttributeGroups.Any(p => p.InUse);
            }
            set
            {

            }
        }

        public int FilterCount
        {
            get
            {
                // I don't think we want to include Product Type as a clearable filter 25/08/2022
                return AttributeGroups.Select(p => p.Selection.Count()).Sum();// + (ProductType == "" ? 0 : 1); 
            }
            set
            {

            }
        }

        [JsonIgnore]
        public List<int> FilteredIDs { get; set; }

        [JsonIgnore]
        public List<object> Contents { get; set; }

        public List<object> GetContents()
        {
            return Contents;
        }

        public int PageCount
        {
            get
            {
                var result = (int)Math.Ceiling((float)FilteredIDs.Count() / (float)PageSize);

                if (result == 0)
                    result += 1;

                return result;
            }
        }

        public PagerViewModel(
            long pageSectionId,
            bool pagingEnabled,
            bool isShop,
            NewMasterDefinition objMaster,
            bool ajax = false,
            string ajaxEndpoint = null)
        {
            PageSectionID = pageSectionId;
            PagingEnabled = pagingEnabled;
            IsShop = isShop;
            MasterDefinition = objMaster;
            Contents = new List<object>();
            AttributeGroupGroups = new List<AttributeGroupGroup>();
            AttributeGroups = new List<AttributeGroup>();

            Ajax = ajax;
            AjaxEndpoint = ajaxEndpoint;

            if (Ajax && String.IsNullOrEmpty(AjaxEndpoint))
            {
                throw new Exception("PagerViewModel cannot AJAX without an Endpoint");
            }

            FilteredIDs = new List<int>();
        }

        public void InitialisePager()
        {
            Sort = HttpContext.Current.Request.QueryString["sort"];

            Page = Int32.Parse(HttpContext.Current.Request.QueryString["Page"] ?? "1");

            PageSize = Int32.Parse(HttpContext.Current.Request.QueryString["PageSize"] ?? "48");

            // Does product type really belong on the pager...?
            if (String.IsNullOrEmpty(ProductType)) // can be non-null if being initialised by a ProductPageGrid in LandingPageViewModel.cs
            {
                ProductType = String.IsNullOrEmpty(HttpContext.Current.Request.QueryString["product-type"])
                    ? ""
                    : HttpContext.Current.Request.QueryString["product-type"];
            }

            if (Page > PageCount)
            {
                Page = PageCount;
            }

            if (Page < 1)
            {
                Page = 1;
            }

            PagerBreadth = 2;

            PagerBreadthNegative = PagerBreadth;
            PagerBreadthPositive = PagerBreadth;

            if (Page + PagerBreadthPositive > PageCount)
            {
                PagerBreadthPositive = PageCount - Page;
            }

            if (PagerBreadth < PageCount)
            {
                if (PagerBreadthPositive < PagerBreadth)
                {
                    PagerBreadthNegative += PagerBreadth - PagerBreadthPositive;
                }
            }

            if (Page < PagerBreadthNegative)
            {
                PagerBreadthNegative = Page - 1;
            }

            while (Page - PagerBreadthNegative < 1)
            {
                PagerBreadthNegative--;
            }

            if (PagerBreadthNegative < PagerBreadth)
            {
                PagerBreadthPositive += PagerBreadth - PagerBreadthNegative;
            }

            while (Page + PagerBreadthPositive > PageCount)
            {
                PagerBreadthPositive--;
            }
        }
    }
}