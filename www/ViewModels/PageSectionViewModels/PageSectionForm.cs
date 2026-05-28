using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Source;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionForm
	{
		public short FormID { get; set; }
		public long PageSectionID { get; set; }
		public bool ConfirmEmail { get; set; }
		public bool AddressRequired { get; set; }
		public bool? PhoneNumberRequired { get; set; }
		public bool PasswordRequired { get; set; }
		public bool CaptchaRequired { get; set; }
		public string SubmitButtonText { get; set; }
		public string HeaderText { get; set; }
		public string FooterText { get; set; }
		public short? CompetitionID { get; set; }
		public string CompetitionTerms { get; set; }
		public short? BrochureID { get; set; }
		public string BrochureTitle { get; set; }
		public string BrochureCoverImagePath { get; set; }
		public string BrochurePDFPath { get; set; }
		public string InteractiveBrochureURL { get; set; }
		public short? StoreEventID { get; set; }
		public string StoreEventName { get; set; }
		public short? ThirdPartyID { get; set; }
		public byte SourceID { get; set; }
		public byte SourceGroupID { get; set; }
		public FormSource Source
		{
			get
			{
				return (FormSource)SourceID;
			}
			set
			{

			}
		}
		public string ThanksText { get; set; }
		public int? RedirectPageID { get; set; }

		public string PromotionCode { get; set; }
		public string PromotionDescription { get; set; }

		public string Heading { get; set; }
		public string SubHeading { get; set; }
	}
}