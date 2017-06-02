using DapperMapper.Helper;
using Newtonsoft.Json.Linq;
using SandBox.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DapperMapper.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Json To Sql Do not Support Indented File each object should be in one line other wise no record processed" + Environment.NewLine);

            if (args[0].ToLower() == "help")
            {
                Console.Write("Notation Help => [Description : Type(DefaultValue)]" + Environment.NewLine);
                Console.Write("jtosql [*TableName : string] [*JsonPath : string] [DuplicateCheckingColumn : string(\"\")] [skip : int(-1)] [numberofrecord : int(-1)]" + Environment.NewLine);
            }
            if (args[0].ToLower() == "jtosql")
            {
                try
                {
                    Jsonam j = new Jsonam();
                    Console.Write("start processing ...");
                    string mastertablename = args[1];
                    string pathjson = args[2];
                    string columnduplicate = "";
                    int skip = -1;
                    int numberofrecord = -1;
                    if (args.Length >= 4 && args[3] != null)
                    {
                        columnduplicate = args[3];
                    }
                    if (args.Length >= 5 && args[4] != null)
                    {
                        skip = Convert.ToInt32(args[4]);
                    }
                    if (args.Length >= 6 && args[5] != null)
                    {
                        numberofrecord = Convert.ToInt32(args[5]);
                    }
                    Console.Write("\n TableName : {0} \t Path : {1} Duplicate Check : {2}", mastertablename, pathjson, columnduplicate + Environment.NewLine);
                    Console.Write("\n SKIP : {0} \t Number Of record: {1}", skip, numberofrecord + Environment.NewLine);
                    Console.ReadKey();
                    j.InsertToSql(mastertablename, pathjson, DuplicateCheckingColumn: columnduplicate, numberofrow: numberofrecord, skip: skip);
                    Console.Write("Number of records processed : {0}", j.ProcessedRecords);
                }
                catch (Exception ex)
                {
                    Console.Write("Error Message : {0} \n \n \t Stack Trace : {1}", ex.Message, ex.StackTrace);
                }

            }
            if (args[0].ToLower() == "sqltoj")
            {
                try
                {
                    Console.Write("processing ...");
                    string mastertablename = args[1];
                    string pathjson = args[2];
                    Console.Write("\n TableName : {0} \t Path : {1}", mastertablename, pathjson);
                    Console.ReadKey();
                    DapperMapper.DataMapper<dynamic> records = new DataMapper<dynamic>(mastertablename);
                    List<dynamic> listofrecords = records.GetAll().ToList();
                    string jresult = Newtonsoft.Json.JsonConvert.SerializeObject(listofrecords, Newtonsoft.Json.Formatting.Indented);
                    using (StreamWriter stw = new StreamWriter(pathjson))
                    {
                        stw.Write(jresult);
                        stw.Close();
                    }
                    Console.Write("Ready.");
                }
                catch (Exception ex)
                {
                    Console.Write("Error Message : {0} \n \n \t Stack Trace : {1}", ex.Message, ex.StackTrace);
                }

            }
            if (args[0].ToLower() == "ftosql")
            {
                string tablename = args[1].ToLower();
                string path = args[2].ToLower();
                string separator = args[3];
                string descriptions = args[4];
                string showjson = args[5];
                string duplicatecheck = args[6];
                Console.WriteLine("path : {0} \n separator : {1} \n description : {2} \n TableName : {3} \n DuplicateCheck : {4} ", path, separator, descriptions, tablename, duplicatecheck);

                Console.ReadKey();

                JToSql J = new JToSql();
                JTable JT = new JTable();
                JT.TableName = tablename;
                JT.MasterTable = true;

                string[] directories = Directory.GetDirectories(path);
                foreach (var item in directories)
                {
                    string classlable = Path.GetFileName(item);
                    Console.WriteLine("Processing Lable : " + classlable);
                    long indexprocess = 1;
                    foreach (var rec in Directory.GetFiles(item))
                    {
                        List<string> Records = new List<string>();
                        using (StreamReader sr = new StreamReader(rec))
                        {
                            string replacement = Regex.Replace(sr.ReadToEnd().Replace("\\", "\\\\"), @"\t|\n|\r", "").Replace("\"", "\\\"");
                            Records.Add("\"value\":\"" + replacement + "\"");
                        }
                        Records.Add("\"classlable\":\"" + classlable + "\"");
                        string filename = Path.GetFileNameWithoutExtension(rec);
                        string[] data = filename.Split(new string[] { separator }, StringSplitOptions.None);
                        for (int i = 0; i < data.Length; i++)
                        {
                            Records.Add("\"" + descriptions.Split(',')[i] + "\":\"" + data[i] + "\"");
                        }
                        string record = string.Format("{{ {0} }}", string.Join(",", Records));
                        if (showjson == "y")
                        {
                            Console.WriteLine(record);
                        }
                        JObject obj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(record);
                        J.ExecuteQuary(obj, JT, duplicatecheck);
                        Console.WriteLine("\t #effected" + indexprocess);
                        indexprocess++;
                    }
                }
            }
        }
        public static async void jsontosql()
        {
            Jsonam j = new Jsonam();
            Console.Write("start");
            await j.InsertToSqlAsync("IMDBMovie", "ImDB.NonPlorizedData.json", "ID", skip: 42957);
            Console.Write(j.ProcessedRecords);
            Console.ReadKey();
        }
    }
}
