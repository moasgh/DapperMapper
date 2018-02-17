using Dapper;
using DapperMapper.Helper;
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
	public static class OpenSql<T> where T : new()
	{
		public static IEnumerable<T> Execute(string quary)
		{
			IEnumerable<T> items = null;
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				items = cn.Query<T>(quary);
			}
			return items;
		}
		public static dynamic DynamicExecute(string quary)
		{
			dynamic items = null;
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				items = cn.Query<T>(quary);
			}
			return items;
		}
	}
}
