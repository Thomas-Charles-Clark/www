using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageFormCountry
	{
		public long PageSectionID { get; set; }
		public short CountryID { get; set; }
		public string Name { get; set; }
		public string Code { get; set; }
	}
}