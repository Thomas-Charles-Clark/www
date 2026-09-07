using System.Runtime.Caching;

namespace wwwDesignersGuild.Models.Cache
{
	public class CacheDescriptor
	{
		public MemoryCache Cache { get; set; }
		public string[] KeyBreakdown { get; set; }
	}
}