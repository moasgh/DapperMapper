using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using DapperMapper.Helper;
using System.Reflection;
using DapperMapper.Attributes;


namespace DapperMapper
{
	public class DataMapper<T> : IRepository<T> where T : new()
	{
		private string _tablename = "";
		public DataMapper(string tablename)
		{
			_tablename = DynamicQuery.TableName(tablename);
		}
		internal PropertyInfo GetColumnID(T Item)
		{
			PropertyInfo[] properties = Item.GetType().GetProperties();
			var _ID = properties.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);
			return _ID;
		}
		internal StringBuilder GetLookUps(T Item, out StringBuilder Columns)
		{
			PropertyInfo[] properties = Item.GetType().GetProperties();
			PropertyInfo[] MapProps = properties.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();
			Columns = new StringBuilder();
			StringBuilder JoinQuary = null;
			int indexcolumns = 1;
			int maxindex = MapProps.Count();
			foreach (var item in MapProps)
			{
				object LookupColumns = item.GetCustomAttribute<LookUp>(false);
				if (LookupColumns != null)
				{
					LookUpQueryBuilder newquary = new LookUpQueryBuilder();
					newquary.OnLookupFiled = LookupColumns.GetType().GetProperty("onLookupFiled")?.GetValue(LookupColumns).ToString();
					newquary.OnFiled = LookupColumns.GetType().GetProperty("onFiled")?.GetValue(LookupColumns).ToString();
					newquary.TableName = LookupColumns.GetType().GetProperty("tableName")?.GetValue(LookupColumns).ToString();
					newquary.type = (JoinType)LookupColumns.GetType().GetProperty("type")?.GetValue(LookupColumns);
					newquary.Property = item;
					newquary.MainTable = _tablename;
					if (JoinQuary == null) JoinQuary = new StringBuilder();
					JoinQuary?.Append(newquary.Quary() + " ");
					string DisplayName = LookupColumns.GetType().GetProperty("displayColumn").GetValue(LookupColumns).ToString();
					Columns.Append(maxindex == indexcolumns ? newquary.TableName + "." + DisplayName + " AS " + item.Name : newquary.TableName + "." + DisplayName + " AS " + item.Name + " , ");
				}
				else
					Columns.Append(maxindex == indexcolumns ? _tablename + "." + item.Name : _tablename + "." + item.Name + " , ");
				indexcolumns++;
			}
			return JoinQuary;
		}
		internal StringBuilder GetLookUps(object Item, out StringBuilder Columns)
		{
			PropertyInfo[] properties = Item.GetType().GetProperties();
			PropertyInfo[] MapProps = properties.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();
			Columns = new StringBuilder();
			StringBuilder JoinQuary = null;
			int indexcolumns = 1;
			int maxindex = MapProps.Count();
			foreach (var item in MapProps)
			{
				object LookupColumns = item.GetCustomAttribute<LookUp>(false);
				if (LookupColumns != null)
				{
					LookUpQueryBuilder newquary = new LookUpQueryBuilder();
					newquary.OnLookupFiled = LookupColumns.GetType().GetProperty("onLookupFiled").GetValue(LookupColumns).ToString();
					newquary.OnFiled = LookupColumns.GetType().GetProperty("onFiled").GetValue(LookupColumns).ToString();
					newquary.TableName = LookupColumns.GetType().GetProperty("tableName").GetValue(LookupColumns).ToString();
					newquary.type = (JoinType)LookupColumns.GetType().GetProperty("type").GetValue(LookupColumns);
					newquary.Property = item;
					newquary.MainTable = _tablename;
					if (JoinQuary == null) JoinQuary = new StringBuilder();
					JoinQuary?.Append(newquary.Quary() + " ");
					string DisplayName = LookupColumns.GetType().GetProperty("displayColumn").GetValue(LookupColumns).ToString();
					Columns.Append(maxindex == indexcolumns ? newquary.TableName + "." + DisplayName : newquary.TableName + "." + DisplayName + " , ");
				}
				else
					Columns.Append(maxindex == indexcolumns ? _tablename + "." + item.Name : _tablename + "." + item.Name);
				indexcolumns++;
			}
			return JoinQuary;
		}

