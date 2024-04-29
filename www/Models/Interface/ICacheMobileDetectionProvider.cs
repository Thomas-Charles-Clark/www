using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheMobileDetectionProvider
    {
        MobileDetectionDefinition Get(String userAgent);
        void Set(String userAgent, MobileDetectionDefinition data);
        bool IsSet(String userAgent);
        void Invalidate(String userAgent);
    }
}
