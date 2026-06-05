using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.ArticleSubType;
using wwwDesignersGuild.Models.ArticleType;
using wwwDesignersGuild.Models.Aspect;

using wwwDesignersGuild.Models.Carousel;

using wwwDesignersGuild.Models.Classes;

using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ArticleGridPageSection : PagerViewModel
	{
		[JsonIgnore]
		public List<PageSectionArticle> Articles { get; set; }

		public byte GridArticleType { get; set; }

		public List<articleTypeSelect> ArticleTypes { get; set; }

		public List<articleSubTypeSelect> ArticleSubTypes { get; set; }

		private ArticleGridPageSection(
			long pageSectionId,
			bool pagingEnabled,
			bool isShop,
			List<PageSectionArticle> articles,
			List<PageSectionArticleImage> images,
			BaseViewModel baseViewModel) : base(pageSectionId,
												pagingEnabled,
												isShop,
												baseViewModel.MasterDefinition)
		{
			ArticleTypes = new List<articleTypeSelect>();

			ArticleSubTypes = new List<articleSubTypeSelect>();

			FilteredIDs = new List<int>();

			Articles = articles.OrderByDescending(art => art.CreatedDT)
							   .ThenBy(art => Regex.Replace(art.Heading, "'", ""))
							   .ToList();

			Articles.ForEach(art =>
			{
				art.Heading = art.Heading?.Trim();
				art.SubHeading = art.SubHeading?.Trim();

				art.CarouselViewModel = new CarouselViewModel
				(
					carouselId: $"#article-{art.ArticleID}-carousel",
					wrapperCssClass: "card-img-top",
					aspectRatio: AspectRatio.LandscapeFeature,
					art.Heading,
					images: images.Where(i => i.ArticleID == art.ArticleID)
								  .Select(i => new CarouselImage(i.ImageID, i.SortOrder, art.Heading, ImageSize.Four))
								  .OrderBy(i => i.SortOrder),
					baseViewModel: baseViewModel
				);
			});

			Contents = Articles.Cast<object>()
							   .ToList();
		}

		public static async Task<ArticleGridPageSection> CreateAsync(
			long pageSectionId,
			bool pagingEnabled,
			bool isShop,
			List<PageSectionArticle> articles,
			List<PageSectionArticleImage> images,
			BaseViewModel baseViewModel)
		{
			var result = new ArticleGridPageSection(pageSectionId,
				pagingEnabled,
				isShop,
				articles,
				images,
				baseViewModel);

			var articleType = new cmsArticleType();
			var articleSubType = new cmsArticleSubType();

			var objArticleTypes = await articleType.loadArticleTypeFromCms(
				baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
				baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
				baseViewModel.MasterDefinition.UrlBreakdown.ContentDate);

			result.ArticleTypes = objArticleTypes.articleTypeSelects
												 .OrderBy(at => at.SortOrder)
												 .ToList();

			// Aha. So, if there are articles of more than one major type in this page section, we do not want to fetch the
			// list of article sub-types to render the sub-type-menu.

			var articleTypes = articles.Select(p => p.ArticleTypeID).Distinct();

			var articleTypeCount = articleTypes.Count();

			// In theory there should never be 0 article types, so that's just a sanity check
			if (articleTypeCount == 0 || articleTypeCount > 1)
			{
				result.GridArticleType = 0;

				result.ArticleSubTypes = new List<articleSubTypeSelect>();
			}
			else
			{
				result.GridArticleType = articleTypes.First();

				var objArticleSubTypes = await articleSubType.loadArticleSubTypeFromCms(
					articleTypes.First(),
					baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
					baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
					baseViewModel.MasterDefinition.UrlBreakdown.ContentDate);

				result.ArticleSubTypes = objArticleSubTypes.articleSubTypeSelects
														   .OrderBy(ast => ast.SortOrder)
														   .ToList();
			}

			var articleCountResourceName = result.Articles.Count() == 1 ? "ArticleSingular" : "articleMenuHeading";

			result.DisplayCount = $@"{result.Articles.Count():N0} {(HtmlHelperExtra.Resource(articleCountResourceName, baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
			  baseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
			  baseViewModel.MasterDefinition.UrlBreakdown.ContentDate))}";

			result.FilteredIDs = result.Articles.Select(a => a.ArticleID).ToList();

			result.InitialisePager();

			return result;
		}

		public new List<PageSectionArticle> GetContents()
		{
			if (AttributeGroups.Any(p => p.InUse))
			{
				var results = Contents.Cast<PageSectionArticle>()
									  .Where(p => FilteredIDs.Contains(p.ArticleID))
									  .ToList();

				return results;
			}
			else
			{
				return Contents.Cast<PageSectionArticle>().ToList();
			}
		}

		public List<ArticlePodViewModel> GetArticles(NewMasterDefinition model)
		{
			switch (Sort)
			{
				case "alphaAZ":
					return GetContents().Select(a => new ArticlePodViewModel(model, a))
						.OrderBy(p => p.Article.Heading)
						.ThenBy(p => p.Article.SubHeading)
						.ToList();
				case "alphaZA":
					return GetContents().Select(a => new ArticlePodViewModel(model, a))
						.OrderByDescending(p => p.Article.Heading)
						.ThenBy(p => p.Article.SubHeading)
						.ToList();
				case "new":
					return GetContents().Select(a => new ArticlePodViewModel(model, a))
						.OrderBy(p => p.Article.CreatedDT)
						.ToList();
				default:
					return GetContents().Select(a => new ArticlePodViewModel(model, a))
						.OrderByDescending(p => p.Article.CreatedDT)
						.ToList();
			}
		}
	}

}