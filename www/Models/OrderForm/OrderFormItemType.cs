using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Models.OrderForm
{
    public enum OrderFormItemType
    {
        [BasketLineType(1)]
        Sample,
        [BasketLineType(1)]
        LargeSample,
        [BasketLineType(1)]
        Order,
        [BasketLineType(2)]
        Reserve,
        [BasketLineType(3)]
        ReserveWithBatchCutting
    }
}