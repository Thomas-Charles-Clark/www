using Braintree;
using Braintree.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using wwwDesignersGuild.Models;
using wwwDesignersGuild.Models.ArticleSubType;
using wwwDesignersGuild.Models.ArticleType;
using wwwDesignersGuild.Models.Attribute;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.BasketClasses;
using wwwDesignersGuild.Models.Careers;
using wwwDesignersGuild.Models.Checkout;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Country;
using wwwDesignersGuild.Models.Form;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Menu;
using wwwDesignersGuild.Models.ProductPage;
using wwwDesignersGuild.Models.Search;
using wwwDesignersGuild.Models.Snippet;
using wwwDesignersGuild.Models.Stockists;
using wwwDesignersGuild.Models.StockistsContract;
using wwwDesignersGuild.Models.Trade;
using wwwDesignersGuild.Models.UnsubscribePage;
using wwwDesignersGuild.Models.ViewedProduct;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.ViewModels;

namespace wwwDesignersGuild.Controllers
{
    public enum paymentType
    {
        unknown,
        creditCard,
        payWithBraintree,
        giftVoucherFull,
        zeroValueRetailOrder,
        tradeOrder,
        amazonPay,
        applePay
    }

    public class SectionController : Controller
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Misc Components

        public ActionResult _googleAnalytics(short countryId)
        {
            Int64 visitorDbId = -1;

            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            ViewData["countryId"] = countryId;

            return View("~/Views-New/Shared/Components/_googleAnalytics.cshtml", visitorDbId);
        }

