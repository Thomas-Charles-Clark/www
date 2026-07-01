using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Hotspot;

using wwwDesignersGuild.Models.ProductPage;

using wwwDesignersGuild.Models.Search;

using wwwDesignersGuild.Models;

using wwwDesignersGuild.ViewModels.PageSectionViewModels;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.Util;
using System.Data;
using Tweetinvi.Streaming;
using System.ServiceModel.PeerResolvers;
using System.Diagnostics;
using wwwDesignersGuild.Models.Cache.ProductPages;
using DocumentFormat.OpenXml.Math;
using Braintree;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ProductPageGridPageSection : PagerViewModel
	{
		/// <summary>
		/// These are rendered unto the view
		/// </summary>
		public string ProductPageIDs { get; set; }

		/// <summary>
		/// All AttributeIDs lent by all dbo.ProductPage_Attribute records for this section
		/// </summary>
		[JsonIgnore]
		public IEnumerable<short> AttributeIDs { get; set; }

		/// <summary>
		/// Some sort of pre-result-set. I don't know why.
		/// </summary>
		[JsonIgnore]
		public IEnumerable<ProductPageFilterablePodWrapper> PreResultSet { get; set; }

		/// <summary>
		/// Is this grid searchable? Only one page 76.
		/// </summary>
		public bool Searchable { get; set; }

		/// <summary>
		/// Is the user approaching viewing this grid through a Lifestyle perspective
		/// </summary>
		public bool IsLifestyleGallery { get; set; }

		/// <summary>
		/// The output from cache.ProductPageGrid_Select
		/// </summary>
		public PageSectionJSON Json { get; set; }

		/// <summary>
		/// dbo.Page.EnableFilterIndexing
		/// </summary>
		public bool FilterIndexingEnabled { get; set; }

		/// <summary>
		/// Does what it says on the tin
		/// </summary>
		public Regex UrlParsingRegex = new Regex(@"(.*)\/([^\s\/]+)(\/)([a-z][\d]+)(\??)(.*)$");

		/// <summary>
		/// Is what it says on the tin
		/// </summary>
		public string OriginalSemanticString { get; set; }

		/// <summary>
		/// Is what it says on the tin
		/// </summary>
		public string OriginalTitle { get; set; }

		/// <summary>
		/// If we've not been able to match a URL-sourced attribute formvalue to any attributes lent by this grid,
		/// we then display a warning stating that we found no matches for that value and have removed it from the filtering.
		/// This was throwing unexpectedly in Preview when returning from a productPage to a grid
		/// </summary>
		public string DeselectedAttributeNames { get; set; }

		/// <summary>
		/// Is what it says on the tin. Might be the same as OriginalSemanticString
		/// </summary>
		public string OriginalURLKeywords { get; set; }

		/// <summary>
		/// Whether or not we must overwrite the parent landingPage's Canonical flag.
		/// </summary>
		public bool MustOverwritePageCanonical { get; set; }

		/// <summary>
		/// The current attribute signature
		/// </summary>
		public string CurrentAttributeSignature { get; set; }

		/// <summary>
		/// The dbo.FilterIndexCombination set - is this filtered in any way?
		/// </summary>
		public IEnumerable<FilterIndexAttributeCombination> AttributeCombinations { get; set; }

		public string AttributeCombinationsString { get; set; }

		/// <summary>
		/// This for use by search - but why? Figure that out after fixing the standard usage.
		/// </summary>
		/// <param name="originalUrlKeywords">Exactly what it says on the tin</param>
		/// <param name="objMaster">The NewMasterViewModel</param>
		/// <param name="filterIndexingEnabled">dbo.Page.FilterIndexingEnabled</param>
		/// <param name="ajax">True</param>
		/// <param name="ajaxEndpoint">/l76</param>
		public ProductPageGridPageSection(
			string originalUrlKeywords,
			NewMasterDefinition objMaster,
			bool filterIndexingEnabled,
			bool ajax = false,
			string ajaxEndpoint = null) : base(-1,
											   true,
											   true,
											   objMaster,
											   ajax,
											   ajaxEndpoint)
		{
			FilterIndexingEnabled = filterIndexingEnabled;
			Searchable = false;
			IsLifestyleGallery = false;
			OriginalSemanticString = null;
			OriginalTitle = null;

			DeselectedAttributeNames = null;

			OriginalURLKeywords = originalUrlKeywords;

			ProductTypes = new List<ProductType>();

			MustOverwritePageCanonical = false;
		}

		// The standard constructor: private because you are not meant to instantiate ProductPageGridPageSection objects directly
		// rather you are supposed to utilitse the static CreateAsync function. Because at least in this version of C#, constructors cannot be async.
		private ProductPageGridPageSection(
			string originalUrlKeywords,
			PageSectionJSON json,
			bool pagingEnabled,
			bool filterIndexingEnabled,
			bool isShop,
			bool isLifestyleGallery,
			NewMasterDefinition objMaster,
			bool ajax = false,
			string ajaxEndpoint = null) : base(json.PageSectionID,
												  pagingEnabled,
												  isShop,
												  objMaster,
												  ajax,
												  ajaxEndpoint)
		{
			OriginalURLKeywords = originalUrlKeywords;
			Json = json;
			FilterIndexingEnabled = filterIndexingEnabled;
			Searchable = false;
			IsLifestyleGallery = isLifestyleGallery;
			OriginalSemanticString = null;
			OriginalTitle = null;

			DeselectedAttributeNames = null;

			ProductTypes = new List<ProductType>();

			MustOverwritePageCanonical = false;

			// All product page grids are now ajax enabled
			Ajax = true;

			// All product page grids ajax to this same endpoint (except Search which is a special case handled separately)
			AjaxEndpoint = "/api/ProductPageAPI/FetchProductPageGridPortion";
		}

		// Determine the purpose of and usage of this function
		public new List<ProductPageFilterablePod> GetContents()
		{
			// If you have at least one Filter specified, or your ProductType is not an empty string
			if (FilterCount > 0 || ProductType != "")
			{
				// If your productType is an empty string
				if (ProductType == "")
				{
					// Just return from Contents where the Filter selected contains ProductPageID
					var r = Contents.Cast<ProductPageFilterablePod>()
						.Where(ir => (!FilteredIDs.Any() || FilteredIDs.Contains(ir.ProductPageID)))
						.ToList();

					// Why are we setting IsLifestyle here? Isn't it already set? Disabling for now
					//r.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

					return r;
				}
				else
				{
					// We have a product type, prefilter the Contents to match:
					var matches = Contents.Cast<ProductPageFilterablePod>()
						.Where(ir => ir.ProductType.FormValue == ProductType)
						.ToList();

					// Then if we have any further filtering to do, apply it.
					if (FilteredIDs.Any())
					{
						matches = matches.Where(ptm => FilteredIDs.Contains(ptm.ProductPageID)).ToList();
					}

					//SortContents(ref matches);

					//Again, why are we... I can't imagine why we need to set this here
					//matches.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

					return matches;
				}
			}
			else
			{
				// No filters, no product type, return everything.
				var r = Contents.Cast<ProductPageFilterablePod>().ToList();

				return r;
			}
		}

		// Determine the purpose of and usage of this function
		public IEnumerable<ProductPagePodViewModel> GetProductPages(NewMasterDefinition model, int? skip, int? take)
		{
			// Should already be sorted correctly, shouldn't it...?
			if (skip == null)
			{
				var results = GetContents();

				results = SortContents(results);

				return results.Select(p => new ProductPagePodViewModel(model, p, IsLifestyleGallery));
			}
			else
			{
				var results = GetContents();

				results = SortContents(results);

				return results.Skip(skip.Value).Take(take.Value).Select(p => new ProductPagePodViewModel(model, p, IsLifestyleGallery));
			}
		}

		/// <summary>
		/// CreateAsync because we may want to call for unrelated database collections and we might as well do that in parallel if we can
		/// </summary>
		/// <param name="originalUrlKeywords">The keywords as they originally were (URL segments between culture and page identifier)</param>
		/// <param name="productPage">The result of cache.ProductPageGrid_Select</param>
		/// <param name="pagingEnabled">Whether or not paging is enabled on this grid. If there's only a handful of product pages in the grid, we might not want paging.</param>
		/// <param name="filterIndexingEnabled">
		///		Allow filter changes to alter markup, title, url segments,
		///		and markup on the page. Permits a filtered view of the page to be accessible with no query string, e.g. /en-gb/orange-silk-cushions/l1046
		/// </param>
		/// <param name="isShop">This seems to be a stupid relic</param>
		/// <param name="isLifestyleGallery">This also might be a stupid relic, but it varies just based on the queryString parameters "lifestyle"</param>
		/// <param name="model">The BaseViewModel upon which the landingPageViewMode for which this pageSection is being created.</param>
		/// <returns></returns>
		public static async Task<ProductPageGridPageSection> CreateAsync(
			string originalUrlKeywords,
			PageSectionJSON productPage,
			bool pagingEnabled,
			bool filterIndexingEnabled,
			bool isShop,
			bool isLifestyleGallery,
			BaseViewModel model
			)
		{
			#region Properties

			// The entire set of product pages assigned to this grid
			var completeResultSet = new List<ProductPageFilterablePod>();

			// The entire set of attributeIds lent by the entire set of product pages via dbo.ProductPage_Attribute
			var attributeIds = new List<short>();

			// The tradeAccountId - soft signed-in is acceptable
			int? tradeAccountId = null;

			// A database context object
			var objDbContext = new databaseDataContext();

			// The cache-enabled wrapper around pulling attributeGroups and attributeIds
			var objAttribute = new cmsAttribute();

			// The result of this CreateAsync 
			var result = (ProductPageGridPageSection)null;

			// The productPageGridPageSectionCacheProvider
			var cacheProvider = new ProductPageGridPageSectionCacheProvider();

			// Stopwatch to help identify slow spots while debugging
#if DEBUG
			var sw = new Stopwatch();

			sw.Start();

			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 1 in\t\t{sw.Elapsed}");

			sw.Restart();
#endif

			#endregion

			#region Pull Session Variables

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}

			#endregion

			#region Set the RequestURL on the BVM

			model.RequestURL = HttpContext.Current.Request.RawUrl;

			#endregion

			#region Check the Cache

			result = cacheProvider.Get(
				productPage.PageSectionID,
				model.LanguageID,
				model.CountryID, 
				tradeAccountId,
				model.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
				model.Date);

			#endregion

			if (result == null)
			{
				// Cache was not hit: we need to do just about seven out of the whole nine yards here, then cache the results.
				// That way when the next request comes through and hits the cache, there are only three yards to be done to return
				// the correct result.
				model.ProductPageGridCacheHit = false;

				var objAttributeLoad = objAttribute.loadAttributeFromCms(model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

				// Thanks https://stackoverflow.com/questions/32119507/multiple-awaits-vs-task-waitall-equivalent#answer-32119616

				await Task.WhenAll(objAttributeLoad); //(, ...); // are there any other loads at all which could be done here

				var attributeData = await objAttributeLoad;

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 2 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				result = new ProductPageGridPageSection(
					originalUrlKeywords,
					productPage,
					pagingEnabled,
					filterIndexingEnabled,
					isShop,
					isLifestyleGallery,
					model.MasterDefinition);

				// again, pre to what?
				var preResultSet = new List<ProductPageFilterablePodWrapper>();

				// problem is that at this point, result.Json.JSON contains pageNumber * pageCount number of entries.
				// is it the procedure obeying the paging which is causing this? We need the procedure to NOT obey
				// the paging, and the full result set (8000 products, for instance) to sit in a MemoryCache object.
				var jsonString = result.Json.JSON.Replace("{\"productPages\":", "").TrimEnd(new char[] { '}' });

				// Stream the string into reified objects without putting it all in RAM at once
				using (var stream = Framework.GenerateStreamFromString(jsonString, System.Text.Encoding.UTF8))
				{
					using (var streamReader = new StreamReader(stream))
					{
						using (JsonReader jsonReader = new JsonTextReader(streamReader))
						{
							jsonReader.SupportMultipleContent = true;

							var serializer = new JsonSerializer
							{
								ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver()
							};

							while (jsonReader.Read())
							{
								if (jsonReader.TokenType == JsonToken.StartObject)
								{
									var obj = JObject.Load(jsonReader);
									preResultSet.Add(obj.ToObject<ProductPageFilterablePodWrapper>(serializer));
								}
							}
						}
					}
				}

				// Keep hold of that result
				// I really should name this somemthing more appropriate
				result.PreResultSet = preResultSet;

				// I really should rename this to something more appropriate
				completeResultSet = result.PreResultSet.Select(p => p.ProductPage).ToList();

				// Argh. Lifestyle.
				completeResultSet.ForEach(c => c.IsLifestyle = isLifestyleGallery);

				// All ProductPageIDs in the grid.
				var productPageIds = completeResultSet.Select(p => p.ProductPageID).ToList();

				// The Json result from ProductPage_Attribute_Select via cache.ProductPageGrid_Select
				attributeIds = result.Json.attributes
												.Where(p => productPageIds.Contains(p.ProductPageID))
												.GroupBy(p => p.AttributeID)
												.Select(p => p.Key).ToList();

				// This loops over the entire result set and assigns AttributeFormValues 
				completeResultSet.ForEach(pp =>
				{
					// attributeIds having association to this productPageId
					var ids = result.Json.attributes.Where(p => p.ProductPageID == pp.ProductPageID)
															.Select(p => p.AttributeID).ToList();

					// set AttributeFormValues (the querySring sanitised version of the Name text)
					pp.AttributeFormValues = attributeData.attributeDetails.Where(p => ids.Contains(p.AttributeID))
																			.Select(p => p.FormValue)
																			.ToList();
					// The full ProductType object for this ProductPage
					pp.ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == pp.ProductTypeID);
				});

				// Distinct list of ProductTypeIDs which feature in this grid
				var gridProductTypeIds = completeResultSet.Select(p => p.ProductTypeID).Distinct();

				// the result's distinct list of ProductType objects (for with which to build the filter option)
				result.ProductTypes = attributeData.productTypes.Where(pt => gridProductTypeIds.Contains(pt.ProductTypeID)).ToList();

				// all of those product types can and will lend a set of productPageIds, same as attributes do, for filtering with
				result.ProductTypes.ForEach(pt =>
				{
					// See?
					pt.ProductPageIDs = completeResultSet.Where(crs => crs.ProductTypeID == pt.ProductTypeID).Select(crs => crs.ProductPageID);
				});

				// If you have more than one productType in the set
				if (result.ProductTypes.Count > 1)
				{
					// Create a new dummy one named All and have it... well, reflect ALL of the productPageIds in the grid.
					result.ProductTypes.Insert(0, new ProductType
					{
						ProductTypeID = 0,
						Name = HtmlHelperExtra.Resource("All", model.LanguageID, model.CountryID, model.Date).ToString(),
						SortOrder = 0,
						ProductPageIDs = preResultSet.Select(p => p.ProductPageID)
					});
				}
				else
				{
					// However, if we have precisely one product type represented in this grid
					if (result.ProductTypes.Count() == 1)
					{
						// We can set the ProductType formValue string on the result now, so as to pre-select the filter option.
						result.ProductType = result.ProductTypes[0].FormValue;

						// Grab some filter index combinations, should any exist.
						// Filter index restrictions fn
						var objFilterIndexCombinations = await new cmsFilterIndexAttributeCombination().loadFilterIndexingAttributeCombinationsFromCmsAsync(
								new PassFilterIndexAttributeCombinationCacheFetch(model.LanguageID, result.ProductTypes[0].ProductTypeID, model.Date, model.CountryURL),
								model
							);

						// It's fine to cache the results of that call on this object
						result.AttributeCombinations = objFilterIndexCombinations.Combinations;

						result.AttributeCombinationsString = JsonConvert.SerializeObject(result.AttributeCombinations);
					}
				}

				// The Json result set provides these attributeIds from ProductPage_Attribute_Select via cache.ProductPageGrid_Select
				result.AttributeIDs = attributeIds;

				#region Set "Contents" on this PageSection (the base class) to an object cast from completeResultSet

				result.Contents = completeResultSet.Cast<object>().ToList();

				#endregion

				#region Run the result.InitialisePager routine

				result.InitialisePager();

				#endregion

				#region

				// If you've got fewer results than PageSize, as has just been determined by the call to InitialisePager, 
				// then set your TakeCount to the lower of the two counts.
				if (result.Contents.Count() < result.PageSize)
					result.TakeCount = result.Contents.Count();
				else // Otherwise, set TakeCount to PageSize. Woo.
					result.TakeCount = result.PageSize;

				#endregion

				// Grab the full attribute object
				var attributes = attributeData.attributeDetails
												.Where(p => attributeIds.Contains(p.AttributeID))
												.OrderBy(p => p.SortOrder)
												.ToList();

				attributes.ForEach(a =>
				{
					// Should... they not already have ProductPageIDs? Check this because it might be redundant

					// Iterate over all attributes and set the ProductPageIDs
					a.ProductPageIDs = result.Json
												.attributes
												.Where(p => p.AttributeID == a.AttributeID)
												.Select(p => p.ProductPageID)
												.ToList();
				});

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 3 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				// Build the attributeGroups
				var attributeGroupIds = attributes.Select(p => p.AttributeGroupID).Distinct();

				result.AttributeGroupGroups = attributeData.attributeGroupGroups;

				result.AttributeGroups = attributeData.attributeGroups
													.Where(p => attributeGroupIds.Contains(p.AttributeGroupID))
													.ToList();

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 4 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				result.AttributeGroups.ForEach(ag =>
				{
					ag.Attributes = attributes.Where(p => p.AttributeGroupID == ag.AttributeGroupID).ToList();

					ag.AttributeGroupGroup = attributeData.attributeGroupGroups.SingleOrDefault(agg => agg.AttributeGroupGroupID == ag.AttributeGroupGroupID);

					ag.ProductTypes = attributeData.attributeGroupProductTypes
						.Where(agpt => agpt.AttributeGroupID == ag.AttributeGroupID)
						.Select(agpt => new AttributeGroupProductType(attributeData.productTypes.Single(pt => pt.ProductTypeID == agpt.ProductTypeID), agpt.LexicographicalSortOrder))
						.ToList();

					if (!String.IsNullOrEmpty(result.ProductType))
					{
						ag.LexicographicalSortOrder = ag.ProductTypes.SingleOrDefault(pt => pt.FormValue == result?.ProductType).LexicographicalSortOrder ?? 0;
					}
				});

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 5 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				if (!model.MasterDefinition.TradeAllowed) /* If you're not trade and trade-allowed */
				{
					// and there's a Clearance attribute group delivered
					result.AttributeGroups.RemoveAll(p => p.AttributeGroupID == 0);
					// remove it
				}

				result.ProductPageIDs = JsonConvert.SerializeObject(result.Json.totals.ProductPageIDs);

				#region Write the object to the cache see as we had a cache miss.

				// But are there not other things to be done to fully fledge this ProductPagePageSectionGrid before caching... check below.

				cacheProvider.Set(
					result.PageSectionID,
					model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
					model.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
					tradeAccountId,
					model.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
					model.Date,
					result);

				#endregion


#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync finished Checkpoint 2 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif
			}
			else
			{
				// Great, we hit the cache, but are there not other things to be done in here to fully fledge this ProductPagePageSectionGrid ?
				model.ProductPageGridCacheHit = true;

				completeResultSet = result.PreResultSet.Select(p => p.ProductPage).ToList();

				completeResultSet.ForEach(c => c.IsLifestyle = isLifestyleGallery);

				result.InitialisePager();

				if (result.Contents.Count() < result.PageSize)
					result.TakeCount = result.Contents.Count();
				else
					result.TakeCount = result.PageSize;

				attributeIds = result.AttributeIDs.ToList();

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync skipped Checkpoint 2 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif
				// We can't have people going caching silly title tags meta descriptions etc.
				// So even if we have a cache hit, we need to use the real NewMasterDefinition
				result.MasterDefinition = model.MasterDefinition;
			}

			// This must be dynamic and cannot be cached: it's the result of attribute filtering
			// is that statement a misgiving? cannot be cached?
			var attributeFormValuesFromQueryString = new List<string>();

			result.AttributeGroups.ForEach(ag =>
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

					attributeFormValuesFromQueryString.AddRange(ag.Selection);
				}
				else
				{
					ag.Selection = new List<string>();
				}
			});

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 6 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			#region QueryString based attribute determination dance

			// produce the power set of all words in the malleable url segment
			// it's possible that some are attributes
			// again this part must be dynamic as we don't include attribute formValues in the cache key
			var queryStringAttributesPowerSet = EnumerableExtensions.PowerSet(attributeFormValuesFromQueryString);

			var queryStringAttributesList = new List<AttributeSorter>();

			foreach (var potentialAttribute in queryStringAttributesPowerSet)
			{
				var value = String.Join("-", potentialAttribute);

				foreach (var ag in result.AttributeGroups.Where(g => g.InvolveInFilterIndexing(result.ProductType)))
				{
					var attrs = ag.Attributes.ToList();
					var attr = attrs.SingleOrDefault(a => a.FormValue == HttpUtility.UrlEncode(value));

					// Do this little dance
					if (attr != null)
					{
						queryStringAttributesList.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, ag.ProductTypes.Single(pt => pt.FormValue == result.ProductType).LexicographicalSortOrder, ag.LogicalOperator));

						ag.Selection.Add(HttpUtility.UrlEncode(value));

						ag.Selection = ag.Selection.Distinct().ToList();
					}
				}
			}

			#endregion

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 7 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			#region URLKeyword based attribute determination dance

			var url = HttpContext.Current.Request.Url.ToString();

			if (!String.IsNullOrEmpty(originalUrlKeywords))
				url = url.Replace(originalUrlKeywords, "");

			var matches = result.UrlParsingRegex.Matches(url);

			var attributesList = new List<AttributeSorter>();

			if (matches.Count == 1)
			{
				if (matches[0].Groups.Count >= 3)
				{
					// produce the power set of all words in the malleable url segment
					// it's possible that some are attributes
					var urlKeywordsAttributesPowerSet = EnumerableExtensions.PowerSet(matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter).AsEnumerable<string>());

					foreach (var potentialAttribute in urlKeywordsAttributesPowerSet)
					{
						var value = String.Join("-", potentialAttribute);

						foreach (var ag in result.AttributeGroups.Where(g => g.InvolveInFilterIndexing(result.ProductType)))
						{
							var attrs = ag.Attributes.ToList();
							var attr = attrs.SingleOrDefault(a => a.FormValue == HttpUtility.UrlEncode(value));

							// Do this little dance
							if (attr != null)
							{
								attributesList.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, ag.ProductTypes.Single(pt => pt.FormValue == result.ProductType).LexicographicalSortOrder, ag.LogicalOperator));

								ag.Selection.Add(HttpUtility.UrlEncode(value));

								ag.Selection = ag.Selection.Distinct().ToList();
							}
						}
					}
				}
			}

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 8 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			#endregion

			if (attributesList.Any())
			{
				// this is a noop! fun to say, even more fun to program. NOOP NOOP!
			}
			else
			{
				attributesList = queryStringAttributesList;
			}

			attributesList = attributesList.OrderBy(al => al.LexicographicalSortOrder).ToList();

			var attributeSignature = String.Join(".", attributesList.Select(a => $"{a.AttributeID}"));

			result.CurrentAttributeSignature = attributeSignature;

			model.AttributeFilterText = String.Join(" ", attributesList.Select(al => al.AttributeName));

			var filterIndexCombination = result.AttributeCombinations?.SingleOrDefault(c => c.AttributeSignature == attributeSignature);

			model.AttributeSignatureText = filterIndexCombination?.FooterText;

			model.FilterIndexCombinationEnabled = filterIndexCombination != null;

			if (!String.IsNullOrEmpty(model.AttributeSignatureText))
			{
				model.AttributeSignatureText = HtmlHelperExtra.ProcessHTMLImages(model.AttributeSignatureText, null, new long[] { }, new ImageHotspot[] { }, model);
			}

			if (!String.IsNullOrEmpty(filterIndexCombination?.TitleTag))
			{
				model.FilterIndexingTitleTag = filterIndexCombination.TitleTag;
			}

			if (attributesList.Any())
			{
				result.MasterDefinition.GoogleHrefs = new databaseDataContext().Page_Hreflang_GenerateMultiple(model.ContentID, model.Date, JsonConvert.SerializeObject(attributesList.Select(al => new { id = al.AttributeID }).ToList())).ToList().ToList();
			}

			//var regex = new Regex(@"([^|]+)");

			//var titleMatches = regex.Matches(result.MasterDefinition.TitleTag);

			//if (titleMatches != null)
			//{
			//    if (titleMatches.Count > 1)
			//    {
			//        result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} | {titleMatches[1].Value.Trim()}";
			//    }
			//}

			result.OriginalTitle = result.MasterDefinition.TitleTag;

			if (result.FilterIndexingEnabled)
			{
				if (!String.IsNullOrEmpty(filterIndexCombination?.TitleTag))
				{
					result.MasterDefinition.TitleTag = filterIndexCombination.TitleTag;
				}

				if (!String.IsNullOrEmpty(filterIndexCombination?.MetaDescription))
				{
					result.MasterDefinition.MetaDescription = filterIndexCombination.MetaDescription;
				}

				if (!String.IsNullOrEmpty(model.AttributeFilterText))
				{
					if (String.IsNullOrEmpty(filterIndexCombination?.TitleTag))
					{
						if (!String.IsNullOrEmpty(model.CategoryPageProductType?.NamePlural))
						{
							result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} {model.CategoryPageProductType.NamePlural} {result.MasterDefinition.UrlBreakdown.countryDetails.TitleTagSuffix}";
						}
						else
						{
							result.MasterDefinition.TitleTag = $"{model.AttributeFilterText} {result.MasterDefinition.TitleTag} {result.MasterDefinition.UrlBreakdown.countryDetails.TitleTagSuffix}";
						}
					}

					if (String.IsNullOrEmpty(filterIndexCombination?.MetaDescription))
					{
						result.MasterDefinition.MetaDescription = HtmlHelperExtra.ProcessAttributeFilterText(
							input: result.MasterDefinition.MetaDescription ?? "",
							attributeFilterText: model.AttributeFilterText,
							productTypePluralPropercase: model.CategoryPageProductType?.NamePlural,
							attributeSignatureText: model.AttributeSignatureText,
							enabledByAttributeCombination: model.FilterIndexCombinationEnabled,
							useHtml: false);

						result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("<span class=\"filter-text\">", "");

						result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("</span> ", "");

						result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("<span class=\"no-filter-text\">", "");

						result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("</span> ", "");

						completeResultSet.ForEach(crs =>
						{
							if (!String.IsNullOrEmpty(crs.ShortHeading))
							{
								crs.Heading = $"{crs.ShortHeading} {model.AttributeFilterText} {crs.ProductType.Name}";
							}
						});
					}
				}

				if (!String.IsNullOrEmpty(filterIndexCombination?.TitleTag))
				{
					result.MasterDefinition.TitleTag = $"{filterIndexCombination.TitleTag} {result.MasterDefinition.UrlBreakdown.countryDetails.TitleTagSuffix}";
				}

				if (!String.IsNullOrEmpty(filterIndexCombination?.MetaDescription))
				{
					result.MasterDefinition.MetaDescription = filterIndexCombination.MetaDescription;
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{ProductTypeStart}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{ProductTypeEnd}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{FilterStart}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{FilterEnd}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{NoFilterStart}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Replace("{NoFilterEnd}", "");
					result.MasterDefinition.MetaDescription = result.MasterDefinition.MetaDescription?.Trim();
				}
			}

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 9 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			var overallSelection = result.AttributeGroups.Where(ag => ag.Selection.Any())
				.SelectMany(ag => ag.Selection).Distinct().ToList();

			var initialProductPageIds = completeResultSet
				.Where(crs => result.ProductType == null || crs.ProductTypeFormValue == result.ProductType)
				.Select(p => p.ProductPageID).ToList();

			var attributeProductPageIds = new List<AttributeGroupProductPage>();

			var detectedAttributeGroups = new List<AttributeGroup>();

			foreach (var a in overallSelection)
			{
				var attributeGroup = result.AttributeGroups.SingleOrDefault(g => g.Selection.Contains(a));

				var attribute = attributeGroup.Attributes.SingleOrDefault(at => at.FormValue == a);

				if (attributeGroup != null &&
					attribute != null)
				{
					detectedAttributeGroups.Add(attributeGroup);

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

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 10 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			if (detectedAttributeGroups.Any() && detectedAttributeGroups.Count == attributesList.Count)
			{
				if (result.AttributeCombinations.FirstOrDefault(r => r.AttributeSignature == attributeSignature) == null)
				{
					result.MustOverwritePageCanonical = true;
				}
			}

			attributeProductPageIds.ForEach(appid =>
			{
				initialProductPageIds = initialProductPageIds.Intersect(appid.ProductPageIDs).ToList();
			});

			result.AttributeGroups.ForEach(ag =>
			{
				var productPageIdsToConsider = initialProductPageIds;

				attributeProductPageIds.ForEach(appid =>
				{
					if (appid.AttributeGroupID != ag.AttributeGroupID)
					{
						productPageIdsToConsider = productPageIdsToConsider.Intersect(appid.ProductPageIDs).ToList();
					}
				});

				var productPageIdsAffectingThisAttributeGroup = new List<int>();

				switch (ag.LogicalOperator)
				{
					case "AND":
						productPageIdsAffectingThisAttributeGroup = productPageIdsToConsider.Intersect(
							attributeProductPageIds.SelectMany(appid => appid.ProductPageIDs)
						).ToList();
						break;
					case "OR":
						productPageIdsAffectingThisAttributeGroup = productPageIdsToConsider.Intersect(
							attributeProductPageIds.Where(appid => appid.AttributeGroupID != ag.AttributeGroupID)
													.SelectMany(appid => appid.ProductPageIDs)
						).ToList();
						break;
				}

				ag.Attributes.ForEach(a =>
				{
					if (ag.LogicalOperator == "AND" && productPageIdsAffectingThisAttributeGroup.Any())
					{
						a.AvailableProductPageIDs = a.ProductPageIDs.Intersect(productPageIdsAffectingThisAttributeGroup).ToList();
					}
					else
					{
						a.AvailableProductPageIDs = a.ProductPageIDs;
					}

					if (a.AvailableProductPageIDs == null)
					{
						a.AvailableProductPageIDs = new List<int>();
					}
				});
			});

			result.FilteredIDs = initialProductPageIds;

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 11 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			if (result.FilteredIDs.Count == 0)
			{
				var problemAttributes = result.AttributeGroups.Where(ag => ag.Selection.Any()).SelectMany(ag =>
				{
					return ag.Attributes.Where(a => ag.Selection.Contains(a.FormValue));
				}).ToList();

				result.DeselectedAttributeNames = String.Join(", ", problemAttributes.Select(pa => pa.Name));

				result.FilteredIDs = completeResultSet.Select(p => p.ProductPageID).ToList(); ;// result.Json.totals.ProductPageIDs;

				model.AttributeSignatureText = null;
			}

			#region Build a list of product page ids to fire the render and pricing fetches for

			var productPageIdsToRender = new List<int>();
			
			var pagingResults = Framework.StreamSegmentOfJsonArray(result.Json.JSON, "productPages", result.Page * result.PageSize, result.PageSize, result.Sort).ToList();

#if DEBUG
			Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 13 in\t\t{sw.Elapsed}");
			sw.Restart();
#endif

			productPageIdsToRender = pagingResults.Select(p => p.ProductPageID).ToList();// result.FilteredIDs.Skip((result.Page - 1) * result.PageSize).Take(result.PageSize).ToList();

			#endregion

			// Datatable to pass to SQL proc
			var productPageToRenderTable = new DataTable();

			// Datatable columns... why is Sequence necessary?
			productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
			productPageToRenderTable.Columns.Add("Sequence", typeof(int));

			int sequence = 0;

			// add rows to datatable
			foreach (var productPageId in productPageIdsToRender)
			{
				productPageToRenderTable.Rows.Add(productPageId, sequence++);
			}

			// If we have any product page ids for the current page...
			if (productPageIdsToRender.Any())
			{
				#region Render the Current Page

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 14 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				//var objSearch = new cmsSearch();
				//var objSearchResultReturn = await objSearch.loadSearchFromCms(
				//	productPageToRenderTable,
				//	model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
				//	model.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
				//	searchTerm: null,
				//	tradeAccountId,
				//	null,
				//	model.Date,
				//	result.PageSectionID);

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 15 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				//Parallel.ForEach(objSearchResultReturn.searchResultCores, tempResult =>
				//{
				//	var tempSearch = pagingResults.Where(p => p.ProductPageID == tempResult.ProductPageID).FirstOrDefault();

			 //		if (tempSearch == null) { }
				//	else
				//	{
				//		tempResult.ScorePercent = tempSearch.ProductPage.ScorePercent;
				//	}
				//});

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 16 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				//objSearchResultReturn.searchResultCores = objSearchResultReturn.searchResultCores.OrderByDescending(p => p.ScorePercent).ThenByDescending(p => p.SearchSortOrder).ThenBy(p => p.ProductPageID).ToList();

				//Parallel.ForEach(pagingResults, (r) =>
				//{
				//	var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);

				//	if (src != null)
				//	{
				//		r.ProductPage.ApplySorting(src);
				//	}
				//});

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 17 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				//Parallel.ForEach(completeResultSet, (r) =>
				//{
				//	var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);

				//	if (src != null)
				//	{
				//		r.ApplySorting(src);
				//	}
				//});

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 18 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif
				var objSearchGenerate = new searchGenerate();

				// call SQL render proc
				var productPages = objSearchGenerate.getRenderForSearchResultsAsync(
					tradeAccountId: model.Visitor.memberTradeAccountID,
					isTrade: model.MasterDefinition.TradeAllowed,
					displayTradePrice: model.Visitor.memberDisplayTradePrice,
					productPageToRenderTable,
					model.Date,
					model.MasterDefinition.UrlBreakdown.countryDetails,
					dateOverride: $"{model.Date:O}",
					countryOverride: model.MasterDefinition.UrlBreakdown.countryDetails.CountryCode
				);

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 19 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				// convert SQL render proc JSON results to List<T>
				var renderedResults = JObject.FromObject(productPages.json)
											.SelectToken("productPages")
											.ToObject<List<ProductPageFilterablePod>>();

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 20 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				#endregion

				#region Fetch Pricing for the Current Page

				// call SQL pricing select for page
				var objPrices = objDbContext.ProductPage_Price_Select(
					productPageIdsToRender,
					model.Visitor,
					model.MasterDefinition);

				var serializer = new JsonSerializer
				{
					ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver()
				};

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

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 21 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				#endregion

				#region Update our Complete Results list with this page's fleshed-out info

				// Iterate over render results
				for (var i = 0; i < renderedResults.Count(); i++)
				{
					// grab product page id
					var productPageId = renderedResults[i].ProductPageID;

					// grab index into complete result set 
					var index = completeResultSet.FindIndex(crs => crs.ProductPageID == productPageId);

					if (index > -1)
					{
						// Set price object for this result
						renderedResults[i].ProductPagePrice = prices.Where(p => p.productPageId == productPageId).Select(p => p.price).SingleOrDefault();

						// Overwrite complete result set at appropriate index with this rendered, priced version
						completeResultSet[index].ApplyRender(renderedResults[i]);

						// Need to re-establish Product Type as it will have been lost
						completeResultSet[index].ProductType = result.ProductTypes.Single(pt => pt.ProductTypeID == completeResultSet[index].ProductTypeID);
					}
				}

#if DEBUG
				Debug.WriteLine($"ProductPageGridPageSection.CreateAsync reached Checkpoint 22 in\t\t{sw.Elapsed}");
				sw.Restart();
#endif

				#endregion
			}
			else
			{
				result.Contents = completeResultSet.Cast<object>()
													.ToList();
			}

			result.InitialisePager();

			return result;
		}

		public List<ProductPageFilterablePod> SortContents(List<ProductPageFilterablePod> items)
		{
			switch (Sort)
			{
				case "bestSellers":
					return items
						.OrderBy(p => p.SalesRank)
						.ToList();
				case "colour":
					return items
						.OrderBy(p => p.ColourSort)
						.ToList();
				case "alphaAZ":
					return items
						.OrderBy(p => p.AZSortOrder)
						.ToList();
				case "alphaZA":
					return items
						.OrderByDescending(p => p.AZSortOrder)
						.ToList();
				case "new":
					return items
						.OrderBy(p => p.NewSort)
						.ToList();
				case "priceHighToLow":
					return items
						.OrderBy(p => p.PriceSortOrder)
						.ToList();
				case "priceLowToHigh":
					return items
						.OrderByDescending(p => p.PriceSortOrder)
						.ToList();
				case "discountHighToLow":
					return items
						.OrderByDescending(p => p.DiscountSortOrder)
						.ToList();
				case "discountLowToHigh":
					return items
						.OrderBy(p => p.DiscountSortOrder)
						.ToList();
				case "stockHightoLow":
					return items
						.OrderByDescending(p => p.StockSortOrder)
						.ToList();
				case "stockLowToHigh":
					return items
						.OrderBy(p => p.StockSortOrder)
						.ToList();
				case "default":
				case null:
				default:
					return items.OrderBy(p => p.MerchandisedSortOrder).ToList();
			}
		}

		public string BuildAttributeURL(AttributeGroup attributeGroup, AttributeDetail attribute, string currentAttributeSignature, IEnumerable<FilterIndexAttributeCombination> attributeCombinations)
		{
			// 1. Guard clauses for early exit
			if (!FilterIndexingEnabled || attributeGroup == null || attribute == null) return "#";
			if (!attributeGroup.InvolveInFilterIndexing(ProductType)) return "#";

			var context = HttpContext.Current;
			if (context?.Request?.Url == null) return "#";

			string originalUrl = context.Request.Url.ToString();
			string placeholder = $"{(char)254}";
			string url = originalUrl.Replace(OriginalURLKeywords, placeholder);

			var matches = UrlParsingRegex.Matches(url);
			if (matches.Count != 1 || matches[0].Groups.Count < 3) return "#";

			// 2. Pre-filter active groups and flatten attributes into an efficient lookup dictionary O(N)
			var activeAttributeGroups = AttributeGroups
				.Where(ag => ag.InvolveInFilterIndexing(ProductType))
				.ToList();

			var attributeLookup = activeAttributeGroups
				.SelectMany(ag => ag.Attributes)
				.GroupBy(a => a.FormValue)
				.ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

			// 3. Directly extract keywords from the URL without using PowerSet
			string[] urlSegments = matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter);
			var split = new List<AttributeSorter>();

			foreach (var segment in urlSegments)
			{
				if (attributeLookup.TryGetValue(segment, out var attr))
				{
					split.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, null, null));
				}
			}

			// 4. Safely inject or replace the target incoming attribute
			split.RemoveAll(s => s.AttributeGroupID == attribute.AttributeGroupID);
			split.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, null, null));

			// 5. Build lookup metadata maps to avoid nested looping
			var groupMap = activeAttributeGroups.ToDictionary(g => g.AttributeGroupID);

			// Populate sorting and logic data
			foreach (var s in split)
			{
				if (groupMap.TryGetValue(s.AttributeGroupID, out var matchingGroup))
				{
					s.LogicalOperator = matchingGroup.LogicalOperator;

					var prodType = matchingGroup.ProductTypes?.FirstOrDefault(pt => pt.FormValue == ProductType);
					if (prodType != null)
					{
						s.LexicographicalSortOrder = prodType.LexicographicalSortOrder;
					}
				}
			}

			// 6. Sort and validate the signatures
			split = split.OrderBy(s => s.LexicographicalSortOrder ?? int.MaxValue).ToList();

			string signatureToMatch = string.IsNullOrEmpty(currentAttributeSignature)
				? attribute.AttributeID.ToString()
				: string.Join(".", split.Where(s => s.AttributeGroupID != 0).Select(s => s.AttributeID));

			bool isCombinationValid = attributeCombinations.Any(ac =>
				ac.Enabled == true &&
				string.Equals(ac.AttributeSignature, signatureToMatch, StringComparison.Ordinal));

			if (!isCombinationValid) return "#";

			// 7. Construct final URL
			split.Add(new AttributeSorter(0, 0, OriginalSemanticString, null, null, null));
			url = UrlParsingRegex.Replace(url, $"$1/{string.Join("-", split.Select(s => s.Attribute))}/$4");

			// Clean query string parameters
			int qIndex = url.IndexOf('?');
			if (qIndex > -1)
			{
				url = url.Substring(0, qIndex);
			}

			return url;


			//if (attribute.FormValue == "blue")
			//{
			//    int f = 0;
			//}

			//// if FilterIndexing is enabled on this grid's containing page
			//if (FilterIndexingEnabled)
			//{
			//	// does this ProductType describe a LexicographicalSortOrder for this AttributeGroup?
			//	if (attributeGroup.InvolveInFilterIndexing(ProductType))
			//	{
			//		// Yes, we can include this attribute in the URL
			//		var originalUrl = HttpContext.Current.Request.Url.ToString();
			//		var placeholder = $"{(char)254}";
			//		var url = HttpContext.Current.Request.Url.ToString().Replace(OriginalURLKeywords, placeholder);

			//		var matches = UrlParsingRegex.Matches(url);

			//		if (matches.Count == 1)
			//		{
			//			if (matches[0].Groups.Count >= 3)
			//			{
			//				var split = new List<AttributeSorter>();

			//				var urlKeywordsAttributesPowerSet = EnumerableExtensions.PowerSet(matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter).AsEnumerable<string>());

			//				foreach (var set in urlKeywordsAttributesPowerSet)
			//				{
			//					var potentialAttribute = String.Join("-", set);

			//					//if (AttributeGroups.SelectMany(a => a.Attributes).Where(a => a.FormValue == potentialAttribute).Any())
			//					//    split.Add(new AttributeSorter(potentialAttribute, null, null, null));

			//					var attr = AttributeGroups.SelectMany(a => a.Attributes).SingleOrDefault(a => a.FormValue == potentialAttribute);
			//					if (attr != null)
			//						split.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, null, null));
			//				}

			//				if (!split.Select(s => s.Attribute).Contains(attribute.FormValue))
			//				{
			//					split.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, null, null));
			//				}

			//				// split now contains e.g. new, cushions, blue, patterned
			//				// we want to rebuild it in the correct sort order
			//				// so we need to pull the attributeGroups having these attributes...

			//				var attributeGroups = AttributeGroups.Where(ag => ag.InvolveInFilterIndexing(ProductType)).ToList();

			//				var toRemove = new List<AttributeSorter>();
			//				var toAdd = new List<AttributeSorter>();

			//				foreach (var s in split)
			//				{
			//					var thisAttributeGroup = attributeGroups
			//						.Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
			//						.SingleOrDefault();

			//					// find the lexSortOrder for this split
			//					var lexSortOrder = attributeGroups
			//						.Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
			//						?.Select(ag => ag.ProductTypes.Single(pt => pt.FormValue == ProductType).LexicographicalSortOrder)
			//						?.SingleOrDefault();

			//					if (lexSortOrder != null)
			//					{
			//						s.LexicographicalSortOrder = lexSortOrder;
			//						s.LogicalOperator = thisAttributeGroup.LogicalOperator;

			//						// we want to remove other attributes already in split belonging to this attribute group, leaving just the incoming attribute representing this group
			//						if (attribute.AttributeGroupID == thisAttributeGroup.AttributeGroupID)
			//						{
			//							toRemove.Add(s);
			//							if (toAdd.SingleOrDefault(a =>
			//								a.Attribute == attribute.FormValue &&
			//								a.LexicographicalSortOrder == lexSortOrder &&
			//								a.LogicalOperator == thisAttributeGroup.LogicalOperator)
			//								== null)
			//							{
			//								toAdd.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, lexSortOrder, thisAttributeGroup.LogicalOperator));
			//							}
			//						}
			//					}
			//				}

			//				foreach (var s in toRemove)
			//				{
			//					split.Remove(s);
			//				}

			//				foreach (var s in toAdd)
			//				{
			//					split.Add(s);
			//				}

			//				split = split.OrderBy(s => s.LexicographicalSortOrder).ToList();

			//				if (attributeCombinations
			//					.SingleOrDefault(ac =>
			//						ac.Enabled == true &&
			//						ac.AttributeSignature == (
			//							String.IsNullOrEmpty(currentAttributeSignature)
			//								? $"{attribute.AttributeID}"
			//								: String.Join(".", split.Where(s => s.AttributeGroupID != 0).Select(s => s.AttributeID)))) == null)
			//				{
			//					return "#";
			//				}
			//				else
			//				{
			//					split.Add(new AttributeSorter(0, 0, OriginalSemanticString, null, null, null));

			//					url = UrlParsingRegex.Replace(url, $"$1/{String.Join("-", split.Select(s => s.Attribute))}/$4");

			//					// we don't want Google to be indexing the query string parameters
			//					if (url.IndexOf("?") > -1)
			//					{
			//						url = url.Substring(0, url.IndexOf("?"));
			//					}

			//					return url;
			//				}
			//			}
			//		}
			//	}

			//	return "#";
			//}

			//return "#";
		}
	}
}