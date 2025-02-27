using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Interface;
using wwwDesignersGuild.Models.Helper;
using NLog;
using wwwDesignersGuild.Models.Cache.Collection;
using wwwDesignersGuild.Models.GiftVoucher;
using DocumentFormat.OpenXml.Wordprocessing;
using U2.Data.ClientTypes;

namespace wwwDesignersGuild.Models.GiftVoucher
{


    public class cmsGiftVoucher
    {
        private static Logger Logger = LogManager.GetCurrentClassLogger();
        
        public async Task<GiftVoucher_SelectResult> FetchGiftVoucher(long? visitorDbId, string giftVoucherCode)
        {
            var dataContext = new databaseDataContext();
           return dataContext.GiftVoucher_Select(visitorDbId, giftVoucherCode).Single();
        }
    }
}