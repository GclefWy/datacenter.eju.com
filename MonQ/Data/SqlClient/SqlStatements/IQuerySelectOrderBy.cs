using System;
using System.Collections.Generic;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行UNION与GO操作
	/// </summary>
	public interface IQuerySelectOrderBy : IQueryExec
	{
		/// <summary>
		/// 联合一个新的查询
		/// </summary>
		IQuerySelectUnion UNION { get; }
	}
}
