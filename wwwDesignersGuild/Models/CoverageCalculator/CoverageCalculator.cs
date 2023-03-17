using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models
{
    public class PassCoverageParameters
    {
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public string Unit { get; set; }
        public string Dimensions { get; set; }
        public bool IncludeWastage { get; set; }
        public int ProductPageID { get; set; }
        public byte Coats { get; set; }
        public short? PaintFinishAttributeID { get; set; }
        public string Email { get; set; }
        public string Captcha { get; set; }
    }

    public class CoverageResponse
    {
        public string RequirementsExplanation { get; set; }
        public bool Errored { get; set; }
        public List<CoverageRequirement> ShoppingList { get; set; }
        public long? EmailID { get; set; }

        public CoverageResponse()
        {
            RequirementsExplanation = null;
            ShoppingList = new List<CoverageRequirement>();
            EmailID = null;
        }
    }

    public class CoverageRequirement
    {
        public int ProductID { get; set; }
        public decimal Quantity { get; set; }
        public byte BasketLineTypeID { get; set; }

        public CoverageRequirement(int productId, decimal quantity, byte basketLineTypeId)
        {
            ProductID = productId;
            Quantity = quantity;
            BasketLineTypeID = basketLineTypeId;
        }
    }
}