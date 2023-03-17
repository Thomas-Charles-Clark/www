using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using wwwDesignersGuild.Models.Banner;
using wwwDesignersGuild.Models.Classes;

namespace wwwDesignersGuild.Models.Database
{
    public class bannerDb
    {
        public cmsBannerDefinition LoadBanners(short countryId, byte languageId, DateTime useDate, bool isTrade, bool platformSwitch)
        {
            var result = new cmsBannerDefinition();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand cmd = new SqlCommand("pub.TemplateBanner_Select", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@CountryID", countryId);
                cmd.Parameters.AddWithValue("@LanguageID", languageId);
                cmd.Parameters.AddWithValue("@Date", useDate);
                cmd.Parameters.AddWithValue("@IsTrade", isTrade);
                cmd.Parameters.AddWithValue("@PlatformSwitch", platformSwitch);

                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    result.JSON = reader.GetString(0);
                }
            }

            return result;
        }
    }
}