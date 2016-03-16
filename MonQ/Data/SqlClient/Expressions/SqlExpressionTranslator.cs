using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Data.SqlClient;
using System.Reflection;
using MonQ.Properties;
using System.Collections.Concurrent;
using System.Data;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace MonQ.Data.SqlClient.Expressions
{
	/// <summary>
	/// LINQ表达式翻译器
	/// </summary>
	/// <remarks>
	/// 这个类负责将LINQ语句翻译为SQL语句以便于提交服务器进行查询
	/// </remarks>
	internal sealed class SqlExpressionTranslator : SqlExpressionVisitor
	{
		private const string CONST_SELECT_ALL = CONST_SELECT + " * ";
		private const string CONST_FROM = " FROM ";

		public string CommandText { get; private set; }



		private StringBuilder select = null;
		private StringBuilder groupby = null;
		private StringBuilder orderby = null;
		private StringBuilder where = null;
		private StringBuilder join = null;

		private string tableName;
		private string tableShortName;


		private const string CONST_SELECT = "SELECT ";
		private const string CONST_GROUP_BY = "GROUP BY ";
		private const string CONST_ORDER_BY = "ORDER BY ";
		private const string CONST_DESC = "DESC";
		private const string CONST_WHERE = "WHERE ";
		private const string CONST_AND_NEW_LINE = @"
	AND ";
		private const string CONST_JOIN = "INNER JOIN ";
		private const string CONST_ON = " ON ";
		private const string CONST_EQUAL = " = ";

		/// <summary>
		/// 将Lambda表达式翻译为SQL语句
		/// </summary>
		/// <param name="expression">Lambda表达式</param>
		internal void Translate(Expression expression)
		{
			if (expression == null) throw new ArgumentNullException("expression");
			this.Visit(expression);
			string[] buffer = new string[shortNames.Count];
			shortNames.CopyTo(buffer);
			for (int i = 0, count = buffer.Length; i < count; i++)
			{
				if (!tableShortNames.Contains(buffer[i]))
				{
					tableShortName = buffer[i];
					break;
				}
			}

			StringBuilder sb = new StringBuilder();
			if (select != null)
			{
				sb.Append(select.ToString());
			}
			else
			{
				sb.Append(CONST_SELECT_ALL);
			}
			sb.Append(CONST_FROM);
			sb.Append(tableName);
			if (!string.IsNullOrEmpty(tableShortName))
			{
				sb.Append(CONST_SPACE);
				sb.Append(CONST_SQUARE_BRACKET_LEFT);
				sb.Append(tableShortName);
				sb.Append(CONST_SQUARE_BRACKET_RIGHT);
			}
			if (join != null)
			{
				sb.AppendLine();
				sb.Append(join.ToString());
			}
			if (where != null)
			{
				sb.AppendLine();
				sb.Append(where.ToString());
			}
			if (groupby != null)
			{
				sb.AppendLine();
				sb.Append(groupby.ToString());
			}
			if (orderby != null)
			{
				sb.AppendLine();
				sb.Append(orderby.ToString());
			}
			this.CommandText = sb.ToString();
		}

		#region 重载基类方法
		internal override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (!string.IsNullOrEmpty(this.CommandText))
			{
				return base.VisitMethodCall(m);
			}
			MethodInfo method = m.Method;
			if (method.DeclaringType == typeof(Queryable))
			{
				switch (method.Name)
				{
					case "Select":
						if (this.select == null) this.select = new StringBuilder(CONST_SELECT);
						this.buffer = this.select;
						this.Visit(m.Arguments[1]);
						if (this.buffer.Length == CONST_SELECT.Length)
						{
							this.buffer.Append(CONST_SELECT_ALL);
						}
						break;
					case "GroupBy":
						if (this.groupby == null) this.groupby = new StringBuilder(CONST_GROUP_BY);
						this.buffer = this.groupby;
						this.Visit(m.Arguments[1]);
						break;
					case "OrderBy":
						if (this.orderby == null) this.orderby = new StringBuilder(CONST_ORDER_BY);
						this.buffer = this.orderby;
						this.Visit(m.Arguments[1]);
						break;
					case "OrderByDescending":
						if (this.orderby == null) this.orderby = new StringBuilder(CONST_ORDER_BY);
						this.buffer = this.orderby;
						this.Visit(m.Arguments[1]);
						this.buffer.Append(CONST_SPACE);
						this.buffer.Append(CONST_DESC);
						break;
					case "Where":
						if (this.where == null) this.where = new StringBuilder(CONST_WHERE);
						else this.where.Append(CONST_AND);
						this.buffer = this.where;
						this.Visit(m.Arguments[1]);
						break;
					case "Join":
						Expression tableB = m.Arguments[1];
						if (tableB.NodeType != ExpressionType.Constant)
						{
							throw new ArgumentException("");
						}
						ISqlLinker linkerB = (tableB as ConstantExpression).Value as ISqlLinker;
						if (linkerB == null)
						{
							throw new NotSupportedException(Resources.SqlClient_QuerySourceNotSupported);
						}
						if (this.join == null) this.join = new StringBuilder();
						else this.join.AppendLine();
						this.buffer = this.join;
						buffer.Length = 0;
						buffer.Append(CONST_JOIN);
						buffer.Append(linkerB.TableName);
						buffer.Append(CONST_SPACE);
						LambdaExpression real = (m.Arguments[3] as UnaryExpression).Operand as LambdaExpression;
						string shortName = real.Parameters[0].Name;
						AddTableShortName(shortName);

						buffer.Append(CONST_AS);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_SQUARE_BRACKET_LEFT);
						buffer.Append(shortName);
						buffer.Append(CONST_SQUARE_BRACKET_RIGHT);

						buffer.Append(CONST_ON);

						this.Visit(m.Arguments[2]);

						buffer.Append(CONST_EQUAL);

						this.Visit(m.Arguments[3]);
						break;
					default:
						throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMethodNotSupported, method.Name));
				}
				this.Visit(m.Arguments[0]);
				return m;
			}
			return base.VisitMethodCall(m);
		}

		internal override Expression VisitConstant(ConstantExpression c)
		{
			if (!string.IsNullOrEmpty(this.CommandText))
			{
				this.CreateParameter(c.Value);
				return c;
			}
			IQueryable q = c.Value as IQueryable;
			if (q != null)
			{
				if (q is ISqlLinker)
				{
					this.tableName = (q as ISqlLinker).TableName;
				}
				else
				{
					this.tableName = q.ElementType.Name;
				}
				return c;
			}
			return base.VisitConstant(c);
		}

		internal override NewExpression VisitNew(NewExpression nex)
		{
			if (Object.ReferenceEquals(buffer, select))
			{
				VisitSelectExpressionList(nex.Arguments, nex.Constructor);
				return nex;
			}

			IEnumerable<Expression> arguments = this.VisitExpressionList(nex.Arguments);
			if (arguments == nex.Arguments)
			{
				try
				{
					this.InternalVisitConstant(GetLambdaHandler(nex)());
					return nex;
				}
				catch(Exception e)
				{
					throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionTypeNotSupported, nex.Type.FullName), e);
				}
			}
			if (nex.Members != null)
			{
				return Expression.New(nex.Constructor, arguments, nex.Members);
			}
			return base.VisitNew(nex);
		}
		#endregion

		#region 内部功能
		internal void VisitSelectExpressionList(ReadOnlyCollection<Expression> original, ConstructorInfo constructor)
		{
			ParameterInfo[] parameters = constructor.GetParameters();
			int count = original.Count;
			if (count > 0)
			{
				this.Visit(original[0]);
				buffer.Append(CONST_SPACE);
				buffer.Append(CONST_AS);
				buffer.Append(CONST_SPACE);
				buffer.Append(CONST_SQUARE_BRACKET_LEFT);
				buffer.Append(parameters[0].Name);
				buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
				int num = 1;
				while (num < count)
				{
					buffer.Append(CONST_COMMA);
					this.Visit(original[num]);
					buffer.Append(CONST_SPACE);
					buffer.Append(CONST_AS);
					buffer.Append(CONST_SPACE);
					buffer.Append(CONST_SQUARE_BRACKET_LEFT);
					buffer.Append(parameters[num].Name);
					buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
					num++;
				}
			}
		}
		#endregion
	}
}
