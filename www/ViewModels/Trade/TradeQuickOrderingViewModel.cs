using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeQuickOrderingViewModel
    {
        public TradeAccountUsersModel TradeAccountModel { get; set; }

        [JsonIgnore]
        public BaseViewModel BaseViewModel { get; set; }

        public string Heading { get; set; }

        public bool OverallPermission { get; set; }

        public List<ProductType> ProductTypes
        {
            get
            {
                return BaseViewModel.MasterDefinition.ProductTypeDropDown.ProductTypes;
            }
        }

        public TradeQuickOrderingViewModel(bool overallPermission, TradeAccountUsersModel taum, BaseViewModel bvm)
        {
            OverallPermission = overallPermission;
            TradeAccountModel = taum;
            BaseViewModel = bvm;
        }

        public static async Task<TradeQuickOrderingViewModel> CreateAsync(bool permission, TradeAccountUsersModel taum, BaseViewModel bvm)
        {
            var result = new TradeQuickOrderingViewModel(permission, taum, bvm);

            await result.Initialise();

            return result;
        }

        public async Task Initialise()
        {

        }
    }
}