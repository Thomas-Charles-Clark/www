using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using System.Web;
using wwwDesignersGuild.Models.GiftVoucher;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        [Function(Name = "pub.GiftVoucher_Select")]
        [ResultTypeAttribute(typeof(GiftVoucher_SelectResult))]
        public ISingleResult<GiftVoucher_SelectResult> GiftVoucher_Select([global::System.Data.Linq.Mapping.ParameterAttribute(Name = "VisitorID", DbType = "BigInt")] System.Nullable<Int64> visitorDbId,
                                                            [global::System.Data.Linq.Mapping.ParameterAttribute(Name = "GiftVoucherCode", DbType = "NVarChar")] string giftVoucherCode)
        {
            IExecuteResult result = this.ExecuteMethodCall(this, (MethodInfo)MethodInfo.GetCurrentMethod(), visitorDbId, giftVoucherCode);
            return (ISingleResult<GiftVoucher_SelectResult>)result.ReturnValue;
        }
    }
}