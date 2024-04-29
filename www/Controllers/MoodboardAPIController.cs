using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Scrapbook;
using wwwDesignersGuild.Models.Visitor;
using wwwDesignersGuild.Util;
using Tweetinvi;
using wwwDesignersGuild.Models.Twitter;
using Newtonsoft.Json.Linq;
using wwwDesignersGuild.ViewModels;
using wwwDesignersGuild.Models.BasketClasses;

namespace wwwDesignersGuild.Controllers
{
    public class MoodboardAPIController : ApiController
    {
        [HttpPost]
        public async Task<IHttpActionResult> SortScrapbook(PassScrapbook objScrapbook)
        {
            var session = HttpContext.Current.Session;

            var vf = new VisitorFactory();

            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            return Ok (new { result = await vf.sortScrapbook(memberId.Value, objScrapbook) });
        }

        [HttpPost]
        public async Task<IHttpActionResult> SendMoodboardShareEmail(MoodboardShareEmail objEmail)
        {
            var result = false;

            if (String.IsNullOrEmpty(objEmail.gRecaptcha.Trim()))
            {
                ModelState.AddModelError("gRecaptcha", "failed catpcha");
            }
            else
            {
                if (ReCaptchaVerifier.Validate2022(objEmail.gRecaptcha) != "true")
                {
                    ModelState.AddModelError("gRecaptcha", "failed captcha");
                }
            }

            if (ModelState.IsValid)
            {
                var objVisitorFactory = new VisitorFactory();

                result = await objVisitorFactory.SendMoodboardEmail(objEmail);
            }

            return Ok(new { result });
        }

        [HttpPost]
        public async Task<IHttpActionResult> ClearScrapbook(webapiClearScrapbook objScrapbook)
        {
            var session = HttpContext.Current.Session;

            Int64 visitorDbId = -1;
            if (session["visitorIdFromDatabase"] == null)
            {
                dgFramework.raiseErrorNotHandled(webapiUnhandledError.MissingSessionParameter);
            }
            else
            {
                visitorDbId = Convert.ToInt64(session["visitorIdFromDatabase"].ToString());
            }

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            var visitorFactory = new VisitorFactory();

            var success = await visitorFactory.clearScrapbook(visitorDbId, memberId, objScrapbook.countryId);

            return Ok(new { success });
        }

        [HttpPost]
        public async Task<IHttpActionResult> AddImageToMoodboard(webapiAddImageToMoodboard imageToAdd)
        {
            var visitorFactory = new VisitorFactory();

            var result = await visitorFactory.addImageToMoodboard(imageToAdd.moodboardId, imageToAdd.imageId, imageToAdd.productPageId);

            return Ok(new { result });
        }

        [HttpPost]
        public async Task<IHttpActionResult> RemoveImagesFromMoodboard(PassMoodboardImageRemoval objRemoval)
        {
            var visitorFactory = new VisitorFactory();

            var removalTasks = new List<Task<bool>>();

            objRemoval.ImageIDs.ForEach(i =>
            {
                removalTasks.Add(visitorFactory.removeImageFromMoodboard(objRemoval.MoodboardID, i));
            });

            await Task.WhenAll(removalTasks);

            removalTasks.ForEach(async t =>
            {
                _ = await t;
            });

            return Ok(new { });
        }

        [HttpPost]
        public async Task<IHttpActionResult> ClearMoodboard(webapiMoodboardID objMoodboard)
        {
            var visitorFactory = new VisitorFactory();

            var result = false;

            var session = HttpContext.Current.Session;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                int? memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
                result = await visitorFactory.clearMoodboard(memberId.Value, objMoodboard.moodboardId);
            }

            return Ok(new { result });
        }

        [HttpPost]
        public async Task<IHttpActionResult> SaveMoodboard(webapiMoodboard objMoodboard)
        {
            VisitorFactory visitorFactory = new VisitorFactory();

            Boolean result = await visitorFactory.saveMoodboard(objMoodboard.moodboardId, objMoodboard.height.Value, objMoodboard.JSON, objMoodboard.customerName, objMoodboard.roomId, objMoodboard.reference);

            return Ok(new { result });
        }

