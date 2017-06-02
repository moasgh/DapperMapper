using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DapperMapper.Attributes;
using DapperMapper.Test.Model;

namespace SandBox.Models
{
    public class Objects
    {
        public long ID { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public DateTime Date { get; set; }
        public string Source { get; set; }
        [LookUp(typeof(Category), "CategoryName", JoinType.Inner, "CategoryID",  "CategoryID")]
        public string Category { get; set; }
        public bool ProsAndCons { get; set; }
    }
}