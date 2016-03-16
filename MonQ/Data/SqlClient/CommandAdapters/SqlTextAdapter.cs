using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Data;
using System.Collections.Concurrent;
using System.Data.Common;
using MonQ.Properties;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	internal class SqlTextAdapter : SqlCommandAdapter
	{
		/// <summary>
		/// 匹配SQL语句中需要移除的多行注释
		/// </summary>
		private static readonly string REGEX_REMOVE_COMMENT_MLINE = @"/\*(?:(?!\*/|/\*).)*(?:/\*(?:(?!\*/|/\*).)*\*/(?:(?!\*/|/\*).)*)*.*?\*/";

		/// <summary>
		/// 匹配SQL语句中需要移除的单行注释
		/// </summary>
		private static readonly string REGEX_REMOVE_COMMENT_LINE = @"--.*$";

		/// <summary>
		/// 匹配SQL语句中需要移除的单引号字符串
		/// </summary>
		private static readonly string REGEX_REMOVE_STR = @"(\bN)?'([^']|\s|'')*'";

		/// <summary>
		/// 匹配SQL语句中需要移除的双引号字符串
		/// </summary>
		private static readonly string REGEX_REMOVE_STR_QUATE = "(\\bN)?\"([^\"]|\\s|\"\")*\"";

		/// <summary>
		/// 匹配SQL语句中需要移除的系统变量
		/// </summary>
		private static readonly string REGEX_REMOVE_SERVER_VARIABLE = @"@@.*$";

		/// <summary>
		/// 匹配SQL语句中的本地变量声明
		/// </summary>
		private static readonly string REGEX_MATCH_DECLAREATION = @"DECLARE\s+(\s*@\w+\s+[\w]+\s*(\([\d\,]+\))?\s*,?\s*)+?([;\n\s])";

		/// <summary>
		/// 匹配SQL语句中的单词结束符
		/// </summary>
		private static readonly string REGEX_MATCH_WORD_BREAK = @"\b";

		/// <summary>
		/// 匹配SQL语句中的变量声明
		/// </summary>
		private static readonly string REGEX_MATCH_PARAMETER = @"(@\w+)\b";

		private static ConcurrentDictionary<string, string> paginationSqlCache = new ConcurrentDictionary<string, string>();
		private SqlDataSchema schema;
		internal string commandText;

		internal SqlTextAdapter(string id, string commandText)
			: base(id)
		{
			if ((commandText ?? string.Empty).Trim().Length == 0) throw new ArgumentNullException("commandText");

			//清除掉注释
			string sql = Regex.Replace(commandText, REGEX_REMOVE_COMMENT_MLINE, string.Empty, RegexOptions.Compiled | RegexOptions.Singleline);
			sql = Regex.Replace(sql, REGEX_REMOVE_COMMENT_LINE, string.Empty, RegexOptions.Compiled | RegexOptions.Multiline);


			//清除掉字符串
			sql = Regex.Replace(sql, REGEX_REMOVE_STR, string.Empty, RegexOptions.Compiled | RegexOptions.Multiline);
			sql = Regex.Replace(sql, REGEX_REMOVE_STR_QUATE, string.Empty, RegexOptions.Compiled | RegexOptions.Multiline);

			//清除掉内置参数
			sql = Regex.Replace(sql, REGEX_REMOVE_SERVER_VARIABLE, string.Empty, RegexOptions.Compiled | RegexOptions.Multiline);


			//清除掉内置变量声明
			Match paramDeclare = Regex.Match(sql, REGEX_MATCH_DECLAREATION, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			while (paramDeclare != null && paramDeclare.Success)
			{
				string declaration = paramDeclare.Value;
				foreach (Match match in Regex.Matches(declaration, REGEX_MATCH_PARAMETER, RegexOptions.Compiled | RegexOptions.Multiline))
				{
					string param = match.Groups[1].Value;
					sql = Regex.Replace(sql, param + REGEX_MATCH_WORD_BREAK, string.Empty, RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.IgnoreCase);
				}
				paramDeclare = Regex.Match(sql, REGEX_MATCH_DECLAREATION, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
			}

			this.commandText = commandText.Trim();


			//检索SQL参数
			MatchCollection matches = Regex.Matches(sql, REGEX_MATCH_PARAMETER, RegexOptions.Compiled | RegexOptions.Multiline);
			SqlParameterCollection parameters = this.Parameters;
			for (int i = 0, count = matches.Count; i < count; i++)
			{
				Match match = matches[i];
				string param = match.Groups[1].Value;
				if (parameters.IndexOf(param) == -1)
				{
					SqlParameter parameter = new SqlParameter(param, null);
					parameter.Direction = ParameterDirection.InputOutput;
					parameters.Add(parameter);
				}
			}

			this.AddReturnParameter();
		}

		internal override void InitCommand(SqlCommand command)
		{
			command.CommandText = this.commandText;
		}
		#region 分页功能实现
		/// <summary>
		/// 分页初始化
		/// </summary>
		/// <param name="executer"></param>
		/// <param name="pager"></param>
		/// <param name="pageID"></param>
		/// <param name="pageSize"></param>
		/// <param name="orderBy"></param>
		internal override void InitPagination(SqlExecuter executer, SqlPager pager, int pageID, int pageSize, string orderBy)
		{
			if (!this.commandText.StartsWith("select", StringComparison.OrdinalIgnoreCase)) throw new NotSupportedException();
			SqlCommand command = executer.command;
			command.CommandText = paginationSqlCache.GetOrAdd(command.CommandText + (orderBy ?? string.Empty) + executer.helper.connectionProvider.ConnectionString, (key) =>
			{
				if (schema == null)
				{
					SqlDataSchema dataSchema = new SqlDataSchema();
					#region 首先获取结构信息
					SqlConnection connection = executer.helper.connectionProvider.OpenConnection();
					SqlCommand clone = command.Clone();
					clone.CommandText = "SELECT TOP 1 * FROM (" + clone.CommandText + ") T";
					clone.Connection = connection;
					try
					{
						using (SqlDataReader reader = clone.ExecuteReader())
						{
							DataTable table = reader.GetSchemaTable();
							if (table.Rows.Count == 0)
							{
								throw new Exception(Resources.SqlClient_CanNotReadSchema);
							}
							for (int i = 0; i < table.Rows.Count; i++)
							{
								DataRow colSchema = table.Rows[i];
								SqlColSchema colSchemaSchema = new SqlColSchema();
								colSchemaSchema.Name = colSchema[SchemaTableColumn.ColumnName].ToString();
								colSchemaSchema.Size = Convert.ToInt32(colSchema[SchemaTableColumn.ColumnSize]);
								colSchemaSchema.TypeName = colSchema["DataTypeName"].ToString();
								colSchemaSchema.IsUniqueKey = Convert.ToBoolean(colSchema[SchemaTableColumn.IsUnique]);
								colSchemaSchema.IsNullable = Convert.ToBoolean(colSchema[SchemaTableColumn.AllowDBNull]);
								colSchemaSchema.IsRowGuidCol = string.Compare(colSchemaSchema.TypeName, "uniqueidentifier", true) == 0;
								colSchemaSchema.IsIdentity = Convert.ToBoolean(colSchema["IsIdentity"]);
								colSchemaSchema.IsPrimaryKey = Convert.ToBoolean(colSchema[SchemaTableColumn.IsKey] == DBNull.Value ? false : colSchema[SchemaTableColumn.IsKey]);
								dataSchema.Add(colSchemaSchema);
							}
						}
						schema = dataSchema;
					}
					finally
					{
						executer.helper.connectionProvider.CloseConnection(connection);
					}
					#endregion
				}
				if (string.IsNullOrEmpty(orderBy))
				{
					StringBuilder orderSb = new StringBuilder();
					#region 首先优先使用主键
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion

					#region 接着使用聚集索引
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion
					#region 接着使用唯一索引
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion
					#region 接着使用标识列
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion
					#region 最后使用唯一列
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion
					#region 使用id列
					foreach (SqlColSchema colSchema in schema)
					{
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
					#endregion
					return GeneratePaginationSql(schema[0].Name);
				}
				else
				{
					return GeneratePaginationSql(orderBy);
				}
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
		/// <summary>
		/// 生成分页SQL代码
		/// </summary>
		/// <param name="pk"></param>
		/// <returns></returns>
		private string GeneratePaginationSql(string pk)
		{

			StringBuilder sql = new StringBuilder();
			StringBuilder columns = new StringBuilder();
			for (int i = 0; i < schema.Count; i++)
			{
				SqlColSchema row = schema[i];
				if (i > 0)
				{
					columns.Append(",");
				}
				columns.AppendFormat("{0}", row.Name);
			}
			sql.AppendFormat("SELECT @__RowTotal = COUNT(1) FROM ({0}) T", commandText);
			sql.AppendLine();
			sql.AppendFormat("SELECT {0} FROM (SELECT *, ROW_NUMBER() OVER (ORDER BY {2}) AS [__MONQ_R_N] FROM ({1}) T) T2 WHERE __MONQ_R_N > (@__PageID - 1) * @__PageSize AND __MONQ_R_N <= @__PageID * @__PageSize", columns, commandText, pk);
			return sql.ToString();
			//            StringBuilder declaration = new StringBuilder();
			//            StringBuilder selection = new StringBuilder();
			//            StringBuilder condition = new StringBuilder();
			//            foreach (string col in pk.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			//            {
			//                declaration.AppendFormat("DECLARE @__{0} SYSNAME\r\n", col);
			//                if (selection.Length > 0) selection.Append(',');
			//                selection.AppendFormat("@__{0} = [{0}]", col.Trim('[', ']'));
			//                if (condition.Length > 0) condition.Append(" AND ");
			//                condition.AppendFormat("@__{0} < [{0}]", col.Trim('[', ']'));
			//            }
			//            string sql = @"
			//{0}
			//SELECT @__RowTotal =  (@__PageID - 1) * @__PageSize
			//IF @__RowTotal > 0
			//BEGIN
			//	SET ROWCOUNT @__RowTotal
			//	SELECT {1} FROM ({2}) T
			//	SET ROWCOUNT @__PageSize
			//	SELECT * FROM ({2}) T WHERE {3}
			//END
			//ELSE
			//BEGIN
			//	SET ROWCOUNT @__PageSize
			//	SELECT * FROM ({2}) T
			//END
			//SET ROWCOUNT 0
			//SELECT @__RowTotal = COUNT(1) FROM ({2}) T
			//			";
			//            return string.Format(sql, declaration, selection, this.commandText, condition);
		}
		#endregion
		internal override void InitParams(SqlCommand command, params SqlParameter[] parameters)
		{
			for (int i = 0, count = parameters.Length; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				if (parameter.Direction == ParameterDirection.Input)
				{
					parameter.Direction = ParameterDirection.InputOutput;
				}
			}
			base.InitParams(command, parameters);
		}

	}
}
