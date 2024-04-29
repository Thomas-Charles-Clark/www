using System;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Database
{

    public partial class databaseDataContext
    {
        [Function(Name = "pub.Product_Alternative_Select")]
        [ResultType(typeof(searchResultRenderCore))]
        [ResultType(typeof(searchResultRenderImage))]
        [ResultType(typeof(ProductPageAttribute))]
        [ResultType(typeof(searchResultRenderCore))]
        [ResultType(typeof(searchResultRenderImage))]
        [ResultType(typeof(ProductPageAttribute))]
        public IMultipleResults AlternativeProducts_GenerateMultiple([Parameter(Name = "Code", DbType = "NVarChar(50)")] string code,
                                                                     [Parameter(Name = "Date", DbType = "Date")] DateTime date,
                                                                     [Parameter(Name = "LanguageID", DbType = "TinyInt")] byte languageId,
                                                                     [Parameter(Name = "CountryID", DbType = "SmallInt")] short countryId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] int? tradeAccountId,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrice", DbType = "Bit")] bool displayTradePrice)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), code, date, languageId, countryId, isTrade, tradeAccountId, displayTradePrice);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}