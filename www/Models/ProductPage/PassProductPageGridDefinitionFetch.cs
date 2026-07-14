using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.ProductPage
{
	public class PassProductPageGridDefinitionFetch
	{
		public long pageSectionId { get; set; }
		public int page { get; set; } = 1;
		public int pageSize { get; set; } = 48;
		public byte languageId { get; set; }
		public short countryId { get; set; }

		[JsonProperty("base")]
		public BaseViewModel @base { get; set; }

		public NewMasterDefinition MasterDefinition 
		{
			get 
			{
				return @base.MasterDefinition;
			} 
		}
	}
}