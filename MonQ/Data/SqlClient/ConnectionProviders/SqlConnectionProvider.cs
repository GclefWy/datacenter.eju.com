using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using MonQ.Data.SqlClient.CommandAdapters;

namespace MonQ.Data.SqlClient.ConnectionProviders
{
	/// <summary>
	/// SQL连接供应器
	/// </summary>
	internal abstract class SqlConnectionProvider
	{
		internal abstract string ConnectionString { get; }

		internal abstract SqlConnection OpenConnection();

		internal abstract void CloseConnection(SqlConnection connection);

		internal abstract void InitSqlCommand(SqlCommand command);

		internal abstract void DeinitSqlCommand(SqlCommand command);
	}
}