        public ActionResult _socialMediaShare(passSocialMediaShare objSocialMediaShare)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_socialMediaShare{0}.cshtml"), objSocialMediaShare);
        }

        #endregion


        #region Display Components

        public ActionResult _topNav(masterDefinition objMaster)
        {
            if (dgFramework.isPagePartOfCheckOutProcess(objMaster.UrlBreakdown.ContentType, objMaster.UrlBreakdown.ContentId))
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/Checkout/_checkoutHeader{0}.cshtml"), objMaster);
            }

            return View("~/Views-New/Shared/Components/_topNav.cshtml", objMaster);
        }


        public ActionResult _googleHref(masterDefinition objMaster)
        {
            return View("~/Views-New/Shared/Components/_googleHref.cshtml", objMaster);
        }


        public ActionResult _branding(masterDefinition objMaster)
        {
            if (dgFramework.isPagePartOfCheckOutProcess(objMaster.UrlBreakdown.ContentType, objMaster.UrlBreakdown.ContentId))
            {
                if (dgFramework.IsMobileDevice())
                {
                    //  Allow menu throughout checkout process ???
                    return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_branding{0}.cshtml"), objMaster);
                }

                return View("~/Views-New/Shared/Components/_empty.cshtml");
            }

            var objPassBranding = new passBranding
            {
                masterDefinition = objMaster
            };

            var objSearchProductType = new cmsSearchProductType();

            cmsSearchProductTypeDropDownDefinition objProductTypeDropDownData = objSearchProductType.loadSearchProductTypeDropDownFromCms(
                                                                                    objMaster.UrlBreakdown.countryDetails.CountryID,
                                                                                    objMaster.UrlBreakdown.countryDetails.LanguageID,
                                                                                    objMaster.UrlBreakdown.ContentDate).Result;

            objPassBranding.searchProductTypeDropDown = objProductTypeDropDownData;

            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_branding{0}.cshtml"), objPassBranding);
        }

        public async Task<ActionResult> _pullDownMenu(passPullDownMenu objPullDownMenu)
        {
            if (dgFramework.isPagePartOfCheckOutProcess(objPullDownMenu.passMaster.masterDefinition.UrlBreakdown.ContentType, objPullDownMenu.passMaster.masterDefinition.UrlBreakdown.ContentId))
            {
                return View("~/Views-New/Shared/Components/_empty.cshtml");
            }

            int? memberTradeAccountId = null;

            if (Session["memberTradeAccountID"] != null)
            {
                memberTradeAccountId = (int?)Session["memberTradeAccountID"];
            }

            cmsMenu objMenuPage = new cmsMenu();
            cmsMenuDefinition objMenuData = await objMenuPage.loadMenuFromCms(objPullDownMenu.passMaster.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                            objPullDownMenu.passMaster.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                            objPullDownMenu.passMaster.masterDefinition.UrlBreakdown.ContentDate,
                                                                            memberTradeAccountId.HasValue);

            objPullDownMenu.menuDefinition = objMenuData;
            return View("~/Views-New/Shared/Components/_pullDownMenu.cshtml", objPullDownMenu);
        }
        
        public async Task<ActionResult> _displaySnippetTextOnly(passSnippet objSnippetData)
        {
            var displayTradePrices = false;
            int? tradeAccountId = null;

            if (Session["memberTradeAccountID"] != null)
            {
                tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"].ToString());
            }

            if (Session["memberDisplayTradePrice"] != null)
            {
                displayTradePrices = Convert.ToBoolean(Session["memberDisplayTradePrice"].ToString());
            }

            cmsSnippet objSnippetSection = new cmsSnippet();
            cmsSnippetDefinition objSnippetStore = await objSnippetSection.loadSnippetFromCms(objSnippetData.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                                            objSnippetData.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                                            tradeAccountId,
                                                                                            displayTradePrices,
                                                                                            objSnippetData.masterDefinition.UrlBreakdown.ContentDate);

            objSnippetData.snippetContent = objSnippetStore.snippetRows.Where(p => p.SnippetLocationID == objSnippetData.SnippetLocationID).FirstOrDefault();

            return View("~/Views-New/Shared/_SnippetTextOnly.cshtml", objSnippetData);

        }


        public async Task<ActionResult> _displaySnippet(passSnippet objSnippetData)
        {
            var displayTradePrices = false;
            int? tradeAccountId = null;

            if (Session["memberTradeAccountID"] != null)
            {
                tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"].ToString());
            }

            if (Session["memberDisplayTradePrice"] != null)
            {
                displayTradePrices = Convert.ToBoolean(Session["memberDisplayTradePrice"].ToString());
            }

            cmsSnippet objSnippetSection = new cmsSnippet();
            cmsSnippetDefinition objSnippetStore = await objSnippetSection.loadSnippetFromCms(objSnippetData.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                                            objSnippetData.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                                            tradeAccountId,
                                                                                            displayTradePrices,
                                                                                            objSnippetData.masterDefinition.UrlBreakdown.ContentDate);

            objSnippetData.snippetContent = objSnippetStore.snippetRows.Where(p => p.SnippetLocationID == objSnippetData.SnippetLocationID).FirstOrDefault();

            return View("~/Views-New/Shared/_Snippet.cshtml", objSnippetData);

        }


        public async Task<ActionResult> _footer(passPullDownMenu objFooterMenu)
        {
            if (dgFramework.isPagePartOfCheckOutProcess(objFooterMenu.passMaster.masterDefinition.UrlBreakdown.ContentType, objFooterMenu.passMaster.masterDefinition.UrlBreakdown.ContentId))
            {
                return View("~/Views/Checkout/_checkoutFooter.cshtml");
            }
            
            int? memberTradeAccountId = null;

            if (Session["memberTradeAccountID"] != null)
            {
                memberTradeAccountId = (int?)Session["memberTradeAccountID"];
            }

            cmsMenu objMenuPage = new cmsMenu();
            cmsMenuDefinition objMenuData = await objMenuPage.loadMenuFromCms(objFooterMenu.passMaster.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                            objFooterMenu.passMaster.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                            objFooterMenu.passMaster.masterDefinition.UrlBreakdown.ContentDate,
                                                                            memberTradeAccountId.HasValue);

            objFooterMenu.menuDefinition = objMenuData;

            return View("~/Views-New/Shared/Components/_footer.cshtml", objFooterMenu);
        }

        public ActionResult _bannerTop(passBanner objBanners)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_bannerTop{0}.cshtml"), objBanners);
        }

        public ActionResult _bannerBottom(passBanner objBanners)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_bannerBottom{0}.cshtml"), objBanners);
        }

        public async Task<ActionResult> _displayPod(pod objPod)
        {
            if (objPod.StockStatusID != null)
            {
                var stockStatusCache = new cmsStockStatus();
            
                var stockStatusList = await stockStatusCache.loadStockStatusesFromCms(objPod.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    objPod.masterDefinition.UrlBreakdown.countryDetails.CountryID, 
                    objPod.masterDefinition.UrlBreakdown.ContentDate);

                objPod.StockStatus = stockStatusList.StockStatuses.Where(p => p.StockStatusID == objPod.StockStatusID).Select(p => p.StockStatus).Single();
            }

            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_pod{0}.cshtml"), objPod);
        }


        public async Task<ActionResult> _recentlyViewedProducts(passViewedProductPage objViewedProductPage)
        {
            Int64 visitorDbId = -1;

            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            var objViewProduct = new cmsViewedProduct();
            if (objViewedProductPage.ProductPageID == null) { }
            else
            {
                _ = await objViewProduct.mergeViewedProduct(visitorDbId,
                    objViewedProductPage.ProductPageID,
                    objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.CountryID);
            }

            objViewedProductPage = await objViewProduct.buildViewedProduct(
                visitorDbId, 
                objViewedProductPage, 
                Session["memberTradeAccountID"] != null, 
                Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"]));


            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_recentlyViewedProducts{0}.cshtml"), objViewedProductPage);
        }


        #endregion


        #region Generate Page Components

        public ActionResult _animation(passAnimation objAnimation)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_animation{0}.cshtml"), objAnimation);
        }

        public ActionResult _htmlSection(passHtml objHtml)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_htmlSection{0}.cshtml"), objHtml);
        }


        public ActionResult _slideShow(passSlideShow objSlideShow)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_slideShow{0}.cshtml"), objSlideShow);
        }


        public ActionResult _video(passVideo objVideo)
        {
            return View("~/Views/LandingPage/Sections/_videoSection.cshtml", objVideo);
        }


        public async Task<ActionResult> _productPageGridSection(passProductPageGrid objProductPageGrid)
        {
            var objAttribute = new cmsAttribute();
            var objAttributeDefinitions = await objAttribute.loadAttributeFromCms(objProductPageGrid.masterDefinition.UrlBreakdown.countryDetails.LanguageID);
            objProductPageGrid.attributeDefinitions = objAttributeDefinitions;

            var PageSectionID = objProductPageGrid.PageSectionID;

            //  Only include the images, attributes and pricing that are linked to the page section "product pages" (all sections are returned)
            var uniqueProductPages = JObject.FromObject(objProductPageGrid.productPages.json).SelectTokens("$...productPageId").Select(s => (int)s).ToList();

            var pageSectionAttributes = objProductPageGrid.productPages.attributes.Where(p => uniqueProductPages.Contains(p.ProductPageID)).ToList<pageSectionAttribute>();

            //Only include the Global Attribute Definitions that are linked to the page section "product pages"
            var uniqueAttribute = pageSectionAttributes.GroupBy(p => p.AttributeID).Select(grp => grp.Key).ToList<short>();
            var attributeDetails = objProductPageGrid.attributeDefinitions.attributeDetails.Where(p => uniqueAttribute.Contains(p.AttributeID)).OrderBy(p => p.SortOrder).ToList<AttributeDetail>();

            var attributeGroupIds = attributeDetails.Select(p => p.AttributeGroupID).Distinct().ToList();

            var attributeGroups = objProductPageGrid.attributeDefinitions.attributeGroups.Where(p => attributeGroupIds.Contains(p.AttributeGroupID)).ToList();

            objProductPageGrid.usePageSectionAttributes = pageSectionAttributes;
            objProductPageGrid.useAttributeDefinitions = new cmsAttributeDefinition { attributeGroups = attributeGroups, attributeDetails = attributeDetails };

            objProductPageGrid.usePageRelateds = new List<pageRelated>();

            foreach (var objRelated in objProductPageGrid.pageRelateds)
            {
                objProductPageGrid.usePageRelateds.Add(new pageRelated
                {
                    Heading = objRelated.Heading,
                    PageID = objRelated.PageID,
                    URLKeywords = objRelated.URLKeywords,
                    FullUrl = dgFramework.buildUrl(objProductPageGrid.masterDefinition.UrlBreakdown.countryDetails, objRelated.URLKeywords, dgContentType.landingPage, objRelated.PageID, Request.QueryString["dateOverride"], Request.QueryString["countryOverride"]),
                    RelatedFilterFontColour = objRelated.RelatedFilterFontColour,
                    RelatedFilterBold = objRelated.RelatedFilterBold
                });
            }

            var tradeChecksPassed = false;

            if (Session["memberTradeChecksPassed"] != null)
            {
                tradeChecksPassed = Convert.ToBoolean(Session["memberTradeChecksPassed"]);
            }

            var displayTradePrice = false;

            if (Session["memberDisplayTradePrice"] == null) { }
            else
            {
                displayTradePrice = Convert.ToBoolean(Session["memberDisplayTradePrice"]);
            }

            if (!tradeChecksPassed || !displayTradePrice) /* If you haven't passed the trade checks */
            {
                // and there's a Clearance attribute group delivered
                var clearanceGroup = objProductPageGrid.useAttributeDefinitions.attributeGroups.Where(p => p.AttributeGroupID == 0).SingleOrDefault();

                if (clearanceGroup != null)
                {
                    // remove it
                    objProductPageGrid.useAttributeDefinitions.attributeGroups.Remove(clearanceGroup);
                }
            }
            
            if (objProductPageGrid.usePageSectionAttributes.Count() == 0 && objProductPageGrid.pageRelateds.Count() == 0)
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_productPageGridSectionNoFilter{0}.cshtml"), objProductPageGrid);
                //return View("~/Views/LandingPage/Sections/_productPageGridSectionNoFilter.cshtml", objProductPageGrid);
            }
            else
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_productPageGridSection{0}.cshtml"), objProductPageGrid);
                //return View("~/Views/LandingPage/Sections/_productPageGridSection.cshtml", objProductPageGrid);
            }

        }


        public ActionResult _collectionGridSection(passCollectionGrid objCollectionGrid)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_collectionGridSection{0}.cshtml"), objCollectionGrid);
        }


        public ActionResult _pageGridSection(passPageGrid objPageGrid)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_pageGridSection{0}.cshtml"), objPageGrid);
        }


        public async Task<ActionResult> _articleGridSection(passArticleGrid objArticleGrid)
        {
            cmsArticleType articleType = new cmsArticleType();
            cmsArticleTypeDefinition objArticleTypes = await articleType.loadArticleTypeFromCms(objArticleGrid.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                                         objArticleGrid.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                                         objArticleGrid.masterDefinition.UrlBreakdown.ContentDate);

            objArticleGrid.pageSectionArticleTypes = objArticleTypes.articleTypeSelects;


            int typeCount = objArticleGrid.pageSectionArticles.Select(p => p.ArticleTypeID).Distinct().Count();

            List<articleSubTypeSelect> subTypeResults = new List<articleSubTypeSelect>();

            if (typeCount == 0 || typeCount > 1)
            {
                objArticleGrid.pageSectionArticleSubTypes = subTypeResults;
            }
            else
            {
                cmsArticleSubType articleSubType = new cmsArticleSubType();
                cmsArticleSubTypeDefinition objArticleSubTypes = await articleSubType.loadArticleSubTypeFromCms(objArticleGrid.pageSectionArticles.Select(p => p.ArticleTypeID).Distinct().Single(),
                                                                                            objArticleGrid.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                                                                                            objArticleGrid.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                                                                                            objArticleGrid.masterDefinition.UrlBreakdown.ContentDate);
                objArticleGrid.pageSectionArticleSubTypes = objArticleSubTypes.articleSubTypeSelects;
            }


            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_articleGridSection{0}.cshtml"), objArticleGrid);
        }


        public ActionResult _pagePageSectionBanner(passPageBannerLocation objPageBanner)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_pagePageSectionBanner{0}.cshtml"), objPageBanner);
        }


        public async Task<ActionResult> _pagePageSectionForm(passPageForm objPageForm)
        {
            _ = new cmsCountry();

            if (objPageForm.pageSectionForms.Count() == 0)
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objPageForm.masterDefinition);
            }

            if (Session["visitorIdFromDatabase"] == null) { }
            else
            {
                //  Determine the default country using the VisitorId
                long visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
                VisitorFactory visitorFactory = new VisitorFactory();
                objPageForm.defaultCountryForDropDown = await visitorFactory.callVisitorCountrySelect(visitorDbId);

                if (objPageForm.defaultCountryForDropDown == null)
                {
                    objPageForm.defaultCountryForDropDown = objPageForm.masterDefinition.UrlBreakdown.countryDetails.CountryID;
                }
            }

            var cmsCountry = new cmsCountry();

            var languagesResult = await cmsCountry.loadCompleteLanguageList(objPageForm.defaultCountryForDropDown.Value, objPageForm.masterDefinition.UrlBreakdown.ContentDate);

            objPageForm.languages = languagesResult.languages;

            return View(dgFramework.GetPreferredDeviceView("~/Views/LandingPage/Sections/_pagePageSectionForm{0}.cshtml"), objPageForm);
        }

        public ActionResult _StoreEventLocationDateEdit(Int16 StoreEventLocationID, String Dates, Int16 CountryID, Byte LanguageID, DateTime ContentDate)
        {
            if (Dates == null)
                Dates = "";

            ViewData["StoreEventLocationID"] = StoreEventLocationID;
            ViewData["Dates"] = Dates;
            ViewData["CountryID"] = CountryID;
            ViewData["LanguageID"] = LanguageID;
            ViewData["ContentDate"] = ContentDate;

            return PartialView("~/Views/LandingPage/Sections/_storeEventLocationDateEdit.cshtml");
        }


        #endregion


        #region Moodboard and Scrapbook


        public async Task<ActionResult> _moodboard(masterDefinition objMaster)
        {
            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }

            if (memberId == null)
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpMoodboard.cshtml", objMaster);
            }
            else if (dgFramework.isUserLoggedIn(Session["loginStatus"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpMoodboard.cshtml", objMaster);
            }

            var visitorFactory = new VisitorFactory();

            MoodboardContainer objMoodboard = new MoodboardContainer();

            objMoodboard = await visitorFactory.getMemberMoodboards(
                memberId.Value,
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                objMaster.UrlBreakdown.ContentDate
            );

            objMoodboard.Moodboards = objMoodboard.Moodboards.OrderByDescending(p => p.CreatedDT).ToList();

            return View(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/moodboard{0}.cshtml"), objMoodboard);
        }

        public ActionResult _displayMoodboardPod(PassMoodboardPod objMoodboardPod)
        {
            return View("~/Views-New/Shared/Components/_moodboardPod.cshtml", objMoodboardPod);
        }

        public ActionResult _displayScrapbookPod(passScrapbookPod objScrapbookPod)
        {
            return View("~/Views-New/Shared/Components/_scrapbookPod.cshtml", objScrapbookPod);
        }

        public async Task<ActionResult> _scrapbook(passScrapbook objScrapbook)
        {
            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }

            var visitorFactory = new VisitorFactory();

            objScrapbook.scrapbookImages = await visitorFactory.getVisitorScrapbook(
                visitorDbId,
                memberId,
                objScrapbook.productTypeId,
                objScrapbook.brandId,
                objScrapbook.isNew,
                objScrapbook.masterDefinition.UrlBreakdown.countryDetails.CountryID,
                objScrapbook.masterDefinition.UrlBreakdown.countryDetails.LanguageID,
                objScrapbook.masterDefinition.UrlBreakdown.ContentDate);

            // Here is where we should probably... merge? scrapbooks?
            // and build the scrapbook from DB to display.

            //cmsViewedProduct objViewProduct = new cmsViewedProduct();
            //if (objViewedProductPage.ProductPageID == null) { }
            //else
            //{

            //    Boolean tempSuccess = await objViewProduct.mergeViewedProduct(visitorDbId,
            //                                                                    objViewedProductPage.ProductPageID,
            //                                                                    objViewedProductPage.masterDefinition.UrlBreakdown.countryDetails.CountryID);
            //}
            //objViewedProductPage = await objViewProduct.buildViewedProduct(visitorDbId, objViewedProductPage);


            return View("~/Views-New/Shared/Components/_scrapbook.cshtml", objScrapbook);
        }

        [ValidateInput(false)]
        public ActionResult MoodboardGridViewPartial(PassMoodboardGridViewModel objMoodboardGVM)
        {
            objMoodboardGVM.composeMoodboard = HtmlHelperExtra.Resource("MoodboardViewEdit", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
            objMoodboardGVM.viewMoodboard = HtmlHelperExtra.Resource("MoodboardView", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
            objMoodboardGVM.orderSamples = HtmlHelperExtra.Resource("MoodboardOrderSamples", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
            objMoodboardGVM.share = HtmlHelperExtra.Resource("MoodboardShare", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
            objMoodboardGVM.delete = HtmlHelperExtra.Resource("Delete", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                _ = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            int? memberId;
            if (Session["memberIdFromDatabase"] == null)
            {
                String urlContent = dgFramework.buildUrl(objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails,
                                                    null,
                                                    dgContentType.landingPage,
                                                    (int)dgFixedPage.memberLogin,
                                                    Request.QueryString["dateOverride"],
                                                    Request.QueryString["countryOverride"]);
                if (Request.QueryString["dateOverride"] == null && Request.QueryString["countryOverride"] == null)
                {
                    urlContent = String.Format("{0}?redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
                }
                else
                {
                    urlContent = String.Format("{0}&redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
                }

                //ASPxWebControl.RedirectOnCallback(urlContent);

                objMoodboardGVM.moodboards = new List<Moodboard>();

                Session["loginStatus"] = dgLoginStatus.notLoggedIn;

                return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }
            if (memberId.HasValue)
            {
                _ = new PassMoodboardGridViewModel();

                var visitorFactory = new VisitorFactory();

                var res = visitorFactory.searchMoodboardsAsync(memberId.Value, null, null, null, objMoodboardGVM.CountryID, objMoodboardGVM.LanguageID, null);

                objMoodboardGVM.rooms = res.Rooms;
                objMoodboardGVM.moodboards = res.Moodboards;

                return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
            }
            else
            {
                String urlContent = dgFramework.buildUrl(objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails,
                                                    null,
                                                    dgContentType.landingPage,
                                                    (int)dgFixedPage.memberLogin,
                                                    Request.QueryString["dateOverride"],
                                                    Request.QueryString["countryOverride"]);
                if (Request.QueryString["dateOverride"] == null && Request.QueryString["countryOverride"] == null)
                {
                    urlContent = String.Format("{0}?redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
                }
                else
                {
                    urlContent = String.Format("{0}&redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
                }

                //ASPxWebControl.RedirectOnCallback(urlContent);

                objMoodboardGVM.moodboards = new List<Moodboard>();
                return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
            }
        }


        //public async Task<ActionResult> MoodboardGridViewUpdate([ModelBinder(typeof(DevExpressEditorsBinder))]moodboardGridViewSave objMoodboard)
        //{
        //    var objMoodboardGVM = new passMoodboardGridViewModel
        //    {
        //        masterDefinition = JsonConvert.DeserializeObject<masterDefinition>(objMoodboard.masterDefString)
        //    };

        //    objMoodboardGVM.composeMoodboard = HtmlHelperExtra.Resource("MoodboardViewEdit", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
        //    objMoodboardGVM.orderSamples = HtmlHelperExtra.Resource("MoodboardOrderSamples", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
        //    objMoodboardGVM.share = HtmlHelperExtra.Resource("MoodboardShare", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();
        //    objMoodboardGVM.delete = HtmlHelperExtra.Resource("Delete", objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.LanguageID, objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails.CountryID, objMoodboardGVM.masterDefinition.UrlBreakdown.ContentDate).ToString();

        //    int? memberId;
        //    if (Session["memberIdFromDatabase"] == null)
        //    {
        //        String urlContent = dgFramework.buildUrl(objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails,
        //                                            null,
        //                                            dgContentType.landingPage,
        //                                            (int)dgFixedPage.memberLogin,
        //                                            Request.QueryString["dateOverride"],
        //                                            Request.QueryString["countryOverride"]);
        //        if (Request.QueryString["dateOverride"] == null && Request.QueryString["countryOverride"] == null)
        //        {
        //            urlContent = String.Format("{0}?redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
        //        }
        //        else
        //        {
        //            urlContent = String.Format("{0}&redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
        //        }

        //        ASPxWebControl.RedirectOnCallback(urlContent);

        //        objMoodboardGVM.moodboards = new List<moodboard>();

        //        Session["loginStatus"] = dgLoginStatus.notLoggedIn;

        //        return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
        //    }
        //    else
        //    {
        //        memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
        //    }

        //    if (memberId.HasValue)
        //    {
        //        var objVisitorFactory = new VisitorFactory();

        //        if (ModelState.IsValid)
        //        {
        //            await objVisitorFactory.saveMoodboardFromGridView(objMoodboard.MoodboardID, objMoodboard.CustomerName, objMoodboard.RoomID, objMoodboard.Reference);
        //        }


        //        var res = objVisitorFactory.searchMoodboardsAsync(memberId.Value, null, null, null, objMoodboard.CountryID, objMoodboard.LanguageID, null);

        //        objMoodboardGVM.rooms = res.rooms;
        //        objMoodboardGVM.moodboards = res.moodboards;

        //        return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
        //    }
        //    else
        //    {
        //        String urlContent = dgFramework.buildUrl(objMoodboardGVM.masterDefinition.UrlBreakdown.countryDetails,
        //                                            null,
        //                                            dgContentType.landingPage,
        //                                            (int)dgFixedPage.memberLogin,
        //                                            Request.QueryString["dateOverride"],
        //                                            Request.QueryString["countryOverride"]);
        //        if (Request.QueryString["dateOverride"] == null && Request.QueryString["countryOverride"] == null)
        //        {
        //            urlContent = String.Format("{0}?redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
        //        }
        //        else
        //        {
        //            urlContent = String.Format("{0}&redirect={1}", urlContent, ((int)dgFixedPage.moodboard).ToString());
        //        }

        //        ASPxWebControl.RedirectOnCallback(urlContent);

        //        objMoodboardGVM.moodboards = new List<moodboard>();

        //        Session["loginStatus"] = dgLoginStatus.notLoggedIn;

        //        return PartialView(dgFramework.GetPreferredDeviceView("~/Views/Moodboards/_MoodboardGridViewPartial{0}.cshtml"), objMoodboardGVM);
        //    }
        //}

        #endregion

        #region Trade

        //public async Task<ActionResult> _tradeAccountRequest(PassTradeAccountRequestForm objRequest)
        //{
        //    cmsTradeAccountRequest objTradeAccountRequest = new cmsTradeAccountRequest();

        //    var objResult = await objTradeAccountRequest.loadTradeAccountRequestFormFromCms(objRequest.masterDefinition);

        //    objResult.heading = objRequest.heading;
        //    objResult.subHeading = objRequest.subHeading;
        //    objResult.description = objRequest.description;
        //    objResult.immediateOrderRequested = false;
        //    if (Int64.TryParse(Request.QueryString["id"], out _))
        //    {
        //        objResult.leadId = Convert.ToInt64(Request.QueryString["id"]);

        //        objResult.immediateOrderRequested = await objTradeAccountRequest.isImmediateOrderFlagSet(objResult.leadId.Value);
        //    }

        //    //return View(dgFramework.GetPreferredDeviceView("~/Views/Trade/_tradeAccountRequest{0}.cshtml"), objResult);
        //    return View("~/Views/Trade/_tradeAccountRequest.cshtml", objResult);
        //}

        public ActionResult _tradeAccountEmailAddressNotFound(masterDefinition objMaster)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_tradeAccountEmailAddressNotFound{0}.cshtml"), objMaster);
        }

        public async Task<ActionResult> _tradeRegistration(masterDefinition objMaster)
        {
            // Right, here we need to consume a URL parameter and determine whether or not to permit the user to see the page.
            var Model = new passTradeRegistration
            {
                masterDefinition = objMaster
            };

            if (Request.QueryString["a"] == null)
            {
                return PartialView("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }
            else
            {
                try
                {
                    var authGuid = new Guid();

                    if (Guid.TryParse(Request.QueryString["a"], out authGuid))
                    {
                        VisitorFactory visitorFactory = new VisitorFactory();

                        var result = await visitorFactory.callMemberTradeAuthCommence(authGuid);

                        Model.data = result;

                        Model.guid = authGuid;

                        if (!result.Result)
                        {
                            return PartialView("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
                        }
                    }
                    else
                    {
                        return PartialView("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
                    }
                }
                catch (Exception)
                {
                    return PartialView("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
                }
            }

            return View("~/Views/Trade/_tradeRegistration.cshtml", Model);
        }

        #endregion


        #region Members


        public ActionResult _memberExists(masterDefinition objMaster)
        {
            return View(dgFramework.GetPreferredDeviceView("~/Views-New/Shared/Components/_memberExists{0}.cshtml"), objMaster);
        }


        public ActionResult _memberRegistration(masterDefinition objMaster)
        {
            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString())
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }
            else
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/Member/_memberRegistration{0}.cshtml"), objMaster);
            }

        }


        public async Task<ActionResult> _memberLogin(masterDefinition objMaster)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            var cmsCountry = new cmsCountry();
            var countrySelectResult = await cmsCountry.countrySelectList(null, null);
            var languageSelectResult = await cmsCountry.loadCompleteLanguageList(objMaster.UrlBreakdown.countryDetails.CountryID, useDate);

            passMemberLogin objReturn = new passMemberLogin
            {
                masterDefinition = objMaster,
                countries = countrySelectResult.countries,
                languages = languageSelectResult.languages
            };

            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString())
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }
            else
            {
                //return View(dgFramework.GetPreferredDeviceView("~/Views/Member/_memberLogin{0}.cshtml"), objReturn);
                return View("~/Views/Member/_memberLogin.cshtml", objReturn);
            }
        }


        public ActionResult _memberMenu(masterDefinition objMaster)
        {

            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString()) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpMenu.cshtml", objMaster);
            }
            return View("~/Views/Member/_memberMenu.cshtml", objMaster);
        }


        public async Task<ActionResult> _memberAddress(masterDefinition objMaster)
        {

            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString()) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpAddress.cshtml", objMaster);
            }

            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }


            //  For the member address book, get all addresses
            VisitorFactory visitorFactory = new VisitorFactory();
            MemberAddressSelect objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, null, objMaster.UrlBreakdown.countryDetails.LanguageID, null, objMaster.UrlBreakdown.ContentDate);


            //  Get the country information
            cmsCountry objCountry = new cmsCountry();
            cmsCountryPriceListDefinition objCountryData = await objCountry.countrySelectList(null, objMaster.UrlBreakdown.countryDetails.LanguageID);


            passMemberAddress objReturn = new passMemberAddress
            {
                masterDefinition = objMaster,
                countyItems = objCountryData.counties,
                countryItems = objCountryData.countries,
                memberAddressSelects = objMemberAddressSelect
            };

            return View(dgFramework.GetPreferredDeviceView("~/Views/Member/_memberAddress{0}.cshtml"), objReturn);
        }


        public async Task<ActionResult> _memberEdit(masterDefinition objMaster)
        {

            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString()) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpEdit.cshtml", objMaster);
            }

            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null) { }
                else
                {
                    String overrideDate = Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }
            
            var visitorFactory = new VisitorFactory();
            var memberStore = await visitorFactory.memberDetails(visitorDbId, useDate);

            var cmsCountry = new cmsCountry();
            var countrySelectResult = await cmsCountry.countrySelectList(null, null);
            var languageSelectResult = await cmsCountry.loadCompleteLanguageList(memberStore.MemberDetail.MarketingCountryID, useDate);

            passMemberEdit objReturn = new passMemberEdit
            {
                masterDefinition = objMaster,
                memberDetail = memberStore.MemberDetail,
                countries = countrySelectResult.countries,
                languages = languageSelectResult.languages
            };

            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }
            
            formRoutines objFormRoutines = new formRoutines();
            var result = await objFormRoutines.marketingPermissionCheck(new webapiMarketingPermissionCheck()
            {
                VisitorID = visitorDbId,
                MemberID = memberId,
                Email = objReturn.memberDetail.Email,
                AddressRequired = true,
                CountryID = objMaster.UrlBreakdown.countryDetails.CountryID,
                LanguageID = objMaster.UrlBreakdown.countryDetails.LanguageID,
                SourceID = 50,
                MarketingCountryID = objReturn.memberDetail.MarketingCountryID,
                ThirdPartyID = null
            });

            objReturn.marketingPermissions = result;
            if (Session["memberTradeChecksPassed"] != null)
            {
                _ = Convert.ToBoolean(Session["memberTradeChecksPassed"]);
            }
            
            return View("~/Views/Member/_memberEdit.cshtml", objReturn);
        }

        public async Task<ActionResult> _memberOrderHistory(masterDefinition objMaster)
        {
            //  Determine the date to obtain the data from the cms
            DateTime useDate = DateTime.Now;
            useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
            if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
            else
            {
                //  Should the "use date" be overriden?
                if (Request.QueryString["dateOverride"] == null)
                {
                    useDate = DateTime.Now;
                }
                else
                {
                    String overrideDate = Request.QueryString["dateOverride"].ToString();
                    if (overrideDate.Length == "ddMMyyyy".Length)
                    {
                        bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
                                                                              "ddMMyyyy",
                                                                              CultureInfo.InvariantCulture,
                                                                              DateTimeStyles.None,
                                                                              out useDate);
                        if (dateConvertSuccess == true) { }
                        else
                        {
                            useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                        }
                    }
                }
            }

            if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString()) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToLoginJumpOrderHistory.cshtml", objMaster);
            }


            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }


            String defaultSearchString = null;
            short? defaultHistoryDays = 182;
            byte? defaultOrderStatus = 99;

            VisitorFactory visitorFactory = new VisitorFactory();
            OrderHistory orderHistoryData = await visitorFactory.getOrderHistory(visitorDbId, objMaster.UrlBreakdown.countryDetails,
                                                                                    defaultSearchString, defaultHistoryDays, defaultOrderStatus, Request.QueryString["dateOverride"]);
            MemberAddressSelect objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, null, objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, useDate);


            cmsCountry objCountry = new cmsCountry();
            cmsCountryPriceListDefinition objCountryData = await objCountry.countrySelectList(null, objMaster.UrlBreakdown.countryDetails.LanguageID);


            passMemberOrderHistory objReturn = new passMemberOrderHistory
            {
                masterDefinition = objMaster,
                memberOrderHistory = orderHistoryData,
                memberAddressSelects = objMemberAddressSelect,
                countyItems = objCountryData.counties,
                countryItems = objCountryData.countries
            };

            return View("~/Views/Member/_memberOrderHistory.cshtml", objReturn);
        }


        #endregion


        #region Checkout

        public async Task<ActionResult> _checkoutPayPalLoginResponse()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            passPayPalResponse objReturn = new passPayPalResponse();

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                objReturn.checkoutValid = false;
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                objReturn.isExpired = true;
            }
            else if (string.IsNullOrEmpty(Request["code"]))
            {
                objReturn.checkoutValid = false;
            }
            {
                objReturn.isExpired = false;

                string clientId = "AUTbrShaelnWFe1Qx6u-L0qxWouKH_7gkpHUh5aCDZ5LjpDkiPhI41PAvIWAcaYlDuBAoLARRZjKV8Mz";
                string appSecret = "EPEO7Em18OklpdETIB-XvvDn4SwJOOHVn-fOvliIK2JmpHaSfPg3suJGvEGtwKxbXMwM1_xIvJWXInC5";
                string code = Request["code"];

                try
                {
                    using (var client = new HttpClient())
                    {
                        var bytes = Encoding.UTF8.GetBytes($"{clientId}:{appSecret}");
                        var credentials = Convert.ToBase64String(bytes);

                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
                        var content = new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("code", code)
                        };

                        var response = await client.PostAsync("https://api.sandbox.paypal.com/v1/identity/openidconnect/tokenservice", new FormUrlEncodedContent(content));
                        var responseContent = await response.Content.ReadAsStringAsync();
                        JObject objResponse = JObject.Parse(responseContent);

                        Session["PayPalAccessToken"] = objResponse["access_token"].Value<string>();
                        objReturn.checkoutValid = true;
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("CheckoutID: {0} - {1} ", Session["checkoutId"].ToString(), "Unable to authenticate PayPal - " + ex.Message);
                    Logger.Error(message);

                    objReturn.checkoutValid = false;
                }
            }

            return View("/Views-New/LandingPages/FixedPages/Checkout/PaymentSections/_CheckoutPayPalResponse.cshtml", objReturn);
        }

        //public async Task<ActionResult> _checkoutLogin(masterDefinition objMaster)
        //{
        //    DateTime snapshotCurrentDateTime = DateTime.Now;
        //    DateTime useDate = DateTime.Now;
        //    useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
        //    if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
        //    else
        //    {
        //        //  Should the "use date" be overriden?
        //        if (Request.QueryString["dateOverride"] == null)
        //        {
        //            useDate = DateTime.Now;
        //            snapshotCurrentDateTime = useDate;
        //        }
        //        else
        //        {
        //            String overrideDate = Request.QueryString["dateOverride"].ToString();
        //            if (overrideDate.Length == "ddMMyyyy".Length)
        //            {
        //                bool dateConvertSuccess = DateTime.TryParseExact(overrideDate,
        //                                                                      "ddMMyyyy",
        //                                                                      CultureInfo.InvariantCulture,
        //                                                                      DateTimeStyles.None,
        //                                                                      out useDate);
        //                if (dateConvertSuccess == true)
        //                {
        //                    snapshotCurrentDateTime = useDate;
        //                }
        //                else
        //                {
        //                    useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        //                }
        //            }
        //        }
        //    }

        //    // call basket_validate
        //    // if valid, call the beneath to construct a checkout if there isn't one in session already...

        //    var objCheckout = new checkoutDb();

        //    long visitorDbId = -1;
        //    int? memberId = null;
        //    int? tradeAccountId = null;

        //    if (Session["visitorIdFromDatabase"] != null)
        //    {
        //        visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
        //    }

        //    if (Session["memberIdFromDatabase"] != null)
        //    {
        //        memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
        //    }

        //    if (Session["memberTradeAccountID"] != null)
        //    {
        //        tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"].ToString());
        //    }

        //    var basketPreflightResult = await objCheckout.validateBasket(visitorDbId, 
        //        memberId,
        //        tradeAccountId,
        //        objMaster.UrlBreakdown.countryDetails.LanguageID,
        //        objMaster.UrlBreakdown.countryDetails.CountryID,
        //        objMaster.UrlBreakdown.ContentDate,
        //        Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"]));

        //    decimal? subTotal = null;

        //    if (basketPreflightResult.JSON == "{}")
        //    {
        //        //  Is this checkout linked to a linkshare request
        //        DateTime? linkshareDate = null;
        //        String linkshareId = null;
        //        String cookieName = null;

        //        if (objMaster.UrlBreakdown.countryDetails.CountryID == 81)
        //        {
        //            cookieName = "_linkshare_uk";
        //        }

        //        if (objMaster.UrlBreakdown.countryDetails.CountryID == 238)
        //        {
        //            cookieName = "_linkshare_us";
        //        }

        //        if (cookieName == null) { }
        //        else
        //        {
        //            HttpCookie cookie = Request.Cookies[cookieName];
        //            if (cookie != null)
        //            {
        //                linkshareId = cookie.Value;
        //                linkshareDate = snapshotCurrentDateTime;
        //            }
        //        }

        //        //  Create the snapshot basket
        //        var objSnapshot = await objCheckout.createCheckoutBasket(
        //            visitorDbId,
        //            objMaster.UrlBreakdown.countryDetails.LanguageID, 
        //            objMaster.UrlBreakdown.countryDetails.CountryID,
        //            snapshotCurrentDateTime,
        //            (string)Session["CF-IPCountry"],
        //            tradeAccountId);

        //        if (objSnapshot == null)
        //        {
        //            dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
        //        }

        //        //  Set the resource file language (so the session["checkoutExpiry"] is in the local display language)
        //        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objMaster.UrlBreakdown.countryDetails.DotNetCulture);
        //        Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        //        //  If okay, generate basket, store in Session
        //        Session["checkoutId"] = objSnapshot.CheckoutID;

        //        objSnapshot.CheckoutExpiryDT = objSnapshot.CheckoutExpiryDT.ToUniversalTime();

        //        subTotal = objSnapshot.SubTotal;

        //        Session["checkoutExpiry"] = objSnapshot.CheckoutExpiryDT;//.ToString("dd MMMM yyyy HH:mm");
        //        Session["checkoutDisplayGiftTab"] = objSnapshot.CheckoutDisplayGiftScreen;
        //        Session["checkoutDisplayGiftVoucherTab"] = objSnapshot.CheckoutDisplayGiftVoucherScreen;
        //    }

        //    var objReturn = new passLogin();

        //    objReturn.subTotal = subTotal;

        //    if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
        //    {
        //        return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //    }
        //    else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
        //    {
        //        return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
        //    }
        //    else
        //    {
        //        Int64 checkoutId = (Int64)Session["checkoutId"];
        //        if (await objCheckout.preflightCheckWelcome(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID) == false)
        //        {
        //            return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //        }
        //        else if (await objCheckout.checkoutTypeIsSet(checkoutId))
        //        {
        //            // do not allow the customer to reset the checkout type, a new checkout must be created.
        //            return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //        }

        //        // Need to know whether or not we're going to limit the display to Trade only.
        //        // This must be done if:
        //        // They're a soft or hard signed in trade member
        //        // We permit trade ordering on the given site
        //        var onlyShowTrade = false;

        //        if (Session["loginStatus"].ToString() == dgLoginStatus.currentLogin.ToString() ||
        //            Session["loginStatus"].ToString() == dgLoginStatus.previousLogin.ToString())
        //        {
        //            // great, we're either soft or hard signed in.
        //            // are we a trade user?
                    
        //            if (Session["memberIdFromDatabase"] == null) { }
        //            else
        //            {
        //                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
        //            }

        //            if (memberId != null)
        //            {
        //                if (Session["memberTradeAccountID"] == null) { }
        //                else
        //                {
        //                    onlyShowTrade = true;
        //                }
        //            }
        //        }

        //        //  Start of checkout process
        //        objReturn.masterDefinition = objMaster;
        //        var countryDetails = objMaster.UrlBreakdown.countryDetails;
        //        objReturn.checkoutTypes = await objCheckout.GetCheckoutTypes(countryDetails.CountryID, countryDetails.LanguageID, useDate, dgFramework.IsMobileDevice());
                
        //        var merchantDetailSelect = await objCheckout.getMerchantDetails(checkoutId);
        //        var merchantDetails = merchantDetailSelect.checkoutParameters;                    
        //        objReturn.currency = merchantDetails.Currency;

        //        if (objReturn.checkoutTypes.Any(c => c.CheckoutTypeId == Convert.ToByte(dgCheckoutType.Paypal))) {
        //            objReturn.braintreeEnvironment = merchantDetails.Environment;
        //            objReturn.provisionalTotal = 0.01M;

        //            var gateway = new BraintreeGateway()
        //            {
        //                Environment = Braintree.Environment.ParseEnvironment(merchantDetails.Environment),
        //                MerchantId = merchantDetails.MerchantID,
        //                PublicKey = merchantDetails.PublicKey,
        //                PrivateKey = merchantDetails.PrivateKey
        //            };

        //            objReturn.braintreeClientKey = gateway.ClientToken.Generate(
        //                new ClientTokenRequest()
        //                {
        //                    MerchantAccountId = merchantDetails.MerchantAccountID
        //                }
        //            );
        //        }
                
        //        objReturn.amazonClientId = merchantDetails.AmazonClientId;
        //        objReturn.amazonMerchantId = merchantDetails.AmazonMerchantId;
        //        objReturn.amazonClientUrl = merchantDetails.AmazonPayAPIURL;
        //        objReturn.acceptAmex = merchantDetails.AcceptAmex;
        //        objReturn.acceptDiscover = merchantDetails.AcceptDiscover;
        //        objReturn.acceptMaestro = merchantDetails.AcceptMaestro;
        //        objReturn.acceptMaestroUK = merchantDetails.AcceptMaestroUK;
        //        objReturn.acceptMasterCard = merchantDetails.AcceptMasterCard;
        //        objReturn.acceptVisa = merchantDetails.AcceptVisa;
        //        objReturn.onlyShowTrade = onlyShowTrade;
        //        objReturn.tradePermissionPlaceOrder = false;

        //        if (onlyShowTrade)
        //        {
        //            var memberID = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());

        //            var vf = new VisitorFactory();

        //            var tradeAccountModel = await vf.callTradeMemberSelect(
        //                    Convert.ToInt32(Session["memberTradeAccountID"].ToString()),
        //                    memberID,
        //                    objMaster.UrlBreakdown.countryDetails.CountryID,
        //                    objMaster.UrlBreakdown.countryDetails.LanguageID);

        //            if (Session["loginStatus"].ToString() != dgLoginStatus.previousLogin.ToString())
        //            {
        //                objReturn.tradeAccountModel = tradeAccountModel;

        //                var user = tradeAccountModel.Users.Where(p => p.MemberID == memberID).SingleOrDefault();
        //                var admin = tradeAccountModel.Administrators.Where(p => p.MemberID == memberID).SingleOrDefault();

        //                objReturn.tradePermissionPlaceOrder = user?.TradePermissionPlaceOrder ?? admin?.TradePermissionPlaceOrder ?? false;
        //            }
        //        }


        //        //var vf = new VisitorFactory();

        //        //var memberStore = await vf.memberDetails(Convert.ToInt64(Session["visitorIdFromDatabase"].ToString()), useDate);

        //        //memberStore.

        //        //objReturn.promoCode = merchantDetailSelect.promoCodes.Select(p => p.PromoCode).FirstOrDefault();

        //        cmsCountry objCountry = new cmsCountry();
        //        cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(DateTime.Now);
        //        var availableCountries = await objCountry.countrySelectList(countryDetails.CountryID, countryDetails.LanguageID);
        //        objReturn.checkoutCountries = availableCountries.countries.Select(c => c.Name).ToList();                

        //        //return View(dgFramework.GetPreferredDeviceView("~/Views/Checkout/_checkoutLogin{0}.cshtml"), objReturn);
        //        return View("~/Views/Checkout/_checkoutLogin.cshtml", objReturn);
        //    }
        //}

        //public async Task<ActionResult> _checkoutSinglePage(BaseViewModel objBase)
        //{
        //    var objMaster = objBase.MasterDefinition;

        //    //  Single page checkout page

        //    checkoutDb objCheckout = new checkoutDb();

        //    // You will have had a checkoutId generated if you hit _checkoutLogin first
        //    if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
        //    {
        //        return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //    }
        //    else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
        //    {
        //        return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
        //    }
        //    else
        //    {
        //        Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

        //        // Run the all-important address preflight check. This should be renamed.
        //        var preflightCheckAddressResults = await objCheckout.preflightCheckAddress(Convert.ToInt64(Session["checkoutId"].ToString()), objMaster.UrlBreakdown.countryDetails.CountryID);

        //        if (!preflightCheckAddressResults.Passed)
        //        {
        //            return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //        }
        //        else
        //        {
        //            var visitorFactory = new VisitorFactory();

        //            var objCountry = new cmsCountry();

        //            var objMemberAddressSelect = new MemberAddressSelect();

        //            var tradeAccountNotification = new tradeAccountNotification();
                    
        //            var checkoutType = await objCheckout.GetCheckoutType(checkoutId);

        //            Int64 visitorDbId = -1;

        //            int? memberId = null;
                    
        //            Int32? tradeAccountId = null;

        //            string tradeAccountNumber = null;

        //            var languageId = objMaster.UrlBreakdown.countryDetails.LanguageID;
        //            var countryId = objMaster.UrlBreakdown.countryDetails.CountryID;
        //            var contentDate = objMaster.UrlBreakdown.ContentDate;
                    
        //            if (Session["memberTradeAccountNumber"] != null)
        //            {
        //                tradeAccountNumber = Session["memberTradeAccountNumber"].ToString();
        //            }

        //            if (Session["visitorIdFromDatabase"] != null)
        //            {
        //                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
        //            }

        //            //if (dgFramework.isUserLoggedIn(Session["loginStatus"]) && Session["memberIdFromDatabase"] != null && (checkoutType == dgCheckoutType.Member || checkoutType == dgCheckoutType.Trade))
        //            //{
        //            //    memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
        //            //}
                    
        //            if (Session["guestCheckoutMemberId"] != null)
        //            {
        //                // pub.Checkout_Address_Generate does not love the guestCheckoutMemberId
        //                memberId = null;// (int?)Session["guestCheckoutMemberId"];
        //            }
        //            else
        //            {
        //                if (Session["memberIdFromDatabase"] == null)
        //                {
        //                }
        //                else
        //                {
        //                    memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
        //                }
        //            }

        //            if (Session["memberTradeAccountID"] != null)
        //            {
        //                tradeAccountId = Convert.ToInt32(Session["memberTradeAccountID"].ToString());
        //            }
                    
        //            //  Get the delivery options
        //            var objDeliveryOptions = await objCheckout.checkoutAddressGenerate(
        //                checkoutId,
        //                objMaster.UrlBreakdown.countryDetails.CountryID,
        //                objMaster.UrlBreakdown.countryDetails.LanguageID,
        //                visitorDbId,
        //                memberId,
        //                tradeAccountId);

        //            Session["checkoutDisplayGiftTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftScreen;
        //            Session["checkoutDisplayGiftVoucherTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftVoucherScreen;

        //            //  For the checkout address book, get all addresses within the current country price list
                   
        //            if (checkoutType == dgCheckoutType.Trade)
        //            {
        //                var basketPreflightResult = await objCheckout.validateBasket(visitorDbId,
        //                    memberId,
        //                    tradeAccountId,
        //                    objMaster.UrlBreakdown.countryDetails.LanguageID,
        //                    objMaster.UrlBreakdown.countryDetails.CountryID,
        //                    objMaster.UrlBreakdown.ContentDate,
        //                    Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"]));

        //                if (basketPreflightResult.JSON != "{}")
        //                {
        //                    return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        //                }
        //            }


        //            if (checkoutType == dgCheckoutType.Trade || checkoutType == dgCheckoutType.Member)
        //            {
        //                objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, null, objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate);

        //                if (checkoutType == dgCheckoutType.Trade && objMemberAddressSelect.TradeAddressErrored)
        //                {
        //                    Logger.Error($"_checkoutAddress: Error loading trade member addresses - visitor ID {visitorDbId}");
        //                }
        //            }
        //            else
        //            {
        //                objMemberAddressSelect.Addresses = new List<MemberAddress>();
        //                objMemberAddressSelect.TradeAddresses = new List<MemberTradeAddress>();
        //            }


        //            //// set the default address within the addressbook
        //            // defunct: default address is now being autosaved into checkoutAddress table
        //            //var defaultAddress = objDeliveryOptions.defaultAddress.FirstOrDefault();

        //            //if (defaultAddress != null)
        //            //{
        //            //    // clear any default already set as it is unreliable
        //            //    // ... why is it unreliable? - Tom 26/11/2018
        //            //    objMemberAddressSelect.memberAddresses.ForEach(a => a.IsDefault = false);

        //            //    var makeDefault = objMemberAddressSelect.memberAddresses.FirstOrDefault(a => a.MemberAddressID == defaultAddress.MemberAddressID);
        //            //    if (makeDefault != null)
        //            //        makeDefault.IsDefault = true;

        //            //    // bring the default address to the front of the list.
        //            //    objMemberAddressSelect.memberAddresses = objMemberAddressSelect.memberAddresses.OrderByDescending(a => a.IsDefault).ToList();
        //            //}

        //            //  Get the country information (within the current price list)
        //            var objCountryData = await objCountry.countrySelectList(objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID);
                    
        //            var merchantDetailSelect = await objCheckout.getMerchantDetails(checkoutId);
        //            var merchantDetails = merchantDetailSelect.checkoutParameters;         

        //            #region _checkoutPay model building
                    
        //            var passCheckoutPay = new passCheckoutPayment();
                    
        //            var objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID, "SectionController._checkoutSinglePage");

        //            decimal grandTotal = 0.0M;
        //            if (objCheckoutPayments.paymentTotals.Count == 0) { }
        //            else
        //            {
        //                grandTotal = (decimal)objCheckoutPayments.paymentTotals[0].TotalBalance;
        //            }

        //            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objMaster.UrlBreakdown.countryDetails.DotNetCulture);
        //            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

        //            passCheckoutPay.checkoutMethod = checkoutType;

        //            if (Session["guestCheckoutMemberId"] != null)
        //            {
        //                // memberDetails must be pulled with a memberId!
        //                // because of the need to set null to the pub.Checkout_Address_Generate earlier
        //                memberId = (int?)Session["guestCheckoutMemberId"];
        //            }

        //            var memberStore = await visitorFactory.memberDetails(visitorDbId, objMaster.UrlBreakdown.ContentDate, memberId);

        //            formRoutines objFormRoutines = new formRoutines();

        //            // the paypal_method_nonce might have been cleared by our sales tax delivery address confirmation modal being dismissed without selection
        //            if (passCheckoutPay.checkoutMethod == dgCheckoutType.Paypal && Session["paypal_nonce_" + checkoutId.ToString()] == null)
        //            {
        //                // if this is the case, we no longer wish to drop the user to the basket: instead we force them to select an address again.

        //                //return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
        //            }

        //            var marketingPermissions = await objFormRoutines.marketingPermissionCheck(new webapiMarketingPermissionCheck()
        //            {
        //                VisitorID = visitorDbId,
        //                MemberID = memberId,
        //                AddressRequired = true,
        //                CountryID = objMaster.UrlBreakdown.countryDetails.CountryID,
        //                LanguageID = objMaster.UrlBreakdown.countryDetails.LanguageID,
        //                Email = memberStore.MemberDetail.Email,
        //                SourceID = 25,
        //                MarketingCountryID = memberStore.MemberDetail.MarketingCountryID,
        //                ThirdPartyID = null
        //            });

        //            passCheckoutPay.masterDefinition = objMaster;
        //            passCheckoutPay.checkoutPayments = objCheckoutPayments;
        //            passCheckoutPay.marketingPermissions = marketingPermissions;
        //            passCheckoutPay.isMemberCheckout = passCheckoutPay.checkoutMethod == dgCheckoutType.Member;

        //            var savedCards = new List<CheckoutBraintreeCreditCardDetails>();
        //            if (passCheckoutPay.checkoutMethod == dgCheckoutType.Guest || passCheckoutPay.checkoutMethod == dgCheckoutType.Member || passCheckoutPay.checkoutMethod == dgCheckoutType.Paypal)
        //            {
        //                var gateway = new BraintreeGateway()
        //                {
        //                    Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
        //                    MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
        //                    PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
        //                    PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
        //                };

        //                var tokenRequest =
        //                    new ClientTokenRequest()
        //                    {
        //                        MerchantAccountId = objCheckoutPayments.checkoutDetails[0].MerchantAccountID
        //                    };

        //                if (passCheckoutPay.checkoutMethod == dgCheckoutType.Member)
        //                {
        //                    string searchCustomerId = memberId.ToString();
        //                    string customerId;
        //                    try
        //                    {
        //                        Customer customer = gateway.Customer.Find(searchCustomerId);
        //                        customerId = customer.Id;
        //                    }
        //                    catch (Braintree.Exceptions.NotFoundException)
        //                    {
        //                        // if Customer.FindAsync fails to find a customer it throws an exception, we need to create a new Braintree customer to be able to save any card tokens against
        //                        CustomerRequest newCustomer = new CustomerRequest()
        //                        {
        //                            Id = searchCustomerId,
        //                            Email = memberStore.MemberDetail.Email,
        //                            FirstName = memberStore.MemberDetail.FullName
        //                        };
        //                        var customer = gateway.Customer.Create(newCustomer);
        //                        if (customer.IsSuccess())
        //                            customerId = customer.Target.Id;
        //                        else
        //                            customerId = null;
        //                    }
        //                    catch (Exception)
        //                    {
        //                        customerId = null;
        //                    }
        //                    tokenRequest.CustomerId = customerId;
        //                }

        //                passCheckoutPay.braintreeClient = gateway.ClientToken.Generate(
        //                    tokenRequest
        //                );

        //                // load any cards in the Braintree vault for this customer
        //                if (passCheckoutPay.checkoutMethod == dgCheckoutType.Member)
        //                {
        //                    try
        //                    {
        //                        Customer cust = gateway.Customer.Find(memberId.ToString());

        //                        foreach (CreditCard card in cust.CreditCards)
        //                        {
        //                            var newCard = new CheckoutBraintreeCreditCardDetails()
        //                            {
        //                                cardType = card.CardType.ToString(),
        //                                endDigits = card.LastFour,
        //                                expiry = card.ExpirationDate,
        //                                defaultCard = card.IsDefault ?? false,
        //                                vaultToken = card.Token
        //                            };

        //                            if (objCheckoutPayments.checkoutDetails[0].Mandate3DSecure)
        //                            {
        //                                newCard.token = card.Token;
        //                            }
        //                            else
        //                            {
        //                                var token = gateway.PaymentMethodNonce.Create(card.Token);

        //                                if (token.IsSuccess())
        //                                    newCard.token = token.Target.Nonce;
        //                                else
        //                                    continue;
        //                            }

        //                            /* the token that is stored in the vault cannot have 3d secure applied to it by default, to be able to do this
        //                               we need to use it to create a new payment nonce. */
        //                            /*var token = gateway.PaymentMethodNonce.Create(card.Token);

        //                            if (token.IsSuccess())
        //                                newCard.token = token.Target.Nonce;
        //                            else
        //                                continue;*/

        //                            bool expired = true;
        //                            if (!string.IsNullOrEmpty(newCard.expiry))
        //                            {
        //                                var dateparts = newCard.expiry.Split('/');
        //                                if (dateparts.Length == 2)
        //                                {
        //                                    var month = Convert.ToInt32(dateparts[0]);
        //                                    var year = Convert.ToInt32(dateparts[1]);

        //                                    expired = (year < DateTime.Now.Year || (year == DateTime.Now.Year && month < DateTime.Now.Month));
        //                                }
        //                            }
        //                            newCard.isExpired = expired;
        //                            if (newCard.isExpired)
        //                                newCard.defaultCard = false;

        //                            savedCards.Add(newCard);
        //                        }
        //                    }
        //                    catch (NotFoundException) { }
        //                    catch (Exception ex)
        //                    {
        //                        var message = string.Format("CheckoutID: {0} - {1} ", checkoutId.ToString(), "Unable to load cards from Braintree vault - " + ex.Message);
        //                        Logger.Error(message);
        //                    }
        //                }

        //                // add any card tokens saved against this basket
        //                var cardTokens = await objCheckout.getPaymentTokens(checkoutId);
        //                var cardsToAppend = cardTokens.Select(t => new CheckoutBraintreeCreditCardDetails()
        //                {
        //                    cardType = t.CardType,
        //                    endDigits = t.LastFourDigits,
        //                    expiry = null,
        //                    token = t.Token,
        //                    bin = t.Bin,
        //                    vaultToken = string.Empty,
        //                    defaultCard = false
        //                });

        //                savedCards.AddRange(cardsToAppend);

        //                // default to the first card if a default isn't set
        //                if (savedCards.Any() && !savedCards.Where(c => c.defaultCard == true).Any())
        //                {
        //                    var defaultCard = savedCards.Where(c => c.isExpired == false).FirstOrDefault();
        //                    if (defaultCard != null)
        //                        defaultCard.defaultCard = true;
        //                }
        //            }

        //            passCheckoutPay.savedCards = savedCards;

        //            #endregion

        //            var checkoutTotals = new passCheckoutTotals();

        //            checkoutTotals.checkoutSummarys = new checkoutTotal
        //            {
        //                checkoutTotals = passCheckoutPay.checkoutPayments.paymentTotals,

        //                checkoutGiftVouchers = await objCheckout.getGiftVouchersInBasket(checkoutId)
        //            };

        //            checkoutTotals.checkoutType = checkoutType;

        //            checkoutTotals.masterDefinition = objMaster;
                    
        //            var productResults = await objCheckout.GetCheckoutProducts(checkoutId);

        //            var viewModel = await FixedPageViewModel_Checkout.CreateAsync(objBase, checkoutId);

        //            return View("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutSinglePage.cshtml", viewModel);
        //        }
        //    }
        //}

        public async Task<ActionResult> _checkoutAddress(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();
            passCheckoutAddress objReturn = new passCheckoutAddress
            {
                tradeAccountMessage = null
            };

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }

            // The beneath will return me a: Boolean to state whether or not the check passed, potentially a Mertex Account Number, and potentially an Email. The latter two are for trade login specific stuff.
            var preflightCheckAddressResults = await objCheckout.preflightCheckAddress(Convert.ToInt64(Session["checkoutId"].ToString()), objMaster.UrlBreakdown.countryDetails.CountryID);

            if (!preflightCheckAddressResults.Passed)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

                dgCheckoutType checkoutMethod = await objCheckout.GetCheckoutType(checkoutId);
                
                Int64 visitorDbId = -1;

                if (Session["visitorIdFromDatabase"] == null)
                {
                }
                else
                {
                    visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
                }

                int? memberId = null;                
                if (dgFramework.isUserLoggedIn(Session["loginStatus"]) && Session["memberIdFromDatabase"] != null && (checkoutMethod == dgCheckoutType.Member || checkoutMethod == dgCheckoutType.Trade))                    
                {
                    memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
                }

                if (Session["memberTradeChecksPassed"] != null)
                {
                    if (Convert.ToBoolean(Session["memberTradeChecksPassed"]) == true)
                    {
                        if (Session["memberTradeAccountID"] != null)
                        {
                            _ = Convert.ToInt32(Session["memberTradeAccountID"].ToString());
                        }
                    }
                }

                //  Get the delivery options
                checkoutAddressGenerateResult objDeliveryOptions = await objCheckout.checkoutAddressGenerate(
                    checkoutId, 
                    objMaster.UrlBreakdown.countryDetails.CountryID, 
                    objMaster.UrlBreakdown.countryDetails.LanguageID,
                    visitorDbId,
                    memberId,
                    tradeAccountId : null);

                Session["checkoutDisplayGiftTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftScreen;
                Session["checkoutDisplayGiftVoucherTab"] = objDeliveryOptions.createSnapshotBaskets[0].CheckoutDisplayGiftVoucherScreen;

                //  For the checkout address book, get all addresses within the current country price list
                VisitorFactory visitorFactory = new VisitorFactory();
                MemberAddressSelect objMemberAddressSelect;
                if (checkoutMethod == dgCheckoutType.Trade || checkoutMethod == dgCheckoutType.Member)
                {
                    objMemberAddressSelect = await visitorFactory.getMemberAddresses(visitorDbId, null, objMaster.UrlBreakdown.countryDetails.LanguageID, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate);

                    if (checkoutMethod == dgCheckoutType.Trade && objMemberAddressSelect.TradeAddressErrored)
                    {
                        Logger.Error($"_checkoutAddress: Error loading trade member addresses - visitor ID {visitorDbId}");
                    }
                } else
                {
                    objMemberAddressSelect = new MemberAddressSelect()
                    {
                        Addresses = new List<MemberAddress>(),
                        TradeAddresses = new List<MemberTradeAddress>()
                    };
                }

                // defunct; defaultAddress is now autosaved into checkoutAddress table
                //// set the default address within the addressbook
                //var defaultAddress = objDeliveryOptions.defaultAddress.FirstOrDefault();             

                //if (defaultAddress == null)
                //{
                //    // do nothing
                //}
                //else
                //{
                //    // clear any default already set as it is unreliable
                //    objMemberAddressSelect.memberAddresses.ForEach(a => a.IsDefault = false);
                    
                //    var makeDefault = objMemberAddressSelect.memberAddresses.FirstOrDefault(a => a.MemberAddressID == defaultAddress.MemberAddressID);
                //    if (makeDefault != null)
                //        makeDefault.IsDefault = true;

                //    // bring the default address to the front of the list.
                //    objMemberAddressSelect.memberAddresses = objMemberAddressSelect.memberAddresses.OrderByDescending(a => a.IsDefault).ToList();
                //}                

                //  Get the country information (within the current price list)
                cmsCountry objCountry = new cmsCountry();
                cmsCountryPriceListDefinition objCountryData = await objCountry.countrySelectList(objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID);                               
                
                objReturn.masterDefinition = objMaster;
                objReturn.deliveryOptions = objDeliveryOptions;
                objReturn.countryItems = objCountryData.countries;
                objReturn.countyItems = objCountryData.counties;
                objReturn.memberAddressSelect = objMemberAddressSelect;
                objReturn.checkoutMethod = checkoutMethod;

                return View("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutAddress.cshtml", objReturn);
            }
        }
        public async Task<ActionResult> _checkoutDespatch(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();
            passCheckoutDispatch objReturn = new passCheckoutDispatch();

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo(objMaster.UrlBreakdown.countryDetails.DotNetCulture);
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

                //  Get the dispatch options
                var stockStatuses = new Models.cmsStockStatus();
                var stockStatusResults = await stockStatuses.loadStockStatusesFromCms(
                    objMaster.UrlBreakdown.countryDetails.LanguageID,
                    objMaster.UrlBreakdown.countryDetails.CountryID,
                    objMaster.UrlBreakdown.ContentDate);

                checkoutDispatchOptions objDispatchOptions = await objCheckout.getDispatchOptions(checkoutId);

                objDispatchOptions.checkoutDispatchStockStatuses = stockStatusResults.StockStatuses;

                if (objDispatchOptions.checkoutDispatchDeliveryOptions.Count == 0)
                {
                    objReturn.dispatchOptionsAvailable = false;
                }
                else
                {
                    objReturn.dispatchOptionsAvailable = true;

                    objReturn.checkoutDispatchOptions = objDispatchOptions;

                    objReturn.checkoutMethod = await objCheckout.GetCheckoutType(checkoutId);
                }
            }

            objReturn.masterDefinition = objMaster;

            return View("~/Views-New/LandingPages/FixedPages/Checkout/DespatchSections/_CheckoutDespatch.cshtml", objReturn);
        }

        public async Task<ActionResult> _checkoutGiftCard(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();
            passCheckoutGiftCard objReturn = new passCheckoutGiftCard();
            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }
            else
            {

                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

                checkoutGiftCard objGiftVoucherData = await objCheckout.getGiftVoucherData(checkoutId);
                if (objGiftVoucherData.checkoutGiftVoucherBaskets.Count == 0)
                    objReturn.displayGiftCards = false;
                else
                    objReturn.displayGiftCards = true;

                //  Build the image URL for the view
                foreach (checkoutGiftVoucherBasket objRow in objGiftVoucherData.checkoutGiftVoucherBaskets)
                {
                    if (objRow.ImageID == null)
                    {
                        objRow.imageUrl = Url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = "-1" });
                    }
                    else
                    {
                        objRow.imageUrl = Url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = objRow.ImageID.ToString() });
                    }
                }


                objReturn.masterDefinition = objMaster;
                objReturn.checkoutGiftVouchers = objGiftVoucherData;
            }

            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/DespatchSections/_CheckoutGiftCard.cshtml", objReturn);
        }


        public async Task<ActionResult> _checkoutGiftWrap(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();
            passCheckoutGift objReturn = new passCheckoutGift();
            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());
                if (Session["memberTradeAccountNumber"] != null)
                {
                    _ = Session["memberTradeAccountNumber"].ToString();
                }

                checkoutGift objGiftData = await objCheckout.getGiftData(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID);

                objReturn.masterDefinition = objMaster;
                objReturn.checkoutGifts = objGiftData;

                return View("~/Views-New/LandingPages/FixedPages/Checkout/DespatchSections/_CheckoutGiftWrap.cshtml", objReturn);
            }
        }


        public async Task<ActionResult> _checkoutTotal(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();

            passCheckoutTotals objReturn = new passCheckoutTotals();

            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }
            else
            {
                Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());
                
                var objPaymentTotals = await objCheckout.checkoutPayGenerate(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID, "SectionController._checkoutTotal");

                objReturn.checkoutSummarys = new checkoutTotal
                {
                    checkoutTotals = objPaymentTotals.paymentTotals,

                    checkoutGiftVouchers = await objCheckout.getGiftVouchersInBasket(checkoutId)
                };

                objReturn.checkoutType = await objCheckout.GetCheckoutType(checkoutId);
                
                objReturn.masterDefinition = objMaster;

                return View("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutTotal.cshtml", objReturn);
            }
        }

        public async Task<ActionResult> _checkoutPay(NewMasterDefinition objMaster)
        {
            checkoutDb objCheckout = new checkoutDb();

            passCheckoutPayment objPayReturn = new passCheckoutPayment();

            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }
            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }

            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());

            string tradeAccountNumber = null;

            if (Session["memberTradeAccountNumber"] != null)
            {
                tradeAccountNumber = Session["memberTradeAccountNumber"].ToString();
            }

            CheckoutPayment objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID, "SectionController._checkoutPay");

            decimal grandTotal = 0.0M;
            if (objCheckoutPayments.paymentTotals.Count == 0) { }
            else
            {
                grandTotal = (decimal)objCheckoutPayments.paymentTotals[0].TotalBalance;
            }

            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(objMaster.UrlBreakdown.countryDetails.DotNetCulture);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            
            objPayReturn.checkoutMethod = await objCheckout.GetCheckoutType(checkoutId);

            var visitorFactory = new VisitorFactory();
            
            Int32? memberId = null;

            if (Session["guestCheckoutMemberId"] != null)
            {
                memberId = (int?)Session["guestCheckoutMemberId"];
            }
            else
            {
                if (Session["memberIdFromDatabase"] == null)
                {
                }
                else
                {
                    memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
                }
            }
            
            var memberStore = await visitorFactory.memberDetails(visitorDbId, objMaster.UrlBreakdown.ContentDate, memberId);
            
            formRoutines objFormRoutines = new formRoutines();           

            if (objPayReturn.checkoutMethod == dgCheckoutType.Paypal && Session["paypal_nonce_" + checkoutId.ToString()] == null)
            {
                //return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }

            var marketingPermissions = await objFormRoutines.marketingPermissionCheck(new webapiMarketingPermissionCheck()
                {
                    VisitorID = visitorDbId,
                    MemberID = memberId,
                    AddressRequired = false,
                    CountryID = objMaster.UrlBreakdown.countryDetails.CountryID,
                    LanguageID = objMaster.UrlBreakdown.countryDetails.LanguageID,
                    Email = memberStore.MemberDetail.Email,
                    SourceID = 25,
                    MarketingCountryID = memberStore.MemberDetail.MarketingCountryID,
                    ThirdPartyID = null
                });

            objPayReturn.masterDefinition = objMaster;
            objPayReturn.checkoutPayments = objCheckoutPayments;
            objPayReturn.marketingPermissions = marketingPermissions;
            objPayReturn.isMemberCheckout = objPayReturn.checkoutMethod == dgCheckoutType.Member;

            var savedCards = new List<CheckoutBraintreeCreditCardDetails>();
            if (objPayReturn.checkoutMethod == dgCheckoutType.Guest || objPayReturn.checkoutMethod == dgCheckoutType.Member || objPayReturn.checkoutMethod == dgCheckoutType.Paypal)
            {
                var gateway = new BraintreeGateway()
                {
                    Environment = Braintree.Environment.ParseEnvironment(objCheckoutPayments.checkoutDetails[0].Environment),
                    MerchantId = objCheckoutPayments.checkoutDetails[0].MerchantID,
                    PublicKey = objCheckoutPayments.checkoutDetails[0].PublicKey,
                    PrivateKey = objCheckoutPayments.checkoutDetails[0].PrivateKey
                };
                
                var tokenRequest =
                    new ClientTokenRequest()
                    {
                        MerchantAccountId = objCheckoutPayments.checkoutDetails[0].MerchantAccountID
                    };

                if (objPayReturn.checkoutMethod == dgCheckoutType.Member)
                {
                    string searchCustomerId = memberId.ToString();
                    string customerId;
                    try
                    {
                        Customer customer = gateway.Customer.Find(searchCustomerId);
                        customerId = customer.Id;
                    } catch (Braintree.Exceptions.NotFoundException)
                    {
                        // if Customer.FindAsync fails to find a customer it throws an exception, we need to create a new Braintree customer to be able to save any card tokens against
                        CustomerRequest newCustomer = new CustomerRequest()
                        {
                            Id = searchCustomerId,
                            Email = memberStore.MemberDetail.Email,
                            FirstName = memberStore.MemberDetail.FullName
                        };
                        var customer = gateway.Customer.Create(newCustomer);
                        if (customer.IsSuccess())
                            customerId = customer.Target.Id;
                        else
                            customerId = null;
                    }
                    catch (Exception)
                    {
                        customerId = null;
                    }
                    tokenRequest.CustomerId = customerId;
                }

                objPayReturn.braintreeClient = gateway.ClientToken.Generate(
                    tokenRequest
                );
                
                // load any cards in the Braintree vault for this customer
                if (objPayReturn.checkoutMethod == dgCheckoutType.Member)
                {
                    try
                    {
                        Customer cust = gateway.Customer.Find(memberId.ToString());

                        foreach (CreditCard card in cust.CreditCards)
                        {
                            var newCard = new CheckoutBraintreeCreditCardDetails()
                            {
                                cardType = card.CardType.ToString(),
                                endDigits = card.LastFour,
                                expiry = card.ExpirationDate,
                                defaultCard = card.IsDefault ?? false,
                                vaultToken = card.Token
                            };

                            if (objCheckoutPayments.checkoutDetails[0].Mandate3DSecure)
                            {
                                newCard.token = card.Token;
                            }
                            else
                            {
                                var token = gateway.PaymentMethodNonce.Create(card.Token);

                                if (token.IsSuccess())
                                    newCard.token = token.Target.Nonce;
                                else
                                    continue;
                            }

                            /* the token that is stored in the vault cannot have 3d secure applied to it by default, to be able to do this
                               we need to use it to create a new payment nonce. */
                            /*var token = gateway.PaymentMethodNonce.Create(card.Token);

                            if (token.IsSuccess())
                                newCard.token = token.Target.Nonce;
                            else
                                continue;*/

                            bool expired = true;
                            if (!string.IsNullOrEmpty(newCard.expiry))
                            {
                                var dateparts = newCard.expiry.Split('/');
                                if (dateparts.Length == 2)
                                {
                                    var month = Convert.ToInt32(dateparts[0]);
                                    var year = Convert.ToInt32(dateparts[1]);

                                    expired = (year < DateTime.Now.Year || (year == DateTime.Now.Year && month < DateTime.Now.Month));
                                }
                            }
                            newCard.isExpired = expired;
                            if (newCard.isExpired)
                                newCard.defaultCard = false;

                            savedCards.Add(newCard);
                        }
                    }
                    catch (NotFoundException) { }
                    catch (Exception ex)
                    {
                        var message = string.Format("CheckoutID: {0} - {1} ", checkoutId.ToString(), "Unable to load cards from Braintree vault - " + ex.Message);
                        Logger.Error(message);
                    }
                }

                // add any card tokens saved against this basket
                var cardTokens = await objCheckout.getPaymentTokens(checkoutId);
                var cardsToAppend = cardTokens.Select(t => new CheckoutBraintreeCreditCardDetails()
                {
                    cardType = t.CardType,
                    endDigits = t.LastFourDigits,
                    expiry = null,
                    bin = t.Bin,
                    token = t.Token,
                    vaultToken = string.Empty,
                    defaultCard = false
                });

                savedCards.AddRange(cardsToAppend);

                // default to the first card if a default isn't set
                if (savedCards.Any() && !savedCards.Where(c => c.defaultCard == true).Any())
                {
                    var defaultCard = savedCards.Where(c => c.isExpired == false).FirstOrDefault();
                    if (defaultCard != null)
                        defaultCard.defaultCard = true;
                }
            }

            objPayReturn.savedCards = savedCards;

            return View("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutPay.cshtml", objPayReturn);
        }

        public async Task<ActionResult> _checkoutConfirm(BaseViewModel objBase)
        {
            var objMaster = objBase.MasterDefinition;

            passCheckoutConfirm objReturn = new passCheckoutConfirm();
            checkoutDb objCheckout = new checkoutDb();

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");


            StringWriter str = new StringWriter();

            passCheckoutPayment objPayReturn = new passCheckoutPayment();

            Session["checkoutEmail"] = null;

            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] != null)
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }
            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/LandingPages/FixedPages/Checkout/_Expired.cshtml", objBase);
            }
            else if (await objCheckout.preflightCheckPay(Convert.ToInt64(Session["checkoutId"].ToString()), objMaster.UrlBreakdown.countryDetails.CountryID) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }

            Int64 checkoutId = Convert.ToInt64(Session["checkoutId"].ToString());
            
            CheckoutPayment objCheckoutPayments = await objCheckout.checkoutPayGenerate(checkoutId, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID, "SectionController._checkoutConfirm");

            String token = "";
            if (objCheckoutPayments.checkoutDetails.Count() == 0)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }

            token = objCheckoutPayments.checkoutDetails[0].GUID.ToString();

            //  Get the order receipt
            checkoutReceipt objCheckoutReceiptStore = await objCheckout.getPaymentReceipt(
                new Guid(token.ToUpper()), 
                visitorDbId, 
                objMaster.UrlBreakdown.countryDetails.CountryID, 
                objMaster.UrlBreakdown.countryDetails.LanguageID, 
                objMaster.UrlBreakdown.ContentDate,
                Session["memberTradeAccountID"] == null ? (int?)null : Convert.ToInt32(Session["memberTradeAccountID"]),
                Session["memberTradeAccountID"] != null,
                Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"]));

            //  Create the display properties
            foreach (checkoutReceiptOrder objOrder in objCheckoutReceiptStore.checkoutReceiptOrders)
            {
                String address = "";
                address = formatAddressLine(address, objOrder.FullName);
                address = formatAddressLine(address, objOrder.CompanyName);
                address = formatAddressLine(address, objOrder.AddressLine1);
                address = formatAddressLine(address, objOrder.AddressLine2);
                address = formatAddressLine(address, objOrder.Town);
                address = formatAddressLine(address, objOrder.County);
                address = formatAddressLine(address, objOrder.PostCode);
                address = formatAddressLine(address, objOrder.Name);
                objOrder.displayAddress = address;

                String minStore = "";
                String maxStore = "";
                if (objOrder.MinDeliveryEstimate == null) { }
                else
                {
                    minStore = ((DateTime)objOrder.MinDeliveryEstimate).ToString("dddd dd MMMM yyyy");
                }
                if (objOrder.MaxDeliveryEstimate == null) { }
                else
                {
                    maxStore = ((DateTime)objOrder.MaxDeliveryEstimate).ToString("dddd dd MMMM yyyy");
                }

                objOrder.displayEstimatedDelivery = minStore;

                if (minStore == maxStore) { }
                else
                {
                    objOrder.displayEstimatedDelivery = String.Format("{0} - {1}", minStore, maxStore);
                }
            }

            //  Clear the checkout session
#if DEBUG

#else
            Session["checkoutId"] = null;
            Session["checkoutExpiry"] = null;
#endif

            objReturn.masterDefinition = objMaster;
            objReturn.checkoutReceipt = objCheckoutReceiptStore;

            ViewBag.Partial = false;

            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutConfirm.cshtml", new FixedPageViewModel_CheckoutConfirm(objBase, objReturn));
            //return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        }


        public async Task<ActionResult> _checkoutConfirmSpartan(BaseViewModel objBase)
        {
            var objMaster = objBase.MasterDefinition;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
            var objCheckout = new checkoutDb();
            if (Session["visitorIdFromDatabase"] != null)
            {
                _ = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }
            if (dgFramework.doesUserHaveACheckoutId(Session["checkoutId"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }
            else if (dgFramework.isCheckoutSessionValid(Session["checkoutExpiry"]) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToExpired.cshtml", objMaster);
            }
            else if (await objCheckout.preflightCheckPay(Convert.ToInt64(Session["checkoutId"].ToString()), objMaster.UrlBreakdown.countryDetails.CountryID) == false)
            {
                return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
            }

            _ = Convert.ToInt64(Session["checkoutId"].ToString());

            //  Clear the checkout session
            Session["checkoutId"] = null;
            Session["checkoutExpiry"] = null;

            return PartialView("~/Views-New/LandingPages/FixedPages/Checkout/_CheckoutConfirmSpartan.cshtml", objBase);

            //return View("~/Views-New/Shared/Error/_returnToBasket.cshtml", objMaster);
        }

        #endregion

        #region Basket


        public async Task<ActionResult> _maintainBasket(masterDefinition objMaster)
        {
            if (objMaster.UrlBreakdown.countryDetails.ShopEnabled == true) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }


            Int64 visitorDbId = -1;
            if (Session["visitorIdFromDatabase"] == null)
            {
            }
            else
            {
                visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (Session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(Session["memberIdFromDatabase"].ToString());
            }

            basketRoutines objBasket = new basketRoutines();
            Basket basketDetails = await objBasket.loadBasketDetail(visitorDbId,
                memberId,
                objMaster.UrlBreakdown.countryDetails.LanguageID,
                objMaster.UrlBreakdown.countryDetails.CountryID,
                objMaster.UrlBreakdown.ContentDate,
                (int?)Session["memberTradeAccountID"],
                Session["memberTradeAccountID"] != null,
                Session["memberDisplayTradePrice"] != null && Convert.ToBoolean(Session["memberDisplayTradePrice"])
                );


            foreach (BasketDetail objBasketLine in basketDetails.BasketItems)
            {
                objBasketLine.ImageUrl = Url.RouteUrl("ImageRoutine", new { width = dgFramework.getImageWidth(dgImageSize.marker), imageId = objBasketLine.ImageID.ToString() });
            }


            passBasketDetail passBasketDetail = new passBasketDetail
            {
                masterDefinition = objMaster,
                basketGenerate = basketDetails
            };
            if (Convert.ToBoolean(Session["memberTradeChecksPassed"]) == true)
            {
                if (Session["memberTradeAccountID"] != null)
                {
                    _ = Convert.ToInt32(Session["memberTradeAccountID"]);
                }
            }

            return View("~/Views/Basket/_maintainBasket.cshtml", passBasketDetail);
        }


#endregion

#region Country Picker


        public async Task<ActionResult> _countryPicker(masterDefinition objMaster)
        {

            cmsCountry objCountry = new cmsCountry();
            cmsCountryDefinition objCountryData = await objCountry.countryLanguageSelect(objMaster.UrlBreakdown.ContentDate);

            passCountryPicker objCountryPicker = new passCountryPicker
            {
                masterDefinition = objMaster,
                cmsCountryDefinitions = objCountryData
            };

            return View("~/Views/CountryPicker/_countryPicker.cshtml", objCountryPicker);
        }



        public async Task<ActionResult> _countryPickerDropDown(masterDefinition objMaster)
        {


            cmsCountry objCountry = new cmsCountry();
            cmsCountryDropDownDefinition objCountryDropDownData = await objCountry.countryDropDownSelectList(
                objMaster.UrlBreakdown.ContentDate,
                objMaster.UrlBreakdown.countryDetails.LanguageID);

            passCountryDropDownPicker objCountryDropDownPicker = new passCountryDropDownPicker
            {
                masterDefinition = objMaster,
                cmsCountryDropDownDefinitions = objCountryDropDownData
            };

            return View(dgFramework.GetPreferredDeviceView("~/Views/CountryPicker/_countryPickerDropDown{0}.cshtml"), objCountryDropDownPicker);
        }
#endregion

        #region Stockists


        //public async Task<ActionResult> _stockists(masterDefinition objMaster)
        //{
        //    //  Get stockist countries
        //    Stockist objStockistCountry = new Stockist();
        //    StockistCountrySelect objStockistCountryData = await objStockistCountry.getStockistCountries(objMaster.UrlBreakdown.ContentDate);


        //    PassStockistCountry objReturn = new PassStockistCountry
        //    {
        //        StockistCountrySelect = objStockistCountryData,
        //        MasterDefinition = objMaster,


        //        //  Calculate the default country
        //        defaultCountryId = 81
        //    };
        //    foreach (StockistCountry objCountry in objStockistCountryData.stockistCountries)
        //    {
        //        if (objCountry.CountryID == objMaster.UrlBreakdown.countryDetails.CountryID)
        //        {
        //            objReturn.defaultCountryId = objCountry.CountryID;
        //            break;
        //        }
        //    }


        //    return View(dgFramework.GetPreferredDeviceView("~/Views/Stockists/_stockists{0}.cshtml"), objReturn);
        //}


        #endregion

        #region Contract Stockists


        //public async Task<ActionResult> _contractStockistSearch(masterDefinition objMaster)
        //{

        //    //  Get the stockist countries
        //    Stockist objStockistCountry = new Stockist();
        //    StockistCountrySelect objStockistCountryData = await objStockistCountry.getStockistCountries(objMaster.UrlBreakdown.ContentDate);


        //    //  Get contract stockist details
        //    StockistsContract objStockistsContract = new StockistsContract();
        //    stockistsContractSearchSelect stockistsContractData = await objStockistsContract.stockistContractSearch(objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.ContentDate, objMaster.UrlBreakdown.countryDetails.LanguageID);


        //    passStockistsContractDetail objReturn = new passStockistsContractDetail
        //    {
        //        MasterDefinition = objMaster,
        //        StockistCountrySelect = objStockistCountryData,
        //        StockistsContractSearchSelect = stockistsContractData,
        //        DefaultCountryForDropDown = objMaster.UrlBreakdown.countryDetails.CountryID
        //    };


        //    return View(dgFramework.GetPreferredDeviceView("~/Views/StockistsContract/_stockistsContract{0}.cshtml"), objReturn);
        //}

        #endregion

        #region Reset Password


        public async Task<ActionResult> _resetPassword(masterDefinition objMaster)
        {


            if (Request.QueryString["token"] == null)
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }
            else if (Request.QueryString["token"].ToString() == "")
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }


            String fullToken = Request.QueryString["token"].ToString();
            if (fullToken.Length == 56) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }


            String partToken = fullToken.Substring(0, 36);


            //  Validate token is a GUID
            _ = new Guid();
            Boolean tokenValid = Guid.TryParse(partToken, out _);
            if (tokenValid == true) { }
            else
            {
                return View("~/Views-New/Shared/Error/_returnToHome.cshtml", objMaster);
            }


            VisitorFactory visitorFactory = new VisitorFactory();
            Boolean success = await visitorFactory.validateResetPasswordToken(fullToken);
            if (success == true)
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/Member/_memberPasswordReset{0}.cshtml"), objMaster);
            }
            else
            {
                return View(dgFramework.GetPreferredDeviceView("~/Views/Member/_memberPasswordResetTokenExpired{0}.cshtml"), objMaster);
            }
        }


