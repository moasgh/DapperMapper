using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
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
		public enum Provider
		{
			SqlClient = 0,
			Sqlite = 1
		}
		public static ConnectionType CnType = ConnectionType.AppConfig;
		public static Provider DataProvider = Provider.SqlClient;
		internal static string ConnectionString = "";
		public static void Set(ConnectionType cnType, string connectionstring = "" , Provider dataProvider = Provider.SqlClient )
		{
			CnType = cnType;
			DataProvider = dataProvider;
			ConnectionString = connectionstring;
		}
		internal static IDbConnection Connection
		{
			get
			{
				if (DataProvider == Provider.SqlClient)
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
				else if(DataProvider == Provider.Sqlite)
				{
					SQLiteConnection con = null;
					if (ConnectionManager.CnType == ConnectionManager.ConnectionType.AppConfig)
					{
						con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["Repositoryconn"].ConnectionString);
					}
					else if (ConnectionManager.CnType == ConnectionManager.ConnectionType.UserConfig)
					{
						con = new SQLiteConnection(ConnectionManager.ConnectionString);
					}
					return con;
				}
				else
				{
					return null;
				}
				
			}
		}
	}
}
