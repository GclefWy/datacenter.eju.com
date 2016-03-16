using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	internal sealed class SqlTableFunctionAdapter : SqlCommandAdapter
	{
		private readonly string commandText;

		private readonly int parameterCount = 0;

		private string columns;
		private string functionName;
		private string parameters;

		private SqlDataSchema schema;

		private static ConcurrentDictionary<string, string> paginationSqlCache = new ConcurrentDictionary<string, string>();

		internal SqlTableFunctionAdapter(string id, string funcName, DataTable schema)
			: base(id)
		{
			this.schema = SqlDataSchema.Create(schema);
			SqlParameterCollection parameters = this.Parameters;
			StringBuilder columnsBuilder = new StringBuilder();
			StringBuilder paramsBuilder = new StringBuilder();
			for (int i = 0, count = schema.Rows.Count; i < count; i++)
			{
				SqlParameter parameter = new SqlParameter();
				DataRow row = schema.Rows[i];
				string paramName = (String)row["Name"];
				if (paramName[0] == '@')
				{
					parameter.ParameterName = paramName;
					parameter.Direction = (int)row["IsOutput"] == 1 ? ParameterDirection.InputOutput : ParameterDirection.Input;
					parameter.IsNullable = (int)row["IsNullable"] == 1;
					parameterCount++;

					parameters.Add(parameter);
					if (paramsBuilder.Length > 0) paramsBuilder.Append(",");
					paramsBuilder.Append(paramName);
				}
				else
				{
					if (columnsBuilder.Length > 0) columnsBuilder.Append(",");
					columnsBuilder.Append(paramName);
				}
			}

			this.AddReturnParameter();
			this.columns = columnsBuilder.ToString();
			this.functionName = funcName;
			this.parameters = paramsBuilder.ToString();
			this.commandText = string.Format("SELECT {0} FROM {1}({2})", columns, functionName, parameters);
		}

		internal override void InitCommand(SqlCommand command)
		{
			command.CommandText = this.commandText;
		}

		internal override void InitPagination(SqlExecuter executer, SqlPager pager, int pageID, int pageSize, string pk)
		{
			SqlCommand command = executer.command;
			command.CommandText = paginationSqlCache.GetOrAdd(command.CommandText + (pk ?? string.Empty), (key) =>
			{
				if (string.IsNullOrEmpty(pk))
				{
					StringBuilder orderSb = new StringBuilder();
					//首先优先使用主键
					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.IsPrimaryKey)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());
					//接着使用聚集索引
					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.IsClusteredKey)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());
					//接着使用唯一索引
					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.IsUniqueKey)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());
					//接着使用标识列
					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.IsIdentity)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());
					//最后使用唯一列
					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.IsRowGuidCol)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());

					foreach (SqlColSchema colSchema in schema)
					{
						if (colSchema.Name[0] == '@') continue;
						if (colSchema.Name.IndexOf("id", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							if (orderSb.Length > 0)
							{
								orderSb.Append(',');
							}
							orderSb.Append(colSchema.Name);
						}
					}
					if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString());

					return GeneratePaginationSql(schema[0].Name);
				}
				return GeneratePaginationSql(pk);
			});
			command.Parameters.Add("@__PageID", SqlDbType.Int).Value = pageID;
			command.Parameters.Add("@__PageSize", SqlDbType.Int).Value = pageSize;
			command.Parameters.Add("@__RowTotal", SqlDbType.Int).Direction = ParameterDirection.Output;
			Action<SqlExecuter, SqlCommand> handler = executer.outputHandler;
			if (handler == null) executer.outputHandler = (e, c) =>
			{
				pager.RowTotal = (int)command.Parameters["@__RowTotal"].Value;
				pager.PageTotal = (int)(Math.Ceiling(pager.RowTotal * 1.0 / pager.PageSize));
			};
			else executer.outputHandler = (e, c) =>
			{
				pager.RowTotal = (int)command.Parameters["@__RowTotal"].Value;
				pager.PageTotal = (int)(Math.Ceiling(pager.RowTotal * 1.0 / pager.PageSize));
				handler(e, c);
			};
		}

		private string GeneratePaginationSql(string orderBy)
		{
			StringBuilder sql = new StringBuilder();
			sql.AppendLine(@"DECLARE @TEMP TABLE(");
			for(int i = 0, n = 0; i < schema.Count; i++)
			{
				SqlColSchema row = schema[i];
				if (row.Name[0] == '@') continue;
				if (n > 0) sql.AppendLine(",");
				sql.AppendFormat("{0} {1}", row.Name, row.TypeName);
				if (row.TypeName.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0 || row.TypeName.IndexOf("var", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					sql.Append("(" + (row.Size < 1 ? "MAX" : row.Size.ToString()) + ")");
				}
				n++;
			}
			sql.AppendLine(",");
			sql.AppendLine("[ROW_NUMBER] INT");
			sql.AppendLine(")");
			sql.AppendLine("INSERT @TEMP");
			sql.AppendFormat("SELECT {0}, ROW_NUMBER() OVER (ORDER BY {3}) AS [ROW_NUMBER] FROM {1}({2})\r\n", columns, functionName, parameters, orderBy);
			sql.AppendFormat("SELECT @__RowTotal = @@ROWCOUNT\r\nSET ROWCOUNT @__PageSize\r\nSELECT {0} FROM @TEMP WHERE [ROW_NUMBER] > (@__PageID - 1) * @__PageSize\r\nSET ROWCOUNT 0", columns);
			return sql.ToString();
		}
	}
}
