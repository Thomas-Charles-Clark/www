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

        [Function(Name = "pub.CountryDropDown_SelectList")]
        [ResultTypeAttribute(typeof(countryDropDownItem))]
        public IMultipleResults CountryDropDown_SelectList(
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] DateTime contentDate,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] byte languageId)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), contentDate, languageId);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.SiteControls_Select")]
        [ResultTypeAttribute(typeof(bool))]
        [ResultTypeAttribute(typeof(countryItem))]
        public IMultipleResults checkCountry(
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] short countryId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] byte languageId,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "UserCountryCode", DbType = "NVarChar")] string userCountryCode)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), countryId, languageId, isTrade, userCountryCode);
            return (IMultipleResults)result.ReturnValue;
        }
    }
} 