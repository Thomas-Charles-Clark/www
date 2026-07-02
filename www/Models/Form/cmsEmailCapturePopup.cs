using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Cache;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.EmailCapturePopupForm;

namespace wwwDesignersGuild.Models.Form
{
    public class cmsEmailCapturePopup
    {
        public async Task<cmsEmailCapturePopupForm> loadEmailCapturePopupFormFromCms(passEmailCaptureLoad objLoad)
        {
            cmsEmailCapturePopupForm store;

            var objEmailCapturePopupFormCache = new EmailCapturePopupFormCacheProvider();

            store = objEmailCapturePopupFormCache.GetOrCreate(objLoad.countryId, objLoad.languageId, objLoad.contentDate, () => {

                var result = Task.Run(() => loadEmailCapturePopupFromCmsDatabase(objLoad)).Result;

                return result;

            });

            return store;
        }

        private cmsEmailCapturePopupForm loadEmailCapturePopupFromCmsDatabase(passEmailCaptureLoad objLoad)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.EmailCapturePopup_Form_Select(objLoad.countryId, objLoad.languageId, objLoad.contentDate);

            var emailCapturePopupForm = new cmsEmailCapturePopupForm
            {
                defaultCountryForDropDown = objLoad.countryId,
                objForm = results.GetResult<pageSectionForm>().Single(),
                availableCountries = results.GetResult<pageFormCountry>().ToList(),
                availableLanguages = results.GetResult<Language>().ToList(),
                banner = results.GetResult<pageSectionBanner>().SingleOrDefault(),
            };

            return emailCapturePopupForm;
        }
    }
}