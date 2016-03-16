using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MonQ.Properties;
using System.Collections;
using System.Reflection;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	/// <summary>
	/// 用于使用表格直接查询时的适配器
	/// </summary>
	internal sealed class SqlTableAdapter : SqlCommandAdapter
	{

		private readonly string commandText;
		private static ConcurrentDictionary<string, string> paginationSqlCache = new ConcurrentDictionary<string, string>();
		private static ConcurrentDictionary<string, ConcurrentDictionary<Type, string>> selectSqlCache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, string>>();
		private static ConcurrentDictionary<string, ConcurrentDictionary<Type, string>> insertSqlCache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, string>>();
		private static ConcurrentDictionary<string, ConcurrentDictionary<Type, string>> updateSqlCache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, string>>();
		private static ConcurrentDictionary<string, ConcurrentDictionary<Type, string>> deleteSqlCache = new ConcurrentDictionary<string, ConcurrentDictionary<Type, string>>();
		private static ConcurrentDictionary<Type, SqlParameterCollection> parameterCache = new ConcurrentDictionary<Type, SqlParameterCollection>();

		private SqlDataSchema schema;
		private string columns;
		private string tableName;
		private static SqlParameterCollection empty = SqlEmitter.CreateParameterCollection();

		internal SqlTableAdapter(string id, string tableName, DataTable schema)
			: base(id)
		{
			this.tableName = tableName;
			this.schema = SqlDataSchema.Create(schema);
			StringBuilder columnSb = new StringBuilder();
			for (int i = 0, count = schema.Rows.Count; i < count; i++)
			{
				string columnName = (String)schema.Rows[i]["Name"];
				columnSb.Append("[" + columnName + "]");
				if (i + 1 < count) columnSb.Append(',');
			}
			columns = columnSb.ToString();
			commandText = "SELECT " + columns + " FROM " + tableName;

			this.AddReturnParameter();
		}

		internal string InitUpdateCommand(Type instanceType)
		{
			return updateSqlCache.GetOrAdd(tableName, new ConcurrentDictionary<Type, string>()).GetOrAdd(instanceType, (t) =>
			{

				StringBuilder whereField = new StringBuilder();
				StringBuilder updateField = new StringBuilder();
				List<string> usedWhere = new List<string>();
				List<string> usedSet = new List<string>();
				/*
				 * 可以作为更新依据的列：
				 * 主键
				 * 标识
				 * 唯一索引列
				 * 聚集索引列
				 * 非空的GUID列
				 */
				foreach (PropertyInfo property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, property);
					SqlColSchema row = schema[colName];
					if (row != null)
					{
						if (usedWhere.IndexOf(colName.ToLower()) == -1)
						{
							if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
							{
								if (whereField.Length > 0) whereField.Append(" AND ");
								whereField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
								usedWhere.Add(colName.ToLower());
							}
						}
						if (usedSet.IndexOf(colName.ToLower()) == -1)
						{
							if (!row.IsPrimaryKey && !row.IsIdentity && !row.IsClusteredKey)
							{
								if (updateField.Length > 0) updateField.AppendLine(",");
								else updateField.AppendLine();
								updateField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
								usedSet.Add(colName.ToLower());
							}
						}
					}
				}
				foreach (FieldInfo field in instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, field);
					SqlColSchema row = schema[colName];
					if (row != null)
					{
						if (usedWhere.IndexOf(colName.ToLower()) == -1)
						{
							if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
							{
								if (whereField.Length > 0) whereField.Append(" AND ");
								whereField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
								usedWhere.Add(colName.ToLower());
							}
						}
						if (usedSet.IndexOf(colName.ToLower()) == -1)
						{
							if (!row.IsPrimaryKey && !row.IsIdentity && !row.IsClusteredKey)
							{
								if (updateField.Length > 0) updateField.AppendLine(",");
								else updateField.AppendLine();
								updateField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
								usedSet.Add(colName.ToLower());
							}
						}
					}
				}
				if (whereField.Length == 0)
				{
					throw new Exception(string.Format(Resources.SqlClient_CanNotUpdateAsNoIdentityColumn, instanceType.FullName));
				}
				if (updateField.Length == 0)
				{
					throw new Exception(string.Format(Resources.SqlClient_CanNotUpdateAsNoColumnForUpdate, instanceType.FullName));
				}
				return string.Format(@"
UPDATE [{0}]
SET
	{1}
WHERE
	{2}", tableName, updateField, whereField);
			});
		}
		internal string InitInsertCommand(Type instanceType)
		{
			return insertSqlCache.GetOrAdd(tableName, new ConcurrentDictionary<Type, string>()).GetOrAdd(instanceType, (t) =>
			{

				StringBuilder valueField = new StringBuilder();
				StringBuilder insertField = new StringBuilder();
				List<string> usedNames = new List<string>();
				/*
				 * 可以作为更新依据的列：
				 * 主键
				 * 标识
				 * 唯一索引列
				 * 聚集索引列
				 * 非空的GUID列
				 */
				foreach (PropertyInfo property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, property);
					SqlColSchema row = schema[colName];
					if (row != null)
					{
						if (usedNames.IndexOf(colName.ToLower()) == -1)
						{
							if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
							{
								if (valueField.Length > 0)
								{
									valueField.Append(", ");
									insertField.Append(", ");
								}
								insertField.AppendFormat("[{0}]", row.Name.Trim('[', ']'));
								valueField.AppendFormat("@{0}", row.Name.Trim('[', ']'));
								usedNames.Add(colName.ToLower());
							}
						}
					}
				}
				foreach (FieldInfo field in instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, field);
					SqlColSchema row = schema[colName];
					if (row != null)
					{
						if (usedNames.IndexOf(colName.ToLower()) == -1)
						{
							if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
							{
								if (valueField.Length > 0)
								{
									valueField.Append(", ");
									insertField.Append(", ");
								}
								insertField.AppendFormat("[{0}]", row.Name.Trim('[', ']'));
								valueField.AppendFormat("@{0}", row.Name.Trim('[', ']'));
								usedNames.Add(colName.ToLower());
							}
						}
					}
				}
				if (valueField.Length == 0)
				{
					throw new Exception(string.Format(Resources.SqlClient_CanNotUpdateAsNoIdentityColumn, instanceType.FullName));
				}
				if (insertField.Length == 0)
				{
					throw new Exception(string.Format(Resources.SqlClient_CanNotUpdateAsNoColumnForUpdate, instanceType.FullName));
				}
				return string.Format(@"
INSERT [{0}]
	({1})
VALUES
	({2})", tableName, insertField, valueField);
			});
		}
		internal string InitDeleteCommand(Type instanceType)
		{
			return deleteSqlCache.GetOrAdd(tableName, new ConcurrentDictionary<Type, string>()).GetOrAdd(instanceType, (t) =>
			{

				StringBuilder whereField = new StringBuilder();
				List<string> usedNames = new List<string>();
				/*
				 * 可以作为更新依据的列：
				 * 主键
				 * 标识
				 * 唯一索引列
				 * 聚集索引列
				 * 非空的GUID列
				 */
				foreach (PropertyInfo property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, property);
					SqlColSchema row = schema[colName];
					if (row != null && usedNames.IndexOf(colName.ToLower()) == -1)
					{
						if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
						{
							if (whereField.Length > 0) whereField.Append(" AND ");
							whereField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
							usedNames.Add(colName.ToLower());
						}
					}
				}
				foreach (FieldInfo field in instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string colName = SqlMapper.GetMemberMap(instanceType, field);
					SqlColSchema row = schema[colName];
					if (row != null && usedNames.IndexOf(colName.ToLower()) == -1)
					{
						if (row.IsPrimaryKey || row.IsIdentity || row.IsUniqueKey || row.IsClusteredKey || (row.IsRowGuidCol && !row.IsNullable))
						{
							if (whereField.Length > 0) whereField.Append(" AND ");
							whereField.AppendFormat("[{0}] = @{0}", row.Name.Trim('[', ']'));
							usedNames.Add(colName.ToLower());
						}
					}
				}
				if (whereField.Length > 0)
				{
					return string.Format(@"
DELETE
	{0}
WHERE
	{1}", tableName, whereField);
				}
				throw new Exception(string.Format(Resources.SqlClient_CanNotDeleteAsNoIdentityColumn, instanceType.FullName));
			});
		}

		internal string InitUpdateCommand(SqlRecord record)
		{
			if (record.changedColumns.Count > 0)
			{
				StringBuilder updateField = new StringBuilder();
				foreach (string col in record.changedColumns)
				{
					if (updateField.Length > 0) updateField.AppendLine(",");
					else updateField.AppendLine();
					updateField.AppendFormat("[{0}] = @{0}", col.Trim('[', ']'));
				}
				StringBuilder whereField = new StringBuilder();
				foreach (SqlColSchema colSchema in schema)
				{
					/*
					 * 可以作为更新依据的列：
					 * 主键
					 * 标识
					 * 唯一索引列
					 * 聚集索引列
					 * 非空的GUID列
					 */
					if (colSchema.IsPrimaryKey || colSchema.IsIdentity || colSchema.IsUniqueKey || colSchema.IsClusteredKey || (colSchema.IsRowGuidCol && !colSchema.IsNullable))
					{
						if (whereField.Length > 0) whereField.Append(" AND ");
						whereField.AppendFormat("[{0}] = @{0}", colSchema.Name.Trim('[', ']'));
					}
				}
				if (whereField.Length > 0)
				{
					return string.Format(@"
UPDATE [{0}]
SET
	{1}
WHERE
	{2}", tableName.Trim('[', ']'), updateField, whereField);
				}
				else
				{
					throw new Exception(string.Format(Resources.SqlClient_CanNotUpdateAsNoIdentityColumn, tableName));
				}
			}
			return ";";
		}

		internal override void InitCommand(SqlCommand command)
		{
			command.CommandText = commandText;
		}


		#region 分页功能实现
		/// <summary>
		/// 分页初始化
		/// </summary>
		/// <param name="executer"></param>
		/// <param name="pager"></param>
		/// <param name="pageID"></param>
		/// <param name="pageSize"></param>
		/// <param name="pk"></param>
		internal override void InitPagination(SqlExecuter executer, SqlPager pager, int pageID, int pageSize, string pk)
		{
			if (!this.commandText.StartsWith("select", StringComparison.OrdinalIgnoreCase)) throw new NotSupportedException();
			SqlCommand command = executer.command;
			command.CommandText = paginationSqlCache.GetOrAdd(command.CommandText + (pk ?? string.Empty), (key) =>
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
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
				if (orderSb.Length > 0) return GeneratePaginationSql(orderSb.ToString(), command.CommandText);
				#endregion
				return GeneratePaginationSql(schema[0].Name, command.CommandText);
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
		/// <param name="commandText"></param>
		/// <returns></returns>
		private string GeneratePaginationSql(string pk, string commandText)
		{
			StringBuilder declaration = new StringBuilder();
			StringBuilder selection = new StringBuilder();
			StringBuilder condition = new StringBuilder();
			foreach (string col in pk.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
			{
				declaration.AppendFormat("DECLARE @__{0} SYSNAME\r\n", col);
				if (selection.Length > 0) selection.Append(',');
				selection.AppendFormat("@__{0} = [{0}]", col.Trim('[', ']'));
				if (condition.Length > 0) condition.Append(" AND ");
				condition.AppendFormat("@__{0} < [{0}]", col.Trim('[', ']'));
			}
			string sql = @"
{0}
SELECT @__RowTotal =  (@__PageID - 1) * @__PageSize
IF @__RowTotal > 0
BEGIN
	SET ROWCOUNT @__RowTotal
	SELECT {1} FROM ({2}) T
	SET ROWCOUNT @__PageSize
	SELECT * FROM ({2}) T WHERE {3}
END
ELSE
BEGIN
	SET ROWCOUNT @__PageSize
	SELECT * FROM ({2}) T
END
SET ROWCOUNT 0
SELECT @__RowTotal = COUNT(1) FROM ({2}) T
			";
			return string.Format(sql, declaration, selection, commandText, condition);
		}
		#endregion

		/// <summary>
		/// 根据所传递的参数动态生成查询语句并且初始化
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameterValues"></param>
		internal override void InitParams(SqlCommand command, object[] parameterValues)
		{
			StringBuilder condition = new StringBuilder();
			for (int i = 0; i < schema.Count && i < parameterValues.Length; i++)
			{
				object value = parameterValues[i];
				if (value != null)
				{
					if (condition.Length > 0) condition.Append(" AND ");
					SqlColSchema row = schema[i];
					if (value == DBNull.Value)
					{
						condition.AppendFormat("[{0}] IS NULL", row.Name);
						continue;
					}
					else if (value is string)
					{
						if (value.ToString().IndexOf('%') >= 0)
						{
							if (row.TypeName.IndexOf("var", StringComparison.OrdinalIgnoreCase) >= 0 || row.TypeName.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								condition.AppendFormat("[{0}] LIKE @{0}", row.Name);
							}
							else
							{
								condition.AppendFormat("CAST([{0}] AS NVARCHAR(MAX)) LIKE @{0}", row.Name);
							}
						}
						else
						{
							condition.AppendFormat("[{0}] = @{0}", row.Name);
						}
						command.Parameters.Add(new SqlParameter("@" + row.Name, value));
					}
					else if (value is IEnumerable<string> || value is IEnumerable<ValueType>)
					{
						IEnumerable collection = value as IEnumerable;
						int j = 0;
						foreach (object o in collection)
						{
							if (j == 0)
							{
								condition.AppendFormat("[{0}] IN (", row.Name);
							}
							else
							{
								condition.Append(",");
							}
							condition.Append("@" + row.Name + (j + 1));
							command.Parameters.Add(new SqlParameter("@" + row.Name + (j + 1), o));
							j++;
						}
						if (j > 0)
						{
							condition.Append(")");
						}
					}
					else
					{
						command.Parameters.Add(new SqlParameter("@" + row.Name, value));
						condition.AppendFormat("[{0}] = @{0}", row.Name);
					}
				}
			}
			if (condition.Length > 0)
			{
				command.CommandText = command.CommandText + " WHERE " + condition.ToString();
			}
			//base.InitParams(command, parameterValues);
		}

		/// <summary>
		/// 根据所传递的参数动态生成查询语句并且初始化
		/// </summary>
		/// <param name="command"></param>
		/// <param name="parameters"></param>
		internal override void InitParams(SqlCommand command, params SqlParameter[] parameters)
		{
			StringBuilder condition = new StringBuilder();
			for (int i = 0; i < schema.Count && i < parameters.Length; i++)
			{
				SqlParameter parameter = parameters[i];
				if (parameter != null)
				{
					SqlColSchema row = schema[parameter.ParameterName.Substring(1)];
					if (row != null)
					{
						if (condition.Length > 0) condition.Append(" AND ");
						command.Parameters.Add(parameter);
						if (parameter.Value is string && parameter.Value.ToString().IndexOf('%') >= 0)
						{
							if (row.TypeName.IndexOf("var", StringComparison.OrdinalIgnoreCase) >= 0 || row.TypeName.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0)
							{
								condition.AppendFormat("[{0}] LIKE @{0}", row.Name);
							}
							else
							{
								condition.AppendFormat("CAST([{0}] AS NVARCHAR(MAX)) LIKE @{0}", row.Name);
							}
						}
						else
						{
							condition.AppendFormat("[{0}] = @{0}", row.Name);
						}
					}
				}
			}
			if (condition.Length > 0)
			{
				command.CommandText = command.CommandText + " WHERE " + condition.ToString();
			}
			//base.InitParams(command, parameters);
		}

		internal override void InitParams<T>(SqlCommand command, T parameters)
		{
			this.InitCommand(command, typeof(T));
			base.InitParams<T>(command, parameters);
			base.parameters = empty;
		}

		internal override void InitParamsByObject(SqlCommand command, object instance)
		{
			this.InitCommand(command, instance.GetType());
			base.InitParamsByObject(command, instance);
			base.parameters = empty;
		}

		private void InitCommand(SqlCommand command, Type type)
		{
			command.CommandText = selectSqlCache.GetOrAdd(this.tableName, new ConcurrentDictionary<Type, string>()).GetOrAdd(type, (key) =>
			{
				SqlParameterCollection parameters = SqlEmitter.CreateParameterCollection();
				#region 添加SQL Parameter
				List<string> usedNames = new List<string>();
				StringBuilder condition = new StringBuilder();
				foreach (PropertyInfo property in key.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string memberName = property.Name;
					if (schema[memberName] != null && SqlTypeConvertor.IsConvertibleType(property.PropertyType) && usedNames.IndexOf(memberName.ToLower()) == -1)
					{
						if (condition.Length > 0) condition.Append(" AND ");
						if (property.PropertyType == typeof(string))
						{
							condition.AppendFormat("([{0}] = @{0} OR ([{0}] LIKE @{0} AND @{0} LIKE '%[%]%'))", memberName);
						}
						else
						{
							condition.AppendFormat("[{0}] = @{0}", memberName);
						}
						usedNames.Add(memberName.ToLower());
						parameters.Add(new SqlParameter("@" + memberName, DBNull.Value));
					}
				}
				foreach (FieldInfo field in key.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					string memberName = field.Name;
					if (schema[memberName] != null && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(memberName.ToLower()) == -1)
					{
						if (condition.Length > 0) condition.Append(" AND ");
						if (field.FieldType == typeof(string))
						{
							condition.AppendFormat("([{0}] = @{0} OR ([{0}] LIKE @{0} AND @{0} LIKE '%[%]%'))", memberName);
						}
						else
						{
							condition.AppendFormat("[{0}] = @{0}", memberName);
						}
						usedNames.Add(memberName.ToLower());
						parameters.Add(new SqlParameter("@" + memberName, DBNull.Value));
					}
				}
				#endregion

				parameterCache.TryAdd(type, parameters);
				if (condition.Length > 0)
				{
					return command.CommandText + " WHERE " + condition;
				}
				else return command.CommandText;
			});
			base.parameters = parameterCache[type];
		}
	}
}
