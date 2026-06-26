using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Redirects;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        [Function(Name = "pub.Redirect_Select")]
        [ResultTypeAttribute(typeof(RedirectSelectResult))]
        public ISingleResult<RedirectSelectResult> Redirect_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "OldURL", DbType = "NVarChar(255)")] string oldUrl = null)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), oldUrl);
            return (ISingleResult<RedirectSelectResult>)result.ReturnValue;
        }

        [Function(Name = "pub.Page_Generate")]
        [ResultTypeAttribute(typeof(PageLanguageSelect))]
        [ResultTypeAttribute(typeof(PageSectionSelect))]
        [ResultTypeAttribute(typeof(PageSectionHTML))]
        [ResultTypeAttribute(typeof(PageSectionSlideShowAspect))]
        [ResultTypeAttribute(typeof(PageSectionSlideShow))]
        [ResultTypeAttribute(typeof(PageSectionForm))]
        [ResultTypeAttribute(typeof(PageSectionBrochure))]
        [ResultTypeAttribute(typeof(pageSectionProductPage))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        [ResultTypeAttribute(typeof(pageSectionAttribute))]
        [ResultTypeAttribute(typeof(pageSectionPrice))]
        [ResultTypeAttribute(typeof(PageSectionCollection))]
        [ResultTypeAttribute(typeof(PageSectionCollectionImage))]
        [ResultTypeAttribute(typeof(PageSectionPageSelect))]
        [ResultTypeAttribute(typeof(PageSectionPageSelectImage))]
        [ResultTypeAttribute(typeof(PageSectionBannerJSON))]
        [ResultTypeAttribute(typeof(PageRelated))]
        [ResultTypeAttribute(typeof(PageStockStatus))]
        [ResultTypeAttribute(typeof(GoogleHref))]
        [ResultTypeAttribute(typeof(PageSectionArticle))]
        [ResultTypeAttribute(typeof(PageSectionArticleImage))]
        [ResultTypeAttribute(typeof(PageFormCountry))]
        [ResultTypeAttribute(typeof(PageSectionVideo))]
        [ResultTypeAttribute(typeof(PageSectionAnimation))]
        [ResultTypeAttribute(typeof(PageSectionAnimationSlide))]
        [ResultTypeAttribute(typeof(PageSectionAnimationSlideCallToAction))]
        [ResultTypeAttribute(typeof(PageRatingAggregate))]
        public IMultipleResults Page_GenerateMultiple(
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PageID", DbType = "Int")] int? pageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] byte? languageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] short? countryID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime? date,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsMember", DbType = "Bit")] bool isMember,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "MemberID", DbType = "Int")] int? memberID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] int? tradeAccountId)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), pageID, languageID, countryID, date, isMember, isTrade, memberID, displayTradePrice, tradeAccountId);
            return (IMultipleResults)result.ReturnValue;

        }


        [Function(Name = "pub.Page_HrefLang_Select")]
        [ResultTypeAttribute(typeof(GoogleHref))]
        public ISingleResult<GoogleHref> Page_Hreflang_GenerateMultiple(
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PageID", DbType = "Int")] int? pageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime? date,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "AttributeList", DbType = "NVarChar(500)")] string attributeList)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), pageID, date, attributeList);
            return (ISingleResult<GoogleHref>)result.ReturnValue;
        }



		[Function(Name = "pub.ProductPageGrid_FilterIndexingEnabled")]
		[ResultTypeAttribute(typeof(ProductPageGrid_FilterIndexingEnabledResult))]
		public ISingleResult<ProductPageGrid_FilterIndexingEnabledResult> ProductPageGrid_FilterIndexingEnabled(
														[global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PageSectionID", DbType = "BigInt")] long pageSectionId)
		{
			IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), pageSectionId);
			return (ISingleResult<ProductPageGrid_FilterIndexingEnabledResult>)result.ReturnValue;
		}
	}
}