using DapperMapper;
using DapperMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SandBox.Models
{
    public class UserRate 
    {
        [Key(true)]
        public long ID { get; set; }
        public long UserID { get; set; }
        public long ObjectID { get; set; }
        public float Rate { get; set; }
        public DateTime Date { get; set; }
        public string Comment { get; set; }
    }
}