using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行SELECT_ALL或者SELECT操作
	/// </summary>
	public interface IQuerySelectTop
	{
		/// <summary>
		/// 设定选择全部列(SELECT *)
		/// </summary>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectColumns ALL{get;}

		/// <summary>
		/// 选取一组列
		/// </summary>
		/// <param name="columns">列</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectColumns COLUMNS(params string[] columns);
	}
}
