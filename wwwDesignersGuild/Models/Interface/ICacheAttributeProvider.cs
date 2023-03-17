using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheAttributeProvider
    {
        cmsAttributeDefinition Get(byte languageId);
        void Set(byte languageId, cmsAttributeDefinition data);
        bool IsSet(byte languageId);
        void Invalidate(byte languageId);
    }
}
