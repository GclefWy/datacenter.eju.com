using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Collections.Specialized;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法分析器，可以生成SQL执行器执行查询
	/// </summary>
	public interface IQueryExec
	{
		/// <summary>
		/// 结束构造，生成无需任何参数，直接执行的SQL执行器
		/// </summary>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC();

		/// <summary>
		/// 结束构造，生成以值列表作为参数的SQL执行器
		/// </summary>
		/// <param name="parameterValues">参数值列表</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC(params object[] parameterValues);

		/// <summary>
		/// 结束构造，生成以参数列表作为参数的SQL执行器
		/// </summary>
		/// <param name="parameters">参数列表</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC(params SqlParameter[] parameters);

		/// <summary>
		/// 结束构造，生成以实体作为参数的SQL执行器
		/// </summary>
		/// <typeparam name="ParameterEntity">实体类型</typeparam>
		/// <param name="entity">实体</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC<ParameterEntity>(ParameterEntity entity);

		/// <summary>
		/// 结束构造，生成以实例作为参数的SQL执行器
		/// </summary>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXECByObject(object instance);

		/// <summary>
		/// 结束构造，生成以数据行作为参数的SQL执行器
		/// </summary>
		/// <param name="row">数据行</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC(DataRow row);

		/// <summary>
		/// 结束构造，生成以字典作为参数的SQL执行器
		/// </summary>
		/// <param name="dictionary">字典</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC(IDictionary dictionary);

		/// <summary>
		/// 结束构造，生成以集合作为参数的SQL执行器
		/// </summary>
		/// <param name="collection">集合</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		SqlExecuter EXEC(NameValueCollection collection);
	}
}
