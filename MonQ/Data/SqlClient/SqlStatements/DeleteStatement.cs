using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient.SqlStatements
{
	internal class DeleteStatement : StatementBase, IQueryDelete, IQueryDeleteTop, IQueryDeleteFrom, IQueryDeleteWhere
	{
		private const string CONST_DELETE = "DELETE";

		internal DeleteStatement(SqlHelper helper)
			: base(helper)
		{
			this.buffer.Append(CONST_DELETE);
		}

		/// <summary>
		/// 构建一个查询条件
		/// </summary>
		/// <param name="condition">条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteWhere WHERE(string condition)
		{
			base.BASE_WHERE_AND(condition);
			return this;
		}

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteWhere WHERE_AND(params string[] conditions)
		{
			base.BASE_WHERE_AND(conditions);
			return this;
		}

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteWhere WHERE_OR(params string[] conditions)
		{
			base.BASE_WHERE_OR(conditions);
			return this;
		}

		/// <summary>
		/// 设定要删除哪张表的数据
		/// </summary>
		/// <param name="table">表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteFrom FROM(string table)
		{
			base.BASE_FROM(table);
			return this;
		}

		/// <summary>
		/// 设定只删除前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteTop TOP(int n)
		{
			base.BASE_TOP(n);
			return this;
		}

		/// <summary>
		/// 设定只删除前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQueryDeleteTop TOP_PERCENT(int percent)
		{
			base.BASE_TOP_PERCENT(percent);
			return this;
		}
	}
}
