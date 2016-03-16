using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Data;

namespace MonQ.Data.SqlClient.SqlStatements
{
	/// <summary>
	/// SQL语法对象
	/// </summary>
	internal class SelectStatement : StatementBase, IQuerySelect, IQuerySelectTop, IQuerySelectColumns, IQuerySelectFrom, IQuerySelectWhere, IQuerySelectGroupBy, IQuerySelectHaving, IQuerySelectOrderBy, IQuerySelectUnion, IQuerySelectJoin, IQueryExec
	{
		private List<String> statementList = new List<string>();

		private const string CONST_SELECT = "SELECT";
		private const string CONST_INNER_JOIN = "INNER JOIN";
		private const string CONST_LEFT_JOIN = "LEFT JOIN";
		private const string CONST_RIGHT_JOIN = "RIGHT JOIN";
		private const string CONST_CROSS_JOIN = "CROSS JOIN";
		private const string CONST_GROUP_BY = "GROUP BY";
		private const string CONST_ORDER_BY = "ORDER BY";
		private const string CONST_HAVING = "HAVING";
		private const string CONST_ON = "ON";
		private const string CONST_UNION_START = "\r\nUNION(\r\n";
		private const string CONST_UNION_END = "\r\n)";

		internal SelectStatement(SqlHelper helper)
			: base(helper)
		{
			this.buffer.Append(CONST_SELECT);
		}

		/// <summary>
		/// 设定只获取前面的指定的行数
		/// </summary>
		/// <param name="n">行数</param>
		public IQuerySelectTop TOP(int n)
		{
			base.BASE_TOP(n);
			return this;
		}

		/// <summary>
		/// 设定只获取前面的按照百分比的指定的行数
		/// </summary>
		/// <param name="percent">行数百分比，0-100之间</param>
		public IQuerySelectTop TOP_PERCENT(int percent)
		{
			base.BASE_TOP_PERCENT(percent);
			return this;
		}

		#region QueryTop Members

		/// <summary>
		/// 设定选择全部列(SELECT *)
		/// </summary>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectColumns ALL
		{
			get
			{
				this.Append("*");
				return this;
			}
		}

		/// <summary>
		/// 设定需要选取的列
		/// </summary>
		/// <param name="columns">列名称列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectColumns COLUMNS(params string[] columns)
		{
			if (columns.Length == 0) throw new ArgumentException("columns");
			this.Append(columns);
			return this;
		}

		#endregion

		#region IQuerySelect Members

		/// <summary>
		/// 设定要从哪些表查询数据
		/// </summary>
		/// <param name="tables">表集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectFrom FROM(params string[] tables)
		{
			base.BASE_FROM(tables);
			return this;
		}

		#endregion

		#region IQueryFrom Members
		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为AND关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectWhere WHERE_AND(params string[] conditions)
		{
			base.BASE_WHERE_AND(conditions);
			return this;
		}

		/// <summary>
		/// 构建一个查询条件
		/// </summary>
		/// <param name="condition">条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectWhere WHERE(string condition)
		{
			base.BASE_WHERE_AND(condition);
			return this;
		}

		/// <summary>
		/// 构建一组查询条件，各个条件之间的关系为OR关系
		/// </summary>
		/// <param name="conditions">条件集合</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectWhere WHERE_OR(params string[] conditions)
		{
			base.BASE_WHERE_OR(conditions);
			return this;
		}

		/// <summary>
		/// 构建内连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectJoin INNERJOIN(string table)
		{
			if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
			this.Append(CONST_INNER_JOIN);
			this.Append(table);
			return this;
		}

		/// <summary>
		/// 构建左连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectJoin LEFTJOIN(string table)
		{
			if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
			this.Append(CONST_LEFT_JOIN);
			this.Append(table);
			return this;
		}

		/// <summary>
		/// 构建右连接连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectJoin RIGHTJOIN(string table)
		{
			if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
			this.Append(CONST_RIGHT_JOIN);
			this.Append(table);
			return this;
		}

		/// <summary>
		/// 构建交叉连接
		/// </summary>
		/// <param name="table">表名</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectJoin CROSSJOIN(string table)
		{
			if (string.IsNullOrEmpty(table)) throw new ArgumentNullException("table");
			this.Append(CONST_CROSS_JOIN);
			this.Append(table);
			return this;
		}

		/// <summary>
		/// 闭合当前查询并且准备联合一个新的查询
		/// </summary>
		public IQuerySelectUnion UNION
		{
			get
			{
				this.Enclose();
				return this;
			}
		}

		#endregion

		#region IQueryUnion Members
		/// <summary>
		/// 设定开始选择
		/// </summary>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelect SELECT
		{
			get
			{
				return this;
			}
		}
		#endregion

		#region IQueryWhere Members

		/// <summary>
		/// 设定分组条件
		/// </summary>
		/// <param name="groupby">聚集列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectGroupBy GROUPBY(params string[] groupby)
		{
			if (groupby.Length == 0) throw new ArgumentException("groupby");
			this.Append(CONST_GROUP_BY);
			this.Append(groupby);
			return this;
		}

		/// <summary>
		/// 设定排序条件
		/// </summary>
		/// <param name="orderby">排序列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectOrderBy ORDERBY(params string[] orderby)
		{
			if (orderby.Length == 0) throw new ArgumentException("orderby");
			this.Append(CONST_ORDER_BY);
			this.Append(orderby);
			return this;
		}

		#endregion

		#region IQueryGroupBy Members

		/// <summary>
		/// 设定聚集条件
		/// </summary>
		/// <param name="having">聚集条件列表</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectHaving HAVING(params string[] having)
		{
			if (having.Length == 0) throw new ArgumentException("having");
			this.Append(CONST_HAVING);
			this.AppendBySpilter(CONST_AND, having);
			return this;
		}

		#endregion

		#region IQueryJoin Members

		/// <summary>
		/// 设定连接条件
		/// </summary>
		/// <param name="on">连接条件</param>
		/// <returns>
		/// 返回下一步操作的语法分析器
		/// </returns>
		public IQuerySelectFrom ON(string on)
		{
			if (string.IsNullOrEmpty(on)) throw new ArgumentNullException("on");
			this.Append(CONST_ON);
			this.Append(on);
			return this;
		}

		#endregion


		/// <summary>
		/// 获得当前的查询语句
		/// </summary>
		/// <returns>
		/// 查询语句
		/// </returns>
		public override string ToString()
		{
			switch (this.statementList.Count)
			{
				case 0: return null;
				case 1: return this.statementList[0];
				default:
					StringBuilder result = new StringBuilder(this.statementList[0]);
					for (int i = 1, count = this.statementList.Count; i < count; i++)
					{
						result.Append(CONST_UNION_START);
						result.Append(this.statementList[i]);
						result.Append(CONST_UNION_END);
					}
					return result.ToString();
			}
		}

		//闭合当前查询
		protected override void Enclose()
		{
			if (this.buffer.Length > 0) this.statementList.Add(this.buffer.ToString());
			this.buffer.Length = 0;
		}
	}
}
