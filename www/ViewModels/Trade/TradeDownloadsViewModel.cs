using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Trade;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeDownloadsViewModel
    {
        public TradeAccountUsersModel TradeAccountModel { get; set; }

        [JsonIgnore]
        public BaseViewModel BaseViewModel { get; set; }

        public bool Permission { get; set; }

        public TradeDownloadFileSystemProvider FileSystemProvider { get; set; }

        public TradeDownloadsViewModel(bool permission, string path, TradeAccountUsersModel taum, BaseViewModel objBase)
        {
            Permission = permission;
            FileSystemProvider = new TradeDownloadFileSystemProvider(path);
            TradeAccountModel = taum;
            BaseViewModel = objBase;
        }

        public static async Task<TradeDownloadsViewModel> CreateAsync(bool permission, string path, TradeAccountUsersModel taum, BaseViewModel objBase)
        {
            var result = new TradeDownloadsViewModel(permission, path, taum, objBase);

            await result.Initialise();

            return result;
        }

        public async Task Initialise()
        {
            await BaseViewModel.SiteCultureCheck();
        }
    }
}