using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels
{
    public class FixedPageViewModel<T>
    {
        public T Definition { get; set; }

        [JsonIgnore]
        public BaseViewModel BaseViewModel { get; set; }

        [JsonIgnore]
        public NewMasterDefinition MasterDefinition
        {
            get
            {
                return BaseViewModel.MasterDefinition;
            }
        }

        [JsonIgnore]
        public VisitorSelect Visitor
        {
            get
            {
                return BaseViewModel.Visitor;
            }
        }

        public byte LanguageID
        {
            get
            {
                return MasterDefinition.UrlBreakdown.countryDetails.LanguageID;
            }
        }

        public short CountryID
        {
            get
            {
                return MasterDefinition.UrlBreakdown.countryDetails.CountryID;
            }
        }

        public DateTime Date
        {
            get
            {
                return MasterDefinition.UrlBreakdown.ContentDate;
            }
        }

        public countryItem CountryItem
        {
            get
            {
                return MasterDefinition.UrlBreakdown.countryDetails;
            }
        }

        public FixedPageViewModel(T objDefinition, BaseViewModel objBase)
        {
            Definition = objDefinition;
            BaseViewModel = objBase;
        }

        public FixedPageViewModel(BaseViewModel objBase)
        {
            Definition = default;
            BaseViewModel = objBase;
        }

        protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif



#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"FixedPageViewModel initialised in {sw.Elapsed}");
#endif
        }
    }
}