using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Stockists;

namespace wwwDesignersGuild.Models.Interface
{
    public interface IStockist
    {
        Task<StockistCountrySelect> getStockistCountries(DateTime useDate);
        Task<StockistSearchSelect> stockistSearch(long visitorDbId, short countryId, DateTime useDate, PassStockistSearch objStockistSearch);
        Task<StockistDetail> stockistDetail(int stockistId, DateTime useDate);
    }
}
