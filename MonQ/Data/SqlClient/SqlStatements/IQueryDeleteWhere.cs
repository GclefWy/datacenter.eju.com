using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析接口，可以继续进行EXEC操作
	/// </summary>
	public interface IQueryDeleteWhere : IQueryExec
	{
	}
}
