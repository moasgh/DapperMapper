using DapperMapper;
using DapperMapper.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SandBox.Models
{
    public class User
    {
        [Key(true)]
        public long ID { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Family { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }
}