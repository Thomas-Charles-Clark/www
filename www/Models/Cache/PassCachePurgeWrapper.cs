using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Models.Cache
{
	public class PassCachePurgeWrapper
	{
		public IEnumerable<PassCachePurge> items { get; set; }
	}
}