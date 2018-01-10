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

			DataMapper<Movie> moviemap= new DataMapper<Movie>(nameof(Movie));
			moviemap.Insert(new SandBox.Models.Movie());
			
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
