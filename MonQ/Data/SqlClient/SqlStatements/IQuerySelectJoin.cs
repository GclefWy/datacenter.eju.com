using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析器，可以设定连接查询条件
	/// </summary>
	public interface IQuerySelectJoin
	{
		/// <summary>
		/// 设定连接条件
		/// </summary>
		/// <param name="on">连接条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectFrom ON(string on);
	}
}
