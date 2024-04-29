using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Reflection;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;


namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.Stockist_Contract_Select")]
        [ResultTypeAttribute(typeof(stockistsContractDetail))]
        public IMultipleResults StockistsContractSearch_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, date, languageID);
            return (IMultipleResults)result.ReturnValue;

        }


    }
}