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
    public class GenericViewModel_404 : BaseViewModel
    {
        public string RedirectURL { get; set; }

        protected GenericViewModel_404(
            string redirectUrl,
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor) : base(objMaster,
                                             objVisitor)
        {
            RedirectURL = redirectUrl;
        }

        public static async Task<GenericViewModel_404> CreateAsync(
            string redirectUrl,
            NewMasterDefinition objMaster, 
            VisitorSelect objVisitor)
        {
            var result = new GenericViewModel_404(redirectUrl, objMaster, objVisitor);

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

            await base.Initialise();

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"GenericViewModel_404 initialised in {sw.Elapsed}");
#endif
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}