using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheBraintreeErrorResponseProvider
    {
        List<braintreeErrorResponseSelect> Get(byte languageId, DateTime useDate);
        void Set(byte languageId, DateTime useDate, List<braintreeErrorResponseSelect> data);
        bool IsSet(byte languageId, DateTime useDate);
        void Invalidate(byte languageId, DateTime useDate);
    }
}