using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Configuration;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// SQL帮助对象提供器
	/// </summary>
	public class SqlHelperProvider : DynamicObject
	{
		public SqlHelper this[int index]
		{
			get
			{
				if (index > -1 && index < ConfigurationManager.ConnectionStrings.Count)
				{
					return SqlHelper.Create(ConfigurationManager.ConnectionStrings[index].ConnectionString);
				}
				return null;
			}
		}

		public SqlHelper this[string name]
		{
			get
			{
				ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings[name];
				if (conn != null) return SqlHelper.Create(conn.ConnectionString);
				return null;
			}
		}

		internal SqlHelperProvider()
		{
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes[0] is int)
			{
				result = this[(int)indexes[0]];
			}
			else if (indexes[0] is string)
			{
				result = this[(string)indexes[0]];
			}
			else return base.TryGetIndex(binder, indexes, out result);
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			return true;
		}
	}
}
