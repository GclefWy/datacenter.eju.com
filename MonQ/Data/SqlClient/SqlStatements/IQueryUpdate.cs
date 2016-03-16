using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	public interface IQueryUpdate
	{
		/// <summary>
		/// 设定要更新哪张表的数据
		/// </summary>
		/// <param name="table">表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateFrom FROM(string table);

		/// <summary>
		/// 设定只更新前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateFrom TOP(int n);

		/// <summary>
		/// 设定只更新前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateFrom TOP_PERCENT(int percent);
	}
}
