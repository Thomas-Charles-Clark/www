using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;


namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.Collection_Generate")]
        [ResultTypeAttribute(typeof(collectionSelect))]
        [ResultTypeAttribute(typeof(collectionImageSelect))]
        [ResultTypeAttribute(typeof(collectionImageHotspotSelect))]
        [ResultTypeAttribute(typeof(collectionRelatedCollectionSelect))]
        [ResultTypeAttribute(typeof(collectionBrochureSelect))]
        [ResultTypeAttribute(typeof(collectionVideo))]
        [ResultTypeAttribute(typeof(ProductPagePod))]
        [ResultTypeAttribute(typeof(GoogleHref))]
        [ResultTypeAttribute(typeof(collectionBreadcrumb))]
        public IMultipleResults Collection_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CollectionID", DbType = "SmallInt")] System.Nullable<short> collectionID, 
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID, 
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "IsTrade", DbType = "Bit")] bool isTrade)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), collectionID, languageID, countryID, date, isTrade);
            return (IMultipleResults) result.ReturnValue;

        }


    }
}