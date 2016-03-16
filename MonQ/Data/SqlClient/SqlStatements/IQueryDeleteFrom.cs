using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析器，FROM语句基类
	/// </summary>
	public interface IQueryDeleteFrom : IQueryExec
	{
		/// <summary>
		/// 构建一个查询条件
		/// </summary>
		/// <param name="condition">条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteWhere WHERE(string condition);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteWhere WHERE_AND(params string[] conditions);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteWhere WHERE_OR(params string[] conditions);
	}
}
