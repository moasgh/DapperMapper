using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperMapper.Helper
{
    public class JColumns
    {
        private static string ParseString(string str)
        {
			Int64 bigintValue;
			bool boolValue;
            Int32 intValue;
            
            double doubleValue;
            DateTime dateValue;
            Guid guidValue;
			
            str = str.Trim();
            // Place checks higher in if-else statement to give higher priority to type.

            if (bool.TryParse(str, out boolValue))
                return "[bit]";
            else if (double.TryParse(str, out doubleValue))
                return "[float]";
            else if (Int32.TryParse(str, out intValue))
                return "[int]";
            else if (Int64.TryParse(str, out bigintValue))
                return "[bigint]";
            else if (DateTime.TryParse(str, out dateValue))
                return "[datetime]";
            else if (Guid.TryParse(str, out guidValue))
                return "[uniqueidentifier]";
            else return "[nvarchar](MAX)";
        }
        public static string ParseString(string str, bool TableExists, string columnname, string tablename)
        {
            if (TableExists)
            {
                object result = Dapper.SqlMapper.ExecuteScalar(ConnectionManager.Connection, "SELECT TYPE_NAME(user_type_id) FROM sys.all_columns where [name] = '" + columnname + "' and object_id = OBJECT_ID(N'" + tablename + "')");
                if (result == null)
                    return ParseString(str);
                else
                    return "[" + result.ToString() + "]" + (result.ToString() == "nvarchar" ? "(MAX)" : "");
            }
            else
            {
                return ParseString(str);
            }
        }
        public static string CheckParsString(string str, string type, bool TableExists, string columnname, string tablename)
        {
            string _type = ParseString(str);
            if (_type == type)
            {
                return str;
            }
            else
            {
                return "Null";
            }
        }
        public string Name { get; set; }
        public string Value { get; set; }
        public string ColumnType { get; set; }
        public bool IsNull { get; set; } = true;
        public string SQLColumn { get { return this.ToString(); } }
        public override string ToString()
        {
            return Name + " " + ColumnType + (IsNull ? " NULL" : " NOT NULL");
        }
    }
    public class JTable
    {

        internal bool Exists = false;
        private string _tablename;
        public string TableName
        {
            get { return _tablename; }
            set
            {
                _tablename = value;
                Exists = Convert.ToBoolean(Dapper.SqlMapper.ExecuteScalar(ConnectionManager.Connection, "IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'" + _tablename + "') AND type in (N'U')) BEGIN SELECT 1 END ELSE BEGIN SELECT 0 END"));
            }
        }
        public Dictionary<string, JColumns> Columns { get; }
        public List<string> Rows { get; set; }
        public int NumberOfRows { get; set; }
        public JColumns ForignKey { get; set; }
        public JColumns Key { get; set; }
        public bool MasterTable { get; set; }
        public JTable()
        {
            Columns = new Dictionary<string, JColumns>();
            Rows = new List<string>();
        }
        public void AddColumn(string name, string value, bool stringtolower)
        {
            if (!Columns.ContainsKey(name))
            {
                JColumns newcol = new JColumns()
                {
                    Name = "[" + name + "]",
                    ColumnType = JColumns.ParseString(value, Exists, name, TableName)
                };
                newcol.Value = ClearValue(value, newcol.ColumnType, name, stringtolower);
                Columns.Add(name, newcol);
            }
            else Columns[name].Value = ClearValue(value, Columns[name].ColumnType, name, stringtolower);
        }
        public void AddColumn(JColumns jcol)
        {
            if (!Columns.ContainsKey(jcol.Name))
            {
                Columns.Add(jcol.Name, jcol);
            }
            else Columns[jcol.Name].Value = jcol.Value;
        }
        private string ClearValue(string value, string coltype, string columnname, bool stringtolower)
        {
            string val = JColumns.CheckParsString(value, coltype, Exists, columnname, TableName);
            return val == "Null" ? "Null" : ("'" + (stringtolower ? val.Replace("\'", "\'\'").Trim().ToLower() : val.Replace("\'", "\'\'").Trim()) + "'");
        }
        public string CreateTableQuary()
        {
            string create = string.Format("IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'" + TableName + "') AND type in (N'U')) BEGIN CREATE TABLE {0} ({1},  CONSTRAINT [PK_" + TableName + "] PRIMARY KEY CLUSTERED  ([" + Key.Name + "] ASC)); END ",
                                 Helper.DynamicQuery.TableName(this.TableName),
                                 string.Join(",", Columns.Values.Select(s => s.SQLColumn)));
            return create;
        }
        public int GetTheNumberOfRow()
        {
            if (Exists == true)
            {
                return Convert.ToInt32(Dapper.SqlMapper.ExecuteScalar(ConnectionManager.Connection, "SELECT MAX(" + TableName + "ID) FROM " + TableName));
            }
            else
                return 0;

        }
    }
    public class JToSql
    {
        public bool StringToLower { get; set; } = false;
        public Dictionary<string, JTable> Tables = new Dictionary<string, JTable>();
        public static string ClearValue(string value)
        {
            return "'" + value.Replace("\'", "\'\'").Trim() + "'";
        }
        public void ExecuteQuary(JObject item, JTable currentTable, string DuplicateCheck = "")
        {
            if (item == null) return;
            if (!Tables.ContainsKey(currentTable.TableName))
            {
                Tables.Add(currentTable.TableName, currentTable);
            }
            JColumns _key = new Helper.JColumns()
            {
                Name = currentTable.TableName + "ID",
                ColumnType = "BigInt",
                IsNull = false
            };
            _key.Value = ((currentTable.NumberOfRows == 0 ? currentTable.NumberOfRows = currentTable.GetTheNumberOfRow() : currentTable.NumberOfRows) + 1).ToString();
            currentTable.Key = _key;
            var valueArray = item.Properties();

            foreach (var interItem in valueArray)
            {
                try
                {
                    string innerobject = interItem.Value.ToString();

                    var valueobject = JToken.Parse(innerobject);
                    if (valueobject is JObject)
                    {
                        JObject innerdata = (JObject)valueobject;
                        var values = innerdata.Values();
                        var innerItemValues = innerdata.Properties();
                        if (innerItemValues.Count() > 1)
                        {
                            if (Tables.ContainsKey(interItem.Name))
                            {
                                Tables[interItem.Name].ForignKey = new Helper.JColumns()
                                {
                                    Name = currentTable.Key.Name,
                                    Value = currentTable.Key.Value,
                                    ColumnType = currentTable.Key.ColumnType,
                                    IsNull = currentTable.Key.IsNull
                                };
                                ExecuteQuary(innerdata, Tables[interItem.Name]);
                            }
                            else
                                ExecuteQuary(innerdata, new JTable()
                                {
                                    TableName = interItem.Name,
                                    ForignKey = new Helper.JColumns()
                                    {
                                        Name = currentTable.Key.Name,
                                        Value = currentTable.Key.Value,
                                        ColumnType = currentTable.Key.ColumnType,
                                        IsNull = currentTable.Key.IsNull
                                    }
                                });
                        }
                        else
                        {
                            foreach (var innerItemValue in innerItemValues)
                            {
                                currentTable.AddColumn(interItem.Name, innerItemValue.Value.ToString(), StringToLower);
                            }
                        }

                    }
                    else if (valueobject is JArray)
                    {
                        JArray valueJarray = (JArray)valueobject;
                        foreach (JObject innerdata in valueJarray)
                        {
                            if (Tables.ContainsKey(interItem.Name))
                            {
                                Tables[interItem.Name].ForignKey = new Helper.JColumns()
                                {
                                    Name = currentTable.Key.Name,
                                    Value = currentTable.Key.Value,
                                    ColumnType = currentTable.Key.ColumnType,
                                    IsNull = currentTable.Key.IsNull
                                };
                                ExecuteQuary(innerdata, Tables[interItem.Name]);
                            }
                            else
                                ExecuteQuary(innerdata, new JTable()
                                {
                                    TableName = interItem.Name,
                                    ForignKey = new Helper.JColumns()
                                    {
                                        Name = currentTable.Key.Name,
                                        Value = currentTable.Key.Value,
                                        ColumnType = currentTable.Key.ColumnType,
                                        IsNull = currentTable.Key.IsNull
                                    }
                                });
                        }
                    }
                    else
                    {
                        currentTable.AddColumn(interItem.Name, interItem.Value.ToString(), StringToLower);
                    }
                }
                catch
                {
                    currentTable.AddColumn(interItem.Name, interItem.Value.ToString(), StringToLower);
                }
            }

            if (currentTable.ForignKey != null) currentTable.AddColumn(currentTable.ForignKey);
            currentTable.AddColumn(currentTable.Key);
            currentTable.AddColumn("ModifiedDate", DateTime.Now.ToString(), StringToLower);

            if (currentTable.Exists == false)
            {
                Dapper.SqlMapper.Execute(ConnectionManager.Connection, currentTable.CreateTableQuary(), commandType: System.Data.CommandType.Text);
                currentTable.Exists = true;
            }

            string insert = "";
            if (!string.IsNullOrEmpty(DuplicateCheck) && currentTable.MasterTable)
            {
                string Detailsinsert = string.Format("INSERT INTO {0} ({1})  VALUES ({2});",
                                 Helper.DynamicQuery.TableName(currentTable.TableName),
                                 string.Join(",", currentTable.Columns.Values.Select(s => s.Name)),
                                 string.Join(",", currentTable.Columns.Values.Select(s => s.Value)));
                currentTable.Rows.Insert(0, Detailsinsert);

                if (!DuplicateCheck.Contains(","))
                {
                    insert = string.Format("IF  NOT EXISTS (SELECT * FROM " + currentTable.TableName + " WHERE " + DuplicateCheck + " = " + currentTable.Columns[DuplicateCheck].Value + " ) BEGIN \n {0} \n END",
                                 string.Join("\n", currentTable.Rows));
                }
                else
                {
                    List<string> DCLIST = new List<string>();
                    foreach (var dck in DuplicateCheck.Split(','))
                    {
                        DCLIST.Add(dck + " = " + currentTable.Columns[dck].Value);
                    }

                    string DubbleCheckConditions = string.Format("( {0} )", string.Join(" and ", DCLIST));

                    insert = string.Format("IF  NOT EXISTS (SELECT * FROM " + currentTable.TableName + " WHERE " + DubbleCheckConditions + " ) BEGIN \n {0} \n END",
                                 string.Join("\n", currentTable.Rows));
                    //Console.Write(insert);
                }
                //currentTable.Rows.Reverse();

                try
                {
                    if (!DuplicateCheck.Contains(","))
                    {
                        Console.Write(" ENTITY : " + currentTable.Columns[DuplicateCheck].Value);
                    }
                    else
                    {
                        List<string> DCLIST = new List<string>();
                        foreach (var dck in DuplicateCheck.Split(','))
                        {
                            DCLIST.Add(currentTable.Columns[dck].Value);
                        }
                        Console.Write(" ENTITY : " + string.Join(",", DCLIST));
                    }

                    int result = Dapper.SqlMapper.Execute(ConnectionManager.Connection, insert, commandType: System.Data.CommandType.Text);
                    Console.Write(" => " + (result == -1 ? "Row Exists" : result.ToString()) + " Row Effected" + Environment.NewLine);
                    currentTable.Rows.Clear();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                currentTable.NumberOfRows++;
            }
            else
            {

                string Detailsinsert = string.Format("INSERT INTO {0} ({1})  VALUES ({2});",
                                 Helper.DynamicQuery.TableName(currentTable.TableName),
                                 string.Join(",", currentTable.Columns.Values.Select(s => s.Name)),
                                 string.Join(",", currentTable.Columns.Values.Select(s => s.Value)));
                currentTable.NumberOfRows++;
                Tables.Where(s => s.Value.MasterTable == true).FirstOrDefault().Value.Rows.Add(Detailsinsert);
            }
            //currentTable.Rows.Add(insert);
        }
    }
}
