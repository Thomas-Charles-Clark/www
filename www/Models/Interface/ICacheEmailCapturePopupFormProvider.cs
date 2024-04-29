using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheEmailCapturePopupFormProvider
    {
        cmsEmailCapturePopupForm Get(short countryId, byte languageId, DateTime useDate);
        void Set(short countryId, byte languageId, DateTime useDate, cmsEmailCapturePopupForm data);
        bool IsSet(short countryId, byte languageId, DateTime useDate);
        void Invalidate(short countryId, byte languageId, DateTime useDate);
    }
}