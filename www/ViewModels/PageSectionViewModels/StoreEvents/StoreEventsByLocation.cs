using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class StoreEventsByLocation
	{
		public List<PageSectionStoreEvent> StoreEvents { get; set; }
		public string StoreEventLocation { get; set; }
		public short StoreEventLocationID { get; set; }

		public StoreEventsByLocation(short storeEventLocationId,
			string storeEventLocation,
			List<PageSectionStoreEvent> events)
		{
			StoreEventLocationID = storeEventLocationId;
			StoreEventLocation = storeEventLocation;
			StoreEvents = events;
		}
	}
}