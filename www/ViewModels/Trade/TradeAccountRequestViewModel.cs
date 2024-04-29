using System.Collections.Generic;
using System.Threading.Tasks;

using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeAccountRequestViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }

        public string heading { get; set; }

        public string subHeading { get; set; }

        public string description { get; set; }

        public long? leadId { get; set; }

        public bool immediateOrderRequested { get; set; }

        public List<Country> availableCountries { get; set; }

        public List<CountyItem> availableCounties { get; set; }

        public List<titleItem> availableTitles { get; set; }

        public List<companyType> companyTypes { get; set; }

        public List<byteSelectOption> tradingForOptions { get; set; }

        public List<productTypeGroup_Brand_Country> productTypeGroups { get; set; }

        public List<channel> channels { get; set; }

        public List<leadContactType> contactTypes { get; set; }

        public List<MertexLanguage> availableLanguages { get; set; }
    }
}