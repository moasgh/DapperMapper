using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperMapper.Attributes
{
	public class Key : Attribute
	{
		public bool isIdentity { get; set; }
		public string Map { get; set; }
		public Key(bool IsIdentity)
		{
			this.isIdentity = IsIdentity;
		}
		public Key(bool IsIdentity, string map)
		{
			this.isIdentity = IsIdentity;
			this.Map = map;
		}
	}
	public class NotMap : Attribute
	{
		public bool Value { get; set; } = true;	
	}
}
