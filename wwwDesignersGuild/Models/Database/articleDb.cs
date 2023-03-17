using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        [Function(Name = "pub.Article_Generate")]
        [ResultTypeAttribute(typeof(articleSelect))]
        [ResultTypeAttribute(typeof(articleImageSelect))]
        [ResultTypeAttribute(typeof(articleImageHotspotSelect))]
        [ResultTypeAttribute(typeof(articleRelatedArticleSelect))]
        [ResultTypeAttribute(typeof(articleVideoSelect))]
        [ResultTypeAttribute(typeof(googleHref))]
        [ResultTypeAttribute(typeof(articleNextArticleSelect))]
        [ResultTypeAttribute(typeof(articlePreviousArticleSelect))]
        public IMultipleResults Article_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ArticleID", DbType = "Int")] System.Nullable<int> articleID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), articleID, languageID, countryID, date);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}