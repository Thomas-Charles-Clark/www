using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Returns
{
    public class Return
    {
        public OrderHistoryHeader OrderHead { get; set; }
        public List<OrderHistoryLine> OrderLines { get; set; }
        public List<OrderHistoryReturnReason> ReturnReasons { get; set; }
    }
}