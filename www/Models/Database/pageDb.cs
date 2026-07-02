using System;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Redirect;
using wwwDesignersGuild.Models.Redirects;

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
        [ResultTypeAttribute(typeof(pageLanguageSelect))]
        [ResultTypeAttribute(typeof(pageSectionSelect))]
        [ResultTypeAttribute(typeof(pageSectionHtml))]
        [ResultTypeAttribute(typeof(pageSectionSlideShowAspect))]
        [ResultTypeAttribute(typeof(pageSectionSlideShow))]
        [ResultTypeAttribute(typeof(pageSectionForm))]
        [ResultTypeAttribute(typeof(pageSectionBrochure))]
        [ResultTypeAttribute(typeof(pageSectionProductPage))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        [ResultTypeAttribute(typeof(pageSectionAttribute))]
        [ResultTypeAttribute(typeof(pageSectionPrice))]
        [ResultTypeAttribute(typeof(pageSectionCollection))]
        [ResultTypeAttribute(typeof(pageSectionCollectionImage))]
        [ResultTypeAttribute(typeof(pageSectionPageSelect))]
        [ResultTypeAttribute(typeof(pageSectionPageSelectImage))]
        [ResultTypeAttribute(typeof(pageSectionBannerJson))]
        [ResultTypeAttribute(typeof(pageRelated))]
        [ResultTypeAttribute(typeof(pageStockStatus))]
        [ResultTypeAttribute(typeof(googleHref))]
        [ResultTypeAttribute(typeof(pageSectionArticle))]
        [ResultTypeAttribute(typeof(pageSectionArticleImage))]
        [ResultTypeAttribute(typeof(pageFormCountry))]
        [ResultTypeAttribute(typeof(pageSectionVideo))]
        [ResultTypeAttribute(typeof(pageSectionAnimation))]
        [ResultTypeAttribute(typeof(pageSectionAnimationSlide))]
        [ResultTypeAttribute(typeof(pageSectionAnimationSlideCallToAction))]
        [ResultTypeAttribute(typeof(pageRatingAggregate))]
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
        [ResultTypeAttribute(typeof(googleHref))]
        public ISingleResult<googleHref> Page_Hreflang_GenerateMultiple(
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PageID", DbType = "Int")] int? pageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime? date,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "AttributeList", DbType = "NVarChar(500)")] string attributeList)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), pageID, date, attributeList);
            return (ISingleResult<googleHref>)result.ReturnValue;
        }
    }
}