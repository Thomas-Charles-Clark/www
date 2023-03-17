using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;


namespace wwwDesignersGuild.Models.Database
{
    public class errorDB
    {
        public Boolean saveToErrorLog(Int64 visitorDbId, String exceptionType, String exceptionMessage, String exceptionSource, String exceptionStackTrace, 
                                        String innerExceptionType, String innerExceptionMessage, String innerExceptionSource, String innerExceptionStackTrace,
                                        String urlOfPage, String userAgent, String ipAddress)
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.LogError", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@VisitorID", visitorDbId);
                cmd.Parameters.AddWithValue("@ErrorDT", DateTime.Now);
                cmd.Parameters.AddWithValue("@ExceptionType", exceptionType);
                cmd.Parameters.AddWithValue("@ExceptionMessage", exceptionMessage ?? "No Message! :(");
                cmd.Parameters.AddWithValue("@ExceptionSource", exceptionSource ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@StackTrace", exceptionStackTrace);
                cmd.Parameters.AddWithValue("@InnerExceptionType", innerExceptionType);
                cmd.Parameters.AddWithValue("@InnerExceptionMessage", innerExceptionMessage);
                cmd.Parameters.AddWithValue("@InnerExceptionSource", innerExceptionSource);
                cmd.Parameters.AddWithValue("@InnerExceptionStackTrace", innerExceptionStackTrace);
                cmd.Parameters.AddWithValue("@URL", urlOfPage);
                cmd.Parameters.AddWithValue("@UserAgent", userAgent);
                cmd.Parameters.AddWithValue("@IP", ipAddress);
                

                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return true;

        }

    }
}