using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Redirect
{
	public class RedirectSelectResult
	{
		public string OldURL { get; set; }
		public string RedirectURL { get; set; }
		public bool Permanent { get; set; }
	}
}