#endregion

#region Order Date Edit History Partial
        public ActionResult UPSReturnsCollectionDateEditPartial()
        {
            return PartialView("~/Views/Member/_OrderHistoryCollectionDateEditPartial.cshtml");
        }
#endregion

#region Helper Routines


        public string getValueFromQueryString(String qsKey)
        {
            String returnValue = "";
            foreach (string key in Request.QueryString.AllKeys)
            {
                if (qsKey == key)
                {
                    returnValue = Request.QueryString[qsKey].ToString();
                }
            }

            if (returnValue == null)
            {
                returnValue = "";
            }

            return returnValue;
        }


        public string getValueFromForm(String formKey)
        {
            String returnValue = "";
            foreach (string key in Request.Form.AllKeys)
            {
                if (formKey == key)
                {
                    returnValue = Request.Form[key].ToString();
                }
            }

            if (returnValue == null)
            {
                returnValue = "";
            }

            return returnValue;
        }


        public string formatAddressLine(String address, String line)
        {
            if (line == null)
            {
                return address;
            }
            else if (line.Trim() == "")
            {
                return address;
            }
            else
            {
                if (address == "")
                {
                    address = String.Format("{0}", line.Trim());
                }
                else
                {
                    address = String.Format("{0}, {1}", address, line.Trim());
                }
                return address;
            }
        }


