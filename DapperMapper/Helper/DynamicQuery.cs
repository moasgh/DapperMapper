using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Dynamic;
using System.Reflection;
using System.Linq.Expressions;
using DapperMapper.Attributes;
using Newtonsoft.Json.Linq;

namespace DapperMapper.Helper
{
	internal sealed class DynamicQuery
	{
		private static string GetSqlType(PropertyInfo col)
		{
			if (typeof(Int64) == col.PropertyType)
				return "[BigInt]";
			else if (typeof(Byte[]) == col.PropertyType)
				return "[VarBinary]";
			else if (typeof(Boolean) == col.PropertyType)
				return "[Bit]";
			else if (typeof(String) == col.PropertyType)
				return "[NVarChar](MAX)";
			else if (typeof(Char[]) == col.PropertyType)
				return "[NVarChar](1000)";
			else if (typeof(DateTime) == col.PropertyType)
				return "[DateTime]";
			else if (typeof(DateTimeOffset) == col.PropertyType)
				return "[DateTimeOffset]";
			else if (typeof(Decimal) == col.PropertyType)
				return "[Decimal]";
			else if (typeof(Double) == col.PropertyType)
				return "[Float]";
			else if (typeof(Int32) == col.PropertyType)
				return "[Int]";
			else if (typeof(Guid) == col.PropertyType)
				return "[UniqueIdentifier]";
			else if (typeof(Byte) == col.PropertyType)
				return "[TinyInt]";
			else if (typeof(TimeSpan) == col.PropertyType)
				return "[Timestamp]";
			else if (typeof(Object) == col.PropertyType)
				return "[Variant]";
			else if (typeof(Int16) == col.PropertyType)
				return "[SmallInt]";
			else if (typeof(Single) == col.PropertyType)
				return "[Real]";
			else if (typeof(float) == col.PropertyType)
				return "[Float]";
			return "[Nvarchar](MAX)";
		}
		private static string GetSqliteType(PropertyInfo col)
		{
			if (typeof(Int64) == col.PropertyType)
				return "[INTEGER]";
			else if (typeof(Byte[]) == col.PropertyType)
				return "[BLOB]";
			else if (typeof(Boolean) == col.PropertyType)
				return "[INTEGER]";
			else if (typeof(String) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(Char[]) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(DateTime) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(DateTimeOffset) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(Decimal) == col.PropertyType)
				return "[NUMBER]";
			else if (typeof(Double) == col.PropertyType)
				return "[REAL]";
			else if (typeof(Int32) == col.PropertyType)
				return "[INTEGER]";
			else if (typeof(Guid) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(Byte) == col.PropertyType)
				return "[INTEGER]";
			else if (typeof(TimeSpan) == col.PropertyType)
				return "[TEXT]";
			else if (typeof(Object) == col.PropertyType)
				return "[BLOB]";
			else if (typeof(Int16) == col.PropertyType)
				return "[INTEGER]";
			else if (typeof(Single) == col.PropertyType)
				return "[REAL]";
			else if (typeof(float) == col.PropertyType)
				return "[REAL]";
			else if (typeof(float) == col.PropertyType)
				return "[REAL]";
			else if (typeof(Enum) == col.PropertyType)
					return "[INTEGER]";
			return "[TEXT]";
		}
		public static string TableName(string tablename)
		{
			return tablename.Contains(".") ? tablename.Split('.')[0] + "." + "[" + tablename.Split('.')[1] + "]" : "[" + tablename + "]";
		}
		public static string GetCreateQuery(string tableName, dynamic item)
		{
			PropertyInfo[] props = item.GetType().GetProperties();

			PropertyInfo[] MapProps = props.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();

			PropertyInfo key = MapProps.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);
			PropertyInfo identitykey = null;
			if (key.CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value.ToString().ToLower() == "true")
				identitykey = key;
			//extract the key column from insert
			IEnumerable<PropertyInfo> _Props = GetColumns(MapProps);
			List<string> columns = new List<string>();
			if (identitykey != null)
				columns.Add(string.Format("[{0}] {1} {2} {3}", identitykey.Name, GetSqlType(identitykey), "IDENTITY(1,1)", "NOT NULL"));
			foreach (var col in _Props)
			{
				if (col.Name == key.Name)
					columns.Add(string.Format("[{0}] {1} {2} {3}", col.Name, GetSqlType(col), "", "NOT NULL"));
				else if (col.Name != key.Name)
					columns.Add(string.Format("[{0}] {1} {2} {3}", col.Name, GetSqlType(col), "", "NULL"));

			}

