using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

using WebGrease.Activities;

using wwwDesignersGuild.Models.GiftVoucher;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Util;

namespace wwwDesignersGuild.Controllers
{
    public class GiftVoucherController : Controller
    {
        // GET: GiftVoucher
        new public async Task<ActionResult> View(string id)
        {
            var sw = new Stopwatch();

            sw.Start();

            if (String.IsNullOrEmpty(id))
            {
                return new RedirectResult("/", true);
            }
            else
            {
                var giftVoucherCode = dgCrypto.Decrypt(id);

                var gv = new cmsGiftVoucher();

                var visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"]);

                var selectResult = await gv.FetchGiftVoucher(visitorDbId, giftVoucherCode);

                var voucherStream = GiftVoucherPDFWriter.Generate(Server.MapPath("~"), selectResult);

                var bytes = voucherStream.ToArray();

                Debug.WriteLine($"/gift-voucher/view/${id} completed in {sw.Elapsed}");

                return File(bytes, "application/pdf");
            }
        }
    }
}