using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析器，可以设定聚合条件
	/// </summary>
	public interface IQuerySelectGroupBy : IQueryExec
	{
		/// <summary>
		/// 设定排序条件
		/// </summary>
		/// <param name="orderby">排序列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectOrderBy ORDERBY(params string[] orderby);

		/// <summary>
		/// 设定聚集条件
		/// </summary>
		/// <param name="having">聚集条件列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectHaving HAVING(params string[] having);
	}
}
