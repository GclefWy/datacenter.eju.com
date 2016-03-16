using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	/// <summary>
	/// 存储过程适配器
	/// </summary>
	internal sealed class SqlStoreProcedureAdapter : SqlCommandAdapter
	{
		/// <summary>
		/// 存储过程名称
		/// </summary>
		private readonly string spName;

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="id"></param>
		/// <param name="spName"></param>
		/// <param name="schema"></param>
		internal SqlStoreProcedureAdapter(string id, string spName, DataTable schema)
			: base(id)
		{
			this.spName = spName;
			SqlParameterCollection parameters = this.Parameters;
			for (int i = 0, count = schema.Rows.Count; i < count; i++)
			{
				SqlParameter parameter = new SqlParameter();
				DataRow row = schema.Rows[i];
				parameter.ParameterName = (String)row["Name"];
				parameter.Direction = (int)row["IsOutput"] == 1 ? ParameterDirection.InputOutput : ParameterDirection.Input;
				parameter.IsNullable = (int)row["IsNullable"] == 1;

				parameters.Add(parameter);
			}

			this.AddReturnParameter();
		}

		internal override void InitCommand(SqlCommand command)
		{
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = spName;
		}
	}
}
