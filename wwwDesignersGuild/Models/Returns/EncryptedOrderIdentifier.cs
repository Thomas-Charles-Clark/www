using DocumentFormat.OpenXml.Wordprocessing;

using Org.BouncyCastle.Bcpg;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.Models.Returns
{
    public class EncryptedOrderIdentifier
    {
        public string id { get; set; }

        internal int? OrderID
        {
            get
            {
                var numericalId = (int?)null;

                if (!String.IsNullOrEmpty(id)) {
                    if (Int32.TryParse(dgCrypto.Decrypt(id), out int i)) 
                    {
                        numericalId = i;
                    }
                }

                return numericalId;
            }
        }
    }
}