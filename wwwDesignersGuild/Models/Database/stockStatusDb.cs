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
        [Function(Name = "pub.StockStatus_Select")]
        [ResultTypeAttribute(typeof(StockStatusSelect))]
        public IMultipleResults StockStatus_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}