#endregion

        [HttpPost]
        public ActionResult LoadBundleNotice(masterDefinition objMaster, int productId, string dateOverride)
        {
            if (objMaster.UrlBreakdown.countryDetails.PriceListID == null)
            {
                return Json(null, JsonRequestBehavior.DenyGet);
            }
            else
            {
#region Date Override Check
                DateTime useDate = DateTime.Now;
                useDate = new DateTime(useDate.Year, useDate.Month, useDate.Day);
                if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
                else
                {
                    if (string.IsNullOrEmpty(dateOverride)) { }
                    else
                    {
                        if (dateOverride.Length == "ddMMyyyy".Length)
                        {
                            bool dateConvertSuccess = DateTime.TryParseExact(dateOverride,
                                                                            "ddMMyyyy",
                                                                            CultureInfo.InvariantCulture,
                                                                            DateTimeStyles.None,
                                                                            out useDate);
                            if (dateConvertSuccess == true) { }
                            else
                            {
                                useDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                            }
                        }
                    }
                }
#endregion

                Models.ProductPage.cmsProductPage objProductPage = new Models.ProductPage.cmsProductPage();

                var result = objProductPage.LoadBundleNoticeFromCms(productId, objMaster.UrlBreakdown.countryDetails.PriceListID.Value, objMaster.UrlBreakdown.countryDetails.CountryID, objMaster.UrlBreakdown.countryDetails.LanguageID, useDate);

                return Json(result, JsonRequestBehavior.DenyGet);
            }
        }
    }
}