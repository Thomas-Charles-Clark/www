using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.Models.EmailCapturePopupForm
{

    public class cmsEmailCapturePopupForm
    {
        public PageSectionForm objForm { get; set; }
        public List<PageFormCountry> availableCountries { get; set; }
        public List<Language> availableLanguages { get; set; }
        public short defaultCountryForDropDown { get; set; }
        public pageSectionBanner banner { get; set; }

    }
}