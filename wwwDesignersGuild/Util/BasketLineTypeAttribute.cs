using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Util
{

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class BasketLineTypeAttribute : Attribute
    {
        public byte BasketLineTypeID { get; set; }

        public BasketLineTypeAttribute(byte basketLineTypeId)
        {
            BasketLineTypeID = basketLineTypeId;
        }
    }
}