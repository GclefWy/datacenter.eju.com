using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using MonQ.Data.SqlClient.CommandAdapters;
using MonQ.Data.SqlClient.ConnectionProviders;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Web.Script.Serialization;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// SQL语句执行器
	/// </summary>
	public class SqlExecuter : IDisposable
	{
		/// <summary>
		/// 如果上一执行的SQL查询语句隶属于一个事务之中，则使用此属性获得这个事务的实例。
		/// 如果尚未执行任何语句或者SQL语句没有使用事务，则返回null
		/// </summary>
		/// <remarks>
		/// 此属性不能返回SQL语句内部主动发起的事务，只能返回由SQLServer驱动程序所发起的事务，通俗而言就是如果SqlConnection有调用BeginTransaction方法，则此属性有值。
		/// </remarks>
		public SqlTransaction Transaction
		{
			get
			{
				return command.Transaction;
			}
		}

		/// <summary>
		/// 定义一个插入数据的命令
		/// </summary>
		public string InsertCommand
		{
			get
			{
				return insertCommand;
			}
			set
			{
				insertCommand = value;
			}
		}

		/// <summary>
		/// 定义一个更新数据的命令
		/// </summary>
		public string UpdateCommand
		{
			get
			{
				return updateCommand;
			}
			set
			{
				updateCommand = value;
			}
		}

		/// <summary>
		/// 定义一个删除数据的命令
		/// </summary>
		public string DeleteCommand
		{
			get
			{
				return deleteCommand;
			}
			set
			{
				deleteCommand = value;
			}
		}

		//回调委托，用于重新输出查询语句的输出参数
		internal Action<SqlExecuter, SqlCommand> outputHandler;

		//SqlCommand适配器
		internal SqlCommandAdapter commandAdapter;

		//Sql连接供应器
		internal SqlHelper helper;

		internal SqlCommand command = new SqlCommand();

		/// <summary>
		/// 插入语句
		/// </summary>
		internal string insertCommand;

		/// <summary>
		/// 更新语句
		/// </summary>
		internal string updateCommand;

		/// <summary>
		/// 删除语句
		/// </summary>
		internal string deleteCommand;

		//缓存ID
		internal readonly string ID;


		//构造方法
		internal SqlExecuter(SqlHelper helper, string id, SqlCommandAdapter adapter, Action<SqlExecuter, SqlCommand> inputHandler, Action<SqlExecuter, SqlCommand> outputHandler)
		{
			this.ID = id;

			this.helper = helper;
			this.commandAdapter = adapter;
			this.outputHandler = outputHandler;

			this.commandAdapter.InitCommand(command);
			if (inputHandler != null) inputHandler(this, command);
		}

		#region 执行方法
		#region ExecuteNonQuery
		/// <summary>
		/// 执行查询并且返回影响的行数
		/// </summary>
		/// <returns>
		/// 影响的行数
		/// </returns>
		public int ExecuteNonQuery()
		{
			return InternalExecuteNonQuery();
		}
		#endregion

		#region ExecuteReader
		/// <summary>
		/// 执行查询,并且返回一个数据访问器
		/// </summary>
		/// <returns>
		/// 数据访问器
		/// </returns>
		public DbDataReader ExecuteDbReader()
		{
			return InternalExecuteReader(CommandBehavior.Default);
		}

		/// <summary>
		/// 执行查询,并且返回一个数据访问器
		/// </summary>
		/// <returns>
		/// 数据访问器
		/// </returns>
		public SqlDataReader ExecuteReader(CommandBehavior commandBehavior = CommandBehavior.Default)
		{
			helper.connectionProvider.InitSqlCommand(command);
			return SqlEmitter.ExecuteReader(command, commandBehavior, SqlEmitter.RETURN_IMMEDIATELY, true);
		}
		#endregion

		#region ExecuteScalar

		/// <summary>
		/// 执行查询,返回记录集的第一行第一列
		/// </summary>
		/// <returns>
		/// 记录集的第一行第一列，如果记录集无数据，返回null
		/// </returns>
		public object ExecuteScalar()
		{
			return InternalExecuteScalar((reader) => { return ColumnFromReader(reader, 0); });
		}

		/// <summary>
		/// 执行查询,返回记录集的第一行指定列
		/// </summary>
		/// <param name="name">列名</param>
		/// <returns>
		/// 记录集的第一行指定列，如果记录集无数据，返回null
		/// </returns>
		public object ExecuteScalar(string name)
		{
			return InternalExecuteScalar((reader) => { return ColumnFromReader(reader, name); });
		}

		/// <summary>
		/// 执行查询,返回记录集的第一行指定列
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns>
		/// 记录集的第一行指定列，如果记录集无数据，返回null
		/// </returns>
		public object ExecuteScalar(int index)
		{
			return InternalExecuteScalar((reader) => { return ColumnFromReader(reader, index); });
		}

		/// <summary>
		/// 执行查询,返回记录集的第一行第一列并转换成指定类型
		/// </summary>
		/// <typeparam name="T">期望返回类型，必须是可转换的类型，例如int, string等</typeparam>
		/// <returns>
		/// 转换后的记录集的第一行第一列，如果记录集无数据，返回null
		/// </returns>
		public T ExecuteScalar<T>() where T : IConvertible
		{
			return InternalConvert<T>(ExecuteScalar());
		}

		/// <summary>
		/// 执行查询,返回记录集的第一行指定列并转换成指定类型
		/// </summary>
		/// <typeparam name="T">期望返回类型，必须是可转换的类型，例如int, string等</typeparam>
		/// <param name="name">列名</param>
		/// <returns>
		/// 转换后的记录集的第一行指定列，如果记录集无数据，返回null
		/// </returns>
		public T ExecuteScalar<T>(string name) where T : IConvertible
		{
			return InternalConvert<T>(ExecuteScalar(name));
		}

		/// <summary>
		/// 执行查询,返回记录集的第一行指定列并转换成指定类型
		/// </summary>
		/// <typeparam name="T">期望返回类型，必须是可转换的类型，例如int, string等</typeparam>
		/// <param name="index">索引</param>
		/// <returns>
		/// 转换后的记录集的第一行指定列，如果记录集无数据，返回null
		/// </returns>
		public T ExecuteScalar<T>(int index) where T : IConvertible
		{
			return InternalConvert<T>(ExecuteScalar(index));
		}
		#endregion

		#region ExecuteJSON
		/// <summary>
		/// 执行查询并且返回JSON字符串
		/// </summary>
		/// <returns>
		/// JSON字符串，内容为一个2维数组，第一维为记录集，第二维为表，表的元素为对象，每个对象对应表里面的一行记录
		/// </returns>
		public string ExecuteJSON()
		{
			return InternalExecuteJSON(CommandBehavior.Default);
		}
		#endregion

		#region ExecuteDataSet
		/// <summary>
		/// 执行查询并且返回数据集
		/// </summary>
		/// <returns>
		/// 返回一个DataSet,如果查询结果无任何记录,则该DataSet为空记录集
		/// 如果查询结果具备记录集但是无数据,则该DataSet对应的DataTable为空白DataTable
		/// </returns>
		public DataSet ExecuteDataSet()
		{
			return InternalExecuteDataSet(CommandBehavior.Default);
		}
		#endregion

		#region ExecuteDataTable
		/// <summary>
		/// 执行查询并且返回第一个结果集的数据表格
		/// </summary>
		/// <returns>
		/// 返回一个DataTable,如果查询结果无任何记录，则返回null
		/// </returns>
		public DataTable ExecuteDataTable()
		{
			DataSet ds = InternalExecuteDataSet(CommandBehavior.SingleResult);
			if (ds.Tables.Count > 0) return ds.Tables[0];
			return null;
		}
		#endregion

		#region ExecuteFirstRow
		/// <summary>
		/// 执行查询并且返回第一个记录集的第一行
		/// </summary>
		/// <returns>
		/// 返回结果记录集的第一个记录集的第一行,如果查询结果不包含任何记录集或者第一个记录集不包含任何数据,则返回null
		/// </returns>
		public DataRow ExecuteFirstRow()
		{
			DataSet ds = InternalExecuteDataSet(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
			if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
			{
				return ds.Tables[0].Rows[0];
			}
			return null;
		}
		#endregion

		#region ExecuteSingleRow
		/// <summary>
		/// 执行查询,返回数据集，这个数据集的每张表包含了对应记录集的第一行
		/// </summary>
		/// <returns>
		/// 数据集，这个数据集的每张表包含了对应记录集的第一行，如果该记录集为空，则对应表为空
		/// </returns>
		public DataSet ExecuteSingleRow()
		{
			return InternalExecuteDataSet(CommandBehavior.SingleRow);
		}
		#endregion

		#region ExecuteActivator
		/// <summary>
		/// 执行查询,并且返回一个SqlActivator实例,SqlActivator实例可以额外进行多种数据封装任务
		/// </summary>
		/// <returns>
		/// SqlActivator实例
		/// </returns>
		/// <remarks>
		/// SqlActivator实现了IEnumerable接口，你可以使用类似于foreach(SqlRecord record in helper.ExecuteActivator()){}的语句直接遍历查询结果。
		/// 其效果等价于ExecuteDynamicList
		/// </remarks>
		/// <seealso cref="MonQ.Data.SqlClient.SqlActivator"/>
		public SqlActivator ExecuteActivator()
		{
			return new SqlActivator(InternalExecuteReader(CommandBehavior.Default), this, this.ID);
		}

		/// <summary>
		/// 执行查询,并且返回一个实现了IEnumerable&lt;EntityType>的SqlActivator实例,SqlActivator实例可以额外进行多种数据封装任务
		/// </summary>
		/// <typeparam name="EntityType">实体类型</typeparam>
		/// <returns>
		/// SqlActivator实例
		/// </returns>
		/// <remarks>
		/// 你可以直接使用类似于foreach(EntityType item in helper.ExecuteActivator&lt;EntityType>()){}的语句直接遍历查询结果。
		/// 其效果等价于ExecuteEntityList
		/// </remarks>
		/// <seealso cref="MonQ.Data.SqlClient.SqlActivator"/>
		public SqlActivator<EntityType> ExecuteActivator<EntityType>()
		{
			return new SqlActivator<EntityType>(InternalExecuteReader(CommandBehavior.Default), this, this.ID);
		}
		#endregion

		#region ExecuteEntity
		/// <summary>
		/// 执行查询，将第一条查询记录封装为指定类型的实例并且返回
		/// </summary>
		/// <returns>
		/// 所生成的实例，如果没有数据，返回指定类型的默认值
		/// </returns>
		public EntityType ExecuteEntity<EntityType>()
		{
			return InternalExecuteEntity<EntityType>();
		}

		#endregion

		#region ExecuteEntityList

		/// <summary>
		/// 执行查询，将第一个记录集封装为指定类型的实例的数组并且返回
		/// </summary>
		/// <returns>
		/// 所生成的实例，如果没有数据，返回空白列表
		/// </returns>
		public List<EntityType> ExecuteEntityList<EntityType>()
		{
			return InternalExecuteEntityList<EntityType>();
		}


		#endregion

		#region ExecuteIterator
		/// <summary>
		/// 执行查询，遍历第一个记录集的对象，逐次生成指定类型的实体供回调方法调用，最后返回数据库查询助手以便调用下一个方法继续查询
		/// </summary>
		/// <typeparam name="EntityType">实体类型</typeparam>
		/// <param name="callback">回调方法</param>
		/// <remarks>
		/// 与ExecuteActivator(CallbackHandler&lt;SqlActivator> callback)的区别：
		/// ExecuteActivator只生成SqlActivator实例，然后以这个实例作为参数供回调方法调用；
		/// ExecuteIterator系列会生成SqlActivator实例后逐次遍历第一个记录表中的行数据，生成对应实体的实例，然后以实体的实例作为参数供回调方法调用。
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">callback为null</exception>
		public void ExecuteIterator<EntityType>(Action<EntityType> callback)
		{
			this.InternalExecuteIterator(activator => callback(activator.CreateEntity<EntityType>()));
		}


		/// <summary>
		/// 执行查询，遍历第一个记录集的对象，逐次生成动态类型的实体供回调方法调用，最后返回数据库查询助手以便调用下一个方法继续查询
		/// </summary>
		/// <param name="callback">回调方法</param>
		/// <returns>
		/// 数据库查询助手
		/// </returns>
		/// <remarks>
		/// 与ExecuteActivator(CallbackHandler&lt;SqlActivator> callback)的区别：
		/// ExecuteActivator只生成SqlActivator实例，然后以这个实例作为参数供回调方法调用；
		/// ExecuteIterator系列会生成SqlActivator实例后逐次遍历第一个记录表中的行数据，每成功读取一行数据，则以生成的SqlActivator实例作为参数供回调方法调用一次。
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">callback为null</exception>
		public void ExecuteIterator(Action<SqlRecord> callback)
		{
			this.InternalExecuteIterator(activator => callback(activator.CreateDynamic()));
		}
		#endregion

		#region ExecutePagination
		#endregion

		#region ExecuteDynamic
		/// <summary>
		/// 以指定的参数执行当前查询，将输出参数的值或返回值重新回写到参数列表里面，返回第一个记录集的第一行数据并且封装成一个派生自SqlDynamicRecord的动态类型实例
		/// </summary>
		/// <returns>
		/// 返回对应实例，若数据不存在，返回null
		/// </returns>
		public dynamic ExecuteDynamic()
		{
			return InternalExecuteDynamic();
		}
		#endregion

		#region ExecuteDynamicList
		/// <summary>
		/// 执行当前查询，将第一个记录集的数据并且封装成一个动态类型的List对象
		/// </summary>
		/// <returns>
		/// 返回对应实例，若数据不存在，返回null
		/// </returns>
		public List<dynamic> ExecuteDynamicList()
		{
			return InternalExecuteDynamicList();
		}
		#endregion
		#endregion

		#region 公开的杂项方法
		/// <summary>
		/// 在执行查询之后重新将查询结果的输出参数的值保存到一个结构体的实例之中
		/// </summary>
		/// <typeparam name="T">结构体的类型</typeparam>
		/// <param name="target">需要保存到的结构体的实例</param>
		/// <remarks>
		/// 这个方法专门针对结构体而设,对于class类型的实例，查询后自动会回写到对应成员中，无需额外调用这个方法
		/// </remarks>
		public void RetrieveParameters<T>(ref T target) where T : struct
		{
			this.commandAdapter.RetrieveParams<T>(this.command, ref target);
		}

		/// <summary>
		/// 设定查询的超时时间
		/// </summary>
		/// <param name="timeout">超时时间</param>
		/// <returns>总是返回当前执行器本身</returns>
		public SqlExecuter SetTimeout(int timeout)
		{
			this.command.CommandTimeout = Math.Max(timeout, 0);
			return this;
		}

		/// <summary>
		/// 对SqlCommand对象进行进一步操作
		/// </summary>
		/// <param name="action">需要执行的操作</param>
		/// <returns>总是返回当前执行器本身</returns>
		/// <remarks>
		/// <para>此时SqlCommand对象的Sql连接尚未初始化</para>
		/// <para>只有在发出明确的查询指令之时SqlExectuer才会初始化数据库连接并且立即执行查询</para>
		/// </remarks>
		public SqlExecuter MoreAction(Action<SqlCommand> action)
		{
			if (action != null)
			{
				action(this.command);
			}
			return this;
		}
		#endregion

		#region 内部方法

		#region Execute用到的内部方法
		//执行并返回受影响行数
		private int InternalExecuteNonQuery()
		{
			try
			{
				this.helper.connectionProvider.InitSqlCommand(command);
				SqlEmitter.ExecuteReader(command, CommandBehavior.Default, SqlEmitter.UNTIL_DONE, false);
				if (outputHandler != null) outputHandler(this, command);
				return SqlEmitter.GetRowsAffected(command);
			}
			catch
			{
				this.helper.RemoveAdapater(this.commandAdapter);
				throw;
			}
			finally
			{
				helper.connectionProvider.DeinitSqlCommand(command);
			}
		}

		//执行并返回数据访问器
		private SqlReader InternalExecuteReader(CommandBehavior commandBehavior)
		{
			helper.connectionProvider.InitSqlCommand(command);
			try
			{
				SqlDataReader reader = SqlEmitter.ExecuteReader(command, commandBehavior, SqlEmitter.RETURN_IMMEDIATELY, true);
				return new SqlReader(reader, (sender, args) => { helper.connectionProvider.DeinitSqlCommand(command); if (outputHandler != null)outputHandler(this, command); });
			}
			catch
			{
				this.helper.RemoveAdapater(this.commandAdapter);
				throw;
			}
		}

		//读取第一行第一列
		private object InternalExecuteScalar(Func<SqlDataReader, object> callback)
		{
			helper.connectionProvider.InitSqlCommand(command);
			try
			{
				using (SqlDataReader reader = SqlEmitter.ExecuteReader(command, CommandBehavior.SingleResult | CommandBehavior.SingleRow, SqlEmitter.RETURN_IMMEDIATELY, true))
				{
					try
					{
						return callback(reader);
					}
					finally
					{
						if (outputHandler != null) outputHandler(this, command);
					}
				}
			}
			catch
			{
				this.helper.RemoveAdapater(this.commandAdapter);
				throw;
			}
			finally
			{
				helper.connectionProvider.DeinitSqlCommand(command);
			}
		}


		//读取数据访问器指定列
		private object ColumnFromReader(DbDataReader reader, int index)
		{
			try
			{
				if (index == -1 || !reader.Read() || reader.FieldCount <= index || index < 0)
				{
					return null;
				}
				else
				{
					return reader.GetValue(index);
				}
			}
			finally
			{
				reader.Close();
			}
		}

		//读取数据访问器指定列
		private object ColumnFromReader(DbDataReader reader, string name)
		{
			try
			{
				return ColumnFromReader(reader, reader.GetOrdinal(name));
			}
			finally
			{
				reader.Close();
			}
		}

		//数据类型转换
		private T InternalConvert<T>(object o)
		{
			return o is T ? (T)o : o == null ? default(T) : (T)Convert.ChangeType(o, typeof(T));
		}

		//查询并且转换为JSON
		private string InternalExecuteJSON(CommandBehavior commandBehavior = CommandBehavior.Default)
		{
			this.helper.connectionProvider.InitSqlCommand(command);

			try
			{

				StringBuilder json = new StringBuilder(200);
				json.Append('[').AppendLine();

				SqlDataReader reader = SqlEmitter.ExecuteReader(command, commandBehavior | CommandBehavior.SequentialAccess, SqlEmitter.RETURN_IMMEDIATELY, true);

				JavaScriptSerializer serializer = new JavaScriptSerializer();

				while (true)
				{
					json.Append('[');
					int fieldCount = reader.FieldCount;
					if (reader.Read())
					{
						while (true)
						{
							json.Append('{');
							for (int i = 0; i < fieldCount; i++)
							{
								if (i > 0) json.Append(',');
								json.AppendFormat("\"{0}\":{1}", reader.GetName(i), serializer.Serialize(reader.GetValue(i)));
							}
							json.Append('}');
							if (!reader.Read())
							{
								break;
							}
							json.Append(',');
						}
					}
					json.Append(']');
					if (!reader.NextResult())
					{
						break;
					}
					json.Append(',').AppendLine();
				}

				json.Append(']').AppendLine();
				return json.ToString();
			}
			catch
			{
				this.helper.RemoveAdapater(this.commandAdapter);
				throw;
			}
			finally
			{
				this.helper.connectionProvider.DeinitSqlCommand(command);
			}
		}

		//查询并封装为数据集
		private DataSet InternalExecuteDataSet(CommandBehavior commandBehavior)
		{
			this.helper.connectionProvider.InitSqlCommand(command);
			try
			{
				SqlDataReader reader = SqlEmitter.ExecuteReader(command, commandBehavior | CommandBehavior.SequentialAccess, SqlEmitter.RETURN_IMMEDIATELY, true);

				DataSet ds = new DataSet();
				do
				{
					DataTable table = new DataTable();
					ds.Tables.Add(table);
					int fieldCount = reader.FieldCount;
					for (int i = 0; i < fieldCount; i++)
					{
						table.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
					}
					while (reader.Read())
					{
						DataRow row = table.NewRow();
						for (int i = 0; i < fieldCount; i++)
						{
							row[i] = reader.GetValue(i);
						}
						table.Rows.Add(row);
					}
				}
				while (reader.NextResult());
				reader.Close();
				if (outputHandler != null) outputHandler(this, command);
				return ds;
			}
			catch
			{
				this.helper.RemoveAdapater(this.commandAdapter);
				throw;
			}
			finally
			{
				this.helper.connectionProvider.DeinitSqlCommand(command);
			}
		}

		//查询并封装实体
		private EntityType InternalExecuteEntity<EntityType>()
		{
			using (SqlActivator activator = new SqlActivator(InternalExecuteReader(CommandBehavior.SingleRow | CommandBehavior.SingleResult), this, this.ID))
			{
				if (activator.Reader.Read())
				{
					return activator.CreateEntity<EntityType>();
				}
				activator.Close();
			}
			return default(EntityType);
		}

		//查询并封装实体列表
		private List<EntityType> InternalExecuteEntityList<EntityType>()
		{
			using (SqlActivator activator = new SqlActivator(InternalExecuteReader(CommandBehavior.SingleResult), this, this.ID))
			{
				return activator.CreateEntities<EntityType>();
			}
		}

		//查询并封装动态类型实例
		private SqlRecord InternalExecuteDynamic()
		{
			using (SqlActivator activator = new SqlActivator(InternalExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow), this, this.ID))
			{
				if (activator.Reader.Read())
				{
					return activator.CreateDynamic();
				}
			}
			return null;
		}

		//查询并封装动态类型实例
		private List<dynamic> InternalExecuteDynamicList()
		{
			using (SqlActivator activator = new SqlActivator(InternalExecuteReader(CommandBehavior.SingleResult), this, this.ID))
			{
				return activator.CreateDynamicList();
			}
		}

		//执行查询并且遍历
		private void InternalExecuteIterator(Action<SqlActivator> callback)
		{
			if (callback == null) throw new ArgumentNullException("callback");
			using (SqlActivator activator = new SqlActivator(InternalExecuteReader(CommandBehavior.SingleResult), this, this.ID))
			{
				while (activator.Reader.Read())
				{
					callback(activator);
				}
				activator.Close();
			}
		}
		#endregion

		#endregion

		/// <summary>
		/// 释放全部资源
		/// </summary>
		public void Dispose()
		{
			this.command.Dispose();
		}
	}
}
