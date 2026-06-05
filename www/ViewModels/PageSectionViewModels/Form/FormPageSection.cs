using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels.PageSectionViewModels
{
	public class FormPageSection
	{
		public PageSectionForm Form { get; set; }
		public List<PageSectionFormRetailAppointmentAgenda> RetailAppointmentAgendas { get; set; }
		public List<PageSectionFormRetailAppointmentType> RetailAppointmentTypes { get; set; }
		public List<StoreEventsByLocation> StoreEventsByLocation { get; set; }
		public List<PageFormCountry> Countries { get; set; }
		public List<Language> Languages { get; set; }
		public short DefaultCountryID { get; set; }

		public FormPageSection(
			PageSectionForm form,
			List<PageSectionFormRetailAppointmentAgenda> appointmentAgendas,
			List<PageSectionFormRetailAppointmentType> appointmentTypes,
			List<PageSectionStoreEvent> storeEvents,
			List<PageFormCountry> countries,
			short defaultCountryId,
			DateTime useDate
			)
		{
			Form = form;
			RetailAppointmentAgendas = appointmentAgendas;
			RetailAppointmentTypes = appointmentTypes;
			Countries = countries;
			DefaultCountryID = defaultCountryId;

			Languages = new cmsCountry().loadCompleteLanguageList(DefaultCountryID, useDate).Result.languages;

			StoreEventsByLocation = storeEvents.GroupBy(p => new
			{
				p.StoreEventLocationID,
				p.StoreEventLocation
			})
				.Select(p => new StoreEventsByLocation(
					storeEventLocationId: p.Key.StoreEventLocationID,
					storeEventLocation: p.Key.StoreEventLocation,
					events: p.ToList()))
				.ToList();
		}
	}

}