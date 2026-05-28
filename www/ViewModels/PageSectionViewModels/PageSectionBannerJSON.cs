using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionBannerJSON
	{
		public string BannerJSON { get; set; }

		public object json
		{
			set
			{

			}
			get
			{
				return JsonConvert.DeserializeObject(string.IsNullOrEmpty(BannerJSON) ? "{}" : BannerJSON);
			}
		}
	}
}