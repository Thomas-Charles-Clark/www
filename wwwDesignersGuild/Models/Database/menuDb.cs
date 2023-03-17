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

        [Function(Name = "pub.Menu_Generate")]
        [ResultTypeAttribute(typeof(menuGroup))]
        [ResultTypeAttribute(typeof(menuItem))]
        public IMultipleResults Menu_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID, countryID, date, isTrade);
            return (IMultipleResults)result.ReturnValue;

        }


    }
}