using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionHTML
	{
		public long HTMLSectionID { get; set; }

		public long PageSectionID { get; set; }

		[JsonIgnore]
		public string Contents { get; set; }

		public string ImageList { get; set; }

		public IEnumerable<long> ImageIDs
		{
			get
			{
				if (!string.IsNullOrEmpty(ImageList))
				{
					return JsonConvert.DeserializeObject<IEnumerable<ImageIDWrapper>>(ImageList).Select(i => i.ImageID);
				}
				else
				{
					return Enumerable.Empty<long>();
				}
			}
			internal set
			{

			}
		}
	}
}