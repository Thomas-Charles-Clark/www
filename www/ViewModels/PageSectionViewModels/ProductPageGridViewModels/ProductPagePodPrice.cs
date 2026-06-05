using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class ProductPagePodPrice
	{
		public decimal? PriceMin { get; set; }
		public decimal? PriceMax { get; set; }
		public decimal? SalePriceMin { get; set; }
		public decimal? SalePriceMax { get; set; }
		public decimal? DiscountPercentageMin { get; set; }
		public decimal? DiscountPercentageMax { get; set; }
		public decimal? ClearanceDiscountPercentageMin { get; set; }
		public decimal? ClearanceDiscountPercentageMax { get; set; }

		[JsonIgnore]
		public string PriceMessage { get; set; }

		[JsonIgnore]
		public string RRPMessage { get; set; }

		[JsonIgnore]
		public string DiscountMessage { get; set; }

		[JsonIgnore]
		public string CodeMessage { get; set; }

		[NotMapped]
		public string PriceMessageBase64
		{
			get
			{
				if (!string.IsNullOrEmpty(PriceMessage))
				{
					return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(PriceMessage));
				}
				else
				{
					return null;
				}
			}
		}

		[NotMapped]
		public string DiscountMessageBase64
		{
			get
			{
				if (!string.IsNullOrEmpty(DiscountMessage))
				{
					return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(DiscountMessage));
				}
				else
				{
					return null;
				}
			}
		}

		[NotMapped]
		public string RRPMessageBase64
		{
			get
			{
				if (!string.IsNullOrEmpty(RRPMessage))
				{
					return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(RRPMessage));
				}
				else
				{
					return null;
				}
			}
		}

		[NotMapped]
		public string CodeMessageBase64
		{
			get
			{
				if (!string.IsNullOrEmpty(CodeMessage))
				{
					return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(CodeMessage));
				}
				else
				{
					return null;
				}
			}
		}
	}
}