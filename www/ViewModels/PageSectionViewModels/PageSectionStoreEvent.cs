using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class PageSectionStoreEvent
	{
		public long PageSectionID { get; set; }
		public short StoreEventID { get; set; }
		public short StoreEventLocationID { get; set; }
		public string StoreEventLocation { get; set; }
		public int? StoreEvent_DateID { get; set; }
		public DateTime? StoreEventDate { get; set; }
		public long? StoreEvent_Date_TimeSlotID { get; set; }
		public string StoreEventTimeSlot { get; set; }
	}
}