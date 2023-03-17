using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Models.Classes
{
    public class MoodboardContainer
    {
        public string Heading { get; set; }
        public string SubHeading { get; set; }
        public List<MoodboardBrand> Brands { get; set; }
        public List<Models.ProductPage.ProductType> ProductTypes { get; set; }
        public List<Moodboard> Moodboards { get; set; }
        public List<MoodboardImage> MoodboardImages { get; set; }
        public List<ScrapbookImage> Scrapbook { get; set; }
        public List<Room> Rooms { get; set; }
    }

    public class MoodboardBrand
    {
        public byte BrandID { get; set; }
        public string Name { get; set; }
        public byte SortOrder { get; set; }
        public string LogoURL { get; set; }
    }

    public class Room
    {
        public byte RoomID { get; set; }
        public string Name { get; set; }
    }

    public class MoodboardGridViewSave
    {
        public string masterDefString { get; set; }
        public int MoodboardID { get; set; }
        public string CustomerName { get; set; }
        public byte? RoomID { get; set; }
        public string Reference { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public DateTime ContentDate { get; set; }
    }

    public class Moodboard
    {
        public int MoodboardID { get; set; }
        public Guid GUID { get; set; }
        public string JSON { get; set; }
        public short Height { get; set; }
        public string CustomerName { get; set; }
        public string Room { get; set; }
        public byte? RoomID { get; set; }
        public string Reference { get; set; }
        public DateTime CreatedDT { get; set; }
    }

    public class PassMoodboardGridViewModel
    {
        public string composeMoodboard { get; set; }
        public string viewMoodboard { get; set; }
        public string orderSamples { get; set; }
        public string share { get; set; }
        public string delete { get; set; }
        public string masterDefstring { get; set; }
        public masterDefinition masterDefinition { get; set; }
        public List<Moodboard> moodboards { get; set; }
        public List<Room> rooms { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public DateTime ContentDate { get; set; }
    }

    public class PassMoodboardPod
    {
        public int MoodboardID { get; set; }
        public byte? RoomID { get; set; }
        public string CustomerName { get; set; }
        public string Reference { get; set; }
        public string GUID { get; set; }
        public string JSON { get; set; }
        public passScrapbookPod Image { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDT { get; set; }
        public masterDefinition masterDefinition { get; set; }
    }

    public class MoodboardImage
    {
        public int MoodboardID { get; set; }
        public long ImageID { get; set; }
        public int? ProductPageID { get; set; }
        public byte SortOrder { get; set; }
    }

    public class CreateMoodboardResult
    {
        public int MoodboardID { get; set; }
        public DateTime CreatedDT { get; set; }
        public string CustomerName { get; set; }
        public string Reference { get; set; }
        public string Room { get; set; }
    }

    public class MoodboardSaveImageResult
    {
        public string GUID { get; set; }
    }

    public class MoodboardShareEmail
    {
        public int MoodboardID { get; set; }
        public bool DetailedProductInfo { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string RecipientName { get; set; }
        public string RecipientEmail { get; set; }
        public string Body { get; set; }
        public string ImageURL { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public string gRecaptcha { get; set; }
    }

    public class PassScrapbook 
    { 
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public List<PassScrapbookImage> Images { get; set; }
    }

    public class PassScrapbookImage
    {
        public long ImageID { get; set; }
        public int SortOrder { get; set; }
    }

    public class PassMoodboardJpegSave
    {
        public int MoodboardID { get; set; }
        public string File { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public string Room { get; set; }
        public string FileName
        {
            get
            {
                var result = $"{MoodboardID}";

                if (!String.IsNullOrEmpty(Name))
                    result += $"-{Name}";

                if (!String.IsNullOrEmpty(Reference))
                    result += $"-{Reference}";

                if (!String.IsNullOrEmpty(Room))
                    result += $"-{Room}";
                
                result += ".jpeg";

                return result.Replace(" ", "_");
            }
            set
            {

            }
        }
    }

    public class PassMoodboardID
    {
        public int moodboardId { get; set; }
    }

    public class PassMoodboardImageRemoval
    {
        public int MoodboardID { get; set; }
        public List<long> ImageIDs { get; set; }
    }

    public class PassMoodboardOrderingFetch
    {
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public int MoodboardID { get; set; }
        public BaseViewModel BaseViewModel { get; set; }
    }
}