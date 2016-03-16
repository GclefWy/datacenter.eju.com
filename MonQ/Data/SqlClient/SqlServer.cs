using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Text.RegularExpressions;
namespace MonQ.Data.SqlClient
{
	public class SqlServer : DynamicObject
	{
		private string connectionString;

		internal SqlServer(string connectionString)
		{
			this.connectionString = connectionString;
		}
		
		public static dynamic Connect(string server, string userID = null, string password = null)
		{
			if (!string.IsNullOrEmpty(userID))
			{
				return new SqlServer(string.Format("Data Source={0};Initial Catalog=;Integrated Security=True;MultipleActiveResultSets=True;Persist Security Info=True;User ID=;Password=", server, userID, password));
			}
			else
			{
				return new SqlServer(string.Format("Data Source={0};Initial Catalog=;Integrated Security=True;MultipleActiveResultSets=True;", server));
			}
		}

		public static dynamic Connect(string connectionString)
		{
			return new SqlServer(connectionString);
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes.Length == 0 && indexes[0] is string)
			{
				SqlHelper helper = SqlHelper.Create(Regex.Replace(this.connectionString, @"(Initial\s*Catalog\s*=\s*)([^;]*)(;|$)", "$1" + (string)indexes[0] + "$3", RegexOptions.Compiled | RegexOptions.IgnoreCase));
				result = helper.DbContext;
				return true;
			}
			return base.TryGetIndex(binder, indexes, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			SqlHelper helper = SqlHelper.Create(Regex.Replace(this.connectionString, @"(Initial\s*Catalog\s*=\s*)([^;]*)(;|$)", "$1" + binder.Name + "$3", RegexOptions.Compiled | RegexOptions.IgnoreCase));
			result = helper.DbContext;
			return true;
		}
	}
}
