using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using wwwDesignersGuild.Models.UPS;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        public UPSAPICredential FetchUPSAPICredential()
        {
            var result = new UPSAPICredential();

            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
                SqlCommand comm = conn.CreateCommand();
                comm.CommandType = System.Data.CommandType.Text;

                comm.CommandText = "SELECT UserID, Password FROM ups.APICredential WHERE APICredentialID = 1";

                conn.Open();

                var reader = comm.ExecuteReader();

                while (reader.Read())
                {
                    result.UserID = reader.GetString(0);
                    result.Password = reader.GetString(1);
                }

                conn.Close();
            }

            return result;
        }
    }
}