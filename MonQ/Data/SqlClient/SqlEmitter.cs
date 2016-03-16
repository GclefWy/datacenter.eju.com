using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using System.Threading.Tasks;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// SqlCommand发射器，使用SqlCommand发射器可以对SqlCommand进行更加底层的调用，其效率一般比直接使用SqlCommand进行查询要高出不少。
	/// </summary>
	/// <remarks>
	/// SqlCommand发射器的原理为：SqlCommand类是微软.Net框架所提供的一个比较全面而通用的类，随着.Net的发展，SqlServer的发展，SqlCommand被设计得越来越复杂，其功能既要满足与SqlServer进行数据交互的窗口，又要满足作为基础的可视化组件功能，还要为SqlServer服务器的CLR运行时功能提供支持，此外，SqlCommand还执行了一些SqlServer服务器会进行操作的重复检查（例如参数检查，即使SqlCommand不进行检查，SqlServer同样会进行相应的检查），SqlCommand发射器会命令SqlCommand忽略这些不必要的工作而将Sql指令直接交付SqlServer执行并且返回结果，由此大大提高了执行效率。
	/// </remarks>
	public static class SqlEmitter
	{
		/// <summary>
		/// 指示不必等待,立即返回数据访问器
		/// </summary>
		internal const int RETURN_IMMEDIATELY = 0x02;

		/// <summary>
		/// 指示查询等待直到全部完成
		/// </summary>
		internal const int UNTIL_DONE = 0x01;

		/// <summary>
		/// 当出现SqlException时触发的事件
		/// </summary>
		public static event Action<SqlCommand, SqlException> SqlException;

		/// <summary>
		/// 当出现Exception时触发的事件
		/// </summary>
		public static event Action<SqlCommand, Exception> Exception;

		#region 委托句柄

		/// <summary>
		/// ExecuteReader句柄
		/// </summary>
		private static readonly Func<SqlCommand, CommandBehavior, int, bool, SqlDataReader> executeReader;

		/// <summary>
		/// GetRowsAffected句柄
		/// </summary>
		private static readonly Func<SqlCommand, int> getRowsAffected;

		private static readonly Func<SqlParameterCollection> createParameterCollection;

		#endregion

		/// <summary>
		/// 静态构造方法
		/// </summary>
		static SqlEmitter()
		{
			Type sqlCommand = typeof(SqlCommand);
			MethodInfo runExecuteNonQueryTds = sqlCommand.GetMethod("RunExecuteNonQueryTds", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo runExecuteReaderTds = sqlCommand.GetMethod("RunExecuteReaderTds", BindingFlags.NonPublic | BindingFlags.Instance);
			DynamicMethod dynamicMethod;
			ILGenerator generator;
			LocalBuilder result;

			#region 生成GetRowsAffected委托
			/*
			 * public int GetRowsAffected(SqlCommand command)
			 * {
			 *		return command._rowsAffected;
			 * }
			 */
			dynamicMethod = new DynamicMethod("GetRowsAffected", typeof(int), new Type[] { typeof(SqlCommand) }, true);

			generator = dynamicMethod.GetILGenerator();
			result = generator.DeclareLocal(typeof(int));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, sqlCommand.GetField("_rowsAffected", BindingFlags.NonPublic | BindingFlags.Instance));
			generator.Emit(OpCodes.Stloc, result);

			generator.Emit(OpCodes.Ldloc, result);
			generator.Emit(OpCodes.Ret);

			try
			{
				getRowsAffected = (Func<SqlCommand, int>)dynamicMethod.CreateDelegate(typeof(Func<SqlCommand, int>));
			}
			catch
			{
				throw;
			}
			#endregion

			#region 生成ExecuteReader委托
			dynamicMethod = new DynamicMethod("RunExecuteReaderTds", typeof(SqlDataReader), new Type[] { typeof(SqlCommand), typeof(CommandBehavior), typeof(int), typeof(bool) }, true);

			generator = dynamicMethod.GetILGenerator();
			result = generator.DeclareLocal(typeof(SqlDataReader));
			LocalBuilder task = generator.DeclareLocal(typeof(Task));
			LocalBuilder timeout = generator.DeclareLocal(typeof(int));

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, sqlCommand.GetField("_commandTimeout", BindingFlags.NonPublic | BindingFlags.Instance));
			generator.Emit(OpCodes.Stloc, timeout);

			generator.Emit(OpCodes.Ldarg_0);//command
			generator.Emit(OpCodes.Ldarg_1);//commandBehavior
			generator.Emit(OpCodes.Ldarg_2);//runBehivor
			generator.Emit(OpCodes.Ldarg_3);//returnStream
			generator.Emit(OpCodes.Ldc_I4_0);//async

			int len = runExecuteReaderTds.GetParameters().Length;

			if (len >= 7)//如果等于7 则为.Net framework 4.0 否则为2.0
			{
				//以下几个参数为4.0所使用
				generator.Emit(OpCodes.Ldloc, timeout);//timeout
				generator.Emit(OpCodes.Ldloca, task);//out task
				generator.Emit(OpCodes.Ldc_I4_0);//asyncWrite
				if (len == 8)
				{
					generator.Emit(OpCodes.Ldnull);
				}
			}

			generator.Emit(OpCodes.Callvirt, runExecuteReaderTds);
			generator.Emit(OpCodes.Stloc, result);

			generator.Emit(OpCodes.Ldloc, result);
			generator.Emit(OpCodes.Ret);

			try
			{
				executeReader = (Func<SqlCommand, CommandBehavior, int, bool, SqlDataReader>)dynamicMethod.CreateDelegate(typeof(Func<SqlCommand, CommandBehavior, int, bool, SqlDataReader>));
			}
			catch
			{
				throw;
			}
			#endregion

			#region 生成CreateParameterCollectionHandler委托
			dynamicMethod = new DynamicMethod("CreateParameterCollection", typeof(SqlParameterCollection), Type.EmptyTypes, true);

			generator = dynamicMethod.GetILGenerator();
			generator.Emit(OpCodes.Newobj, typeof(SqlParameterCollection).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null));
			generator.Emit(OpCodes.Ret);

			try
			{
				createParameterCollection = (Func<SqlParameterCollection>)dynamicMethod.CreateDelegate(typeof(Func<SqlParameterCollection>));
			}
			catch
			{
				throw;
			}
			#endregion
		}

		/// <summary>
		/// 获取指定SqlCommand影响的行数
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <returns>
		/// 影响的行数
		/// </returns>
		public static int GetRowsAffected(SqlCommand command)
		{
			return getRowsAffected(command);
		}

		/// <summary>
		/// 命令指定的SqlCommand执行查询并且返回一个数据访问器
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="commandBehavior">数据访问器的行为</param>
		/// <returns>
		/// 执行查询所得到的数据访问器
		/// </returns>
		public static SqlDataReader ExecuteReader(SqlCommand command, CommandBehavior commandBehavior = CommandBehavior.Default)
		{
			return ExecuteReader(command, commandBehavior, SqlEmitter.RETURN_IMMEDIATELY, true);
		}

		/// <summary>
		/// 命令指定的SqlCommand执行查询并且返回一个数据访问器
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="commandBehavior">数据访问器的行为</param>
		/// <param name="runBehavior">数据访问器的执行行为</param>
		/// <param name="returnStream">是否要返回一个数据流,如果为false,返回的数据访问器为null</param>
		/// <returns>
		/// 执行查询所得到的数据访问器
		/// </returns>
		internal static SqlDataReader ExecuteReader(SqlCommand command, CommandBehavior commandBehavior, int runBehavior, bool returnStream)
		{
			try
			{
				command.CommandTimeout = 60;
				return executeReader(command, commandBehavior, runBehavior, returnStream);
			}
			catch (SqlException e)
			{
				if (Exception != null)
				{
					SqlException(command, e);
				}
				throw;
			}
			catch (Exception e)
			{
				if (Exception != null)
				{
					Exception(command, e);
				}
				throw;
			}
		}

		/// <summary>
		/// 创建一个SqlParameter集合
		/// </summary>
		/// <returns>
		/// 所创建的SqlParameter集合
		/// </returns>
		internal static SqlParameterCollection CreateParameterCollection()
		{
			return createParameterCollection();
		}
	}
}
