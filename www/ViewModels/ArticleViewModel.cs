using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using wwwDesignersGuild.Models.Carousel;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Database;
using wwwDesignersGuild.Models.ImgSize;
using wwwDesignersGuild.ViewModels.PageSectionViewModels;

namespace wwwDesignersGuild.ViewModels
{
    public class ArticleViewModel : BaseViewModel
    {
        public cmsArticlePageDefinition Definition { get; set; }

        public CarouselViewModel Carousel { get; set; }

        public PageSectionViewModel Video { get; set; }

        public articleTypeSelect ArticleType
        {
            get
            {
                return ArticleTypes.Single(at => at.ArticleTypeID == Definition.ArticleDetails.ArticleTypeID);
            }
        }

        public byte ArticleTemplateID
        {
            get
            {
                return Definition.ArticleDetails.ArticleTemplateID;
            }
        }

        public byte ArticleTypeID 
        { 
            get
            {
                return ArticleType.ArticleTypeID;
            }
        }

        public articleSubTypeSelect ArticleSubType
        {
            get
            {
                return ArticleSubTypes.Single(ast => ast.ArticleSubTypeID == Definition.ArticleDetails.ArticleSubTypeID);
            }
            set
            {

            }
        }

        public byte ArticleSubTypeID
        {
            get
            {
                return ArticleSubType.ArticleSubTypeID;
            }
        }

        public List<articleTypeSelect> ArticleTypes
        {
            get
            {
                return Definition.articleTypeSelects;
            }
            set 
            {
                
            }
        }

        public List<articleSubTypeSelect> ArticleSubTypes
        {
            get
            {
                return Definition.articleSubTypeSelects;
            }
            set 
            {
                
            }
        }

        public string Contents
        {
            get
            {
                var result = Definition.ArticleDetails.Contents ?? "";

                result = result.Replace("\n\n", "\n");

                result = result.Replace("\n", "\n\n");

                result = result.Replace("\n\n", "\r\n");

                result = result.Replace(Environment.NewLine, $"</p><p>");

                result = $"<p>{result}</p>";

                return result;
            }
        }

        public string RawContents
        {
            get
			{
				return Definition.ArticleDetails.Contents;
			}
        }

        public string Heading
        {
            get
            {
                return Definition.ArticleDetails.Heading;
            }
        }

        public string SubHeading
        {
            get
            {
                return Definition.ArticleDetails.SubHeading;
            }
        }

        public List<articleRelatedArticleSelect> RelatedArticles
        {
            get
            {
                return Definition.articleRelatedArticleSelects.OrderBy(p => p.SortOrder).ToList();
            }
        }

        public bool ShowArticleNavButtons
        {
            get
            {
                return ShowNextArticle || ShowPreviousArticle;
            }
        }

        public bool ShowNextArticle
        {
            get
            {
                return Definition.articleNextArticleSelects.Any();
            }
        }

        public bool ShowPreviousArticle
        {
            get
            {
                return Definition.articlePreviousArticleSelects.Any();
            }
        }

        public articlePreviousArticleSelect PreviousArticle
        {
            get
            {
                return Definition.articlePreviousArticleSelects.FirstOrDefault();
            }
        }

        public articleNextArticleSelect NextArticle
        {
            get
            {
                return Definition.articleNextArticleSelects.FirstOrDefault();
            }
        }

        public IHtmlString Breadcrumbs()
        {
            var ti = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;

            //{ti.ToTitleCase(Definition.pageBreadcrumbs[0].ToLowerInvariant())?.Replace("<Li><Span", @"<li class=""breadcrumb-item active""><span").Replace("<Li>", @"<li class=""breadcrumb-item"">")}

            var breadcrumbs = String.Join("", Definition.Breadcrumbs.Select(b =>
            {
                var heading = ti.ToTitleCase(b.Heading.ToLowerInvariant());

                var url = $"/{MasterDefinition.UrlBreakdown.countryDetails.URL}/{b.URLKeywords}/{b.ContentType}{b.ContentID}";

                if (b != Definition.Breadcrumbs.Last())
                    return $@"<li class=""breadcrumb-item""><a href=""{url}"">{heading}</a></li>";
                else
                    return $@"<li class=""breadcrumb-item active""><span>{heading}</span></li>";
            }));

            return new HtmlString($@"<ul class=""breadcrumb"">{breadcrumbs}</ul>");
        }

        [JsonIgnore]
        public SocialMediaShareViewModel SocialMediaViewModel { get; set; }


        protected ArticleViewModel(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            cmsArticlePageDefinition objDefinition) : base(objMaster,
                                             objVisitor)
        {
            Definition = objDefinition;
            SocialMediaViewModel = new SocialMediaShareViewModel(directionClass: "dropdown", buttonOnly: false, objBase: this);
        }

        public static async Task<ArticleViewModel> CreateAsync(
            NewMasterDefinition objMaster,
            VisitorSelect objVisitor,
            cmsArticlePageDefinition objDefinition)
        {
            var result = new ArticleViewModel(objMaster, objVisitor, objDefinition);

            await result.Initialise();

            return result;
        }

        new protected async Task Initialise()
        {
#if DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif

            Carousel = new CarouselViewModel(
                carouselId: $"#collection-{MasterDefinition.UrlBreakdown.ContentId}-carousel",
                wrapperCssClass: null,
                aspectRatio: 1.636m,
                heading: Definition.ArticleDetails.Heading,
                images: Definition.articleImageSelects
                                  .Where(i => i.TemplateImageID == 2 || i.TemplateImageID == 3)
                                  .OrderBy(i => i.SortOrder)
                                  .Select(i => new CarouselImage(i.ImageID, i.SortOrder, i.AltText, ImageSize.Twelve)),
                this
            );

            var imageIds = Carousel.Images.Select(i => i.ImageID);

            if (Definition.articleVideoSelects.Any())
            {
                Video = new PageSectionViewModel(
                    pageSection: new PageSectionSelect(),
                    baseViewModel: this,
                    sectionDefinition: new VideoPageSection(new PageSectionVideo()
                    {
                        PageSectionID = 0,
                        VideoEmbedCode = Definition.articleVideoSelects[0].EmbedCode,
                        AspectRatio = Definition.articleVideoSelects[0].AspectRatio,
                        VideoID = Definition.articleVideoSelects[0].VideoID
                    }),
                    isShop: false,
                    pagingOverride: false
                );
            }

            await base.Initialise();

            await base.InitialiseHotspots(imageIds);

            Carousel.Images.ForEach(i =>
            {
                var hs = Hotspots.SingleOrDefault(h => h.ImageID == i.ImageID);
                if (hs != null)
                {
                    i.Hotspots = hs.Hotspots;
                }
            });

            NoIndex = Definition.ArticleDetails.NoIndex;

            NoFollow = Definition.ArticleDetails.NoFollow;

#if DEBUG
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"ArticleViewModel initialised in {sw.Elapsed}");
#endif
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}