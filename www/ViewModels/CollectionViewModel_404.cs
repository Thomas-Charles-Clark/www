using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.CollectionPage;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels
{
    public class CollectionViewModel_404 : BaseViewModel
    {
        public string RedirectURL { get; set; }

        protected CollectionViewModel_404(
            string redirectUrl,
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            bool verifiedBot) : base(objMaster, objVisitor, verifiedBot)
        {
            RedirectURL = redirectUrl;
        }

        public static async Task<CollectionViewModel_404> CreateAsync(
            string redirectUrl,
            NewMasterDefinition objMaster, 
            VisitorSelect objVisitor,
            bool verifiedBot)
        {
            var result = new CollectionViewModel_404(redirectUrl, objMaster, objVisitor, verifiedBot);

            await result.Initialise();

            return result;
        }

        new protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            
            NoIndex = true;

            NoFollow = true;

            await base.Initialise(VerifiedBot);

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"CollectionViewModel_404 initialised in {sw.Elapsed}");
#endif
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}