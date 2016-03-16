using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行FROM操作
	/// </summary>
	public interface IQuerySelectColumns
	{
		/// <summary>
		/// 设定要从哪些表查询数据
		/// </summary>
		/// <param name="tables">表集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectFrom FROM(params string[] tables);
	}
}
