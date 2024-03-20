using MertexerV2.Models.Accounts;

using MertexerV2SQL.Models.Accounts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeAddressBookManagementViewModel : FixedPageViewModel_MemberAddress
    {
        public TradeAccountUsersModel Users { get; set; }
        public MertexInvoiceAddress InvoiceAddress { get; set; }

        public bool Permission { get; set; }

        public static async Task<TradeAddressBookManagementViewModel> CreateAsync(
            MemberAddressSelect objDefinition,
            TradeAccountUsersModel users,
            MertexInvoiceAddress invoiceAddress,
            bool permission,
            BaseViewModel objBase)
        {
            var result = new TradeAddressBookManagementViewModel(objDefinition,
                users,
                invoiceAddress,
                permission,
                objBase);

            await result.InitialiseAsync();

            return result;
        }

        public TradeAddressBookManagementViewModel(
            MemberAddressSelect objDefinition,
            TradeAccountUsersModel users,
            MertexInvoiceAddress invoiceAddress,
            bool permission,
            BaseViewModel objBase) : base(objDefinition, objBase)
        {
            InvoiceAddress = invoiceAddress;
            Users = users;
            Permission = permission;
        }
    }
}