using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Interface
{
    public interface ICacheImageProvider
    {
        byte[] Get(String imageType, String width, String imageId);
        void Set(String imageType, String width, String imageId, byte[] data);
        bool IsSet(String imageType, String width, String imageId);
        void Invalidate(String imageType, String width, String imageId);
    }
}
