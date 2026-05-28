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
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.Form
{
    public class cmsEmailCapturePopup
    {
        public async Task<cmsEmailCapturePopupForm> loadEmailCapturePopupFormFromCms(passEmailCaptureLoad objLoad)
        {
            //  Is page in cache?
            cmsEmailCapturePopupForm store;
            var objEmailCapturePopupFormCache = new EmailCapturePopupFormCacheProvider();
            store = objEmailCapturePopupFormCache.Get(objLoad.countryId, objLoad.languageId, objLoad.contentDate);

            if (store == null)
            {
                store = await Task.FromResult<cmsEmailCapturePopupForm>(loadEmailCapturePopupFromCmsDatabase(objLoad));

                objEmailCapturePopupFormCache.Set(objLoad.countryId, objLoad.languageId, objLoad.contentDate, store);
            }

            return store;
        }

        private cmsEmailCapturePopupForm loadEmailCapturePopupFromCmsDatabase(passEmailCaptureLoad objLoad)
        {
            var dataContext = new databaseDataContext();
            
            IMultipleResults results = dataContext.EmailCapturePopup_Form_Select(objLoad.countryId, objLoad.languageId, objLoad.contentDate);

            var emailCapturePopupForm = new cmsEmailCapturePopupForm
            {
                defaultCountryForDropDown = objLoad.countryId,
                objForm = results.GetResult<PageSectionForm>().Single(),
                availableCountries = results.GetResult<PageFormCountry>().ToList(),
                availableLanguages = results.GetResult<Language>().ToList(),
                banner = results.GetResult<pageSectionBanner>().SingleOrDefault(),
            };

            return emailCapturePopupForm;
        }
    }
}