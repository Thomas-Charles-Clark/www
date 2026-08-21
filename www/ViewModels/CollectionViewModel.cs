using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.CollectionPage;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ImgSize;

namespace wwwDesignersGuild.ViewModels
{
    public class CollectionViewModel : BaseViewModel
    {
        public cmsCollectionPageDefinition Definition { get; set; }
        public CarouselViewModel Carousel { get; set; }

        [JsonIgnore]
        public SocialMediaShareViewModel SocialMediaViewModel { get; set; }

        public IHtmlString Breadcrumbs()
        {
            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;

            //{ti.ToTitleCase(Definition.pageBreadcrumbs[0].ToLowerInvariant())?.Replace("<Li><Span", @"<li class=""breadcrumb-item active""><span").Replace("<Li>", @"<li class=""breadcrumb-item"">")}

            if (Definition.collectionPageBreadcrumbs != null)
            {
                var breadcrumbs = String.Join("", Definition.collectionPageBreadcrumbs.Select(b =>
                {
                    var heading = ti.ToTitleCase(b.Heading?.ToLowerInvariant());

                    var url = $"/{MasterDefinition.UrlBreakdown.countryDetails.URL}/{b.URLKeywords}/l{b.PageID}";

                    if (b != Definition.collectionPageBreadcrumbs.Last())
                        return $@"<li class=""breadcrumb-item""><a href=""{url}"">{heading}</a></li>";
                    else
                        return $@"<li class=""breadcrumb-item active""><span>{heading}</span></li>";
                }));

                return new HtmlString($@"<ul class=""breadcrumb"">{breadcrumbs}</ul>");
            }
            else
            {
                return null;
            }
        }

        protected CollectionViewModel(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            bool verifiedBot) : base(objMaster, objVisitor, verifiedBot)
        {
            SocialMediaViewModel = new SocialMediaShareViewModel(directionClass: "dropdown", buttonOnly: false, objBase: this);
        }

        public static async Task<CollectionViewModel> CreateAsync(NewMasterDefinition objMaster, VisitorSelect objVisitor, bool verifiedBot)
        {
            var result = new CollectionViewModel(objMaster, objVisitor, verifiedBot);

            await result.Initialise();

            return result;
        }

        protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            var objDb = new databaseDataContext();

            var objCollectionPage = new cmsCollectionPage();

            Definition = await objCollectionPage.loadCollectionPageFromCms
                (
                    (short)MasterDefinition.UrlBreakdown.ContentId,
                    MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    Visitor.memberId,
                    Visitor.memberTradeAccountID,
                    Visitor.memberDisplayTradePrice,
					this.MasterDefinition.UrlBreakdown.CountryAwareness.Match,
                    MasterDefinition.UrlBreakdown.ContentDate,
                    MasterDefinition.TradeMember
                );

            if (Definition.CollectionDetails == null && Definition.objUnavailabilityRedirect != null)
            {
#if DEBUG
                sw.Stop();

                System.Diagnostics.Debug.WriteLine($"CollectionViewModel initialised in {sw.Elapsed}");
#endif
                return;
            }

            Carousel = new CarouselViewModel(
                carouselId: $"#collection-{MasterDefinition.UrlBreakdown.ContentId}-carousel",
                wrapperCssClass: null,
                aspectRatio: 1.636m,
                heading: Definition.CollectionDetails?.Heading,
                images: Definition.collectionImageSelects
                                  .Where(i => i.TemplateImageID == 2)
                                  .OrderBy(i => i.SortOrder)
                                  .Select(i => new CarouselImage(i.ImageID, i.SortOrder, i.AltText, ImageSize.Nine, null, i.MaxWidth)),
                this,
                true,
                4000
            );

            var imageIds = Carousel.Images.Select(i => i.ImageID);

            await base.Initialise(VerifiedBot);

            await base.InitialiseHotspots(imageIds, this.MasterDefinition.UrlBreakdown.CountryAwareness.Match);

            Carousel.Images.ForEach(i =>
            {
                var hs = Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID);
                if (hs != null)
                {
                    i.Hotspots = hs.Hotspots;
                }
            });

            NoIndex = Definition.CollectionDetails?.NoIndex == true;

            NoFollow = Definition.CollectionDetails?.NoFollow == true;

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"CollectionViewModel initialised in {sw.Elapsed}");
#endif
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}