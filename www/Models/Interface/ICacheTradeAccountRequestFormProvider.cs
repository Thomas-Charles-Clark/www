using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels.Trade;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheTradeAccountRequestFormProvider
    {
        TradeAccountRequestViewModel Get(short countryId, byte languageId, DateTime useDate);
        void Set(short countryId, byte languageId, DateTime useDate, TradeAccountRequestViewModel data);
        bool IsSet(short countryId, byte languageId, DateTime useDate);
        void Invalidate(short countryId, byte languageId, DateTime useDate);
    }
}