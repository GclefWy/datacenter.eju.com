using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行TOP或者COLUMNS操作
	/// </summary>
	public interface IQuerySelect
	{
		/// <summary>
		/// 设定只获取前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectTop TOP(int n);

		/// <summary>
		/// 设定只获取前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectTop TOP_PERCENT(int percent);

		/// <summary>
		/// 设定需要选取的列
		/// </summary>
		/// <param name="columns">列名称列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectColumns COLUMNS(params string[] columns);

		/// <summary>
		/// 设定选择全部列(SELECT *)
		/// </summary>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelectColumns ALL { get; }
	}
}
