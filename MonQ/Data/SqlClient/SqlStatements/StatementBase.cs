using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using System.Data;
using System.Data.SqlClient;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// 查询语句的基类
	/// </summary>
	internal abstract class StatementBase
	{
		protected const string CONST_AND = " AND ";
		protected const string CONST_OR = " AND ";
		protected const string CONST_TOP = " TOP";
		protected const string CONST_PERCENT = " PERCENT";
		protected const string CONST_FROM = "FROM";
		protected const string CONST_WHERE = "WHERE";

		/// <summary>
		/// 查询对象的实例
		/// </summary>
		protected SqlHelper helper;

		/// <summary>
		/// 查询语句构造缓存
		/// </summary>
		protected StringBuilder buffer = new StringBuilder();

		protected StatementBase(SqlHelper helper)
		{
			this.helper = helper;
		}

		#region IQueryExecuteable

		/// <summary>
		/// 结束构造，生成无需任何参数，直接执行的SQL执行器
		/// </summary>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC()
		{
			this.Enclose();
			return helper.FromText(this.ToString());
		}

		/// <summary>
		/// 结束构造，生成生成使用值列表作为参数的SQL执行器
		/// </summary>
		/// <param name="parameterValues">参数值列表</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC(params object[] parameterValues)
		{
			this.Enclose();
			return helper.FromArray(this.ToString(), parameterValues);
		}

		/// <summary>
		/// 结束构造，生成生成使用参数列表作为参数的SQL执行器
		/// </summary>
		/// <param name="parameters">参数列表</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC(params SqlParameter[] parameters)
		{
			this.Enclose();
			return helper.FromParameters(this.ToString(), parameters);
		}

		/// <summary>
		/// 结束构造，生成以实体作为参数的SQL执行器
		/// </summary>
		/// <typeparam name="ParameterEntity">实体类型</typeparam>
		/// <param name="entity">实体</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC<ParameterEntity>(ParameterEntity entity)
		{
			this.Enclose();
			return helper.FromEntity(this.ToString(), entity);
		}

		/// <summary>
		/// 结束构造，生成以实例作为参数的SQL执行器
		/// </summary>
		/// <param name="instance">实例</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXECByObject(object instance)
		{
			this.Enclose();
			return helper.FromObject(this.ToString(), instance);
		}

		/// <summary>
		/// 结束构造，生成以数据行作为参数的SQL执行器
		/// </summary>
		/// <param name="row">数据行</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC(DataRow row)
		{
			this.Enclose();
			return helper.FromRow(this.ToString(), row);
		}

		/// <summary>
		/// 结束构造，生成以字典作为参数的SQL执行器
		/// </summary>
		/// <param name="dictionary">字典</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC(IDictionary dictionary)
		{
			this.Enclose();
			return helper.FromDictionary(this.ToString(), dictionary);
		}

		/// <summary>
		/// 结束构造，生成以集合作为参数的SQL执行器
		/// </summary>
		/// <param name="collection">集合</param>
		/// <returns>
		/// 所生成的执行器
		/// </returns>
		public SqlExecuter EXEC(NameValueCollection collection)
		{
			this.Enclose();
			return helper.FromCollection(this.ToString(), collection);
		}

		#endregion



		/// <summary>
		/// 设定只操作前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		protected void BASE_TOP(int n)
		{
			if (n < 0) throw new ArgumentOutOfRangeException("n");
			buffer.Append(CONST_TOP);
			buffer.Append(n);
		}

		/// <summary>
		/// 设定只操作前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		protected void BASE_TOP_PERCENT(int percent)
		{
			if (percent < 0 || percent > 100) throw new ArgumentOutOfRangeException("n");
			buffer.Append(CONST_TOP);
			buffer.Append(percent);
			buffer.Append(CONST_PERCENT);
			buffer.Append(percent);
		}

		/// <summary>
		/// 设定要从哪些表处理数据
		/// </summary>
		/// <param name="tables">表集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		protected void BASE_FROM(params string[] tables)
		{
			if (tables.Length == 0) throw new ArgumentException("tables");
			this.Append(CONST_FROM);
			this.Append(tables);
		}
		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public void BASE_WHERE_AND(params string[] conditions)
		{
			if (conditions.Length == 0) throw new ArgumentException("conditions");
			this.Append(CONST_WHERE);
			buffer.Append(' ');
			buffer.Append('(');
			this.AppendBySpilter(CONST_AND, conditions);
			buffer.Append(' ');
			buffer.Append(')');
		}

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public void BASE_WHERE_OR(params string[] conditions)
		{
			if (conditions.Length == 0) throw new ArgumentException("conditions");
			this.Append(CONST_WHERE);
			buffer.Append(' ');
			buffer.Append('(');
			this.AppendBySpilter(CONST_OR, conditions);
			buffer.Append(' ');
			buffer.Append(')');
		}

		protected virtual void Enclose()
		{
		}


		protected void Append(params string[] values)
		{
			if (values.Length == 0) throw new ArgumentNullException("values");
			if (buffer.Length > 0) buffer.Append(' ');
			buffer.Append(values[0]);
			for (int i = 1, len = values.Length; i < len; i++)
			{
				buffer.Append(',');
				string value = values[i];
				if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("values");
				buffer.Append(value);
			}
		}

		protected void AppendBySpilter(string spilter, params string[] values)
		{
			if (buffer.Length > 0) buffer.Append(' ');
			buffer.Append(values[0]);
			for (int i = 1, len = values.Length; i < len; i++)
			{
				buffer.Append(spilter);
				buffer.Append(values[i]);
			}
		}

		protected void AppendFormat(string format, params string[] args)
		{
			if (buffer.Length > 0) buffer.Append(' ');
			buffer.AppendFormat(format, args);
		}
	}
}
