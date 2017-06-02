using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using DapperMapper.Helper;
using System.Text.RegularExpressions;

namespace DapperMapper
{
    public class Jsonam
    {
        public string JsonPath { get; set; }
        private string jsonValue;
        private string sqlTables;

        private string SqlTables
        {
            get { return sqlTables; }
            set { sqlTables = value; }
        }
        private string JsonValue
        {
            get { return jsonValue; }
            set
            {

                jsonValue = value;
            }
        }

        private JObject JsonFormat(StreamReader sr = null)
        {
            try
            {
                JObject data = null;
                if (sr != null)
                {
                    using (JsonTextReader reader = new JsonTextReader(sr))
                    {
                        data = (JObject)JToken.ReadFrom(reader);
                    }
                }
                else
                {
                    if (jsonValue.EndsWith(",")) JsonValue = JsonValue.Remove(JsonValue.Length - 1, 1);
                    var objectids = Regex.Matches(JsonValue, "objectid\\(\\\"[\\S\\s]*\\\"\\)\\,[\\s]*\\\"", RegexOptions.IgnoreCase);
                    foreach (Match item in objectids)
                    {
                        string re = Regex.Replace(item.Value, "\\,[\\s]*\\\"", "");
                        JsonValue = JsonValue.Replace(re, Regex.Match(re, "\\\"[\\S\\s]*\\\"").Value);
                    }
                    if (jsonValue.StartsWith("{"))
                    {
                        data = (JObject)JToken.Parse(JsonValue);
                    }
                }
                return data;
            }
            catch
            {
                return null;
            }

        }
        public int ProcessedRecords { get; set; }
        public void InsertToSql(string TableName, string path, string DuplicateCheckingColumn = "", int numberofrow = -1, int skip = -1, int end = -1)
        {
            Jsonam j = new Jsonam();
            int index = numberofrow;
            JToSql JS = new JToSql();
            JTable JT = new JTable();
            JT.TableName = TableName;
            JT.MasterTable = true;
            string line = "";
            int count = 0;
            int countskip = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    try
                    {
                        j.JsonValue = line;
                        //Console.Write("Processing :{0}", line + Environment.NewLine);
                        JObject currententity = j.JsonFormat();
                        if (currententity != null)
                        {
                            if (countskip <= skip)
                            {
                                Console.Write("skiped :" + countskip + Environment.NewLine);
                                countskip++;
                                continue;
                            }
                            else
                            {
                                if (index == -1 || count < index)
                                {
                                    //Console.Write("inserted Data :" + currententity.ToString());
                                    Console.Write("Processing :Object [ " + count + " ] ..." + Environment.NewLine);
                                    JS.ExecuteQuary(currententity, JT, DuplicateCheckingColumn);
                                    count++;
                                    ProcessedRecords = count;
                                }
                                else
                                {
                                    Console.Write("Processing Finished.");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            //Console.Write("Current Entity Is null");
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sr.Close();
            }
        }
        public async Task<int> InsertToSqlAsync(string TableName, string path, string DuplicateCheckingColumn = "", int numberofrow = -1, int skip = -1, int end = -1)
        {
            Jsonam j = new Jsonam();
            int index = numberofrow;
            JToSql JS = new JToSql();
            JS.StringToLower = true;
            JTable JT = new JTable();
            JT.TableName = TableName;
            string line = "";
            int count = 0;
            JT.MasterTable = true;
            int countskip = 0;
            using (StreamReader sr = new StreamReader(path))
            {
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    try
                    {
                        j.JsonValue = line;
                        JObject currententity = j.JsonFormat();
                        if (currententity != null)
                        {
                            if (countskip <= skip)
                            {
                                countskip++;
                                continue;
                            }
                            else
                            {
                                if (index == -1 || count < index)
                                {
                                    JS.ExecuteQuary(currententity, JT, DuplicateCheckingColumn);
                                    count++;
                                    ProcessedRecords = count;
                                }
                                else
                                    break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sr.Close();
            }
            return count;
        }
    }
}
