using DapperMapper.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DapperMapper.Attributes
{
    public enum JoinType
    {
        Inner,
        Left,
        Right,
        Cross
    }
    public class LookUp : Attribute
    {
        public string tableName { get; set; }
        /// <summary>
        /// It is the Condition that Join must be created Based on
        /// </summary>
        public string onLookupFiled { get; set; }
        public string onFiled { get; set; }
        public JoinType type { get; set; }
        public string displayColumn { get; set; }
        public LookUp(Type TableName, string DisplayColumn, JoinType Type, string OnLookupFiled, string OnFiled = "")
        {
            tableName = DynamicQuery.TableName(TableName.Name);
            type = Type;
            onLookupFiled = OnLookupFiled;
            onFiled = OnFiled;
            displayColumn = DisplayColumn;
        }
    }
    public class LookUpQueryBuilder
    {
        public string TableName { get; set; }
        public string MainTable { get; set; }
        public string OnLookupFiled { get; set; }
        public JoinType type { get; set; }
        public PropertyInfo Property { get; set; }
        public string OnFiled { get; internal set; }

        public string Quary()
        {
            OnFiled = string.IsNullOrEmpty(OnFiled) ? Property.Name : OnFiled;
            switch (type)
            {
                case JoinType.Inner:
                    return " INNER JOIN " + TableName + " ON " + TableName + "." + OnLookupFiled + " = " + MainTable + "." + OnFiled + " ";
                case JoinType.Left:
                    return " LEFT OUTER JOIN " + TableName + " ON " + TableName + "." + OnLookupFiled + " = " + MainTable + "." + OnFiled + " ";
                case JoinType.Right:
                    return " RIGHT OUTER JOIN " + TableName + " ON " + TableName + "." + OnLookupFiled + " = " + MainTable + "." + OnFiled + " ";
                case JoinType.Cross:
                    return " CROSS JOIN " + TableName + " ";
                default:
                    return "";
            }

        }

    }
}
