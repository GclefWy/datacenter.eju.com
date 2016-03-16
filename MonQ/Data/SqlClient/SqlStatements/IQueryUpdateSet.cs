using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	public interface IQueryUpdateSet
	{
		IQueryUpdateSet SET(string column, string value);

		/// <summary>
		/// 构建一个查询条件
		/// </summary>
		/// <param name="condition">条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateWhere WHERE(string condition);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateWhere WHERE_AND(params string[] conditions);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateWhere WHERE_OR(params string[] conditions);
	}
}
