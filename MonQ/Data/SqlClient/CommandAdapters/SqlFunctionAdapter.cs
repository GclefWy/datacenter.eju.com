using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	internal sealed class SqlFunctionAdapter : SqlCommandAdapter
	{
		private readonly string commandText;


		private readonly int parameterCount = 0;

		internal SqlFunctionAdapter(string id, string funcName, DataTable schema)
			: base(id)
		{
			SqlParameterCollection parameters = this.Parameters;
			StringBuilder sb = new StringBuilder();
			sb.Append("SELECT " + funcName);
			sb.Append("(");
			for (int i = 0, count = schema.Rows.Count; i < count; i++)
			{
				SqlParameter parameter = new SqlParameter();
				DataRow row = schema.Rows[i];
				parameter.ParameterName = (String)row["Name"];
				parameter.Direction = (int)row["IsOutput"] == 1 ? ParameterDirection.InputOutput : ParameterDirection.Input;
				parameter.IsNullable = (int)row["IsNullable"] == 1;
				parameterCount++;

				parameters.Add(parameter);
				sb.Append(parameter.ParameterName);
				if (i + 1 < count) sb.Append(",");
			}
			sb.Append(")");
			this.commandText = sb.ToString();

			this.AddReturnParameter();
		}

		internal override void InitCommand(SqlCommand command)
		{
			command.CommandText = commandText;
		}
	}
}
