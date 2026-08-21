using System;
using System.Configuration;
using System.Web.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using wwwDesignersGuild.Models.Classes;
using wwwDesignersGuild.Models.Helper;
using wwwDesignersGuild.Models.Visitor;
using System.Data.SqlClient;

namespace wwwDesignersGuild.Controllers
{
    public class ProductRedirectController : Controller 
    {
        // GET: ProductRedirect
        public async Task<ActionResult> Index(string Code, string Source)
        {
            try
            {
                VisitorFactory objVisitorFactory = new VisitorFactory();

                VisitorSelect objVisitorSelect = new VisitorSelect();

                DateTime useDate = dgFramework.DetermineContentDate(Source);

				var cfVerifiedBot = Request.Headers["cf-verified-bot"];
				var cfBotScore = Request.Headers["cf-bot-score"];
				var ja3fingerprints = Request.Headers["cf-ja3-hash"];
				var ja4fingerprints = Request.Headers["cf-ja4-hash"];

				var verifiedBot = false;
				var botScore = 100;

				if (Boolean.TryParse(cfVerifiedBot, out bool v))
				{
					verifiedBot = v;
				}

				if (Int32.TryParse(cfBotScore, out int bs))
				{
					botScore = bs;
				}

				if (verifiedBot)
				{
					dgFramework.setVerifiedBot(ref objVisitorSelect);
				}
                else
				{
					//  Get the session identifier
					if (Session["visitorIdFromDatabase"] == null)
					{
						// Looks for VisitorID using GUID in cookie

						String cookieGuid = dgFramework.getIdentificationFromCookie();

						objVisitorSelect = await dgFramework.identifySession(objVisitorSelect, objVisitorFactory, cookieGuid, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

						// The beneath four can be null...
						Session["memberName"] = objVisitorSelect.FullName;
						Session["memberEmail"] = objVisitorSelect.Email;
						Session["visitorCulture"] = objVisitorSelect.Culture;
						Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
						Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
						Session["memberDisplayTradePrice"] = objVisitorSelect.memberDisplayTradePrice;
						Session["memberTradeDataSourceJson"] = objVisitorSelect.memberTradeDataSourceJson;
						Session["visitorCreatedDT"] = objVisitorSelect.VisitorCreatedDT;

						// And identifySession may have returned a visitorDbId of -1
						if (objVisitorSelect.visitorDbId == -1)
						{
							Session["loginStatus"] = dgLoginStatus.notLoggedIn.ToString();
						}
						else
						{
							// however, if identifySession didn't return -1 then store the visitorDbId in Session.
							Session["visitorIdFromDatabase"] = objVisitorSelect.visitorDbId.ToString();
							Session["memberIdFromDatabase"] = objVisitorSelect.memberId;

							if (objVisitorSelect.FullName == null && objVisitorSelect.Email == null) { }
							else
							{
								Session["loginStatus"] = dgLoginStatus.previousLogin.ToString();
							}
						}
					}
					else
					{
						objVisitorSelect.visitorDbId = Convert.ToInt64(Session["visitorIdFromDatabase"].ToString());
						objVisitorSelect.memberTradeAccountNumber = (string)Session["memberTradeAccountNumber"];
						objVisitorSelect.memberTradeAccountID = (int?)Session["memberTradeAccountID"];
						objVisitorSelect.memberId = (Int32?)Session["memberIdFromDatabase"];
						objVisitorSelect.Email = (string)Session["memberEmail"];
						objVisitorSelect.memberDisplayTradePrice = (Boolean)Session["memberDisplayTradePrice"];
						objVisitorSelect.memberTradeDataSourceJson = (string)Session["memberTradeDataSourceJson"];
						objVisitorSelect.VisitorCreatedDT = (DateTime)Session["visitorCreatedDT"];

						if (Session["visitorCulture"] == null) { }
						else
						{
							objVisitorSelect.Culture = Session["visitorCulture"].ToString();
						}
					}
				}

                // If we have a VisitorID to use, this saves the "Culture" column on [mbr].[Visitor]
                // If not, this creates a new Visitor storing UserAgent, IP and Culture, saves the returned GUID to cookie and gives us the VisitorID
                if (objVisitorSelect.visitorDbId == -1)
                {
                    identificationDefinition identDef = await dgFramework.generateNewVisitorId(objVisitorFactory, dgFramework.getUserAgent(), dgFramework.getIPAddress(), null, Request.Headers["CF-IPCountry"], Request.Headers["CF-Device-Type"]);

                    objVisitorSelect.visitorDbId = identDef.visitorDbId;

                    dgFramework.saveIdentificationToCookie(identDef.userGuid);

                    Session["memberIdFromDatabase"] = objVisitorSelect.memberId;
                    Session["visitorIdFromDatabase"] = objVisitorSelect.visitorDbId.ToString();
                    Session["memberTradeAccountNumber"] = objVisitorSelect.memberTradeAccountNumber;
                    Session["memberTradeAccountID"] = objVisitorSelect.memberTradeAccountID;
                    Session["memberDisplayTradePrice"] = objVisitorSelect.memberDisplayTradePrice;
                    Session["visitorCreatedDT"] = objVisitorSelect.VisitorCreatedDT;

                }

                // Get the CountryCode header from CloudFlare.
                string cfIPCountryCode = Request.Headers["CF-IPCountry"];

                // If we're not in production, permit countryOverride query string parameter.
                if (Convert.ToBoolean(System.Web.Configuration.WebConfigurationManager.AppSettings["productionVersion"]) == true) { }
                else
                {
                    if (Request.QueryString["countryOverride"] == null)
                    {
                        if (cfIPCountryCode == null)
                        {
                            // This should only be hit when developing locally.
                            // You're not in production, not used a countryOverride, not got a CountryCode from CloudFlare.
                            // Won't be showing you any RRPs, and will be given the country awareness prompt.
                            // Unless we fudge it and set cfIPCountryCode to the current URL's countryCode.
                            cfIPCountryCode = "GB";
                        }
                    }
                }

                string result = null;

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ConnectionString))
                {
                    SqlCommand comm = conn.CreateCommand();
                    comm.CommandText = "pub.ProductCode_Redirect";
                    comm.CommandType = System.Data.CommandType.StoredProcedure;

                    comm.Parameters.AddWithValue("@VisitorCulture", objVisitorSelect.Culture);
                    comm.Parameters.AddWithValue("@CountryCode", cfIPCountryCode);
                    comm.Parameters.AddWithValue("@VisitorID", objVisitorSelect.visitorDbId);
                    comm.Parameters.AddWithValue("@Code", Code);
                    comm.Parameters.AddWithValue("@Source", Source);
                    comm.Parameters.AddWithValue("@Date", useDate);

                    conn.Open();

                    var reader = comm.ExecuteReader();

                    while (reader.Read())
                    {
                        result = reader.IsDBNull(0) ? "" : reader.GetString(0);
                    }

                    reader.Close();

                    conn.Close();
                }

                return new RedirectResult(result, false);
            }
            catch (Exception ex)
            {
                NLog.LogManager.GetCurrentClassLogger().Error(ex, "ProductRedirect failure");
                return new RedirectResult("/l0", false);
            }
        }
    }
}