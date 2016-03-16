using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行UNION、GROUPBY与ORDERBY操作
	/// </summary>
	public interface IQuerySelectWhere : IQueryExec
	{
		/// <summary>
		/// 设定分组条件
		/// </summary>
		/// <param name="groupby">聚集列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectGroupBy GROUPBY(params string[] groupby);

		/// <summary>
		/// 设定排序条件
		/// </summary>
		/// <param name="orderby">排序列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectOrderBy ORDERBY(params string[] orderby);

		/// <summary>
		/// 闭合当前查询并且准备联合一个新的查询
		/// </summary>
		IQuerySelectUnion UNION { get; }
	}
}
