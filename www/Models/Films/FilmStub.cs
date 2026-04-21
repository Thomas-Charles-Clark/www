using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Films
{
	public class FilmStub
	{
		[JsonProperty("provider")]
		public string Provider { get; set; }

		[JsonProperty("videoId")]
		public string VideoID { get; set; }

		[JsonProperty("watchUrl")]
		public string WatchURL { get; set; }

		[JsonProperty("title")]
		public string Title { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("uploadDate")]
		public DateTime UploadDate { get; set; }
	}
}