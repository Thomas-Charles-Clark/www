using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Films
{
	public class VimeoVideoStub
	{
		public string type { get; set; }
		public string version { get; set; }
		public string provider_name { get; set; }
		public string provider_url { get; set; }
		public string title { get; set; }
		public string author_name { get; set; }
		public string author_url { get; set; }
		public string thumbnail_url { get; set; }
		public DateTime upload_date { get; set; }
	}
}