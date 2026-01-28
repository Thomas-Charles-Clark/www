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
using wwwDesignersGuild.Models.Stockists;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        [Function(Name = "pub.Stockists_CountryList_Select")]
        [ResultTypeAttribute(typeof(StockistCountry))]
        public IMultipleResults StockistCountryList_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), date);
            return (IMultipleResults)result.ReturnValue;

        }

        [Function(Name = "pub.Stockist_Generate")]
        [ResultTypeAttribute(typeof(FeaturedStockist))]
        [ResultTypeAttribute(typeof(StockistResult))]
        public IMultipleResults StockistSearch_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryID", DbType = "SmallInt")] System.Nullable<short> countryID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Latitude", DbType = "Float")] System.Nullable<float> latitude,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Longitude", DbType = "Float")] System.Nullable<float> longitude,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_DG", DbType = "bit")] System.Nullable<Boolean> brandDg,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_DGU", DbType = "bit")] System.Nullable<Boolean> brandDgu,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_WY", DbType = "bit")] System.Nullable<Boolean> brandWy,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_RC", DbType = "bit")] System.Nullable<Boolean> brandRc,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_CL", DbType = "bit")] System.Nullable<Boolean> brandCl,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_RL", DbType = "bit")] System.Nullable<Boolean> brandRl,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_JD", DbType = "bit")] System.Nullable<Boolean> brandJd,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Brand_EH", DbType = "bit")] System.Nullable<Boolean> brandEh,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Fabric", DbType = "bit")] System.Nullable<Boolean> productFabric,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Accessories", DbType = "bit")] System.Nullable<Boolean> productAccessories,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Rug", DbType = "bit")] System.Nullable<Boolean> productRug,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Furniture", DbType = "bit")] System.Nullable<Boolean> productFurniture,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Wallpaper", DbType = "bit")] System.Nullable<Boolean> productWallpaper,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_BedAndBath", DbType = "bit")] System.Nullable<Boolean> productBedAndBath,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Product_Paint", DbType = "bit")] System.Nullable<Boolean> productPaint,
                                                                //[global::System.Data.Linq.Mapping.ParameterAttribute(Name = "CountryCodeOverride", DbType = "nvarchar(2)")] String countryCodeOverride,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Mobile", DbType = "bit")] Boolean mobile,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "PremiumPartner", DbType = "bit")] Boolean premiumPartner,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "QR", DbType = "bit")] Boolean qr
                                                                )

        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(),
                visitorDbId,
                countryID,
                date,
                latitude,
                longitude,                                                                                     
                brandDg,
                brandDgu,
                brandWy,
                brandRc,
                brandCl,
                brandRl,
                brandJd,
                brandEh,
                productFabric,
                productAccessories,
                productRug,
                productFurniture,
                productWallpaper,
                productBedAndBath,
                productPaint,
                //countryCodeOverride,
                mobile,
                premiumPartner,
                qr);
            return (IMultipleResults)result.ReturnValue;
        }

        [Function(Name = "pub.Stockist_SelectDetails")]
        [ResultTypeAttribute(typeof(StockistDetail))]
        public ISingleResult<StockistDetail> StockistDetail_GenerateMultiple([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "StockistID", DbType = "Int")] System.Nullable<Int32> StockistID,
                                                                [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "Date", DbType = "Date")] System.Nullable<System.DateTime> date)
        {

            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), StockistID, date);
            return (ISingleResult<StockistDetail>)result.ReturnValue;
        }
    }
}