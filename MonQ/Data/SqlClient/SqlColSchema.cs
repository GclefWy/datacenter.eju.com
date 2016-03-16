using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient
{
	internal class SqlColSchema
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 类型名称
		/// </summary>
		public string TypeName
		{
			get
			{
				return typeName;
			}
			set
			{
				typeName = value.Substring(value.LastIndexOf('.') + 1);
			}
		}
		public bool IsDefault { get; set; }
		public bool IsNullable { get; set; }
		public bool IsIdentity { get; set; }
		public bool IsRowGuidCol { get; set; }
		public bool IsRowNewID { get; set; }
		public bool IsPrimaryKey { get; set; }
		public bool IsClusteredKey { get; set; }
		public bool IsUniqueKey { get; set; }
		public int Size { get; set; }
		public int ColID { get; set; }


		private string typeName;
	}
}
