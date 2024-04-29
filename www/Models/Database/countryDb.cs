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

        [Function(Name = "pub.Country_Language_Select")]
        [ResultTypeAttribute(typeof(countryRegion))]
        [ResultTypeAttribute(typeof(countryItem))]
        [ResultTypeAttribute(typeof(countryTradePriceList))]
        public IMultipleResults Country_Language_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime contentDate)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), contentDate);
            return (IMultipleResults)result.ReturnValue;
        }


        [Function(Name = "pub.Country_SelectList")]
        [ResultTypeAttribute(typeof(Classes.Country))]
        [ResultTypeAttribute(typeof(CountyItem))]
        public IMultipleResults Country_SelectList([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                  [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryID, languageID);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.Language_SelectList")]
        [ResultTypeAttribute(typeof(Classes.Country))]
        [ResultTypeAttribute(typeof(CountyItem))]
        public IMultipleResults Language_SelectCompleteList([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime contentDate)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryId, contentDate);
            return (IMultipleResults)result.ReturnValue;

        }
    }
} 