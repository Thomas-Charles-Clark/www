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
        [Function(Name = "pub.Braintree_ErrorResponse_Select")]
        [ResultTypeAttribute(typeof(resourceSelect))]
        public IMultipleResults Braintree_ErrorResponse_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID, date);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}