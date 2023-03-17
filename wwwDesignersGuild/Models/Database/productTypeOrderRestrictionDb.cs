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
        [Function(Name = "pub.ProductType_OrderRestriction_Select")]
        [ResultTypeAttribute(typeof(productTypeOrderRestrictionSelect))]
        public IMultipleResults ProductType_OrderRestriction_Select(
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "TradeAccountID", DbType = "Int")] System.Nullable<Int32> tradeAccountId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), tradeAccountId, languageId);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}