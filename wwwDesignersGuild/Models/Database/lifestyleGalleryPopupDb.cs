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

        [Function(Name = "pub.ProductPage_LifestyleGallery_Generate")]
        [ResultTypeAttribute(typeof(productPageHeader))]
        [ResultTypeAttribute(typeof(searchResultRenderImage))]
        [ResultTypeAttribute(typeof(productPageAlternateColourway))]
        [ResultTypeAttribute(typeof(lifestyleBuyOnline))]
        [ResultTypeAttribute(typeof(productPageProduct))]
        public IMultipleResults LifestyleGalleryPopup_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "ProductPageID", DbType = "Int")] System.Nullable<int> productPageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade,
                                                                        [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), productPageID, languageID, countryID, isTrade, date);
            return (IMultipleResults)result.ReturnValue;
        }
    }
}