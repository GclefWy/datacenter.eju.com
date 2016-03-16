using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient
{
	internal interface ISqlLinker
	{
		/// <summary>
		/// 当前查询分析所使用到的表名称
		/// </summary>
		string TableName { get; }
	}
}
