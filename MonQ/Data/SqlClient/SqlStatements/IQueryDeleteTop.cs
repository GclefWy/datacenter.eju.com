using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行FROM操作
	/// </summary>
	public interface IQueryDeleteTop
	{
		/// <summary>
		/// 设定要删除哪张表的数据
		/// </summary>
		/// <param name="table">表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryDeleteFrom FROM(string table);
	}
}
