using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;


namespace wwwDesignersGuild.Models.Interface
{
    public interface IStockistsContract
    {
        Task<stockistsContractSearchSelect> stockistContractSearch(short countryId, DateTime useDate, byte languageId);
    }
}
