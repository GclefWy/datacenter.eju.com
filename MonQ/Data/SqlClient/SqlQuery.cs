using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.ObjectModel;
using MonQ.Properties;
using System.Reflection;
using MonQ.Data.SqlClient.Expressions;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 数据库LINQ查询器
	/// </summary>
	/// <typeparam name="EntityType">
	/// 查询器的查询语句可以封装出的实体类型
	/// </typeparam>
	public class SqlQuery<EntityType> : IQueryable<EntityType>, IOrderedQueryable<EntityType>, ISqlLinker
	{

		private string table;

		private QueryProvider provider;

		internal SqlHelper helper;

		internal SqlQuery(SqlHelper helper, string table)
		{
			if (table.IndexOf("SELECT ", StringComparison.OrdinalIgnoreCase) == 0) this.table = "(" + table + ")";
			else if (table.IndexOf(".") > 0) this.table = "[" + table.Replace(".", "].[").Replace("]].[[", "].[") + "]";
			else this.table = "[" + table + "]";
			this.provider = new QueryProvider(helper, table, Expression.Constant(this));
		}

		internal SqlQuery(SqlHelper helper, string table, Expression expression)
		{
			this.helper = helper;
			this.provider = new QueryProvider(helper, table, expression);
		}

		#region 公开方法

		public static SqlQuery<EntityType> Create(string connectionString, string table = null)
		{
			return new SqlQuery<EntityType>(SqlHelper.Create(connectionString), table ?? SqlMapper.GetEntityMap<EntityType>());
		}
		#endregion

		#region IQueryable相关协议实现

		/// <summary>
		/// 获取此实例的枚举器
		/// </summary>
		/// <returns>
		/// 枚举器
		/// </returns>
		public IEnumerator<EntityType> GetEnumerator()
		{
			return provider.GetEnumerator();
		}

		/// <summary>
		/// 获取此实例的枚举器
		/// </summary>
		/// <returns>
		/// 枚举器
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return provider.GetEnumerator();
		}

		/// <summary>
		/// 查询类型
		/// </summary>
		public Type ElementType
		{
			get
			{
				return typeof(EntityType);
			}
		}

		/// <summary>
		/// 查询表达式
		/// </summary>
		public Expression Expression
		{
			get
			{
				return this.provider.expression;
			}
		}

		/// <summary>
		/// 用于负责提供查询功能的查询供应器
		/// </summary>
		public IQueryProvider Provider
		{
			get
			{
				return this.provider;
			}
		}

		#region ISqlLinker Members
		/// <summary>
		/// ISqlLinker实现，用于定义要查询的表
		/// </summary>
		public string TableName
		{
			get
			{
				return this.table;
			}
		}

		#endregion

		/// <summary>
		/// LINKER对象所需的查询供应器，这个类完成真正的Lambda表达式分析并进行最终的查询
		/// </summary>
		internal class QueryProvider : IQueryProvider, IEnumerable<EntityType>
		{
			internal Expression expression;
			private SqlHelper helper;
			private string table;
			private SqlExpressionTranslator translator = null;

			internal QueryProvider(SqlHelper helper, string tableName)
			{
				this.helper = helper;
				this.table = tableName;
			}
			internal QueryProvider(SqlHelper helper, string tableName, Expression expression)
				: this(helper, tableName)
			{
				this.expression = expression;
			}

			#region IQueryProvider Members

			public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
			{
				if (typeof(TElement) == typeof(EntityType)) return new SqlQuery<TElement>(this.helper, this.table, expression);
				return new SqlQuery<TElement>(this.helper, SqlMapper.GetEntityMap<TElement>(), expression);
			}

			public IQueryable CreateQuery(Expression expression)
			{
				return new SqlQuery<EntityType>(this.helper, this.table, expression);
			}

			public TResult Execute<TResult>(Expression expression)
			{
				translator = new SqlExpressionTranslator();
				translator.Translate(expression);
				return this.helper.FromParameters(translator.CommandText, translator.Parameters).ExecuteEntity<TResult>();
			}

			public object Execute(Expression expression)
			{
				return this.Execute<EntityType>(expression);
			}

			#endregion

			#region IEnumerable<EntityType> Members

			public IEnumerator<EntityType> GetEnumerator()
			{
				return this.Execute().GetEnumerator();
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				return this.Execute().GetEnumerator();
			}

			#endregion

			internal List<EntityType> Execute()
			{
				translator = new SqlExpressionTranslator();
				translator.Translate(expression);
				return this.helper.FromParameters(translator.CommandText, translator.Parameters).ExecuteEntityList<EntityType>();
			}
		}
		#endregion
	}
}
