using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Linq;
using System.Reflection;
using System.Data.Linq.Mapping;
using wwwDesignersGuild.Models.Classes;

using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using wwwDesignersGuild.Models.UPS;

namespace wwwDesignersGuild.Models.Database
{
    public partial class databaseDataContext
    {
        public UPSAccessPointLocatorClientKeys FetchUPSAPICredential()
        {
            using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["FlockConnectionString"].ToString()))
            {
				var result = conn.QuerySingle<UPSAccessPointLocatorClientKeys>(
                    @"SELECT Name, 
                             ClientID, 
                             ClientSecret,
                             AccountNumber
                      FROM ups.App 
                      WHERE AppID = 3",
                    null,
                    null,
                    null,
                    CommandType.Text);

				return result;
			}
        }
    }
}