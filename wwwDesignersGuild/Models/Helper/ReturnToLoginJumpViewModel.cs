using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Helper
{
    public class ReturnToLoginJumpViewModel
    {
        public masterDefinition objMaster { get; set; }
        public int destination { get; set; }

        public ReturnToLoginJumpViewModel (int destination, masterDefinition objMaster)
        {
            this.objMaster = objMaster;
            this.destination = destination;
        }
    }
}