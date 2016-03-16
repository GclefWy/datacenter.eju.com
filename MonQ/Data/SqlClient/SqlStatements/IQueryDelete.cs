using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行FROM或TOP操作
	/// </summary>
	public interface IQueryDelete
	{
		/// <summary>
		/// 设定要删除哪张表的数据
		/// </summary>
		/// <param name="table">表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteFrom FROM(string table);

		/// <summary>
		/// 设定只删除前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteTop TOP(int n);

		/// <summary>
		/// 设定只删除前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteTop TOP_PERCENT(int percent);
	}
}
