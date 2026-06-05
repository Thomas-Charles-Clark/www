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

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ProductPageGridPageSection : PagerViewModel
	{
		public bool Searchable { get; set; }
		public bool IsLifestyleGallery { get; set; }
		public PageSectionJSON Json { get; set; }
		public bool FilterIndexingEnabled { get; set; }

		public Regex UrlParsingRegex = new Regex(@"(.*)\/([^\s\/]+)(\/)([a-z][\d]+)(\??)(.*)$");
		public string OriginalSemanticString { get; set; }
		public string OriginalTitle { get; set; }
		public string DeselectedAttributeNames { get; set; }
		public string OriginalURLKeywords { get; set; }
		public bool MustOverwritePageCanonical { get; set; }
		public string CurrentAttributeSignature { get; set; }
		public IEnumerable<FilterIndexAttributeCombination> AttributeCombinations { get; set; }

		// This for use by search - but why? Figure that out after fixing the standard usage.
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

			Ajax = true;

			AjaxEndpoint = "/api/ProductPageAPI/FetchProductPageGridPortion";
		}

		//public List<int> FilteredProductPageIDs { get; set; }

		// What is the intention behind this function? It has no parameters: is it supposed to return ALL contents?
		// No; it checks internal properties and returns potentially subsets which are potentially sorted
		public new List<ProductPageFilterablePod> GetContents()
		{
			if (FilterCount > 0 || ProductType != "")
			{
				if (ProductType == "")
				{
					var r = Contents.Cast<ProductPageFilterablePod>()
						.Where(ir => (!FilteredIDs.Any() || FilteredIDs.Contains(ir.ProductPageID)))
						.ToList();

					r.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

					return r;
				}
				else
				{
					var matches = Contents.Cast<ProductPageFilterablePod>()
						.Where(ir => ir.ProductType.FormValue == ProductType)
						.ToList();

					if (FilteredIDs.Any())
					{
						matches = matches.Where(ptm => FilteredIDs.Contains(ptm.ProductPageID)).ToList();
					}

					//SortContents(ref matches);

					matches.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

					return matches;
				}
			}
			else
			{
				var r = Contents.Cast<ProductPageFilterablePod>().ToList();

				//SortContents(ref r);

				r.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

				return r;
			}
		}

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

				return results.Skip(skip.Value).Take(take.Value).Select(p => new ProductPagePodViewModel(model, p, IsLifestyleGallery, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Three));
			}
		}

		public ProductPagePodViewModel GetProductPage(int productPageId, NewMasterDefinition model)
		{
			var results = Contents.Cast<ProductPageFilterablePod>().ToList();

			results.ForEach(c => c.IsLifestyle = IsLifestyleGallery);

			return results.Where(p => p.ProductPageID == productPageId)
						  .Select(p => new ProductPagePodViewModel(model, p, IsShop, dgFramework.IsMobileDevice() ? ImageSize.Six : ImageSize.Three))
						  .Single();
		}

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
			var result = new ProductPageGridPageSection(
				originalUrlKeywords,
				productPage,
				pagingEnabled,
				filterIndexingEnabled,
				isShop,
				isLifestyleGallery,
				model.MasterDefinition);

			var objDb = new databaseDataContext();

			var objAttribute = new cmsAttribute();

			var objAttributeLoad = objAttribute.loadAttributeFromCms(model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID);

			// Thanks https://stackoverflow.com/questions/32119507/multiple-awaits-vs-task-waitall-equivalent#answer-32119616

			await Task.WhenAll(objAttributeLoad); //(, ...);

			var attributeData = await objAttributeLoad;

			var preResultSet = new List<ProductPageFilterablePodWrapper>();

			// problem is that at this point, result.Json.JSON contains pageNumber * pageCount number of entries.
			// is it the procedure obeying the paging which is causing this? We need the procedure to NOT obey
			// the paging, and the full result set (8000 products, for instance) to sit in a MemoryCache object.
			var jsonString = result.Json.JSON.Replace("{\"productPages\":", "").TrimEnd(new char[] { '}' });

			using (var stream = Framework.GenerateStreamFromString(jsonString, System.Text.Encoding.UTF8))
			{
				using (var streamReader = new StreamReader(stream))
				{
					using (JsonReader jsonReader = new JsonTextReader(streamReader))
					{
						//jsonReader.CloseInput = false;
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

			var completeResultSet = preResultSet.Select(p => p.ProductPage).ToList();

			//var completeResultSet = JObject.FromObject(result.Json.json)
			//                                .SelectToken("productPages")
			//                                .Children()
			//                                .Select(p => JsonConvert.DeserializeObject<ProductPageFilterablePod>(p.SelectToken("productPage").ToString(), 
			//                                                new JsonSerializerSettings() { ContractResolver = new JsonIgnoreAttributeIgnorerContractResolver() }))
			//                                .ToList();

			completeResultSet.ForEach(c => c.IsLifestyle = isLifestyleGallery);

			var productPageIds = completeResultSet.Select(p => p.ProductPageID).ToList();

			var attributeIds = result.Json.attributes
										  .Where(p => productPageIds.Contains(p.ProductPageID))
										  .GroupBy(p => p.AttributeID)
										  .Select(p => p.Key).ToList();

			completeResultSet.ForEach(pp =>
			{
				var ids = result.Json.attributes.Where(p => p.ProductPageID == pp.ProductPageID)
														.Select(p => p.AttributeID).ToList();

				pp.AttributeFormValues = attributeData.attributeDetails.Where(p => ids.Contains(p.AttributeID))
																	   .Select(p => p.FormValue)
																	   .ToList();

				pp.ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == pp.ProductTypeID);
			});

			var gridProductTypeIds = completeResultSet.Select(p => p.ProductTypeID).Distinct();

			result.ProductTypes = attributeData.productTypes.Where(pt => gridProductTypeIds.Contains(pt.ProductTypeID)).ToList();

			result.ProductTypes.ForEach(pt =>
			{
				pt.ProductPageIDs = completeResultSet.Where(crs => crs.ProductTypeID == pt.ProductTypeID).Select(crs => crs.ProductPageID);
			});

			if (result.ProductTypes.Count > 1)
			{
				result.ProductTypes.Insert(0, new ProductType
				{
					ProductTypeID = 0,
					Name = HtmlHelperExtra.Resource("All", model.LanguageID, model.CountryID, model.Date).ToString(),
					SortOrder = 0,
					ProductPageIDs = completeResultSet.Select(p => p.ProductPageID)
				});
			}
			else
			{
				if (result.ProductTypes.Count() == 1)
				{
					result.ProductType = result.ProductTypes[0].FormValue;

					// Filter index restrictions fn
					var objFilterIndexCombinations = await new cmsFilterIndexAttributeCombination().loadFilterIndexingAttributeCombinationsFromCmsAsync(
							new PassFilterIndexAttributeCombinationCacheFetch(model.LanguageID, result.ProductTypes[0].ProductTypeID, model.Date, model.CountryURL),
							model
						);

					result.AttributeCombinations = objFilterIndexCombinations.Combinations;
				}
			}

			var attributes = attributeData.attributeDetails
										  .Where(p => attributeIds.Contains(p.AttributeID))
										  .OrderBy(p => p.SortOrder)
										  .ToList();

			attributes.ForEach(a =>
			{
				a.ProductPageIDs = result.Json
										 .attributes
										 .Where(p => p.AttributeID == a.AttributeID)
										 .Select(p => p.ProductPageID)
										 .ToList();
			});

			var attributeGroupIds = attributes.Select(p => p.AttributeGroupID).Distinct();

			result.AttributeGroupGroups = attributeData.attributeGroupGroups;

			result.AttributeGroups = attributeData.attributeGroups
											   .Where(p => attributeGroupIds.Contains(p.AttributeGroupID))
											   .ToList();

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

			if (!model.MasterDefinition.TradeAllowed) /* If you're not trade and trade-allowed */
			{
				// and there's a Clearance attribute group delivered
				result.AttributeGroups.RemoveAll(p => p.AttributeGroupID == 0);
			}

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

			#region QueryString based attribute determination dance

			// produce the power set of all words in the malleable url segment
			// it's possible that some are attributes
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

			var overallSelection = result.AttributeGroups.Where(ag => ag.Selection.Any())
				.SelectMany(ag => ag.Selection).Distinct().ToList();

			result.InitialisePager();

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

			#region Perform Paging

			#region Build a list of product page ids to fire the render and pricing fetches for

			var productPageIdsToRender = new List<int>();

			#region Initialise Pager

			// The call to InitialisePager is necessary to cause the PagerViewModel to carry appropriate values for things like page, pageCount, pageSize.
			result.InitialisePager();

			#endregion

			var page = Convert.ToInt32(HttpContext.Current.Request.QueryString["page"] ?? "0");

			var pageSize = Convert.ToInt32(HttpContext.Current.Request.QueryString["pagesize"] ?? "200");

			var sort = HttpContext.Current.Request.QueryString["sort"] ?? "default";

			var pagingResults = Framework.StreamSegmentOfJsonArray(result.Json.JSON, "productPages", page * pageSize, pageSize, sort).ToList();

			productPageIdsToRender = pagingResults.Select(p => p.ProductPageID).ToList();// result.FilteredIDs.Skip((result.Page - 1) * result.PageSize).Take(result.PageSize).ToList();

			#endregion

			int? tradeAccountId = null;

			if (HttpContext.Current.Session["memberTradeAccountID"] != null)
			{
				tradeAccountId = Convert.ToInt32(HttpContext.Current.Session["memberTradeAccountID"].ToString());
			}

			// Datatable to pass to SQL proc
			var productPageToRenderTable = new DataTable();

			// Datatable columns... why is Sequence necessary?
			productPageToRenderTable.Columns.Add("ProductPageID", typeof(int));
			productPageToRenderTable.Columns.Add("Sequence", typeof(int));

			int sequence = 0;

			// add rows to datatable
			foreach (var productPageId in productPageIdsToRender)
			{
#if DEBUG
				//System.Diagnostics.Debug.WriteLine(productPage);
#endif
				productPageToRenderTable.Rows.Add(productPageId, sequence++);
			}

			// If we have any product page ids for the current page...
			if (productPageIdsToRender.Any())
			{
				#region Render the Current Page


				var objSearchGenerate = new searchGenerate();
				var objSearch = new cmsSearch();
				var objSearchResultReturn = await objSearch.loadSearchFromCms(
					productPageToRenderTable,
					model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
					model.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
					searchTerm: null,
					tradeAccountId,
					null,
					model.Date);

				Parallel.ForEach(objSearchResultReturn.searchResultCores, tempResult =>
				{
					var tempSearch = pagingResults.Where(p => p.ProductPageID == tempResult.ProductPageID).FirstOrDefault();

					if (tempSearch == null) { }
					else
					{
						tempResult.ScorePercent = tempSearch.ProductPage.ScorePercent;
					}
				});

				objSearchResultReturn.searchResultCores = objSearchResultReturn.searchResultCores.OrderByDescending(p => p.ScorePercent).ThenByDescending(p => p.SearchSortOrder).ThenBy(p => p.ProductPageID).ToList();

				Parallel.ForEach(pagingResults, (r) =>
				{
					var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);

					if (src != null)
					{
						r.ProductPage.ApplySorting(src);
					}
				});

				Parallel.ForEach(completeResultSet, (r) =>
				{
					var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);

					if (src != null)
					{
						r.ApplySorting(src);
					}
				});

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

				// convert SQL render proc JSON results to List<T>
				var renderedResults = JObject.FromObject(productPages.json)
											.SelectToken("productPages")
											.ToObject<List<ProductPageFilterablePod>>();

				#endregion

				#region Fetch Pricing for the Current Page

				// call SQL pricing select for page
				var objPrices = objDb.ProductPage_Price_Select(
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
						completeResultSet[index].ProductType = attributeData.productTypes.Single(pt => pt.ProductTypeID == completeResultSet[index].ProductTypeID);
					}
				}

				#endregion

				#region We've modified the complete result set, overwrite it on the product page grid. Also set TakeCount, to avoid Razor rendering upto PageSize if only n have been fetched for render.

				result.Contents = completeResultSet.Cast<object>().ToList();

				#endregion
			}
			else
			{
				var objSearchGenerate = new searchGenerate();
				var objSearch = new cmsSearch();
				var objSearchResultReturn = await objSearch.loadSearchFromCms(
					productPageToRenderTable,
					model.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
					model.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
					searchTerm: null,
					tradeAccountId,
					null,
					model.Date);

				Parallel.ForEach(completeResultSet, (r) =>
				{
					var src = objSearchResultReturn.searchResultCores.FirstOrDefault(p => p.ProductPageID == r.ProductPageID);

					if (src != null)
					{
						r.ApplySorting(src);
					}
				});

				result.SortContents(completeResultSet);

				result.Contents = completeResultSet.Cast<object>()
												   .ToList();
			}
			#endregion

			if (result.Contents.Count() < result.PageSize)
				result.TakeCount = result.Contents.Count();
			else
				result.TakeCount = result.PageSize;

			result.InitialisePager();

			result.SortContents(completeResultSet);

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
			//if (attribute.FormValue == "blue")
			//{
			//    int f = 0;
			//}

			// if FilterIndexing is enabled on this grid's containing page
			if (FilterIndexingEnabled)
			{
				// does this ProductType describe a LexicographicalSortOrder for this AttributeGroup?
				if (attributeGroup.InvolveInFilterIndexing(ProductType))
				{
					// Yes, we can include this attribute in the URL
					var originalUrl = HttpContext.Current.Request.Url.ToString();
					var placeholder = $"{(char)254}";
					var url = HttpContext.Current.Request.Url.ToString().Replace(OriginalURLKeywords, placeholder);

					var matches = UrlParsingRegex.Matches(url);

					if (matches.Count == 1)
					{
						if (matches[0].Groups.Count >= 3)
						{
							var split = new List<AttributeSorter>();

							var urlKeywordsAttributesPowerSet = EnumerableExtensions.PowerSet(matches[0].Groups[2].Value.Split(Framework.HyphenDelimiter).AsEnumerable<string>());

							foreach (var set in urlKeywordsAttributesPowerSet)
							{
								var potentialAttribute = String.Join("-", set);

								//if (AttributeGroups.SelectMany(a => a.Attributes).Where(a => a.FormValue == potentialAttribute).Any())
								//    split.Add(new AttributeSorter(potentialAttribute, null, null, null));

								var attr = AttributeGroups.SelectMany(a => a.Attributes).SingleOrDefault(a => a.FormValue == potentialAttribute);
								if (attr != null)
									split.Add(new AttributeSorter(attr.AttributeID, attr.AttributeGroupID, attr.FormValue, attr.Name, null, null));
							}

							if (!split.Select(s => s.Attribute).Contains(attribute.FormValue))
							{
								split.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, null, null));
							}

							// split now contains e.g. new, cushions, blue, patterned
							// we want to rebuild it in the correct sort order
							// so we need to pull the attributeGroups having these attributes...

							var attributeGroups = AttributeGroups.Where(ag => ag.InvolveInFilterIndexing(ProductType)).ToList();

							var toRemove = new List<AttributeSorter>();
							var toAdd = new List<AttributeSorter>();

							foreach (var s in split)
							{
								var thisAttributeGroup = attributeGroups
									.Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
									.SingleOrDefault();

								// find the lexSortOrder for this split
								var lexSortOrder = attributeGroups
									.Where(ag => ag.Attributes.SingleOrDefault(a => a.FormValue == s.Attribute) != null)
									?.Select(ag => ag.ProductTypes.Single(pt => pt.FormValue == ProductType).LexicographicalSortOrder)
									?.SingleOrDefault();

								if (lexSortOrder != null)
								{
									s.LexicographicalSortOrder = lexSortOrder;
									s.LogicalOperator = thisAttributeGroup.LogicalOperator;

									// we want to remove other attributes already in split belonging to this attribute group, leaving just the incoming attribute representing this group
									if (attribute.AttributeGroupID == thisAttributeGroup.AttributeGroupID)
									{
										toRemove.Add(s);
										if (toAdd.SingleOrDefault(a =>
											a.Attribute == attribute.FormValue &&
											a.LexicographicalSortOrder == lexSortOrder &&
											a.LogicalOperator == thisAttributeGroup.LogicalOperator)
											== null)
										{
											toAdd.Add(new AttributeSorter(attribute.AttributeID, attribute.AttributeGroupID, attribute.FormValue, attribute.Name, lexSortOrder, thisAttributeGroup.LogicalOperator));
										}
									}
								}
							}

							foreach (var s in toRemove)
							{
								split.Remove(s);
							}

							foreach (var s in toAdd)
							{
								split.Add(s);
							}

							split = split.OrderBy(s => s.LexicographicalSortOrder).ToList();

							if (attributeCombinations
								.SingleOrDefault(ac =>
									ac.Enabled == true &&
									ac.AttributeSignature == (
										String.IsNullOrEmpty(currentAttributeSignature)
											? $"{attribute.AttributeID}"
											: String.Join(".", split.Where(s => s.AttributeGroupID != 0).Select(s => s.AttributeID)))) == null)
							{
								return "#";
							}
							else
							{
								split.Add(new AttributeSorter(0, 0, OriginalSemanticString, null, null, null));

								url = UrlParsingRegex.Replace(url, $"$1/{String.Join("-", split.Select(s => s.Attribute))}/$4");

								// we don't want Google to be indexing the query string parameters
								if (url.IndexOf("?") > -1)
								{
									url = url.Substring(0, url.IndexOf("?"));
								}

								return url;
							}
						}
					}
				}

				return "#";
			}

			return "#";
		}
	}
}