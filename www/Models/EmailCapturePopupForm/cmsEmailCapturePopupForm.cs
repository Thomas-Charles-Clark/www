using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.EmailCapturePopupForm
{

    public class cmsEmailCapturePopupForm
    {
        public pageSectionForm objForm { get; set; }
        public List<pageFormCountry> availableCountries { get; set; }
        public List<Language> availableLanguages { get; set; }
        public short defaultCountryForDropDown { get; set; }
        public pageSectionBanner banner { get; set; }

    }
}