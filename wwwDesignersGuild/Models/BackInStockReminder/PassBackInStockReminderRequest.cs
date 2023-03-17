using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.BackInStockReminder
{
    public class PassBackInStockReminderRequest
    {
        public int productId { get; set; }
        public short countryId { get; set; }
        public byte languageId { get; set; }
    }
}