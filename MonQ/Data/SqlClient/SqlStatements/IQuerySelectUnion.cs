using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行TOP与SELECT操作
	/// </summary>
	public interface IQuerySelectUnion
	{
		/// <summary>
		/// 设定开始选择
		/// </summary>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQuerySelect SELECT { get; }
	}
}
