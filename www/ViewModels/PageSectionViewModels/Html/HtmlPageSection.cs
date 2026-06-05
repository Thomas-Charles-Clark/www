using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

using wwwDesignersGuild.Models.Hotspot;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class HtmlPageSection
	{
		[JsonIgnore]
		public string Contents { get; set; }

		public long HtmlSectionID { get; set; }

		public IEnumerable<long> ImageIDs { get; set; }

		public List<ImageHotspot> Hotspots { get; set; }

		public HtmlPageSection(
			long htmlSectionId,
			string contents,
			IEnumerable<long> imageIds
			)
		{
			HtmlSectionID = htmlSectionId;
			Contents = contents;
			ImageIDs = imageIds;
			Hotspots = new List<ImageHotspot>();
		}

		public static async Task<HtmlPageSection> CreateAsync(
			long htmlSectionId,
			string contents,
			IEnumerable<long> imageIds)
		{
			var result = new HtmlPageSection(htmlSectionId, contents, imageIds);

			result.Hotspots = new List<ImageHotspot>();

			return result;
		}
	}
}