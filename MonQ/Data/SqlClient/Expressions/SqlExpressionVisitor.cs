using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using MonQ.Properties;
using System.Data;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Data.SqlClient;

namespace MonQ.Data.SqlClient.Expressions
{
	internal class SqlExpressionVisitor : ExpressionVisitor
	{
		public SqlParameter[] Parameters
		{
			get
			{
				return this.parameters.ToArray();
			}
		}

		protected delegate object LambdaHandler();

		private const string CONST_REDUCE_1 = "-1";
		private const string CONST_ADD_1 = "+1";
		private const string CONST_MULTIPLE_NEG_1 = "*(-1)";

		protected internal StringBuilder buffer = new StringBuilder();
		protected HashSet<string> shortNames = new HashSet<string>();
		protected HashSet<string> tableShortNames = new HashSet<string>();

		private static ConcurrentDictionary<string, Func<string[], string>> formatHandlers = new ConcurrentDictionary<string, Func<string[], string>>();
		private static ConcurrentDictionary<string, LambdaHandler> lambdaHandlers = new ConcurrentDictionary<string, LambdaHandler>();

		private List<SqlParameter> parameters = new List<SqlParameter>();

		internal SqlExpressionVisitor()
		{
		}

		#region 基类方法重载
		/// <summary>
		/// 访问成员
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		internal override Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Expression != null)
			{
				if (m.Expression is MemberExpression)
				{
					MemberExpression expression = m.Expression as MemberExpression;
					if (expression.Expression != null && expression.Expression.NodeType == ExpressionType.Parameter)
					{
						buffer.Append(CONST_SQUARE_BRACKET_LEFT);
						string shortName = expression.Member.Name;
						AddShortName(shortName);
						buffer.Append(shortName);
						buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
						buffer.Append(CONST_DOT);
						buffer.Append(CONST_SQUARE_BRACKET_LEFT);
						buffer.Append(SqlMapper.GetMemberMap(m.Member.DeclaringType, m.Member));
						buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
						return m;
					}
				}
				else if (m.Expression is ParameterExpression)
				{
					ParameterExpression expression = m.Expression as ParameterExpression;
					string shortName = expression.Name;
					if (shortName.IndexOf('<') == -1)
					{
						buffer.Append(CONST_SQUARE_BRACKET_LEFT);
						AddShortName(shortName);
						this.buffer.Append(shortName);
						buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
						buffer.Append(CONST_DOT);
						buffer.Append(CONST_SQUARE_BRACKET_LEFT);
						buffer.Append(SqlMapper.GetMemberMap(m.Member.DeclaringType, m.Member));
						buffer.Append(CONST_SQUARE_BRACKET_RIGHT);
					}
					return m;
				}
			}
			Type declaringType = m.Member.DeclaringType;
			if (declaringType == typeof(DateTime))
			{
				if (VisitDateTimeMember(m)) return m;
			}
			else if (declaringType == typeof(string))
			{
				if (VisitStringMember(m.Expression, m.Member)) return m;
			}
			try
			{
				this.InternalVisitConstant(GetLambdaHandler(m));
				return m;
			}
			catch (Exception e)
			{
				throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMemberNotSupported, m.Member.Name), e);
			}
		}

		/// <summary>
		/// 访问常量
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		internal override Expression VisitConstant(ConstantExpression c)
		{
			this.InternalVisitConstant(c.Value);
			return c;
		}

		/// <summary>
		/// 访问方法
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		internal override Expression VisitMethodCall(MethodCallExpression m)
		{
			MethodInfo method = m.Method;
			Type declaringType = method.DeclaringType;

			if (declaringType == typeof(Enumerable))//Enumerable方法
			{
				if (VisitAggregateMethod(m, method)) return m;
			}
			else if (declaringType == typeof(TSQL))//TSQL方法
			{
				VisitTSQLMethod(m, method);
				return m;
			}
			else if (declaringType == typeof(string))//String方法
			{
				if (VisitStringMethod(m, method)) return m;
			}
			else if (declaringType == typeof(DateTime))//String方法
			{
				if (VisitDateTimeMethod(m, method)) return m;
			}
			try
			{
				this.InternalVisitConstant(GetLambdaHandler(m));
				return m;
			}
			catch (Exception e)
			{
				throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMethodNotSupported, method.Name), e);
			}
		}

		internal override Expression VisitUnary(UnaryExpression u)
		{
			switch (u.NodeType)
			{
				case ExpressionType.Not:
					buffer.Append(" NOT ");
					this.Visit(u.Operand);
					break;
				case ExpressionType.Convert:
					Type convertType = u.Type;
					SqlDbType convertTo;
					int size = 0;
					switch (Type.GetTypeCode(convertType))
					{
						case TypeCode.Byte:
							convertTo = SqlDbType.TinyInt;
							break;
						case TypeCode.Boolean:
							convertTo = SqlDbType.Int;
							break;
						case TypeCode.DateTime:
							convertTo = SqlDbType.DateTime;
							break;
						case TypeCode.Decimal:
							convertTo = SqlDbType.Decimal;
							break;
						case TypeCode.Double:
							convertTo = SqlDbType.Decimal;
							break;
						case TypeCode.Int16:
						case TypeCode.UInt16:
							convertTo = SqlDbType.SmallInt;
							break;
						case TypeCode.Int32:
						case TypeCode.UInt32:
							convertTo = SqlDbType.Int;
							break;
						case TypeCode.Int64:
						case TypeCode.UInt64:
							convertTo = SqlDbType.BigInt;
							break;
						case TypeCode.SByte:
							convertTo = SqlDbType.TinyInt;
							break;
						case TypeCode.Single:
							convertTo = SqlDbType.Float;
							break;
						case TypeCode.String:
						case TypeCode.Char:
							convertTo = SqlDbType.VarChar;
							size = 8000;
							break;
						default:
							if (convertType == typeof(byte[]))
							{
								convertTo = SqlDbType.VarBinary;
								size = 8000;
							}
							if (convertType == typeof(Guid))
							{
								convertTo = SqlDbType.UniqueIdentifier;
								size = 8000;
							}
							else
								throw new NotSupportedException(string.Format(Resources.SqlClient_ConvertTypeNotSupported, convertType.FullName));
							break;
					}
					buffer.Append(CONST_CAST);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(u.Operand);
					buffer.Append(CONST_SPACE);
					buffer.Append(CONST_AS);
					buffer.Append(CONST_SPACE);
					buffer.Append(convertTo.ToString().ToUpper());
					if (size > 0)
					{
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(size);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					}
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					break;
				default:
					this.Visit(u.Operand);
					break;
			}
			return u;
		}

		internal override Expression VisitBinary(BinaryExpression b)
		{
			buffer.Append("(");

			this.Visit(b.Left);
			switch (b.NodeType)
			{
				case ExpressionType.AndAlso:
					buffer.Append(" AND ");
					break;
				case ExpressionType.OrElse:
					buffer.Append(" OR ");
					break;
				case ExpressionType.Equal:
					//任何一个值与NULL进行比较都需要进行特别翻译
					if (b.Left is ConstantExpression && ((b.Left as ConstantExpression).Value == null || (b.Left as ConstantExpression).Value == DBNull.Value))
					{
						buffer.Append(" IS ");
					}
					else if (b.Right is ConstantExpression && ((b.Right as ConstantExpression).Value == null || (b.Right as ConstantExpression).Value == DBNull.Value))
					{
						buffer.Append(" IS ");
					}
					else
					{
						buffer.Append(" = ");
					}
					break;
				case ExpressionType.NotEqual:
					//任何一个值与NULL进行比较都需要进行特别翻译
					if (b.Left is ConstantExpression && ((b.Left as ConstantExpression).Value == null || (b.Left as ConstantExpression).Value == DBNull.Value))
					{
						buffer.Append(" IS NOT ");
					}
					else if (b.Right is ConstantExpression && ((b.Right as ConstantExpression).Value == null || (b.Right as ConstantExpression).Value == DBNull.Value))
					{
						buffer.Append(" IS NOT ");
					}
					else
					{
						buffer.Append(" <> ");
					}
					break;
				case ExpressionType.LessThan:
					buffer.Append(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					buffer.Append(" <= ");
					break;
				case ExpressionType.GreaterThan:
					buffer.Append(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					buffer.Append(" >= ");
					break;
				case ExpressionType.Modulo:
					buffer.Append(" % ");
					break;
				case ExpressionType.And:
					buffer.Append(" & ");
					break;
				case ExpressionType.Or:
					buffer.Append(" | ");
					break;
				case ExpressionType.OnesComplement:
					buffer.Append(" ~ ");
					break;
				case ExpressionType.ExclusiveOr:
					buffer.Append(" ^ ");
					break;
				case ExpressionType.RightShift:
					buffer.Append(" >> ");
					break;
				case ExpressionType.LeftShift:
					buffer.Append(" << ");
					break;
				default:
					throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", b.NodeType));
			}
			this.Visit(b.Right);
			buffer.Append(")");
			return b;
		}

		internal override ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
		{
			return base.VisitExpressionList(original);
		}
		#endregion

		#region VisitConstant
		protected void InternalVisitConstant(object value)
		{
			if (value is SqlDbType)
			{
				buffer.Append(((SqlDbType)value).ToString().ToUpper());
			}
			else
			{
				string parameterName = CreateParameter(value);
				if (string.IsNullOrEmpty(parameterName))
				{
					if (value == null || value == DBNull.Value) buffer.Append(CONST_NULL);
					else throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionTypeNotSupported, value.GetType().FullName));
				}
				else
				{
					buffer.Append(parameterName);
				}
			}
		}
		#endregion

		#region String方法处理
		private bool VisitStringMethod(MethodCallExpression m, MethodInfo method)
		{
			string methodName = null;
			ReadOnlyCollection<Expression> arguments;
			switch (method.Name)
			{
				case "Contains"://Contains
					buffer.Append(CONST_CHARINDEX);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					arguments = m.Arguments;
					this.Visit(arguments[0]);
					buffer.Append(CONST_COMMA);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_LARGER_THAN);
					buffer.Append('0');
					return true;
				case "IndexOf"://IndexOf
					buffer.Append(CONST_CHARINDEX);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					arguments = m.Arguments;
					this.Visit(arguments[0]);
					buffer.Append(CONST_COMMA);
					this.Visit(m.Object);
					if (arguments.Count == 2)
					{
						buffer.Append(CONST_COMMA);
						this.Visit(arguments[1]);
						buffer.Append(CONST_ADD_1);
					}
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_REDUCE_1);
					return true;
				case "ToLower"://ToLower
					methodName = CONST_LOWER;
					break;
				case "ToUpper"://ToUpper
					methodName = CONST_UPPER;
					break;
				case "Trim"://Trim
					buffer.Append(CONST_LTRIM);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					buffer.Append(CONST_RTRIM);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				case "TrimEnd"://TrimEnd
					buffer.Append(CONST_RTRIM);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				case "TrimStart"://TrimStart
					buffer.Append(CONST_LTRIM);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				case "StartsWith"://StartsWith
					buffer.Append(CONST_CHARINDEX);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_EQUAL_ZERO);
					return true;
				case "Replace"://Replace
					methodName = CONST_REPLACE;
					break;
				case "Substring"://SubString
					buffer.Append(CONST_SUBSTRING);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					int len = buffer.Length;
					this.Visit(m.Object);
					int len2 = buffer.Length;
					buffer.Append(CONST_COMMA);
					arguments = m.Arguments;
					int len3 = buffer.Length;
					this.Visit(arguments[0]);
					int len4 = buffer.Length;
					buffer.Append(CONST_COMMA);
					if (arguments.Count == 2)
					{
						this.Visit(arguments[1]);
					}
					else
					{
						buffer.Append(CONST_LEN);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(buffer.ToString(len, len2 - len));
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_REDUCE);
						buffer.Append(buffer.ToString(len3, len4 - len3));
					}
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				default:
					return false;
			}
			buffer.Append(methodName);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			this.Visit(m.Object);
			buffer.Append(CONST_COMMA);
			arguments = m.Arguments;
			this.Visit(arguments[0]);
			for (int i = 1, count = arguments.Count; i < count; i++)
			{
				buffer.Append(CONST_COMMA);
				this.Visit(arguments[i]);
			}
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
			return true;
		}
		private bool VisitStringMember(Expression expression, MemberInfo member)
		{
			switch (member.Name)
			{
				case "Length":
					buffer.Append(CONST_LEN);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(expression);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				case "Empty":
					buffer.Append(CONST_QUOTE);
					buffer.Append(CONST_QUOTE);
					return true;
				default:
					return false;
			}
		}
		#endregion

		#region 聚合函数处理
		private bool VisitAggregateMethod(MethodCallExpression m, MethodInfo method)
		{
			switch (method.Name.ToUpper())
			{
				case CONST_SUM:
					VisitAggregateMethodCall(m, CONST_SUM);
					break;
				case CONST_MIN:
					VisitAggregateMethodCall(m, CONST_MIN);
					break;
				case CONST_MAX:
					VisitAggregateMethodCall(m, CONST_MAX);
					break;
				case CONST_AVERAGE:
					VisitAggregateMethodCall(m, CONST_AVERAGE);
					break;
				case CONST_COUNT:
					VisitCOUNTMethodCall(m);
					break;
				case CONST_DISTINCT:
					VisitDISTINCTMethodCall(m);
					break;
				default:
					return false;
			}
			return true;
		}
		private void VisitAggregateMethodCall(MethodCallExpression m, string methodName)
		{
			buffer.Append(methodName);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			if (m.Arguments.Count != 2) throw new ArgumentException(string.Format(Resources.SqlClient_ExpressionArgumentLengthError, m.Method.Name));
			this.Visit(m.Arguments[1]);
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
		}
		private void VisitCOUNTMethodCall(MethodCallExpression m)
		{
			buffer.Append(CONST_COUNT);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			if (m.Arguments.Count < 2)
			{
				buffer.Append('1');
			}
			else
			{
				this.Visit(m.Arguments[1]);
			}
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
		}
		private void VisitDISTINCTMethodCall(MethodCallExpression m)
		{
			buffer.Append(CONST_DISTINCT);
			buffer.Append(CONST_SPACE);
			if (m.Arguments.Count != 2) throw new ArgumentException(string.Format(Resources.SqlClient_ExpressionArgumentLengthError, m.Method.Name));
			this.Visit(m.Arguments[1]);
		}
		#endregion

		#region TSQL函数处理
		private void VisitTSQLMethod(MethodCallExpression m, MethodInfo method)
		{
			Func<string[], string> formatHandler = formatHandlers.GetOrAdd(method.Name, (key) =>
			{
				TSQL.TSQLFormattor[] formats = method.GetCustomAttributes(typeof(TSQL.TSQLFormattor), false) as TSQL.TSQLFormattor[];
				if (formats.Length > 0)
				{
					return formats[0].FormatHandler;
				}
				return TSQL.TSQLFormattor.CreateDynamicParameterMethodHandler(method.Name);
			});
			StringBuilder buffer = this.buffer;
			this.buffer = new StringBuilder();
			string[] parameters = new string[m.Arguments.Count];
			for (int i = 0, count = parameters.Length; i < count; i++)
			{
				if (m.Arguments[i].Type == typeof(TSQL.OVER_CLAUSE))
				{
					//SqlOverClauseExpressionVisitor visitor = new SqlOverClauseExpressionVisitor(this.root);
					//visitor.Visit(m.Arguments[i]);
					//parameters[i] = visitor.CommandText;
				}
				else
				{
					this.buffer.Length = 0;
					this.Visit(m.Arguments[i]);
					parameters[i] = this.buffer.ToString();
				}
			}
			this.buffer = buffer;
			buffer.Append(formatHandler(parameters));
		}
		#endregion

		#region DateTime方法
		private bool VisitDateTimeMember(MemberExpression m)
		{
			string part;
			switch (m.Member.Name)
			{
				case "Day":
					part = CONST_DAY;
					break;
				case "DayOfWeek":
					part = CONST_DAY_OF_WEEK;
					break;
				case "DayOfYear":
					part = CONST_DAY_OF_YEAR;
					break;
				case "Hour":
					part = CONST_HOUR;
					break;
				case "Millisecond":
					part = CONST_MILLI_SECOND;
					break;
				case "Minute":
					part = CONST_MINUTE;
					break;
				case "Month":
					part = CONST_MONTH;
					break;
				case "Now":
					buffer.Append(CONST_GETDATE);
					return true;
				case "Second":
					part = CONST_SECOND;
					break;
				case "Year":
					part = CONST_YEAR;
					break;
				case "Today":
					//CAST(CONVERT(VARCHAR(10),@P,102) AS DATETIME)
					buffer.Append(CONST_CAST);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					buffer.Append(CONST_CONVERT);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					buffer.Append(CONST_VARCHAR);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					buffer.Append(10);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_GETDATE);
					buffer.Append(CONST_COMMA);
					buffer.Append(102);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_SPACE);
					buffer.Append(CONST_AS);
					buffer.Append(CONST_SPACE);
					buffer.Append(CONST_DATETIME);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				default:
					return false;
			}
			buffer.Append(CONST_DATEPART);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			buffer.Append(part);
			buffer.Append(CONST_COMMA);
			this.Visit(m.Expression);
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
			return true;
		}
		private bool VisitDateTimeMethod(MethodCallExpression m, MethodInfo method)
		{
			string part = null;
			switch (method.Name)
			{
				case "Add"://DateAdd，SQLSERVER的DateAdd方法只能精确到毫秒，所以相对于TimeSpan的精度降低了1000倍（目前SQL Server家族中只有2008版本加入了对纳秒的支持，考虑到兼容2000，2005，还是使用ms作为算法）
					VisitDateTimeAddMethod(m, method);
					return true;
				case "Subtract"://Subtract，SQLSERVER的DateAdd方法只能精确到毫秒，所以相对于TimeSpan的精度降低了1000倍
					Expression argument = m.Arguments[0];
					if (argument == null || argument.Type != typeof(TimeSpan))
					{
						throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMethodNotSupported, method.Name));
					}
					VisitDateTimeSubstractMethod(m, method);
					return true;
				case "AddDays":
					part = CONST_DAY;
					break;
				case "AddHours":
					part = CONST_HOUR;
					break;
				case "AddMilliseconds":
					part = CONST_MILLI_SECOND;
					break;
				case "AddMinutes":
					part = CONST_MINUTE;
					break;
				case "AddMonths":
					part = CONST_MONTH;
					break;
				case "AddSeconds":
					part = CONST_SECOND;
					break;
				case "AddYears":
					part = CONST_YEAR;
					break;
				case "AddTicks":
					buffer.Append(CONST_DATEADD);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					buffer.Append(CONST_MILLI_SECOND);
					buffer.Append(CONST_COMMA);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Arguments[0]);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_DIVIDE);
					buffer.Append(10000);
					buffer.Append(CONST_COMMA);
					buffer.Append(CONST_ROUND_BRACKET_LEFT);
					this.Visit(m.Object);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					buffer.Append(CONST_ROUND_BRACKET_RIGHT);
					return true;
				default:
					return false;
			}
			buffer.Append(CONST_DATEADD);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			buffer.Append(part);
			buffer.Append(CONST_COMMA);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			this.Visit(m.Arguments[0]);
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
			buffer.Append(CONST_COMMA);
			buffer.Append(CONST_ROUND_BRACKET_LEFT);
			this.Visit(m.Object);
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
			buffer.Append(CONST_ROUND_BRACKET_RIGHT);
			return true;
		}

		private void VisitDateTimeAddMethod(MethodCallExpression m, MethodInfo method)
		{
			VisitDateTimeAddReduceMethod(m, method, CONST_DATEADD, string.Empty);
		}
		private void VisitDateTimeSubstractMethod(MethodCallExpression m, MethodInfo method)
		{
			VisitDateTimeAddReduceMethod(m, method, CONST_DATEADD, CONST_MULTIPLE_NEG_1);
		}
		private void VisitDateTimeAddReduceMethod(MethodCallExpression m, MethodInfo method, string function, string op)
		{
			Expression argument = m.Arguments[0];
			if (argument == null)
			{
				throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMethodNotSupported, method.Name));
			}
			if (argument is NewExpression)
			{
				NewExpression constructor = argument as NewExpression;
				switch (constructor.Arguments.Count)
				{
					case 0://如果无构造参数，则这个TimeSpan的参数为0
						this.Visit(m.Object);
						return;
					case 1://使用100纳秒的刻度，1刻度=0.0001毫秒 DATEDADD(ss, ts.Ticks / 10000, datetime) 
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_MILLI_SECOND);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[0]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_DIVIDE);
						buffer.Append(10000);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_CAST);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(m.Object);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_AS);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_DATETIME);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						return;
					case 3://使用小时，分，秒 DATEADD(hh, ts.Hours, DATEADD(mi, ts.Minutes, DATEDADD(ss, ts.Seconds, datetime)))
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_HOUR);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[0]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_MINUTE);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[1]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_SECOND);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[2]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_CAST);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(m.Object);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_AS);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_DATETIME);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						return;
					case 4://使用天，小时，分，秒 DATEADD(dd, ts.Days, DATEADD(hh, ts.Hours, DATEADD(mi, ts.Minutes, DATEDADD(ss, ts.Seconds, datetime))))
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_DAY);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[0]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_HOUR);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[1]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_MINUTE);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[2]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_SECOND);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[3]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_CAST);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(m.Object);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_AS);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_DATETIME);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						return;
					case 5://使用天，小时，分，秒，毫秒 DATEADD(dd, ts.Days, DATEADD(hh, ts.Hours, DATEADD(mi, ts.Minutes, DATEDADD(ss, ts.Seconds, DATEADD(ms, ts.Milliseconds, datetime)))))
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_DAY);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[0]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_YEAR);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[1]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_MINUTE);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[2]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_SECOND);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[3]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(function);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						buffer.Append(CONST_MILLI_SECOND);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(constructor.Arguments[4]);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(op);
						buffer.Append(CONST_COMMA);
						buffer.Append(CONST_CAST);
						buffer.Append(CONST_ROUND_BRACKET_LEFT);
						this.Visit(m.Object);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_AS);
						buffer.Append(CONST_SPACE);
						buffer.Append(CONST_DATETIME);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						buffer.Append(CONST_ROUND_BRACKET_RIGHT);
						return;
				}
			}
			else if (argument is ConstantExpression)
			{
				TimeSpan timeSpan = (TimeSpan)(argument as ConstantExpression).Value;
				buffer.Append(function);
				buffer.Append(CONST_ROUND_BRACKET_LEFT);
				buffer.Append(CONST_MILLI_SECOND);
				buffer.Append(CONST_COMMA);
				buffer.Append(timeSpan.TotalMilliseconds);
				buffer.Append(CONST_COMMA);
				this.Visit(m.Object);
				buffer.Append(CONST_ROUND_BRACKET_RIGHT);
				return;
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_ExpressionMethodNotSupported, method.Name));
		}
		#endregion

		#region 内部方法
		internal void AddTableShortName(string shortName)
		{
			if (!tableShortNames.Contains(shortName)) tableShortNames.Add(shortName);
		}
		internal void AddShortName(string shortName)
		{
			if (!shortNames.Contains(shortName)) shortNames.Add(shortName);
		}
		internal string CreateParameter(object value)
		{
			string parameterName = "@P" + (parameters.Count + 1);
			SqlParameter parameter;
			if (value != null && value != DBNull.Value)
			{
				switch (Type.GetTypeCode(value.GetType()))
				{
					case TypeCode.Boolean:
						parameter = new SqlParameter(parameterName, ((bool)value) ? (object)1 : (object)0);
						parameters.Add(parameter);
						return parameterName;
					case TypeCode.Object:
						if (!(value is byte[])) return null;
						parameter = new SqlParameter(parameterName, value);
						parameters.Add(parameter);
						return parameterName;
					default:
						parameter = new SqlParameter(parameterName, value);
						parameters.Add(parameter);
						return parameterName;
				}
			}
			else
			{
				return null;
			}
		}

		protected LambdaHandler GetLambdaHandler(Expression expression)
		{
			return lambdaHandlers.GetOrAdd(expression.ToString(), key =>
			{
				return Expression.Lambda<LambdaHandler>(expression).Compile();
			});
		}
		#endregion
	}
}