			string insert = string.Format(@"CREATE TABLE {0} ({1}, CONSTRAINT[PK_{3}] PRIMARY KEY CLUSTERED
			(
			   [{2}] ASC
			)WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]
			) ON[PRIMARY] TEXTIMAGE_ON[PRIMARY]",
								 tableName,
								 string.Join(",", columns),
								 key.Name,
								 tableName.Replace("[", "").Replace("]", ""));
			return insert;
		}
		public static string GetCreateQuerySqlite(string tableName, dynamic item)
		{
			PropertyInfo[] props = item.GetType().GetProperties();

			PropertyInfo[] MapProps = props.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();

			PropertyInfo key = MapProps.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);
			PropertyInfo identitykey = null;
			if (key.CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value.ToString().ToLower() == "true")
				identitykey = key;
			//extract the key column from insert
			IEnumerable<PropertyInfo> _Props = GetColumns(MapProps);
			List<string> columns = new List<string>();
			if (identitykey != null)
				columns.Add(string.Format("[{0}] {1} {2} {3}", identitykey.Name, GetSqliteType(identitykey), "NOT NULL" , " PRIMARY KEY AUTOINCREMENT"));
			foreach (var col in _Props)
			{
				if (col.Name == key.Name)
					columns.Add(string.Format("[{0}] {1} {2} {3}", col.Name, GetSqliteType(col), "", "NOT NULL"));
				else if (col.Name != key.Name)
					columns.Add(string.Format("[{0}] {1} {2} {3}", col.Name, GetSqliteType(col), "", ""));

			}

			string insert = string.Format(@"CREATE TABLE {0} ({1});",
								 tableName,
								 string.Join(",", columns));
			return insert;
		}
		/// <summary>
		/// Gets the insert query.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		/// The Sql query based on the item properties.
		/// </returns>
		public static string GetInsertQuery(string tableName, dynamic item)
		{
			PropertyInfo[] props = item.GetType().GetProperties();

			PropertyInfo[] MapProps = props.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();

			PropertyInfo identitykey = MapProps.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);
			if (identitykey.CustomAttributes.FirstOrDefault().ConstructorArguments.FirstOrDefault().Value.ToString().ToLower() == "false")
				identitykey = null;
			//extract the key column from insert
			IEnumerable<PropertyInfo> _Props = GetColumns(MapProps);
			string[] columns;
			if (identitykey != null)
			{
				columns = _Props.Select(p => p.Name).Where(s => s != identitykey.Name).ToArray();
			}
			else
				columns = _Props.Select(p => p.Name).ToArray();


			//var guidmape = props.FirstOrDefault(p => p.GetCustomAttributes(typeof(GuidMap), false).Count() == 1);
			//GuidMap guidmap = (GuidMap)guidmape.GetCustomAttribute(typeof(GuidMap));
			//string _Guidmap = guidmap != null ? guidmap.dBName : "ID";

			//string insert = string.Format("INSERT INTO {0} ({1}) OUTPUT inserted.{2} VALUES (@{3})",
			//                     tableName,
			//                     string.Join(",", columns),
			//                     _Guidmap,
			//                     string.Join(",@", columns));
			string insert = string.Format("INSERT INTO {0} ({1})  VALUES (@{2})",
								 tableName,
								 string.Join(",", columns),
								 string.Join(",@", columns));
			return insert;
		}
		/// <summary>
		/// Gets the update query.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="item">The item.</param>
		/// <returns>
		/// The Sql query based on the item properties.
		/// </returns>
		public static string GetUpdateQuery(string tableName, dynamic item)
		{


			PropertyInfo[] props = item.GetType().GetProperties();

			PropertyInfo[] MapProps = props.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();


			// get the ID
			var identitykey = props.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);

			IEnumerable<PropertyInfo> _Props = GetColumns(props);
			string[] columns;
			if (identitykey != null)
			{
				columns = _Props.Select(p => p.Name).Where(s => s != identitykey.Name).ToArray();
			}
			else
				columns = _Props.Select(p => p.Name).ToArray();
			//string[] columns = _Props.Select(p => p.Name).ToArray();

			var parameters = columns.Select(name => name + "=@" + name).ToList();

			//var guidmape = props.FirstOrDefault(p => p.GetCustomAttributes(typeof(GuidMap), false).Count() == 1);
			//GuidMap guidmap = (GuidMap)guidmape.GetCustomAttribute(typeof(GuidMap));
			//string _Guidmap = guidmap != null ? guidmap.dBName : "ID";

			string update = string.Format("UPDATE {0} SET {1} WHERE {2}=@{3}", tableName, string.Join(",", parameters), identitykey.Name, identitykey.Name);


			return update;
		}

		/// <summary>
		/// Gets the dynamic query.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="expression">The expression.</param>
		/// <returns>A result object with the generated sql and dynamic params.</returns>
		public static QueryResult GetDynamicQuery<T>(string tableName, Expression<Func<T, bool>> expression, StringBuilder JoinQuary = null, StringBuilder Columns = null)
		{
			//expression.Compile();
			var queryProperties = new List<QueryParameter>();
			var t = expression.Compile();

			IDictionary<string, Object> expando = new ExpandoObject();
			var builder = new StringBuilder();
			if (expression != null)
			{
				if (expression.Body is BinaryExpression)
				{
					var body = (BinaryExpression)expression.Body;
					// walk the tree and build up a list of query parameter objects
					// from the left and right branches of the expression tree
					WalkTree(body, ExpressionType.Default, ref queryProperties);
				}
				if (expression.Body is MethodCallExpression)
				{
					MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
					//MethodInfo method = methodCallExpression.Method;
					var source = methodCallExpression.Arguments[0];
					WalkTree(methodCallExpression, ExpressionType.Default, ref queryProperties);
					// queryProperties.Add(new QueryParameter());
				}
			}

			// PropertyInfo[] props = item.GetType().GetProperties();

			// convert the query parms into a SQL string and dynamic property object
			if (Columns != null)
				builder.AppendLine("SELECT " + Columns.ToString() + " FROM ");
			else
				builder.AppendLine("SELECT * FROM ");

			builder.Append(tableName);

			if (JoinQuary != null)
			{
				builder.Append(JoinQuary.ToString());
			}

			if (expression != null)
			{
				builder.AppendLine(" WHERE ");

				for (int i = 0; i < queryProperties.Count(); i++)
				{
					QueryParameter item = queryProperties[i];
					string VariablePropertyName = "";
					if (expando.ContainsKey(item.PropertyName))
					{
						VariablePropertyName = item.PropertyName + i;
					}
					else
						VariablePropertyName = item.PropertyName;
					if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0)
					{
						builder.Append(string.Format("{0} {1} {2} @{3} ", item.LinkingOperator, item.PropertyName,
													 item.QueryOperator, VariablePropertyName));
					}
					else
					{
						builder.Append(string.Format("{0} {1} @{2} ", item.PropertyName, item.QueryOperator, VariablePropertyName));
					}

					expando[VariablePropertyName] = item.PropertyValue;
				}
			}

			return new QueryResult(builder.ToString().TrimEnd(), expando);
		}
		public static QueryResult GetDynamicQuery(string tableName, StringBuilder JoinQuary, StringBuilder Columns)
		{
			//expression.Compile();
			var builder = new StringBuilder();
			IDictionary<string, Object> expando = new ExpandoObject();
			// PropertyInfo[] props = item.GetType().GetProperties();

			// convert the query parms into a SQL string and dynamic property object
			if (Columns != null && !string.IsNullOrEmpty(Columns.ToString()))
				builder.AppendLine("SELECT " + Columns.ToString() + " FROM ");
			else
				builder.AppendLine("SELECT * FROM ");

			builder.Append(tableName);

			if (JoinQuary != null)
			{
				builder.Append(JoinQuary.ToString());
			}

			return new QueryResult(builder.ToString().TrimEnd(), expando);
		}

		public static QueryResult GetDynamicQueryFullTextSearch<T>(string tableName, Expression<Func<T, bool>> expression)
		{
			//expression.Compile();
			var queryProperties = new List<QueryParameter>();
			var t = expression.Compile();

			IDictionary<string, Object> expando = new ExpandoObject();
			var builder = new StringBuilder();

			if (expression.Body is MethodCallExpression)
			{
				MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
				//MethodInfo method = methodCallExpression.Method;
				var source = methodCallExpression.Arguments[0];
				WalkTreeFullTextSearch(methodCallExpression, ExpressionType.Default, ref queryProperties);
				// queryProperties.Add(new QueryParameter());
			}
			else
			{
				return new QueryResult(builder.ToString().TrimEnd(), expando);
			}
			// convert the query parms into a SQL string and dynamic property object
			builder.Append("SELECT * FROM ");
			builder.Append(tableName);
			builder.Append(" WHERE ");

			for (int i = 0; i < queryProperties.Count(); i++)
			{
				QueryParameter item = queryProperties[i];

				if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0)
				{
					builder.Append(string.Format("{0} {2}({1},@{1})  ", item.LinkingOperator, item.PropertyName,
												 item.QueryOperator));
				}
				else
				{
					builder.Append(string.Format("{1}({0},@{0}) ", item.PropertyName, item.QueryOperator));
				}

				expando[item.PropertyName] = item.PropertyValue;
			}

			return new QueryResult(builder.ToString().TrimEnd(), expando);
		}

		public static QueryResult GetDynamicQuery<T>(string SelectStatement, string tableName, Expression<Func<T, bool>> expression)
		{

			var queryProperties = new List<QueryParameter>();
			IDictionary<string, Object> expando = new ExpandoObject();
			var builder = new StringBuilder();

			if (expression.Body is BinaryExpression)
			{
				var body = (BinaryExpression)expression.Body;
				// walk the tree and build up a list of query parameter objects
				// from the left and right branches of the expression tree
				WalkTree(body, ExpressionType.Default, ref queryProperties);
			}
			if (expression.Body is MethodCallExpression)
			{
				MethodCallExpression methodCallExpression = (MethodCallExpression)expression.Body;
				//MethodInfo method = methodCallExpression.Method;
				var source = methodCallExpression.Arguments[0];
				WalkTree(methodCallExpression, ExpressionType.Default, ref queryProperties);
				// queryProperties.Add(new QueryParameter());
			}
			// convert the query parms into a SQL string and dynamic property object
			builder.Append(SelectStatement + " FROM ");
			builder.Append(tableName);
			builder.Append(" WHERE ");

			for (int i = 0; i < queryProperties.Count(); i++)
			{
				QueryParameter item = queryProperties[i];

				if (!string.IsNullOrEmpty(item.LinkingOperator) && i > 0)
				{
					builder.Append(string.Format("{0} {1} {2} @{1} ", item.LinkingOperator, item.PropertyName,
												 item.QueryOperator));
				}
				else
				{
					builder.Append(string.Format("{0} {1} @{0} ", item.PropertyName, item.QueryOperator));
				}

				expando[item.PropertyName] = item.PropertyValue;
			}

			return new QueryResult(builder.ToString().TrimEnd(), expando);
		}


		/// <summary>
		/// Walks the tree.
		/// </summary>
		/// <param name="body">The body.</param>
		/// <param name="linkingType">Type of the linking.</param>
		/// <param name="queryProperties">The query properties.</param>
		private static void WalkTree(BinaryExpression body, ExpressionType linkingType,
									 ref List<QueryParameter> queryProperties)
		{
			if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
			{
				string propertyName = GetPropertyName(body);
				dynamic propertyValue = body.Right;
				string opr = GetOperator(body.NodeType);
				string link = GetOperator(linkingType);
				object value = GetValue(propertyValue);
				queryProperties.Add(new QueryParameter(link, propertyName, value, opr));
			}
			else
			{
				if (body.Left is BinaryExpression)
					WalkTree((BinaryExpression)body.Left, body.NodeType, ref queryProperties);
				else if (body.Left is MethodCallExpression)
					WalkTree((MethodCallExpression)body.Left, body.NodeType, ref queryProperties);

				if (body.Right is BinaryExpression)
					WalkTree((BinaryExpression)body.Right, body.NodeType, ref queryProperties);
				else if (body.Right is MethodCallExpression)
					WalkTree((MethodCallExpression)body.Right, body.NodeType, ref queryProperties);
			}
		}

		private static void WalkTreeFullTextSearch(MethodCallExpression body, ExpressionType linkingType,
								  ref List<QueryParameter> queryProperties)
		{
			if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
			{
				string propertyName = GetPropertyName(body);
				string opr = "";
				dynamic propertyValue = body.Arguments[0];
				object value = GetValue(propertyValue);
				if (body.Method.Name == "Contains")
				{
					value = "'" + value.ToString() + "'";
					opr = "Contains";
				}


				string link = GetOperator(linkingType);

				queryProperties.Add(new QueryParameter(link, propertyName, value, opr));
			}
		}

		private static void WalkTree(MethodCallExpression body, ExpressionType linkingType,
									ref List<QueryParameter> queryProperties)
		{
			if (body.NodeType != ExpressionType.AndAlso && body.NodeType != ExpressionType.OrElse)
			{
				string propertyName = GetPropertyName(body);
				string opr = "";
				dynamic propertyValue = body.Arguments[0];
				object value = GetValue(propertyValue);
				if (body.Method.Name == "Contains")
				{
					value = "%" + value.ToString() + "%";
					opr = "LIKE";
				}
				else if (body.Method.Name == "StartsWith")
				{
					value = "" + value.ToString() + "%";
					opr = "LIKE";
				}
				else if (body.Method.Name == "EndsWith")
				{
					value = "%" + value.ToString() + "";
					opr = "LIKE";
				}

				string link = GetOperator(linkingType);

				queryProperties.Add(new QueryParameter(link, propertyName, value, opr));
			}
		}

		/// <summary>
		/// MemberExpression GetValue 
		/// </summary>
		/// <param name="member"></param>
		/// <returns></returns>
		private static object ExpressionGetValue(MemberExpression member)
		{
			var objectMember = Expression.Convert(member, typeof(object));

			var getterLambda = Expression.Lambda<Func<object>>(objectMember);

			var getter = getterLambda.Compile();

			return getter();
		}
		/// <summary>
		/// Get value Of expression
		/// </summary>
		/// <param name="propertyValue"></param>
		/// <returns></returns>
		private static object GetValue(dynamic propertyValue)
		{
			object value;
			if (propertyValue is UnaryExpression)
			{
				UnaryExpression ue = propertyValue as UnaryExpression;
				dynamic unaryepressionvalue = ue.Operand;
				value = GetValue(unaryepressionvalue);
			}
			else if (propertyValue is MemberExpression)
			{
				value = ExpressionGetValue(propertyValue);
			}
			else if (propertyValue is MethodCallExpression)
			{
				MethodCallExpression MCE = propertyValue as MethodCallExpression;
				if (MCE.Method.Name == "ToString")
				{
					value = ExpressionGetValue(MCE.Object as MemberExpression);
				}
				else
				{
					value = "";
				}
			}
			else
			{
				value = propertyValue.Value;
			}
			return value;

		}
		/// <summary>
		/// Check All The Columns and Evaluate Attributes for mapping
		/// </summary>
		/// <param name="properties"></param>
		/// <returns></returns>
		private static IEnumerable<PropertyInfo> GetColumns(PropertyInfo[] properties)
		{
			PropertyInfo[] MapProps = properties.Where(p => p.GetCustomAttributes(typeof(NotMap), false).Count() == 0).ToArray();
			var identitykey = MapProps.FirstOrDefault(p => p.GetCustomAttributes(typeof(Key), false).Count() == 1);
			// var guidmape = properties.FirstOrDefault(p => p.GetCustomAttributes(typeof(GuidMap), false).Count() == 1);
			IEnumerable<PropertyInfo> _Props;
			if (identitykey != null)
			{
				Key checkidentity = (Key)identitykey.GetCustomAttribute(typeof(Key));
				if (checkidentity.isIdentity == true)
					_Props = MapProps.Where(p => p.Name != identitykey.Name);
				else
					_Props = MapProps;
			}
			else
			{
				_Props = MapProps;
			}

			return _Props;
		}

		/// <summary>
		/// Gets the name of the property.
		/// </summary>
		/// <param name="body">The body.</param>
		/// <returns>The property name for the property expression.</returns>
		private static string GetPropertyName(BinaryExpression body)
		{
			string propertyName = body.Left.ToString().Split(new char[] { '.' })[1];

			if (body.Left.NodeType == ExpressionType.Convert)
			{
				// hack to remove the trailing ) when convering.
				propertyName = propertyName.Replace(")", string.Empty);
			}

			return propertyName;
		}

		private static string GetPropertyName(MethodCallExpression body)
		{
			string propertyName = "";
			//remove . 
			propertyName = body.Object.ToString().Split('.')[1];
			return propertyName;
		}

		/// <summary>
		/// Gets the operator.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>
		/// The expression types SQL server equivalent operator.
		/// </returns>
		/// <exception cref="System.NotImplementedException"></exception>
		private static string GetOperator(ExpressionType type)
		{
			switch (type)
			{
				case ExpressionType.Equal:
					return "=";
				case ExpressionType.NotEqual:
					return "!=";
				case ExpressionType.LessThan:
					return "<";
				case ExpressionType.LessThanOrEqual:
					return "<=";
				case ExpressionType.GreaterThanOrEqual:
					return ">=";
				case ExpressionType.GreaterThan:
					return ">";
				case ExpressionType.AndAlso:
				case ExpressionType.And:
					return "AND";
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					return "OR";
				case ExpressionType.Default:
					return string.Empty;
				default:
					throw new NotImplementedException();
			}
		}
	}

	/// <summary>
	/// Class that models the data structure in coverting the expression tree into SQL and Params.
	/// </summary>
	internal class QueryParameter
	{
		public string LinkingOperator { get; set; }
		public string PropertyName { get; set; }
		public object PropertyValue { get; set; }
		public string QueryOperator { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="QueryParameter" /> class.
		/// </summary>
		/// <param name="linkingOperator">The linking operator.</param>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <param name="queryOperator">The query operator.</param>
		internal QueryParameter(string linkingOperator, string propertyName, object propertyValue, string queryOperator)
		{
			this.LinkingOperator = linkingOperator;
			this.PropertyName = propertyName;
			this.PropertyValue = propertyValue;
			this.QueryOperator = queryOperator;
		}
	}
}
