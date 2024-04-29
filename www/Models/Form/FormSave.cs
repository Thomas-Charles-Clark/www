using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Form
{
    public class PassFormSave
    {
        public short FormID { get; set; }
        public string Email { get; set; }
        public string EmailConfirmation { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string Town { get; set; }
        public string County { get; set; }
        public string Postcode { get; set; }
        public short? MarketingCountryID { get; set; }
        public byte? MarketingLanguageID { get; set; }
        public string PhoneNumber { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public bool ConfirmEmail { get; set; }
        public bool AddressRequired { get; set; }
        public bool PasswordRequired { get; set; }
        public bool CaptchaRequired { get; set; }
        public string gRecaptcha { get; set; }
        public byte SourceID { get; set; }
        public string SubmitButtonText { get; set; }
        public int ContentID { get; set; }
        public string HeaderText { get; set; }
        public string FooterText { get; set; }
        public long? PageSectionID { get; set; }
        public short? ThirdPartyID { get; set; }
        public short? CompetitionID { get; set; }
        public short? BrochureID { get; set; }
        public string AppointmentLocation { get; set; }
        public string AppointmentTime { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int? TransactionID { get; set; }
        public short? StoreEventID { get; set; }
        public short? StoreEventLocationID { get; set; }
        public DateTime? StoreEventDate { get; set; }
        public int? StoreEventDateID { get; set; }
        public long? StoreEventTimeSlot { get; set; }
        public List<MarketingPermission> MarketingPermissions { get; set; }
        public byte? AppointmentAgendaID { get; set; }
        public byte? AppointmentTypeID { get; set; }
        public string AppointmentComments { get; set; }
    }
}