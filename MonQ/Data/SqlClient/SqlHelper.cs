using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using MonQ.Data.SqlClient.ConnectionProviders;
using MonQ.Data.SqlClient.CommandAdapters;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Configuration;
using MonQ.Properties;
using System.Web.Script.Serialization;
using MonQ.Data.SqlClient.SqlStatements;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 数据库查询助手
	/// </summary>
	public class SqlHelper
	{
		private const string CONST_SQLTEXTADAPTER = "SQLTEXTADAPTER";

		/// <summary>
		/// 同步标签
		/// </summary>
		public readonly Object SyncRoot = new object();

		/// <summary>
		/// 一个动态属性，可以以此创建SqlHelper对象
		/// </summary>
		public static dynamic Helper
		{
			get
			{
				return helper;
			}
		}

		/// <summary>
		/// 一个动态属性，可以以此创建基于数据库对象的SQL执行器
		/// </summary>
		public dynamic DbContext
		{
			get
			{
				db = db ?? new SqlDbContext(this);
				return db;
			}
		}

		/// <summary>
		/// 获得一个可以构建执行删除操作的SQL执行器
		/// </summary>
		public IQueryDelete DELETE
		{
			get
			{
				return new DeleteStatement(this);
			}
		}

		/// <summary>
		/// 获得一个可以构建选取操作的SQL执行器
		/// </summary>
		public IQuerySelect SELECT
		{
			get
			{
				return new SelectStatement(this);
			}
		}

		/// <summary>
		/// 获得一个可以进行更新操作的SQL执行器
		/// </summary>
		public IQueryUpdate UPDATE
		{
			get
			{
				return new UpdateStatement(this);
			}
		}

		/// <summary>
		/// 获取Sql连接供应器
		/// </summary>
		internal SqlConnectionProvider ConnectionProvider
		{
			get
			{
				return connectionProvider;
			}
		}

		private SqlDbContext db;

		private static SqlHelperProvider helper = new SqlHelperProvider();

		//标记SqlConnection实例的ID
		private const string CONNECTION_INSTANCE = "CONNECTION_INSTANCE";

		private static ConcurrentDictionary<string, SqlConnectionProvider> cachedProviders = new ConcurrentDictionary<string, SqlConnectionProvider>();

		//Sql连接供应器
		internal readonly SqlConnectionProvider connectionProvider;

		#region 构造方法
		internal SqlHelper(SqlConnectionProvider connectionProvider)
		{
			this.connectionProvider = connectionProvider;
		}
		#endregion

		#region 公共方法

		/// <summary>
		/// 获取一个SQL连接
		/// </summary>
		public SqlConnection CreateConnection()
		{
			return this.connectionProvider.OpenConnection();
		}

		#region 创建SQL执行器
		/// <summary>
		/// 根据指定的查询语句生成查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <example>
		/// helper.CreateExecuter("SELECT * FROM Employee WHERE ID = 12 AND Title = 'UFO'").ExecuteNonQuery();
		/// </example>
		public SqlExecuter FromText(string commandText)
		{
			return InternalCreateExecuter(commandText, null, null);
		}

		/// <summary>
		/// 根据指定的查询语句及顺序传递的参数值生成一个查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="parameterValues">参数值列表</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到数组之中
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <remarks>
		/// 以可变数量参数的方式传递参数的情形下无法正确回写数据，如果有需求，应该明确提供一个数组类型的实例
		/// </remarks>
		/// <example>
		/// int id = 12;
		/// helper.CreateExecuter("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", id, "UFO").ExecuteNonQuery();//无法得到输出参数@ID
		/// object[] parameters = new object[]{12, "UFO"};
		/// helper.CreateExecuter("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", parameters).ExecuteNonQuery();//可以得到输出参数@ID
		/// </example>
		public SqlExecuter FromArray(string commandText, params object[] parameterValues)
		{
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, parameterValues);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, parameterValues);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及参数列表生成一个查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="parameters">参数列表</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到对应的参数
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <example>
		/// SqlParameter id = new SqlParameter("@ID", (object)12);
		/// helper.CreateExecuter("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", id, new SqlParameter("@Title", "UFO")).ExecuteNonQuery();
		/// Console.WriteLine(id.Value);
		/// </example>
		public SqlExecuter FromParameters(string commandText, params SqlParameter[] parameters)
		{
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, parameters);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, parameters);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实体类型的实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <typeparam name="ParameterEntity">实体类型</typeparam>
		/// <param name="commandText">查询语句</param>
		/// <param name="entity">实体</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实体对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">entity参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlExecuter FromEntity<ParameterEntity>(string commandText, ParameterEntity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, entity);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, entity);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实例对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <remarks>
		///	使用此方法可以将返回值输出到匿名类型的实例
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">instance参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlExecuter FromObject(string commandText, object instance)
		{
			if (instance == null) throw new ArgumentNullException("instance");
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParamsByObject(command, instance);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParamsByObject(command, instance);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实例对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <remarks>
		///	使用此方法可以将返回值输出到匿名类型的实例
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">instance参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlExecuter FromJSON(string commandText, string jsonString)
		{
			if (jsonString == null) throw new ArgumentNullException("jsonString");
			object json = new JavaScriptSerializer().DeserializeObject(jsonString);
			if (json is object[]) return FromArray(commandText, json as object[]);
			else if (json is Dictionary<string, object>) return FromEntity(commandText, json as Dictionary<string, object>);
			else if (json.GetType().IsValueType) return FromArray(commandText, json);
			else throw new NotSupportedException(string.Format(Resources.SqlClient_TypeNotSupported, json.GetType().FullName));
		}

		/// <summary>
		/// 根据指定的查询语句及一个数据行，以这一行的各个列作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="row">数据行</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到行
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">row参数为null</exception>
		public SqlExecuter FromRow(string commandText, DataRow row)
		{
			if (row == null) throw new ArgumentNullException("row");
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, row);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, row);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个字典，以这个字典的各个键值对作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="dictionary">字典</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到字典
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">dictionary参数为null</exception>
		public SqlExecuter FromDictionary(string commandText, IDictionary dictionary)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, dictionary);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, dictionary);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个集合，以这个集合的各个键值对作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="collection">集合</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到集合
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">collection参数为null</exception>
		public SqlExecuter FromCollection(string commandText, NameValueCollection collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			return InternalCreateExecuter(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, collection);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, collection);
				});
		}
		#endregion

		#region 创建SQL分页器
		/// <summary>
		/// 根据指定的查询语句生成查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <example>
		/// helper.CreatePager("SELECT * FROM Employee WHERE ID = 12 AND Title = 'UFO'").ExecuteNonQuery();
		/// </example>
		public SqlPager Pager(string commandText)
		{
			return InternalCreatePager(commandText, null, null);
		}

		/// <summary>
		/// 根据指定的查询语句及顺序传递的参数值生成一个查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="parameterValues">参数值列表</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到数组之中
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <remarks>
		/// 以可变数量参数的方式传递参数的情形下无法正确回写数据，如果有需求，应该明确提供一个数组类型的实例
		/// </remarks>
		/// <example>
		/// int id = 12;
		/// helper.CreatePager("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", id, "UFO").ExecuteNonQuery();//无法得到输出参数@ID
		/// object[] parameters = new object[]{12, "UFO"};
		/// helper.CreatePager("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", parameters).ExecuteNonQuery();//可以得到输出参数@ID
		/// </example>
		public SqlPager PagerFromArray(string commandText, params object[] parameterValues)
		{
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, parameterValues);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, parameterValues);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及参数列表生成一个查询执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="parameters">参数列表</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到对应的参数
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <example>
		/// SqlParameter id = new SqlParameter("@ID", (object)12);
		/// helper.CreatePager("SELECT * FROM Employee WHERE ID = @ID AND Title = @Title; SELECT @ID = 0;", id, new SqlParameter("@Title", "UFO")).ExecuteNonQuery();
		/// Console.WriteLine(id.Value);
		/// </example>
		public SqlPager PagerFromParameters(string commandText, params SqlParameter[] parameters)
		{
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, parameters);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, parameters);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实体类型的实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <typeparam name="ParameterEntity">实体类型</typeparam>
		/// <param name="commandText">查询语句</param>
		/// <param name="entity">实体</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实体对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">entity参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlPager PagerFromEntity<ParameterEntity>(string commandText, ParameterEntity entity)
		{
			if (entity == null) throw new ArgumentNullException("entity");
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, entity);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, entity);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实例对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <remarks>
		///	使用此方法可以将返回值输出到匿名类型的实例
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">instance参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlPager PagerFromObject(string commandText, object instance)
		{
			if (instance == null) throw new ArgumentNullException("instance");
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParamsByObject(command, instance);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParamsByObject(command, instance);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个实例，以此实例的成员作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到实例对应的成员，如果数据类型不一致，自动进行转换
		/// </returns>
		/// <remarks>
		///	使用此方法可以将返回值输出到匿名类型的实例
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">instance参数为null</exception>
		/// <exception cref="System.InvalidCastException">参数的输出值类型无法转换到实体成员对应的类型</exception>
		public SqlPager PagerFromJSON(string commandText, string jsonString)
		{
			if (jsonString == null) throw new ArgumentNullException("jsonString");
			object json = new JavaScriptSerializer().DeserializeObject(jsonString);
			if (json is object[]) return PagerFromArray(commandText, json as object[]);
			else if (json is Dictionary<string, object>) return PagerFromDictionary(commandText, json as Dictionary<string, object>);
			else if (json.GetType().IsValueType) return PagerFromArray(commandText, json);
			else throw new NotSupportedException(string.Format(Resources.SqlClient_TypeNotSupported, json.GetType().FullName));
		}

		/// <summary>
		/// 根据指定的查询语句及一个数据行，以这一行的各个列作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="row">数据行</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到行
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">row参数为null</exception>
		public SqlPager PagerFromRow(string commandText, DataRow row)
		{
			if (row == null) throw new ArgumentNullException("row");
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, row);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, row);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个字典，以这个字典的各个键值对作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="dictionary">字典</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到字典
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">dictionary参数为null</exception>
		public SqlPager PagerFromDictionary(string commandText, IDictionary dictionary)
		{
			if (dictionary == null) throw new ArgumentNullException("dictionary");
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, dictionary);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, dictionary);
				});
		}

		/// <summary>
		/// 根据指定的查询语句及一个集合，以这个集合的各个键值对作为查询命令的参数来源生成一个数据库执行对象
		/// </summary>
		/// <param name="commandText">查询语句</param>
		/// <param name="collection">集合</param>
		/// <returns>
		/// 查询执行对象，使用此对象可以进行各种方式的查询,每次执行查询后，重新将输出参数的值回写到集合
		/// </returns>
		/// <exception cref="System.ArgumentNullException">commandText参数为null或者空</exception>
		/// <exception cref="System.ArgumentNullException">collection参数为null</exception>
		public SqlPager PagerFromCollection(string commandText, NameValueCollection collection)
		{
			if (collection == null) throw new ArgumentNullException("collection");
			return InternalCreatePager(commandText,
				(executer, command) =>
				{
					executer.commandAdapter.InitParams(command, collection);
				},
				(executer, command) =>
				{
					executer.commandAdapter.RetrieveParams(command, collection);
				});
		}
		#endregion

		#region 创建SQL帮助对象
		/// <summary>
		/// 创建数据库查询助手对象
		/// </summary>
		/// <param name="connection">可以是数据库连接字符串本身，连字符串在Connections配置中的配置名称或者AppSettings中的配置名称</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(string connection)
		{
			return InternalCreateHelper(connection, false, IsolationLevel.Unspecified, null);
		}

		/// <summary>
		/// 创建数据库查询助手对象并且在查询时创建一个指定隔离级别的事务
		/// </summary>
		/// <param name="connection">可以是数据库连接字符串本身，连字符串在Connections配置中的配置名称或者AppSettings中的配置名称</param>
		/// <param name="iso">事务的隔离级别</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(string connection, IsolationLevel iso)
		{
			SqlHelper helper = InternalCreateHelper(connection, true, iso, null);
			return helper;
		}

		/// <summary>
		/// 创建数据库查询助手对象并且在查询时创建一个指定名称的默认隔离级别的事务
		/// </summary>
		/// <param name="connection">可以是数据库连接字符串本身，连字符串在Connections配置中的配置名称或者AppSettings中的配置名称</param>
		/// <param name="transactionName">事务名称</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(string connection, string transactionName)
		{
			return InternalCreateHelper(connection, true, IsolationLevel.Unspecified, transactionName);
		}

		/// <summary>
		/// 创建数据库查询助手并且在查询时创建指定的名称与级别的隔离事务
		/// </summary>
		/// <param name="connection">可以是数据库连接字符串本身，连字符串在Connections配置中的配置名称或者AppSettings中的配置名称</param>
		/// <param name="transactionName">事务名称</param>
		/// <param name="iso">事务的隔离级别</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(string connection, IsolationLevel iso, string transactionName)
		{
			return InternalCreateHelper(connection, true, iso, transactionName);
		}

		/// <summary>
		/// 创建数据库查询助手对象
		/// </summary>
		/// <param name="connection">数据库连接</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(SqlConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");
			if (string.IsNullOrEmpty(connection.ConnectionString)) throw new ArgumentException("connection");
			if (connection.State == ConnectionState.Closed || connection.State == ConnectionState.Broken) throw new ArgumentException("connection");
			string hashCode = CONNECTION_INSTANCE + connection.GetHashCode();
			SqlConnectionProvider connectionProvider = cachedProviders.GetOrAdd(hashCode, (key) =>
			{
				SqlConnectionProvider cached;
				connection.StateChange += (sender, state) =>
				{
					switch (state.CurrentState)
					{
						case ConnectionState.Closed:
						case ConnectionState.Broken:
							cachedProviders.TryRemove(hashCode, out cached);
							break;
					}
				};
				return new SqlExternalConnectionProvider(connection);
			});
			return new SqlHelper(connectionProvider);
		}

		/// <summary>
		/// 创建数据库查询助手对象
		/// </summary>
		/// <param name="transaction">数据库事务</param>
		/// <returns>
		/// 如果对应数据库查询助手在缓存中已经存在，返回已经缓存的数据库查询助手，否则创建一个新的数据库查询助手实例
		/// </returns>
		public static SqlHelper Create(SqlTransaction transaction)
		{
			if (transaction == null) throw new ArgumentNullException("transaction");
			string hashCode = CONNECTION_INSTANCE + transaction.GetHashCode();
			SqlConnectionProvider connectionProvider = cachedProviders.GetOrAdd(hashCode, (key) =>
			{
				SqlConnectionProvider cached;
				transaction.Connection.StateChange += (sender, state) =>
				{
					switch (state.CurrentState)
					{
						case ConnectionState.Closed:
						case ConnectionState.Broken:
							cachedProviders.TryRemove(hashCode, out cached);
							break;
					}
				};
				return new SqlExternalConnectionProvider(transaction);
			});
			return new SqlHelper(connectionProvider);
		}
		#endregion

		#endregion

		#region 内部调用方法


		#region InternalCreateExecuter

		private SqlExecuter InternalCreateExecuter(string commandText, Action<SqlExecuter, SqlCommand> inputHandler, Action<SqlExecuter, SqlCommand> outputHandler)
		{
			if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException("commandText");
			lock (SyncRoot)
			{
				string id = commandText + "$" + connectionProvider.ConnectionString;
				SqlCommandAdapter commandAdapter = InternalCreateCommandAdapter(connectionProvider, commandText);
				return new SqlExecuter(this,
					id,
					commandAdapter,
					inputHandler,
					outputHandler);
			}
		}
		#endregion

		#region InternalCreatePager
		private SqlPager InternalCreatePager(string commandText, Action<SqlExecuter, SqlCommand> inputHandler, Action<SqlExecuter, SqlCommand> outputHandler)
		{
			return new SqlPager(InternalCreateExecuter(commandText, inputHandler, outputHandler));
		}
		#endregion

		#region InternalCreateHelper
		private static SqlHelper InternalCreateHelper(string connection, bool createTransaction, IsolationLevel iso, string transactionName)
		{
			if (string.IsNullOrEmpty(connection)) throw new ArgumentNullException("connection");

			SqlConnectionProvider connectionProvider = cachedProviders.GetOrAdd(connection, (key) =>
			{
				if (Regex.IsMatch(connection, @"(\bserver\s*=)|(\bdata source\s*=)", RegexOptions.Compiled | RegexOptions.IgnoreCase))//本身即是连接字符串
				{
					return new SqlInternalConnectionProvider(connection, createTransaction, iso, transactionName);
				}
				ConnectionStringSettings str = ConfigurationManager.ConnectionStrings[connection];//配置文件中的连接字符串
				if (str != null)
				{
					return new SqlInternalConnectionProvider(str.ConnectionString, createTransaction, iso, transactionName);
				}
				string config = ConfigurationManager.AppSettings[connection];
				if (config != null && Regex.IsMatch(config, @"(\bserver\s*=)|(\bdata source\s*=)", RegexOptions.Compiled | RegexOptions.IgnoreCase))
				{
					return new SqlInternalConnectionProvider(config, createTransaction, iso, transactionName);
				}

				key = Regex.Replace(key, @"(User\s*ID\s*=\s*)([^;]*)(;|$)", "$1$3", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				key = Regex.Replace(key, @"(Password\s*=\s*)([^;]*)(;|$)", "$1$3", RegexOptions.Compiled | RegexOptions.IgnoreCase);
				throw new Exception(string.Format(Resources.SqlClient_CanNotParseConnectionString, key));
			});
			return new SqlHelper(connectionProvider);
		}
		#endregion

		#region CreateCommandAdapter用到的方法及成员

		#region SQL对象分析查询语句
		/// <summary>
		/// SQL对象分析查询语句,用于分析一个对象以便确定对象类型
		/// </summary>
		private static readonly string OBJECT_ANALYSE_QUERY = @"DECLARE @ObjectID INT
DECLARE @DefaultValue VARCHAR(8000), @Query NVARCHAR(4000), @Name VARCHAR(200)

SELECT
	@Query	= 'SELECT @ObjectID = ID FROM {0}sysobjects WHERE Name = @ObjectName'
	
EXEC sp_executesql @Query , N'@ObjectID INT OUTPUT, @ObjectName VARCHAR(200)', @ObjectName = @ObjectName, @ObjectID = @ObjectID OUTPUT

PRINT @ObjectID

SELECT
	O.xtype ObjectType
FROM
	{0}sysobjects O
WHERE
	id	= @ObjectID

SELECT
	C.[Name],
	CASE ISNULL(C3.Text, '') WHEN '' THEN 0 ELSE 1 END IsDefault,
	C.IsNullable,
	C.isoutparam IsOutput,
	ISNULL(COLUMNPROPERTY(@ObjectID, C.[Name], N'IsIdentity'), 0) IsIdentity,
	ISNULL(COLUMNPROPERTY(@ObjectID, C.[Name], N'IsRowGuidCol'), 0) IsRowGuidCol,
	SIGN(PATINDEX('%NEWID%(%)%', ISNULL(C3.[Text], ''))) IsRowNewID,
	ISNULL(SIGN(PK.colid), 0) IsPrimaryKey,
	ISNULL(SIGN(CIK.colid), 0) IsClusteredKey,
	ISNULL(SIGN(UIK.colid), 0) IsUniqueKey,
	T.name TypeName,
	C.length Size,
	C.ColID ColID
FROM
	{0}syscolumns C
LEFT JOIN
	{0}sysconstraints C2 ON C2.[ID] = @ObjectID AND C2.ColID = C.ColID AND C2.Status & 0x05 = 0x05
LEFT JOIN
	{0}sysindexes P ON P.[ID] = @ObjectID AND OBJECTPROPERTY(OBJECT_ID(P.name), 'IsPrimaryKey') = 1
LEFT JOIN
	{0}sysindexkeys PK ON PK.id = @ObjectID AND PK.indid = P.indid AND PK.colid = C.colid
LEFT JOIN
	{0}sysindexes CI ON CI.[ID] = @ObjectID AND INDEXPROPERTY(@ObjectID, CI.name, 'IsClustered') = 1
LEFT JOIN
	{0}sysindexkeys CIK ON CIK.id = @ObjectID AND CIK.indid = CI.indid AND CIK.colid = C.colid
LEFT JOIN
	{0}sysindexes UI ON UI.[ID] = @ObjectID AND INDEXPROPERTY(@ObjectID, UI.name, 'IsUnique') = 1 AND UI.indid <> CI.indid AND UI.indid <> P.indid
LEFT JOIN
	{0}sysindexkeys UIK ON CIK.id = @ObjectID AND UIK.indid = UI.indid AND UIK.colid = C.colid
LEFT JOIN
	{0}syscomments C3 ON C3.[ID] = C2.ConstID
LEFT JOIN
	{0}systypes T ON C.xtype = T.xtype and C.xusertype = T.xusertype
WHERE
	C.[ID]=@ObjectID
	AND C.name <> ''
ORDER BY C.ColID ASC";

		#endregion

		/// <summary>
		/// 正则表达式 匹配SQL对象
		/// $1:对象数据库
		/// $2:对象名称
		/// </summary>
		private static readonly string REGEX_OBJECT = @"^([\[\]\._\-\w ]+\.)?([\[\]\._\-\w ]+)$";

		internal static DataSet AnalyseObject(SqlConnectionProvider connectionProvider, string objectName)
		{
			Match match;
			match = Regex.Match(objectName, REGEX_OBJECT, RegexOptions.Compiled | RegexOptions.IgnoreCase);
			if (match == null || !match.Success)
			{
				return null;
			}
			using (SqlCommand command = new SqlCommand())
			{
				connectionProvider.InitSqlCommand(command);
				try
				{
					command.CommandText = string.Format(OBJECT_ANALYSE_QUERY, match.Groups[1].Value);
					command.Parameters.Add(new SqlParameter("@ObjectName", match.Groups[2].Value.Trim('[', ']')));
					SqlDataReader reader = SqlEmitter.ExecuteReader(command, CommandBehavior.Default | CommandBehavior.SequentialAccess, SqlEmitter.RETURN_IMMEDIATELY, true);

					DataSet ds = new DataSet();
					while (true)
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
						if (!reader.NextResult())
						{
							break;
						}
					}
					reader.Close();
					return ds;
				}
				finally
				{
					connectionProvider.DeinitSqlCommand(command);
				}
			}
		}

		//缓存的适配器实例
		private static ConcurrentDictionary<string, ConcurrentDictionary<string, SqlCommandAdapter>> cachedAdapters = new ConcurrentDictionary<string, ConcurrentDictionary<string, SqlCommandAdapter>>();

		//生成SqlCommand适配器，主要分析查询语句的类型，到数据库中分析参数
		internal SqlCommandAdapter InternalCreateCommandAdapter(string commandText)
		{
			return InternalCreateCommandAdapter(connectionProvider, commandText);
		}
		//生成SqlCommand适配器，主要分析查询语句的类型，到数据库中分析参数
		private static SqlCommandAdapter InternalCreateCommandAdapter(SqlConnectionProvider connectionProvider, string commandText)
		{
			if (string.IsNullOrEmpty(commandText)) throw new ArgumentNullException("commandText");
			return cachedAdapters.GetOrAdd(connectionProvider.ConnectionString, (key) =>
			{
				return new ConcurrentDictionary<string, SqlCommandAdapter>();
			}).GetOrAdd(commandText, (key) =>
			{

				DataSet ds = AnalyseObject(connectionProvider, commandText);
				if (ds != null)
				{
					if (ds.Tables[0].Rows.Count == 1)
					{
						switch (((string)ds.Tables[0].Rows[0][0]).ToLower().TrimEnd())
						{
							case "u":
							case "v":
								//表或者视图
								return new SqlTableAdapter(connectionProvider.ConnectionString + commandText, commandText, ds.Tables[1]);
							case "p":
								//存储过程
								return new SqlStoreProcedureAdapter(connectionProvider.ConnectionString + commandText, commandText, ds.Tables[1]);
							case "fn":
								//标量函数
								return new SqlFunctionAdapter(connectionProvider.ConnectionString + commandText, commandText, ds.Tables[1]);
							case "if"://内联表函数
							case "tf"://表函数
								return new SqlTableFunctionAdapter(connectionProvider.ConnectionString + commandText, commandText, ds.Tables[1]);
							default:
								return null;
						}
					}
				}
				return new SqlTextAdapter(commandText, commandText);
			});
		}

		/// <summary>
		/// 删除指定的SQL适配器
		/// </summary>
		/// <param name="connectionProvider"></param>
		/// <param name="commandAdapter"></param>
		internal void RemoveAdapater(SqlCommandAdapter commandAdapter)
		{
			ConcurrentDictionary<string, SqlCommandAdapter> adapaters;
			if (cachedAdapters.TryGetValue(connectionProvider.ConnectionString, out adapaters))
			{
				adapaters.TryRemove(commandAdapter.ID, out commandAdapter);
			}
		}
		#endregion
		#endregion

	}


}