        [HttpPost]
        public async Task<IHttpActionResult> SaveMoodboardImage([FromBody]PassMoodboardJpegSave objSave)
        {
            try
            {
                var targetFolder = System.Web.Hosting.HostingEnvironment.MapPath("~/Content/images/moodboards/");

                var completePath = $"{targetFolder}\\{objSave.FileName}";

                var bytes = Convert.FromBase64String(objSave.File.Replace("data:image/jpeg;base64,", ""));

                using (var imageFile = new FileStream(completePath, FileMode.Create))
                {
                    imageFile.Write(bytes, 0, bytes.Length);
                    imageFile.Flush();
                }

                return Ok(new {
                    url = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}/moodboards/{HttpUtility.UrlEncode(objSave.FileName)}"
                });
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, $"{e}");
            }
        }

        //[HttpPost]
        //public async Task<IHttpActionResult> SaveMoodboardImage(webapiMoodboardSaveImage objMoodboardImage)
        //{
        //    var visitorFactory = new VisitorFactory();

        //    var moodboard = await visitorFactory.fetchMoodboard(objMoodboardImage.moodboardId);

        //    var fileName = "~/content/images/moodboards/" + moodboard.GUID + ".png";

        //    var bytes = Convert.FromBase64String(objMoodboardImage.imageData.Replace("data:image/png;base64,", ""));
        //    using (var imageFile = new System.IO.FileStream(HttpContext.Current.Server.MapPath(fileName), System.IO.FileMode.Create))
        //    {
        //        imageFile.Write(bytes, 0, bytes.Length);
        //        imageFile.Flush();
        //    }

        //    return Ok( new { result = true });
        //}

        [HttpPost]
        public async Task<IHttpActionResult> CreateMoodboard(webapiMoodboardCreate objMoodboardCreate)
        {
            var session = HttpContext.Current.Session;

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            if (memberId == null)
            {
                return Ok(new { result = false });
            }

            var visitorFactory = new VisitorFactory();

            return Ok(new CreateMoodboardResult
            {
                MoodboardID = await visitorFactory.createMoodboard(memberId.Value, objMoodboardCreate.countryId),
                CreatedDT = DateTime.Now
            });
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeleteMoodboard(PassMoodboardID objMoodboard)
        {
            var session = HttpContext.Current.Session;

            try
            {
                var vf = new VisitorFactory();

                int memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());

                var result = await vf.deleteMoodboard(memberId, objMoodboard.moodboardId);

                return Ok(new { result });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, $"{e}");
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> ReloadMoodboards(webapiMoodboardReload objMoodboardReload)
        {
            var session = HttpContext.Current.Session;

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            if (memberId == null)
            {
                return Ok(new { result = false });
            }

            VisitorFactory visitorFactory = new VisitorFactory();

            var result = await visitorFactory.reloadMoodboards(memberId.Value, objMoodboardReload.countryId, objMoodboardReload.languageId, objMoodboardReload.contentDate);

            return Ok(new { result });
        }

        [HttpPost]
        public async Task<IHttpActionResult> MoodboardFetchProducts(PassMoodboardOrderingFetch objFetch)
        {
            var session = HttpContext.Current.Session;

            int? memberId = null;

            if (session["memberIdFromDatabase"] != null)
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            if (memberId == null)
            {
                return Ok(new { result = false });
            }

            int? tradeAccountId = null;

            if (session["memberTradeAccountId"] != null)
            {
                tradeAccountId = Convert.ToInt32(session["memberTradeAccountId"].ToString());
            }

            bool displayTradePrice = false;

            if (session["memberDisplayTradePrice"] != null)
            {
                displayTradePrice = Convert.ToBoolean(session["memberDisplayTradePrice"].ToString());
            } 

            var visitorFactory = new VisitorFactory();

            var objBasket = new basketRoutines();

            var basket = await objBasket.loadBasketDetailQuick
                (
                    objFetch.BaseViewModel.Visitor.visitorDbId,
                    memberId,
                    objFetch.BaseViewModel.MasterDefinition.UrlBreakdown.countryDetails.LanguageID,
                    objFetch.BaseViewModel.MasterDefinition.UrlBreakdown.countryDetails.CountryID,
                    objFetch.BaseViewModel.MasterDefinition.UrlBreakdown.ContentDate,
                    tradeAccountId,
                    tradeAccountId.HasValue,
                    objFetch.BaseViewModel.Visitor.memberDisplayTradePrice
                );

            objFetch.BaseViewModel.Basket = basket;

            var result = await visitorFactory.fetchMoodboardProducts(memberId.Value, tradeAccountId, displayTradePrice, objFetch);

            var controller = ViewRenderer.CreateController<GenericController>();

            var orderFormItems = JObject.FromObject(result.json)
                .SelectToken("products")
                .Skip(1)
                .Select(p => new OrderFormItemViewModel(p, objFetch.BaseViewModel.MasterDefinition, objFetch.BaseViewModel))
                .ToList();

            var markup = $"";

            foreach (var item in orderFormItems)
            {
                controller.ViewBag.OrderFormPrefix = $@"-product-{Guid.NewGuid()}";

                markup += $@"<div class=""order-form-group"" data-bind=""with: getProductById({item.ProductID})"">
                    {ViewRenderer.RenderViewToString(controller.ControllerContext, "~/Views-New/ProductPages/Shared/_OrderFormItem.cshtml", item, true)}
                </div>";
            }

            return Ok(new { result, markup });
        }

        [HttpPost]
        public async Task<IHttpActionResult> MoodboardSearch(webapiMoodboardSearch objMoodboardSearch)
        {
            var session = HttpContext.Current.Session;

            Int32? memberId = null;
            if (session["memberIdFromDatabase"] == null)
            {
            }
            else
            {
                memberId = Convert.ToInt32(session["memberIdFromDatabase"].ToString());
            }

            if (memberId == null)
            {
                return Ok(new { result = false });
            }

            var visitorFactory = new VisitorFactory();

            var result = await visitorFactory.searchMoodboards(memberId.Value, objMoodboardSearch.customerName, objMoodboardSearch.roomId, objMoodboardSearch.reference, objMoodboardSearch.countryId, objMoodboardSearch.languageId, objMoodboardSearch.contentDate);

            return Ok(new { result });
        }

        [HttpGet]
        public HttpResponseMessage DownloadMoodboardImage(string guid)
        {
            var localFilePath = HttpContext.Current.Server.MapPath("~/content/images/moodboards/" + guid + ".png");

            HttpResponseMessage result;
            if (!File.Exists(localFilePath))
            {
                result = Request.CreateResponse(HttpStatusCode.Gone);
            }
            else
            {
                result = Request.CreateResponse(HttpStatusCode.OK);
                result.Content = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
                result.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                {
                    FileName = "Moodboard.png"
                };
            }

            return result;
        }

        [HttpPost]
        public async Task<IHttpActionResult> FetchTwitterAuth()
        {
            var result = await Twitter.RequestToken();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IHttpActionResult> TweetMoodboard([FromBody]PassMoodboardTweet objMoodboardTweet)
        {
            var url = await Twitter.UploadMoodboardMediaAndTweet(objMoodboardTweet);

            return Ok(new { url });
        }
    }
}