		internal virtual dynamic Mapping(T item)
		{
			return item;
		}
		public T Insert(T entity)
		{
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				var parameters = (object)Mapping(entity);
				cn.Open();
				cn.Insert(_tablename, parameters);
				var Id = GetColumnID(entity);
				if (Id != null)
				{
					dynamic insertetid = cn.ExecuteScalar("SELECT IDENT_CURRENT('" + _tablename + "')");
					if (insertetid != null)
						entity.GetType().GetProperty(Id.Name).SetValue(entity, Convert.ChangeType(insertetid, Id.PropertyType));
				}
				return entity;
			}
		}
		public void Update(T entity)
		{
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				var parameters = (object)Mapping(entity);
				cn.Open();
				cn.Update(_tablename, parameters);
			}
		}
		/// <summary>
		/// Delete will be done based on your property that has the Key attribute
		/// </summary>
		/// <param name="entity"></param>
		public void Delete(T entity)
		{
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				// Get the ID value and Delete the record
				var Id = GetColumnID(entity);
				var val = entity.GetType().GetProperty(Id.Name).GetValue(entity);
				cn.Execute("DELETE FROM " + _tablename + " WHERE " + Id.Name + "=" + val);
			}
		}
		public T FindByID(object idvalue)
		{
			T item = default(T);
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				var Id = GetColumnID(item);
				item = cn.Query<T>("SELECT * FROM " + _tablename + " WHERE " + Id.Name + "=" + idvalue.ToString()).SingleOrDefault();
			}
			return item;
		}
		public long Count()
		{
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				long item = Convert.ToInt64(cn.ExecuteScalar("SELECT Count(*) FROM " + _tablename));
				return item;
			}
		}
		public long Count(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			QueryResult result = DynamicQuery.GetDynamicQuery("SELECT Count(*) ", _tablename, predicate);
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				long item = Convert.ToInt64(cn.ExecuteScalar(result.Sql, (object)result.Param));
				return item;
			}
		}
		public IEnumerable<T> SearchBy(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			IEnumerable<T> items = null;
			StringBuilder Columns;
			StringBuilder JoinQuary = GetLookUps(new T(), out Columns);

			// extract the dynamic sql query and parameters from predicate
			QueryResult result = DynamicQuery.GetDynamicQuery(_tablename, predicate, JoinQuary, Columns);

			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				items = cn.Query<T>(result.Sql, (object)result.Param);
			}

			return items;
		}
		public IEnumerable<T> FullTextSearch(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
		{
			IEnumerable<T> items = null;
			// extract the dynamic sql query and parameters from predicate
			QueryResult result = DynamicQuery.GetDynamicQueryFullTextSearch(_tablename, predicate);

			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();
				items = cn.Query<T>(result.Sql, (object)result.Param);
			}

			return items;
		}
		public IEnumerable<Y> SearchBy<Y>(System.Linq.Expressions.Expression<Func<Y, bool>> predicate)
		{
			try
			{
				IEnumerable<Y> items = null;
				StringBuilder Columns;
				StringBuilder JoinQuary = GetLookUps(new T(), out Columns);

				// extract the dynamic sql query and parameters from predicate
				QueryResult result = DynamicQuery.GetDynamicQuery(_tablename, predicate, JoinQuary, Columns);

				using (IDbConnection cn = ConnectionManager.Connection)
				{
					cn.Open();
					items = cn.Query<Y>(result.Sql, (object)result.Param);
				}

				return items;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		public IEnumerable<T> GetAll()
		{
			IEnumerable<T> items = null;
			StringBuilder columns;
			StringBuilder joinQuary = GetLookUps(new T(), out columns);

			QueryResult result = DynamicQuery.GetDynamicQuery(_tablename, joinQuary, columns);

			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();

				items = cn.Query<T>(result.Sql);
			}

			return items;
		}
		public IEnumerable<Y> GetAll<Y>() where Y : new()
		{
			IEnumerable<Y> items = null;

			StringBuilder columns;
			StringBuilder joinQuary = GetLookUps(new Y(), out columns);

			QueryResult result = DynamicQuery.GetDynamicQuery(_tablename, joinQuary, columns);

			using (IDbConnection cn = ConnectionManager.Connection)
			{
				cn.Open();

				items = cn.Query<Y>(result.Sql);
			}

			return items;
		}

		public void Create()
		{
			using (IDbConnection cn = ConnectionManager.Connection)
			{
				T entity = new T();
				var parameters = (object)Mapping(entity);
				cn.Open();
				var Id = GetColumnID(entity);
				if (Id != null)
				{
					cn.Create(_tablename, parameters);
				}
			}
		}
	}
	public static class DapperExtensions
	{
		public static void Insert(this IDbConnection cnn, string tableName, dynamic param)
		{
			SqlMapper.Query(cnn, DynamicQuery.GetInsertQuery(tableName, param), param);
		}
		public static void Update(this IDbConnection cnn, string tableName, dynamic param)
		{
			SqlMapper.Execute(cnn, DynamicQuery.GetUpdateQuery(tableName, param), param);
		}

		public static void Create(this IDbConnection cnn, string tableName, dynamic param)
		{
			try
			{
				SqlMapper.Query(cnn, DynamicQuery.GetCreateQuery(tableName, param), param);
			}
			catch (SqlException ex)
			{
				throw ex;
			}

		}
	}
}
