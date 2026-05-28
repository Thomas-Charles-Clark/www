using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

using wwwDesignersGuild.Models.Helper;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionViewModel
	{
		[JsonIgnore]
		public BaseViewModel Base { get; set; }

		[JsonIgnore]
		public PageSectionSelect PageSection { get; set; }

		public object Definition { get; set; }

		public bool IsShop { get; set; }

		public PageSectionType PageSectionType
		{
			get
			{
				return (PageSectionType)PageSection.PageSectionTypeID;
			}
		}

		public dgFixedPageSection? BespokeContentPageSection
		{
			get
			{
				if (Enum.TryParse<dgFixedPageSection>($"{PageSection.PageSectionID}", out dgFixedPageSection dgfps))
					return dgfps;
				else
					return null;
			}
		}

		public PageSectionViewModel(
			PageSectionSelect pageSection,
			BaseViewModel baseViewModel,
			object sectionDefinition,
			bool isShop,
			bool? pagingOverride = null)
		{
			PageSection = pageSection;
			Base = baseViewModel;
			Definition = sectionDefinition;
			IsShop = isShop;
			if (pagingOverride != null)
			{
				PageSection.Paging = pagingOverride.Value;
			}
		}
	}
}