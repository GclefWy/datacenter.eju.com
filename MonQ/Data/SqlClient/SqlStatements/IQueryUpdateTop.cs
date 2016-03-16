using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	public interface IQueryUpdateTop
	{
		/// <summary>
		/// 设定要更新哪张表的数据
		/// </summary>
		/// <param name="table">表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		IQueryUpdateFrom FROM(string table);
	}
}
