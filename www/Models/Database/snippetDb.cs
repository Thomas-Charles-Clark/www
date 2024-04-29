using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.Snippet_Select")]
        [ResultType(typeof(Classes.Snippet))]
        public IMultipleResults Snippet_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<int> tradeAccountId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "DisplayTradePrices", DbType = "Bit")] bool displayTradePrices,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID, countryID, tradeAccountId, displayTradePrices, date);
            return (IMultipleResults)result.ReturnValue;

        }


    }
}