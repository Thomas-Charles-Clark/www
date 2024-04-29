using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Interface;


namespace wwwDesignersGuild.Models.StockistsContract
{
    public class StockistsContract : IStockistsContract
    {


        #region Stockist Contract Search


        public async Task<stockistsContractSearchSelect> stockistContractSearch(short countryId, DateTime useDate, byte languageId)
        {
            stockistsContractSearchSelect stockistsContractSearchStore = await Task.FromResult<stockistsContractSearchSelect>(stockistContractSearchAsync(countryId, useDate, languageId));
            return stockistsContractSearchStore;
        }


        private stockistsContractSearchSelect stockistContractSearchAsync(short countryId, DateTime useDate, byte languageId)
        {


            var dataContext = new databaseDataContext();


            IMultipleResults results = dataContext.StockistsContractSearch_GenerateMultiple(countryId, useDate, languageId);


            //  Convert from IEnumerable to a List so a query can be run multiple times
            stockistsContractSearchSelect stockistsContractSearchStore = new stockistsContractSearchSelect
            {
                stockistsContractDetails = results.GetResult<stockistsContractDetail>().ToList<stockistsContractDetail>()
            };


            return stockistsContractSearchStore;


        }


        #endregion


    }
}