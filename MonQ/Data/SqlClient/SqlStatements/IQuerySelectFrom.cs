using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行WHERE与JOIN或者UNION操作
	/// </summary>
	public interface IQuerySelectFrom
	{
		/// <summary>
		/// 构建一个查询条件
		/// </summary>
		/// <param name="condition">条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectWhere WHERE(string condition);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectWhere WHERE_AND(params string[] conditions);

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectWhere WHERE_OR(params string[] conditions);

		/// <summary>
		/// 构建内连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectJoin INNERJOIN(string table);

		/// <summary>
		/// 构建左连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectJoin LEFTJOIN(string table);

		/// <summary>
		/// 构建右连接连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectJoin RIGHTJOIN(string table);

		/// <summary>
		/// 构建交叉连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectJoin CROSSJOIN(string table);

		/// <summary>
		/// 闭合当前查询并且准备联合一个新的查询
		/// </summary>
		IQuerySelectUnion UNION { get; }
	}
}
