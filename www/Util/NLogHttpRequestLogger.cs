using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Web;
using System.Configuration;
using System.Data;
using System.Web.SessionState;

namespace wwwDesignersGuild.Util
{
    public class NLogHttpRequestLogger : IHttpModule
    {
        public static Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes the HttpModule.
        /// </summary>
        /// <param name="application">
        /// ASP.NET application.
        /// </param>
        public void Init(HttpApplication application)
        {
            application.PreRequestHandlerExecute +=
            (new EventHandler(this.PreRequestHandler));
            //application.PostRequestHandlerExecute +=
            //    (new EventHandler(this.PostRequestHandler));
        }

        /// <summary>
        /// Disposes the module.
        /// </summary>
        public void Dispose()
        {
            
        }

        private void PreRequestHandler(object sender, EventArgs args)
        {
            
        }

        private void PostRequestHandler(object sender, EventArgs args)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;

            if (context.Session != null)
            {

                if (context.Session["LOG-VisitorID"] != null)
                {
                    if (!context.Request.FilePath.ToLower().Contains("api/basket/")
                        && !context.Request.FilePath.ToLower().Contains("bundle")
                        && !context.Request.FilePath.ToLower().Contains("image")
                        && !context.Request.FilePath.ToLower().Contains("favicon")
                        && !context.Request.FilePath.ToLower().Contains("content")
                        && !context.Request.FilePath.ToLower().Contains("script")
                        && !context.Request.FilePath.ToLower().Contains("keepsessionalive"))
                    {
                        //Logger.Trace(String.Format("VisitorID: {0}\t\tCulture: {1}\t\tRequested URL: {2}\t\tResponse StatusCode: {3}\t\tRedirect URL: {4}\n",
                        //    context.Application["VisitorID"] == null ? "NULL" : context.Application["VisitorID"].ToString(),
                        //    context.Application["VisitorCulture"] == null ? "NULL" : context.Application["VisitorCulture"].ToString(),
                        //    context.Request.Url.AbsoluteUri,
                        //    context.Response.StatusCode,
                        //    context.Response.RedirectLocation == null ? "N/A" : context.Response.RedirectLocation));

                        DateTime
                            StartDT = context.Session["LOG-StartDT"] == null ? DateTime.Now : (DateTime)context.Session["LOG-StartDT"],
                            EndDT = DateTime.Now;

                        Int16 ResponseCode = Convert.ToInt16(context.Response.StatusCode);

                        Int64 VisitorID = Convert.ToInt64(context.Session["LOG-VisitorID"]);

                        String RequestedURL = context.Request.Url.AbsoluteUri,
                                ResultantURL = context.Response.RedirectLocation,
                                SessionGUID = context.Session["LOG-SessionGUID"] == null ? null : context.Session["LOG-SessionGUID"].ToString(),
                                VisitorIP = context.Session["LOG-VisitorIP"].ToString(),
                                VisitorGUID = context.Session["LOG-VisitorGUID"] == null ? null : context.Session["LOG-VisitorGUID"].ToString(),
                                VisitorName = context.Session["LOG-VisitorName"] == null ? null : context.Session["LOG-VisitorName"].ToString(),
                                VisitorCulture = context.Session["LOG-VisitorCulture"] == null ? null : context.Session["LOG-VisitorCulture"].ToString();

                        Char VisitorSelect = context.Session["LOG-VisitorSelect"] == null ? 'N' : 'Y',
                                VisitorInsert = context.Session["LOG-VisitorInsert"] == null ? 'N' : 'Y',
                                VisitorUpdate = context.Session["LOG-VisitorUpdate"] == null ? 'N' : 'Y';

                        using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
                        {
                            SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO log.RequestLog  (   
                            StartDT, 
                            EndDT, 
                            RequestedURL,
                            ResultantURL,
                            ResponseCode,
                            SessionGUID,
                            VisitorID,
                            VisitorGUID,
                            VisitorName,
                            VisitorCulture,
                            VisitorIP,
                            VisitorSelect,
                            VisitorInsert,
                            VisitorUpdate
                        ) VALUES (
                            @StartDT, 
                            @EndDT, 
                            @RequestedURL,
                            @ResultantURL,
                            @ResponseCode,
                            @SessionGUID,
                            @VisitorID,
                            @VisitorGUID,
                            @VisitorName,
                            @VisitorCulture,
                            @VisitorIP,
                            @VisitorSelect,
                            @VisitorInsert,
                            @VisitorUpdate
                        )", conn)
                            {
                                CommandType = CommandType.Text
                            };

                            cmd.Parameters.AddWithValue("@StartDT", StartDT);
                            cmd.Parameters.AddWithValue("@EndDT", EndDT);
                            cmd.Parameters.AddWithValue("@RequestedURL", RequestedURL);

                            if (ResultantURL == null)
                                cmd.Parameters.AddWithValue("@ResultantURL", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@ResultantURL", ResultantURL);

                            cmd.Parameters.AddWithValue("@ResponseCode", ResponseCode);

                            if (SessionGUID == null)
                                cmd.Parameters.AddWithValue("@SessionGUID", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@SessionGUID", SessionGUID);

                            cmd.Parameters.AddWithValue("@VisitorID", VisitorID);

                            if (VisitorGUID == null)
                                cmd.Parameters.AddWithValue("@VisitorGUID", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@VisitorGUID", VisitorGUID);

                            if (VisitorName == null)
                                cmd.Parameters.AddWithValue("@VisitorName", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@VisitorName", VisitorName);

                            if (VisitorCulture == null)
                                cmd.Parameters.AddWithValue("@VisitorCulture", DBNull.Value);
                            else
                                cmd.Parameters.AddWithValue("@VisitorCulture", VisitorCulture);

                            cmd.Parameters.AddWithValue("@VisitorIP", VisitorIP);
                            
                            cmd.Parameters.AddWithValue("@VisitorSelect", VisitorSelect);
                            cmd.Parameters.AddWithValue("@VisitorInsert", VisitorInsert);
                            cmd.Parameters.AddWithValue("@VisitorUpdate", VisitorUpdate);

                            conn.Open();
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}