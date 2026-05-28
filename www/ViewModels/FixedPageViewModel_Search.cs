using Braintree;

using DocumentFormat.OpenXml.EMMA;

using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Util;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel_Search : FixedPageViewModel<serverSearchDatabase>
    {
        public bool NoResultsPageShowLink(byte productTypeId)
        {
            return ((ProductPageGridPageSection)ProductPageGridPageSection.Definition)?.DropDownProductTypeIDs.Contains(productTypeId) == true;
        }

        public bool Ajax
        {
            get
            {
                return true;
            }
        }

        public string AjaxEndpoint
        {
            get
            {
                return "/api/generalapi/SearchRender";
            }
        }

        public string SearchTerm { get; set; }

        public List<ProductType> ProductTypes { get; set; }

        public cmsAttributeDefinition Attribute { get; set; }

        public ProductPageGridPageSection ProductPageGrid { get; set; }

        public PageSectionViewModel ProductPageGridPageSection { get; set; }

        public passPredictiveSearch SearchHints { get; set; }

        public bool AnySearchResults
        {
            get
            {
                return ProductPageGrid.Contents.Any();
            }
        }

        public FixedPageViewModel_Search(
            serverSearchDatabase objDefinition,
            BaseViewModel objBase)
            : base(objDefinition, objBase)
        {
            SearchTerm = objBase.SearchTerm;

#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            Initialise();

#if DEBUG
            sw.Stop();
            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel_Search initialised in {sw.Elapsed}");
#endif
        }

        new protected void Initialise()
        {
            #region Preamble

            // Database methods
            var db = new databaseDataContext();

            // Attribute cache provider
            var objAttribute = new cmsAttribute();

            // Search methods
            var objSearchGenerate = new searchGenerate();

            // Price result holder
            var objPrices = new DatabaseJSONResponse();

            // HttpRequestWrapper - for pulling country / date overrides
            var objRequestWrapper = new HttpRequestWrapper(HttpContext.Current.Request);

            // Date to use
            var useDate = Framework.GetUseDate(objRequestWrapper);

            // Country to use
            var useCountry = Framework.GetCountryOverride(objRequestWrapper);

            // Attribute load fn
            var objAttributeLoad = objAttribute.loadAttributeFromCms(LanguageID).Result;

            // Attribute results (cached)
            var attributeData = objAttributeLoad;

            // Attribute Group Product Type mapping (This attribute group has that set of product types)
            var attributeGroupProductTypes = attributeData.attributeGroupProductTypes;

            // Attribute Group IDs which are common across all of the product types lent by the search results.
            var commonAttributeGroupIds = new List<byte>();

            // Instantiate ProductPageGridPageSection : PagerViewModel

            ProductPageGrid = new ProductPageGridPageSection(MasterDefinition.UrlBreakdown.SemanticString, MasterDefinition, false, Ajax, AjaxEndpoint);

            ProductPageGrid.Searchable = true;

            ProductPageGrid.InitialisePager(); // This to force Sort to pull from Request.QueryString

            // Product Type IDs as lent by the search results
            var searchResultProductTypes = Definition.searchResultCores.Select(p => p.ProductTypeID).Distinct();

            #endregion

            #region SearchTerm lends Complete Result Set

            // Attribute results from... the same? Somewhere. These pair with the product page results "initialResults".
            var attributeIds = Definition.searchResultAttributes.Select(a => a.AttributeID);

            #endregion

            #region Complete Result Set lends Product Types, Attribute Groups, Attributes

            // We're only interested in those attributes which happen to be involved with our search results.
            var attributes = attributeData.attributeDetails
                .Where(p => attributeIds.Contains(p.AttributeID))
                .OrderBy(p => p.SortOrder)
                .ToList();

            // Enumerate the attributes-of-interest
            attributes.ForEach(a =>
            {
                // For each attribute, build a list of Product Page IDs which belong to the attribute.
                a.ProductPageIDs = Definition.searchResultAttributes
                    .Where(p => p.AttributeID == a.AttributeID)
                    .Select(p => p.ProductPageID)
                    .ToList();
            });

            // Build a list of Attribute Group IDs from the list of attributes-of-interest
            var attributeGroupIdsFromSearchResults = attributes
                .Select(p => p.AttributeGroupID)
                .Distinct();

            var distinctProductTypeIDsFromSearchResults = Definition.searchResultCores.Select(src => src.ProductTypeID).Distinct();

            // Build our main list of Product Types 
            ProductTypes = attributeData // from the cached attribute result
                .productTypes            // 's product types property
                                         // but again: we're only interested in those product types which appear in the search results
                .Where(pt => Definition.searchResultCores.Where(p => p.ProductTypeID == pt.ProductTypeID).Count() > 0)
                .OrderBy(pt => pt.SortOrder)
                .Select(pt => new ProductType(
                    productType: pt,
                    productPageIds: Definition.searchResultCores.Where(p => p.ProductTypeID == pt.ProductTypeID).Select(p => p.ProductPageID)
                ))
                .ToList();

            // Search results from the DT Search Engine
            var completeResultSet = Definition.searchResultCores
                .Select(p => new ProductPageFilterablePod(p, ProductTypes.Single(pt => pt.ProductTypeID == p.ProductTypeID)))
                .ToList();

            #region Perform Sorting

            ProductPageGrid.SortContents(ref completeResultSet);

            #endregion

            // We have our base list of contents: this is what we should be building our product type list and attribute groups from.
            // We should not be building our paging around this list though. Paging should be built AFTER filtering.
            ProductPageGrid.Contents = completeResultSet.Cast<object>().ToList();

            foreach (var objGroup in attributeData.attributeGroups)
            {
                objGroup.AttributeGroupGroup = attributeData.attributeGroupGroups.SingleOrDefault(agg => agg.AttributeGroupGroupID == objGroup.AttributeGroupGroupID);

                objGroup.ProductTypes = attributeData.attributeGroupProductTypes
                    .Where(agpt => agpt.AttributeGroupID == objGroup.AttributeGroupID)
                    .Select(agpt => new AttributeGroupProductType(attributeData.productTypes.Single(pt => pt.ProductTypeID == agpt.ProductTypeID), null))
                    .ToList();

                bool productTypeMatch = true;

                foreach (byte tempProductType in searchResultProductTypes)
                {
                    if (attributeGroupProductTypes
                        .Where(p => p.AttributeGroupID == objGroup.AttributeGroupID
                                 && p.ProductTypeID == tempProductType).Count() == 0)
                    {
                        productTypeMatch = false;
                        break;
                    }
                }

                if (productTypeMatch == true)
                {
                    commonAttributeGroupIds.Add(objGroup.AttributeGroupID);
                }
            }

            ProductPageGrid.ProductTypes = searchResultProductTypes.Select(pt => ProductTypes.Single(a => a.ProductTypeID == pt)).ToList();

            if (ProductPageGrid.ProductTypes.Count() == 1)
            {
                ProductPageGrid.ProductType = ProductPageGrid.ProductTypes.First().FormValue;
            }

            ProductPageGrid.CommonAttributeGroups = attributeData.attributeGroups
                .Where(ag => commonAttributeGroupIds.Contains(ag.AttributeGroupID))
            .ToList();

            ProductPageGrid.AttributeGroupGroups = attributeData.attributeGroupGroups;

            ProductPageGrid.AttributeGroups = attributeData.attributeGroups
                .Where(p => attributeGroupIdsFromSearchResults.Contains(p.AttributeGroupID))
                .ToList();

            ProductPageGrid.AttributeGroups.ForEach(ag =>
            {
                ag.Attributes = attributes.Where(p => p.AttributeGroupID == ag.AttributeGroupID).ToList();
            });

            var check = ProductPageGrid.AttributeGroups.Where(ag => ag.Attributes.Count <= 1);

            if (!MasterDefinition.TradeAllowed) /* If you're not trade and trade-allowed */
            {
                // and there's a Clearance attribute group delivered
                ProductPageGrid.CommonAttributeGroups.RemoveAll(p => p.AttributeGroupID == 0);
                ProductPageGrid.AttributeGroups.RemoveAll(p => p.AttributeGroupID == 0);
            }

            ProductPageGrid.AttributeGroups.ForEach(ag =>
            {
                var attributeNames = HttpContext.Current.Request.QueryString[$"{HttpUtility.UrlEncode(ag.Name.ToLowerInvariant())}"]?
                                      .Split(Framework.CommaDelimiter)
                                      .ToList();

                if (attributeNames != null)
                {
                    ag.Selection = ag.Attributes
                        .Where(p => attributeNames.Contains(p.FormValue))
                        .Select(p => p.FormValue)
                        .ToList();

                    ag.Selection = ag.Selection.Distinct().ToList();
                }
                else
                {
                    ag.Selection = new List<string>();
                }
            });

            var overallSelection = ProductPageGrid.AttributeGroups.Where(ag => ag.Selection.Any())
                .SelectMany(ag => ag.Selection).Distinct().ToList();

            var initialProductPageIds = completeResultSet
                .Where(crs => String.IsNullOrEmpty(ProductPageGrid.ProductType) || crs.ProductTypeFormValue == ProductPageGrid.ProductType)
                .Select(p => p.ProductPageID).ToList();

            var attributeProductPageIds = new List<AttributeGroupProductPage>();

            foreach (var a in overallSelection)
            {
                var attributeGroup = ProductPageGrid.AttributeGroups.SingleOrDefault(g => g.Selection.Contains(a));

                var attribute = attributeGroup.Attributes.SingleOrDefault(at => at.FormValue == a);

                if (attributeGroup != null &&
                    attribute != null)
                {
                    switch (attributeGroup.LogicalOperator)
                    {
                        case "AND":
                            attributeProductPageIds.Add(new AttributeGroupProductPage(attributeGroup.AttributeGroupID, attribute.ProductPageIDs));
                            break;
                        case "OR":
                            // if this attributeGroupId is already in the array we're putting together
                            var extant = attributeProductPageIds.SingleOrDefault(p => p.AttributeGroupID == attributeGroup.AttributeGroupID);

                            if (extant != null)
                            {
                                // concatenate
                                extant.ProductPageIDs.AddRange(attribute.ProductPageIDs);
                                extant.ProductPageIDs = extant.ProductPageIDs.Distinct().ToList();
                            }
                            else
                            {
                                // if not, add to the array as normal
                                attributeProductPageIds.Add(new AttributeGroupProductPage(attributeGroup.AttributeGroupID, attribute.ProductPageIDs));
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            attributeProductPageIds.ForEach(appid =>
            {
                initialProductPageIds = initialProductPageIds.Intersect(appid.ProductPageIDs).ToList();
            });

            ProductPageGrid.FilteredIDs = initialProductPageIds;

            // What do we use FilteredProductPageIDs for...?
            //ProductPageGrid.FilteredProductPageIDs = ProductPageGrid.AttributeGroups
            //    .SelectMany(ag => ag.Attributes
            //    .Where(a => ag.Selection.Contains(a.FormValue))
            //    .SelectMany(p => p.ProductPageIDs))
            //    .ToList();

            #endregion

            #region Product Type Dance

            if (BaseViewModel.LanguageID != 1)
            {
                var englishProductTypes = new cmsSearchProductType().loadSearchProductTypeDropDownFromCms(81, 1, BaseViewModel.Date).Result;

                var productType = englishProductTypes.ProductTypes.SingleOrDefault(ept => ept.FormValue == ProductPageGrid.ProductType);

                if (productType != null)
                {
                    ProductPageGrid.ProductType = attributeData.productTypes.SingleOrDefault(pt => pt.ProductTypeID == productType.ProductTypeID)
                        ?.FormValue;
                }
            }

            // if the previous call to initialise pager didn't pull a valid product-type value from the query string, set it to 'all' now
            if (attributeData.productTypes.SingleOrDefault(pt => pt.FormValue == ProductPageGrid.ProductType) == null)
            {
                ProductPageGrid.ProductType = "";
            }

#if DEBUG

            System.Diagnostics.Debug.WriteLine($"Search restricted to Product Type: \"{ProductPageGrid.ProductType}\"");

#endif

            #endregion

            #region Build a list of product page ids to fire the render and pricing fetches for

            var productPageIdsToRender = new List<int>();

            ProductPageGrid.SortContents(ref completeResultSet);

            #region Initialise Pager

            // The call to InitialisePager is necessary to cause the PagerViewModel to carry appropriate values for things like page, pageCount, pageSize.
            ProductPageGrid.InitialisePager();

            #endregion

            #region Attempt to infer Product Type selection from search term

            if (ProductPageGrid.ProductType == null && !String.IsNullOrEmpty(SearchTerm))
            {
                var searchTermLowered = SearchTerm.ToLowerInvariant();

                var productType = ProductPageGrid.ProductTypes.SingleOrDefault(pt => {
                    return pt.Name?.ToLowerInvariant() == searchTermLowered ||
                           pt.NamePlural?.ToLowerInvariant() == searchTermLowered;
                    }
                );

                if (productType != null)
                {
                    ProductPageGrid.ProductType = productType.FormValue;
                }
            }

            #endregion

            productPageIdsToRender = ProductPageGrid.FilteredIDs.Skip((ProductPageGrid.Page - 1) * ProductPageGrid.PageSize).Take(ProductPageGrid.PageSize).ToList();

            #endregion

            // If we have any product page ids for the current page...
            if (productPageIdsToRender.Any())
            {
                #region Render the Current Page

                // Datatable to pass to SQL proc
                var productPageToRenderTable = new DataTable();

                // Datatable columns... why is Sequence necessary?
                productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
                productPageToRenderTable.Columns.Add("Sequence", typeof(int));

                int sequence = 0;

                // add rows to datatable
                foreach (var productPage in productPageIdsToRender)
                {
#if DEBUG
                    //System.Diagnostics.Debug.WriteLine(productPage);
#endif
                    productPageToRenderTable.Rows.Add(productPage, sequence++);
                }

                // call SQL render proc
                var productPages = objSearchGenerate.getRenderForSearchResultsAsync(
                    Visitor.memberTradeAccountID,
                    MasterDefinition.TradeAllowed,
                    Visitor.memberDisplayTradePrice,
                    productPageToRenderTable,
                    Date,
                    CountryItem,
                    $"{useDate:O}",
                    useCountry);

                // convert SQL render proc JSON results to List<T>
                var renderedResults = JObject.FromObject(productPages.json)
                                            .SelectToken("productPages")
                                            .ToObject<List<ProductPageFilterablePod>>();

                #endregion

                #region Fetch Pricing for the Current Page

                // call SQL pricing select for page
                objPrices = db.ProductPage_Price_Select(
                    productPageIdsToRender,
                    Visitor,
                    MasterDefinition);

                var serializer = new Newtonsoft.Json.JsonSerializer();

                serializer.ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver();

                // convert SQL pricing proc JSON results to List<a'>
                var prices = JObject.FromObject(objPrices.json ?? (new { productPages = new int[] { } }))
                    .SelectToken("productPages")
                    .Children()
                    .Select(p => p.SelectToken("productPage"))
                    .Select(p => new
                    {
                        productPageId = p.SelectToken("productPageId").Value<int>(),
                        price = p.SelectToken("productPagePrice").ToObject<ProductPagePodPrice>(serializer)
                    });

                #endregion

                #region Update our Complete Results list with this page's fleshed-out info

                // Iterate over render results
                for (var i = 0; i < renderedResults.Count(); i++)
                {
                    // grab product page id
                    var productPageId = renderedResults[i].ProductPageID;

                    // grab index into complete result set 
                    var index = completeResultSet.FindIndex(crs => crs.ProductPageID == productPageId);

                    // Set price object for this result
                    renderedResults[i].ProductPagePrice = prices.Where(p => p.productPageId == productPageId).Select(p => p.price).SingleOrDefault();

                    // Overwrite complete result set at appropriate index with this rendered, priced version
                    completeResultSet[index].ApplyRender(renderedResults[i]);

                    // Need to re-establish Product Type as it will have been lost
                    completeResultSet[index].ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == completeResultSet[index].ProductTypeID);
                }

                #endregion

                #region We've modified the complete result set, overwrite it on the product page grid. Also set TakeCount, to avoid Razor rendering upto PageSize if only n have been fetched for render.

                ProductPageGrid.Contents = completeResultSet.Cast<object>().ToList();

                ProductPageGrid.TakeCount = productPageIdsToRender.Count();

                #endregion
            }
            // We shouldn't ever not have ids for a page, but... hm. What should we do if we don't?
            else
            {
                // Probably deliver a polite error message to the client.
                // In fact this could all do with wrapping in a try catch, falling back to delivering a polite message.
                completeResultSet = new List<ProductPageFilterablePod>();
                ProductPageGrid.AttributeGroupGroups = new List<AttributeGroupGroup>();
                ProductPageGrid.AttributeGroups = new List<AttributeGroup>();
                ProductPageGrid.CommonAttributeGroups = new List<AttributeGroup>();
                ProductPageGrid.Contents = completeResultSet.Cast<object>().ToList();
            }

            if (completeResultSet.Count == 0)
            {
                var productType = attributeData.productTypes.SingleOrDefault(pt => pt.FormValue == ProductPageGrid.ProductType);

                var hints = objSearchGenerate.getPredictiveSearchHints(SearchTerm, LanguageID, CountryID, Date, productType?.ProductTypeID, Visitor.memberTradeAccountID).Result;
                var results = objSearchGenerate.getPredictiveSearch(SearchTerm, LanguageID, CountryID).Result;

                SearchHints = new passPredictiveSearch
                {
                    predictiveSearchHints = hints,
                    predictiveSearchResults = results
                };
            }
            else
            {
                SearchHints = null;
            }

            #region Create our resultant view model

            var heading = "";

            if (String.IsNullOrEmpty(SearchTerm))
            {
                var productType = ProductTypes.SingleOrDefault(pt => pt.FormValue == ProductPageGrid.ProductType);

                var productTypeName = productType?.Name ?? HtmlHelperExtra.Resource("All", LanguageID, CountryID, Date).ToString();

                heading = $"{HtmlHelperExtra.Resource("ProductType", LanguageID, CountryID, Date)} {productTypeName} ({ProductPageGrid.FilteredIDs.Count()})";
            }
            else
            {
                var allProductTypes = new cmsSearchProductType().loadSearchProductTypeDropDownFromCms(CountryID, LanguageID, Date).Result;

                var productType = allProductTypes.ProductTypes.SingleOrDefault(pt => pt.FormValue == ProductPageGrid.ProductType);

                var productTypeName = productType?.Name ?? HtmlHelperExtra.Resource("All", LanguageID, CountryID, Date).ToString();

                if (!String.IsNullOrEmpty(productTypeName))
                {
                    heading = $@"""{SearchTerm}"" {productTypeName} ({ProductPageGrid.FilteredIDs.Count()})";
                }
                else
                {
                    heading = $@"""{SearchTerm}"" ({ProductPageGrid.FilteredIDs.Count()})";
                }
            }

            if (ProductPageGrid.ProductTypes.Count > 1)
            {
                ProductPageGrid.ProductTypes.Insert(0, new ProductType
                {
                    ProductTypeID = 0,
                    Name = HtmlHelperExtra.Resource("All", LanguageID, CountryID, Date).ToString(),
                    SortOrder = 0,
                    ProductPageIDs = completeResultSet.Select(p => p.ProductPageID)
                });
            }

            ProductPageGridPageSection = new PageSectionViewModel(
                new PageSectionSelect()
                {
                    PageSectionID = -1,
                    PageSectionTypeID = 4,
                    SortOrder = null,
                    PlatformSwitch = null,
                    //Heading = $@"""{SearchTerm}"" {HtmlHelperExtra.Resource("SearchResults", LanguageID, CountryID, Date)} ({ProductPageGrid.Contents.Count})",
                    Heading = heading,
                    TradeSwitch = null,
                    Paging = true,
                    IsLifestyleGallery = false,
                    IsShop = true
                },
                BaseViewModel,
                (object)ProductPageGrid,
                true);

            #endregion

            #region Call initialise on our underlying class

            Task.Run(async () =>
            {
                await base.Initialise();
            }).Wait();

            #endregion
        }
    }
}