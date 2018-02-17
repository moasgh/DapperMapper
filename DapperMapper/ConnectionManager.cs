using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperMapper
{
	public static class ConnectionManager
	{
		public enum ConnectionType
		{
			AppConfig = 0,
			UserConfig = 1
		}
		public static ConnectionType CnType = ConnectionType.AppConfig;
		internal static string ConnectionString = "";
		public static void Set(ConnectionType cnType, string connectionstring = "")
		{
			CnType = cnType;
			ConnectionString = connectionstring;
		}
		internal static IDbConnection Connection
		{
			get
			{
				SqlConnection con = null;
				if (ConnectionManager.CnType == ConnectionManager.ConnectionType.AppConfig)
				{
					con = new SqlConnection(ConfigurationManager.ConnectionStrings["Repositoryconn"].ConnectionString);
				}
				else if (ConnectionManager.CnType == ConnectionManager.ConnectionType.UserConfig)
				{
					con = new SqlConnection(ConnectionManager.ConnectionString);
				}
				return con;
			}
		}
	}
}
