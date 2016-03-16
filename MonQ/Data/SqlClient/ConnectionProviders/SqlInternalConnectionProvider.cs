using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Text.RegularExpressions;

namespace MonQ.Data.SqlClient.ConnectionProviders
{
	/// <summary>
	/// 内部数据库连接供应器，所谓“内部”是指调用时仅仅提供一个连接字符串，数据库的连接及关闭由框架完成
	/// </summary>
	internal class SqlInternalConnectionProvider : SqlConnectionProvider
	{
		private string connectionString;
		private string transactionName;
		private IsolationLevel iso;
		private bool createTransaction;


		internal SqlInternalConnectionProvider(string connectionString, bool createTransaction, IsolationLevel iso, string transactionName)
		{
			this.connectionString = connectionString;
			this.transactionName = transactionName;
			this.iso = iso;
			this.createTransaction = createTransaction;
		}

		internal override string ConnectionString
		{
			get
			{
				return this.connectionString;
			}
		}

		internal override void InitSqlCommand(SqlCommand command)
		{
			SqlConnection connection = new SqlConnection(this.connectionString);
			connection.Open();
			if (createTransaction)
			{
				command.Transaction = connection.BeginTransaction(iso, transactionName);
			}
			command.Connection = connection;
		}

		internal override void DeinitSqlCommand(SqlCommand command)
		{
			command.Connection.Close();
			command.Connection.Dispose();
		}

		internal override SqlConnection OpenConnection()
		{
			SqlConnection connection = new SqlConnection(this.connectionString);
			connection.Open();
			return connection;
		}

		internal override void CloseConnection(SqlConnection connection)
		{
			connection.Close();
			connection.Dispose();
		}
	}
}
