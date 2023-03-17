using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ProductPage;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {

        [Function(Name = "pub.Attribute_Generate")]
        [ResultTypeAttribute(typeof(AttributeGroup))]
        [ResultTypeAttribute(typeof(AttributeGroup_ProductType))]
        [ResultTypeAttribute(typeof(AttributeDetail))]
        [ResultTypeAttribute(typeof(ProductType))]
        public IMultipleResults Attribute_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "LanguageID", DbType = "TinyInt")] System.Nullable<byte> languageID)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), languageID);
            return (IMultipleResults)result.ReturnValue;

        }


    }
}