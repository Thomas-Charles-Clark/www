using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Visitor;

namespace wwwDesignersGuild.ViewModels.Trade
{
    public class TradeRegistrationViewModel
    {
        public BaseViewModel BaseViewModel { get; set; }
        public Guid? GUID { get; set; }
        public tradeAuthGuidCheckResponse Data { get; set; }

        private TradeRegistrationViewModel(BaseViewModel objBase, Guid? guid)
        {
            BaseViewModel = objBase;
            GUID = guid;
        }

        public static async Task<TradeRegistrationViewModel> CreateAsync(BaseViewModel objBase)
        {
            Guid? guid = null;

            if (Guid.TryParse(HttpContext.Current.Request.QueryString["a"], out Guid g))
            {
                guid = g;
            }

            var result = new TradeRegistrationViewModel(objBase, guid);

            await result.Initialise();

            return result;
        }

        protected async Task Initialise()
        {
            if (GUID != null)
            {
                Data = await new VisitorFactory().callMemberTradeAuthCommence(GUID.Value);
            }
        }
    }
}