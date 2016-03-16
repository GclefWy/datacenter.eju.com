using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace MonQ.Data.SqlClient.ConnectionProviders
{
	/// <summary>
	/// 外部数据库连接供应器，所谓“外部”是指在调用时提供一个已经打开数据库连接的实例，这个数据库连接的开启与关闭不受框架管理。
	/// </summary>
	internal class SqlExternalConnectionProvider : SqlConnectionProvider
	{

		private SqlConnection connection;
		private SqlTransaction transaction;

		internal SqlExternalConnectionProvider(SqlConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}
			this.connection = connection;
		}
		internal SqlExternalConnectionProvider(SqlTransaction transaction)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("transaction");
			}
			this.transaction = transaction;
			this.connection = transaction.Connection;
		}

		internal override string ConnectionString
		{
			get
			{
				return this.connection.ConnectionString;
			}
		}

		internal override void InitSqlCommand(SqlCommand command)
		{
			command.Connection = this.connection;
			command.Transaction = transaction;
		}

		internal override void DeinitSqlCommand(SqlCommand command)
		{
		}

		internal override SqlConnection OpenConnection()
		{
			return this.connection;
		}

		internal override void CloseConnection(SqlConnection connection)
		{
		}
	}
}
