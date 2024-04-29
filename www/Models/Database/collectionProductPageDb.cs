using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.Collection_ProductPage_Generate")]
        [ResultTypeAttribute(typeof(collectionAll))]
        [ResultTypeAttribute(typeof(collectionHeader))]
        [ResultTypeAttribute(typeof(collectionProduct))]
        [ResultTypeAttribute(typeof(collectionProductType))]
        [ResultTypeAttribute(typeof(collectionBrand))]
        [ResultTypeAttribute(typeof(pageSectionBanner))]
        public IMultipleResults CollectionProduct_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CollectionID", DbType = "SmallInt")] System.Nullable<short> collectionID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "BrandID ", DbType = "TinyInt")] System.Nullable<byte> brandID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductTypeID", DbType = "TinyInt")] System.Nullable<byte> productTypeID,
                                                                    [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Sort", DbType = "NVarChar(10)")] string sort)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), collectionID, languageID, countryID, date, brandID, productTypeID, sort);
            return (IMultipleResults)result.ReturnValue;

        }


    }
}