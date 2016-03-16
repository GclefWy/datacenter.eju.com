using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public class DataMappingAttribute : Attribute
	{
		public string Source { get; set; }

		public DataMappingAttribute(string source = null)
		{
			this.Source = source;
		}
	}
}
