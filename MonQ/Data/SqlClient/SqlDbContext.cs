using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using System.Data.SqlClient;
using System.Collections;
using System.Data;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 用于动态创建基于表格的查询执行器的对象
	/// </summary>
	public class SqlDbContext : DynamicObject
	{
		private SqlHelper helper;

		internal SqlDbContext(SqlHelper helper)
		{
			this.helper = helper;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			if (indexes.Length > 0 && indexes[0] is string)
			{
				object[] args = new object[indexes.Length - 1];
				indexes.CopyTo(args, 1);
				result = CreateExecuter(indexes[0] as string, args);
				return true;
			}
			return base.TryGetIndex(binder, indexes, out result);
		}

		/// <summary>
		/// 重载动态对象属性
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this.helper.FromText(binder.Name);
			return true;
		}

		/// <summary>
		/// 重载动态对象的成员
		/// </summary>
		/// <param name="binder"></param>
		/// <param name="args"></param>
		/// <param name="result"></param>
		/// <returns></returns>
		public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
		{
			result = CreateExecuter(binder.Name,args);
			return true;
		}

		/// <summary>
		/// 创建一个SQL执行器
		/// </summary>
		/// <param name="name"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		private SqlExecuter CreateExecuter(string name, object[] args)
		{
			if (args.Length == 0)
			{
				return this.helper.FromText(name);
			}
			else if (args is SqlParameter[])
			{
				return  this.helper.FromParameters(name, args as SqlParameter[]);
			}
			else if (args.Length > 1)
			{
				return  this.helper.FromArray(name, args);
			}
			else if (SqlTypeConvertor.IsConvertibleType(args[0].GetType()) && !(args[0] is string))
			{
				return  this.helper.FromArray(name, args[0]);
			}
			else if (args[0] is string)
			{
				string arg = ((string)args[0]).Trim();
				if (arg.StartsWith("{") && arg.EndsWith("}"))
				{
					return  this.helper.FromJSON(name, arg);
				}
				else if (arg.StartsWith("[") && arg.EndsWith("]"))
				{
					return  this.helper.FromJSON(name, arg);
				}
				else
				{
					return  this.helper.FromEntity(name, arg);
				}
			}
			else if (args[0] is IDictionary)
			{
				return  this.helper.FromDictionary(name, args[0] as IDictionary);
			}
			else if (args[0] is DataRow)
			{
				return  this.helper.FromRow(name, args[0] as DataRow);
			}
			else if (args[0] is NameValueCollection)
			{
				return  this.helper.FromCollection(name, args[0] as NameValueCollection);
			}
			else
			{
				return  this.helper.FromObject(name, args[0]);
			}
		}
	}
}
