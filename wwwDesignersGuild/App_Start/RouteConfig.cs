using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace wwwDesignersGuild
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute(".well-known/(.*)");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("favicon.ico");
            routes.IgnoreRoute("Content/images/moodboards/{*pathInfo}");
            routes.IgnoreRoute("{*video}", new { video = @"(_assets/flock_assets/videos/)(.*).mp4" });
            routes.Ignore("{*sitemap}", new { sitemap = @"(.*/)?sitemap(.*).xml" });
            routes.Ignore("{*pdf}", new { pdf = @"(pdf/)(.*).pdf" });
            routes.Ignore("{*pdf}", new { pdf = @"(pdf/furniturepdfs/)(.*).pdf" });

            routes.MapRoute(
                name: "TwitterMoodboardCallback",
                url: "moodboard/twittercallback",
                defaults: new { controller = "Moodboard", action = "TwitterCallback" }
            );

            routes.MapRoute(
                name: "ErrorRoutine",
                url: "major-error",
                defaults: new { controller = "error", action = "HandleError" }
            );

            routes.MapRoute(
                name: "MoodboardImage",
                url: "moodboards/{fileName}",
                defaults: new { controller = "moodboard", action = "index" }
            );

            routes.MapRoute(
                name: "PdfReturnRoutine",
                url: "pdf/return",
                defaults: new { controller = "pdf", action = "GenerateReturn" }
            );

            routes.MapRoute(
                name: "PdfReceiptRoutine",
                url: "pdf/receipt",
                defaults: new { controller = "pdf", action = "GenerateReceipt" }
            );

            routes.MapRoute(
                name: "BundleNoticeRoutine",
                url: "section/loadBundleNotice",
                defaults: new { controller = "section", action = "LoadBundleNotice" }
            );

            routes.MapRoute(
                name: "LifeStyleRoutine",
                url: "lifestyle/{basketCountry}/{productPageId}",
                defaults: new { controller = "LifeStyle", action = "Index" }
            );

            routes.MapRoute(
                name: "LinkShareUkRoutine",
                url: "linkshare/uk",
                defaults: new { controller = "LinkShare", action = "IndexUk" }
            );

            routes.MapRoute(
                name: "LinkShareUsaRoutine",
                url: "linkshare/us",
                defaults: new { controller = "LinkShare", action = "IndexUsa" }
            );

            routes.MapRoute(
                name: "PayPalIpnRoutine",
                url: "PayPal-Ipn",
                defaults: new { controller = "PayPalIpn", action = "Index" }
            );

            routes.MapRoute(
                name: "ImageRoutine",
                url: "image/{width}/{imageid}",
                defaults: new { controller = "Image", action = "CallImage" }
            );

            routes.MapRoute(
                name: "ImageRoutineNonWebP",
                url: "image-nonwebp/{width}/{imageid}",
                defaults: new { controller = "Image", action = "CallImageNonWebP" }
            );

            routes.MapRoute(
                name: "ImageBannerRoutine",
                url: "imagebanner/{width}/{imageid}",
                defaults: new { controller = "Image", action = "CallImageBanner" }
            );

            routes.MapRoute(
                name: "ImageBannerRoutineNonWebP",
                url: "imagebanner-nonwebp/{width}/{imageid}",
                defaults: new { controller = "Image", action = "CallImageBannerNonWebP" }
            );

            routes.MapRoute(
                name: "SiteSelectorBackgroundImage",
                url: "images/site-selector-background/{platform}/{size}",
                defaults: new { controller = "Image", action = "CallSiteSelectorImage" }
            );

            routes.MapRoute(
                name: "SiteSelectorBackgroundImageNonWebP",
                url: "images-nonwebp/site-selector-background/{platform}/{size}",
                defaults: new { controller = "Image", action = "CallSiteSelectorImageNonWebP" }
            );

            routes.MapRoute(
                name: "TradeDownloadsExampleCSV",
                url: "downloads/trade-order-import-example",
                defaults: new { controller = "Trade", action = "TradeDownloadExampleCsv" }
            );

            routes.MapRoute(
                name: "TradeDownloadsStandardFileFormat",
                url: "downloads/trade-order-import-format",
                defaults: new { controller = "Trade", action = "TradeDownloadStandardImportFormat" }
            );

            routes.MapRoute(
                name: "TradeDownloadsDistributorFileFormat",
                url: "downloads/trade-order-import-distributor-format",
                defaults: new { controller = "Trade", action = "TradeDownloadDistributorImportFormat" }
            );

            routes.MapRoute(
                name: "TradeDownloadsGridViewDownload",
                url: "downloads/pricelists-brochures-grid-download-file",
                defaults: new { controller = "trade", action = "TradeDownloadFileManagerDownloadFile" }
            );

            routes.MapRoute(
                name: "TradeDownloadsGridViewUpdate",
                url: "downloads/pricelists-brochures-grid",
                defaults: new { controller = "trade", action = "TradeDownloadFileManagerDataBindingPartial" }
            );

            routes.MapRoute(
                name: "TradeDocument",
                url: "downloads/trade-document",
                defaults: new { controller = "trade", action = "ViewDocument" }
            );

            routes.MapRoute(
                name: "TradeDocumentFetch",
                url: "downloads/trade-document-fetch",
                defaults: new { controller = "trade", action = "FetchDocument" }
            );

            routes.MapRoute(
                name: "TradeDocumentEmail",
                url: "downloads/trade-document-email",
                defaults: new { controller = "trade", action = "EmailDocument" }
            );

            routes.MapRoute(
                name: "TradeProforma",
                url: "downloads/trade-proforma",
                defaults: new { controller = "trade", action = "ViewProformaDocument" }
            );

            routes.MapRoute(
                name: "TradeProformaFetch",
                url: "downloads/trade-proforma-fetch",
                defaults: new { controller = "trade", action = "FetchProformaDocument" }
            );

            routes.MapRoute(
                name: "TradeDocumentProformaEmail",
                url: "downloads/trade-proforma-email",
                defaults: new { controller = "trade", action = "EmailProformaDocument" }
            );

            routes.MapRoute(
                name: "MoodboardGridViewUpdate",
                url: "moodboard/moodboardgridviewupdate",
                defaults: new { controller = "section", action = "MoodboardGridViewUpdate" }
            );

            routes.MapRoute(
                name: "MoodboardGridView",
                url: "moodboard/moodboardgridviewpartial",
                defaults: new { controller = "Section", action = "MoodboardGridViewPartial" }
            );

            routes.MapRoute(
                name: "UPSReturnsCollectionDateEdit",
                url: "section/upsreturnscollectiondateeditpartial",
                defaults: new { controller = "Section", action = "UPSReturnsCollectionDateEditPartial" }
            );

            routes.MapRoute(
                name: "StoreEventDateEdit",
                url: "section/_StoreEventLocationDateEdit",
                defaults: new { controller = "Section", action = "_StoreEventLocationDateEdit" }
            );

            routes.MapRoute(
                name: "ProductRedirect",
                url: "productRedirect",
                defaults: new { controller = "ProductRedirect", action = "Index" } 
            );

            routes.MapRoute(
                name: "AlternativeProducts",
                url: "alternativeProducts",
                defaults: new { controller = "AlternativeProducts", action = "Index" }
            );

            routes.MapRoute(
                name: "CheckoutParts",
                url: "checkoutPart/{action}",
                defaults: new { controller = "CheckoutPart", action = "Index" }
                );

            routes.MapRoute(
                name: "PayPalLoginConfirm",
                url: "section/_checkoutPayPalLoginResponse",
                defaults: new { controller = "Section", action = "_checkoutPayPalLoginResponse" }
                );


            routes.MapRoute(
                name: "DownloadClearanceSpreadsheet",
                url: "download/ClearanceSpreadsheet",
                defaults: new { controller = "Trade", action = "downloadClearanceSpreadsheet" }
                );

            routes.MapRoute(
                name: "DownloadClearancePDF",
                url: "download/ClearancePDF",
                defaults: new { controller = "Trade", action = "downloadClearancePDF" }
                );

            routes.MapRoute(
                name: "DownloadClearanceSpreadsheetSited",
                url: "{site}/download/ClearanceSpreadsheet",
                defaults: new { controller = "Trade", action = "downloadClearanceSpreadsheet" }
                );

            routes.MapRoute(
                name: "DownloadClearancePDFSited",
                url: "{site}/download/ClearancePDF",
                defaults: new { controller = "Trade", action = "downloadClearancePDF" }
                );

            routes.MapRoute(
                name: "CatchAllRoutine",
                url: "{*urlparameter}",
                defaults: new { controller = "Home", action = "CatchAll" }
            );

            routes.MapRoute(
                name: "GeneralRoutine",
                url: "{controller}/{action}",
                defaults: new { controller = "Home", action = "Index" }
            );
        }
    }
}
