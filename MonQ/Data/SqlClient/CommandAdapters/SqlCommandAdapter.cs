using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection.Emit;
using System.Reflection;
using MonQ.Properties;
using MonQ.Reflection;
using System.Collections.Concurrent;

namespace MonQ.Data.SqlClient.CommandAdapters
{
	/// <summary>
	/// SqlCommand适配器
	/// </summary>
	internal abstract class SqlCommandAdapter
	{
		/// <summary>
		/// 适配器的ID
		/// </summary>
		public readonly string ID;

		/// <summary>
		/// 参数名称列表
		/// </summary>
		public SqlParameterCollection Parameters
		{
			get
			{
				return parameters;
			}
		}

		protected SqlParameterCollection parameters = SqlEmitter.CreateParameterCollection();


		/// <summary>
		/// 构造方法
		/// </summary>
		public SqlCommandAdapter(string id)
		{
			this.ID = id;
		}


		/// <summary>
		/// 准备执行一般性指令
		/// </summary>
		/// <param name="command"></param>
		internal abstract void InitCommand(SqlCommand command);


		/// <summary>
		/// 准备执行分页
		/// </summary>
		/// <param name="command"></param>
		internal virtual void InitPagination(SqlExecuter executer, SqlPager pager, int pageID, int pageSize, string pk)
		{
			throw new NotSupportedException(Resources.SqlClient_NotSupported);
		}

		/// <summary>
		/// 使用指定的参数值初始化SqlCommand参数集合
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="parameterValues">参数值列表</param>
		internal virtual void InitParams(SqlCommand command, object[] parameterValues)
		{
			SqlParameterCollection parameters = this.Parameters;
			for (int i = 0, len = parameters.Count - 1; i < len && i < parameterValues.Length; i++)
			{
				SqlParameter parameter = (parameters[i] as ICloneable).Clone() as SqlParameter;
				object value = parameterValues[i];
				parameter.Value = value;
				if (parameter.Size == 0)
				{
					parameter.Size = 8000;
				}
				command.Parameters.Add(parameter);
			}
		}


		/// <summary>
		/// 使用指定类型的实例初始化SqlCommand参数集合
		/// </summary>
		/// <typeparam name="T">实例的类型</typeparam>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="parameters">包含参数数据的实例</param>
		internal virtual void InitParams<T>(SqlCommand command, T parameters)
		{
			HandlerCache<T>.InitParameters(this, parameters, command.Parameters);
		}


		/// <summary>
		/// 使用一个DataRow中的数据初始化SqlCommand参数集合
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="row">包含参数数据的数据行</param>
		internal virtual void InitParams(SqlCommand command, DataRow row)
		{
			DataColumnCollection columns = row.Table.Columns;
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				int index = columns.IndexOf(parameter.ParameterName.Substring(1));
				if (index > 0)
				{
					parameter = (parameter as ICloneable).Clone() as SqlParameter;
					parameter.Value = row[index];
					command.Parameters.Add(parameter);
				}
			}
		}

		/// <summary>
		/// 使用字典中的数据初始化SqlCommand参数集合
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="dictionary">包含参数数据的字典</param>
		internal virtual void InitParams(SqlCommand command, IDictionary dictionary)
		{
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				string key = parameter.ParameterName.Substring(1);
				if (dictionary.Contains(key))
				{
					parameter = (parameter as ICloneable).Clone() as SqlParameter;
					parameter.Value = dictionary[key];
					command.Parameters.Add(parameter);
				}
			}
		}

		/// <summary>
		/// 使用集合中的数据初始化SqlCommand参数集合
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="collection">包含参数数据的集合</param>
		internal virtual void InitParams(SqlCommand command, NameValueCollection collection)
		{
			string[] allKeys = collection.AllKeys;
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				parameter = (parameter as ICloneable).Clone() as SqlParameter;
				parameter.Value = collection[parameter.ParameterName.Substring(1)];
				command.Parameters.Add(parameter);
			}
		}

		/// <summary>
		/// 使用一个实例的成员数据初始化SqlCommand参数集合
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="instance">包含参数数据的实例</param>
		internal virtual void InitParamsByObject(SqlCommand command, object instance)
		{
			HandlerCache.InitParameters(this, instance, command.Parameters);
		}

		/// <summary>
		/// 使用指定的Sql参数列表初始化SqlCommand
		/// </summary>
		/// <param name="command">需要初始化参数的SqlCommand实例</param>
		/// <param name="parameters">Sql参数列表</param>
		internal virtual void InitParams(SqlCommand command, params SqlParameter[] parameters)
		{
			for (int i = 0, len = parameters.Length; i < len; i++)
			{
				command.Parameters.Add(parameters[i]);
			}
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给参数值列表
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="parameterValues">参数值列表</param>
		internal virtual void RetrieveParams(SqlCommand command, object[] parameterValues)
		{
			for (int i = 0, count = command.Parameters.Count; i < count; i++)
			{
				if (i < parameterValues.Length) parameterValues[i] = command.Parameters[i].Value;
			}
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给参数列表
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="parameters">参数列表</param>
		internal virtual void RetrieveParams(SqlCommand command, SqlParameter[] parameters)
		{
			command.Parameters.Clear();
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给参数实体
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="entity">参数实体</param>
		internal virtual void RetrieveParams<T>(SqlCommand command, T entity)
		{
			RetrieveParams<T>(command, ref entity);
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给参数实体
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="entity">参数实体</param>
		internal virtual void RetrieveParams<T>(SqlCommand command, ref T entity)
		{
			HandlerCache<T>.RetrieveParams(this, command, ref entity);
		}


		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给数据行
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="row">数据行</param>
		internal virtual void RetrieveParams(SqlCommand command, DataRow row)
		{
			SqlParameterCollection parameters = command.Parameters;
			DataColumnCollection columns = row.Table.Columns;
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				if (parameter.Direction != ParameterDirection.Input)
				{
					int index = columns.IndexOf(parameter.ParameterName.Substring(1));
					if (index > 0)
					{
						row[index] = parameter.Value;
					}
				}
			}
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给字典
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="dictionary">字典</param>
		internal virtual void RetrieveParams(SqlCommand command, IDictionary dictionary)
		{
			SqlParameterCollection parameters = command.Parameters;
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				if (parameter.Direction != ParameterDirection.Input)
				{
					string key = parameter.ParameterName.Substring(1);
					if (dictionary.Contains(key))
					{
						dictionary[key] = parameter.Value;
					}
				}
			}
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给集合
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="collection">集合</param>
		internal virtual void RetrieveParams(SqlCommand command, NameValueCollection collection)
		{
			SqlParameterCollection parameters = command.Parameters;
			for (int i = 0, count = parameters.Count; i < count; i++)
			{
				SqlParameter parameter = parameters[i];
				if (parameter.Direction != ParameterDirection.Input)
				{
					collection[parameter.ParameterName.Substring(1)] = Convert.ToString(parameter.Value);
				}
			}
		}

		/// <summary>
		/// 读取SqlCommand的参数并且重新赋值给实例
		/// </summary>
		/// <param name="command">SqlCommand实例</param>
		/// <param name="instance">实例</param>
		internal virtual void RetrieveParamsByObject(SqlCommand command, object instance)
		{
			HandlerCache.RetrieveParams(this, command, instance);
		}


		/// <summary>
		/// 增加一个返回参数
		/// </summary>
		protected void AddReturnParameter()
		{
			SqlParameter parameter = new SqlParameter("@RETURN_VALUE", null);
			parameter.Direction = ParameterDirection.ReturnValue;
			parameter.SqlDbType = SqlDbType.Int;
			parameter.Size = 4;
			parameters.Add(parameter);
		}

		/// <summary>
		/// 适配器辅助类
		/// </summary>
		internal class CommandAdapterHelper
		{

			internal static void GetParameterType(Type type, out int size, out SqlDbType sqlDbType)
			{
				size = -1;
				sqlDbType = 0;


				if (type == typeof(string))
				{
					sqlDbType = SqlDbType.NVarChar;
					size = 4000;
				}
				else if (type == typeof(char))
				{
					sqlDbType = SqlDbType.NChar;
					size = 8;
				}
				else if (type == typeof(int) || type == typeof(uint))
				{
					sqlDbType = SqlDbType.Int;
					size = 4;
				}
				else if (type == typeof(short) || type == typeof(ushort))
				{
					sqlDbType = SqlDbType.SmallInt;
					size = 2;
				}
				else if (type == typeof(byte) || type == typeof(sbyte))
				{
					sqlDbType = SqlDbType.TinyInt;
					size = 1;
				}
				else if (type == typeof(long) || type == typeof(ulong))
				{
					sqlDbType = SqlDbType.BigInt;
					size = 8;
				}
				else if (type == typeof(float))
				{
					sqlDbType = SqlDbType.Real;
					size = 8;
				}
				else if (type == typeof(double))
				{
					sqlDbType = SqlDbType.Float;
					size = 8;
				}
				else if (type == typeof(byte[]))
				{
					sqlDbType = SqlDbType.Image;
				}
				else if (type == typeof(bool))
				{
					sqlDbType = SqlDbType.Bit;
					size = 1;
				}
				else if (type == typeof(decimal))
				{
					sqlDbType = SqlDbType.Decimal;
					size = 17;
				}
				else if (type == typeof(DateTime))
				{
					sqlDbType = SqlDbType.DateTime;
					size = 8;
				}
			}

			internal static ConstructorInfo sqlParameterConstructor = typeof(SqlParameter).GetConstructor(new Type[] { typeof(string), typeof(object) });
			internal static MethodInfo setParameterDirectionMethod = typeof(SqlParameter).GetProperty("Direction").GetSetMethod();
			internal static MethodInfo setSqlDbTypeMethod = typeof(SqlParameter).GetProperty("SqlDbType").GetSetMethod();
			internal static MethodInfo setParameterSizeMethod = typeof(SqlParameter).GetProperty("Size").GetSetMethod();
			internal static MethodInfo addParameterMethod = typeof(SqlParameterCollection).GetMethod("Add", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(SqlParameter) }, null);

			//生成初始化属性的IL代码
			internal static void GeneratePropertyInitIL(ILGenerator generator, LocalBuilder instance, Type instanceType, PropertyInfo property, ParameterDirection direction)
			{
				LocalBuilder parameter = generator.DeclareLocal(typeof(SqlParameter));
				generator.Emit(OpCodes.Ldstr, "@" + property.Name);
				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}
				MethodInfo getMethod = property.GetGetMethod();
				if (getMethod.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, getMethod);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, getMethod);
				}
				Type propertyType = property.PropertyType;
				if (propertyType.IsValueType)
				{
					generator.Emit(OpCodes.Box, propertyType);
				}
				generator.Emit(OpCodes.Newobj, sqlParameterConstructor);
				generator.Emit(OpCodes.Stloc, parameter);

				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Ldc_I4, (int)direction);
				generator.Emit(OpCodes.Callvirt, setParameterDirectionMethod);

				int size;
				SqlDbType sqlDbType;

				GetParameterType(propertyType, out size, out sqlDbType);

				if (size > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, size);
					generator.Emit(OpCodes.Callvirt, setParameterSizeMethod);
				}
				if (sqlDbType > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, (int)sqlDbType);
					generator.Emit(OpCodes.Call, setSqlDbTypeMethod);
				}

				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Callvirt, addParameterMethod);
				generator.Emit(OpCodes.Pop);
			}

			//生成根据索引初始化属性的IL代码
			internal static void GenerateIndexedPropertyInitIL(ILGenerator generator, LocalBuilder instance, Type instanceType, PropertyInfo indexedProperty, int index, SqlParameter srcParameter)
			{
				string parameterName = srcParameter.ParameterName;
				ParameterDirection direction = srcParameter.Direction;
				//声明变量
				LocalBuilder parameter = generator.DeclareLocal(typeof(SqlParameter));
				LocalBuilder value = generator.DeclareLocal(typeof(object));
				//获得必要的方法
				MethodInfo getInstanceItem = indexedProperty.GetGetMethod(true);
				MethodInfo getParamByIndex = typeof(SqlParameterCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				MethodInfo getParamValue = typeof(SqlParameter).GetProperty("Value").GetGetMethod();

				//object value = src[index].Value;
				#region 首先设置一个默认值
				generator.Emit(OpCodes.Ldarg_2);
				generator.Emit(OpCodes.Ldc_I4, index);
				generator.Emit(OpCodes.Callvirt, getParamByIndex);
				generator.Emit(OpCodes.Callvirt, getParamValue);
				generator.Emit(OpCodes.Stloc, value);
				#endregion

				/*
				 * try
				 * {
				 *		value = instance[parameterName.Trim('@')];
				 * }
				 * catch(Exception)
				 * {
				 *		try
				 *		{
				 *			value = instance[parameterName];
				 *		}
				 *		catch(Exception)
				 *		{
				 *		}
				 * }
				 */
				#region 尝试使用去掉@字符的参数名称获取
				generator.BeginExceptionBlock();//try 1

				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}

				generator.Emit(OpCodes.Ldstr, parameterName.Trim('@'));
				if (getInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, getInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, getInstanceItem);
				}
				if (getInstanceItem.ReturnType.IsValueType)
				{
					generator.Emit(OpCodes.Box, getInstanceItem.ReturnType);
				}
				generator.Emit(OpCodes.Stloc, value);

				generator.BeginCatchBlock(typeof(Exception));
				generator.Emit(OpCodes.Pop);

				#region 尝试使用完整的参数名称获取
				generator.BeginExceptionBlock();//try 2

				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}

				generator.Emit(OpCodes.Ldstr, parameterName);
				if (getInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, getInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, getInstanceItem);
				}
				if (getInstanceItem.ReturnType.IsValueType)
				{
					generator.Emit(OpCodes.Box, getInstanceItem.ReturnType);
				}
				generator.Emit(OpCodes.Stloc, value);

				generator.BeginCatchBlock(typeof(Exception));
				generator.Emit(OpCodes.Pop);

				generator.EndExceptionBlock();
				#endregion

				generator.EndExceptionBlock();
				#endregion

				#region 对parameter赋值并添加到参数集合
				generator.Emit(OpCodes.Ldstr, parameterName);
				generator.Emit(OpCodes.Ldloc, value);
				generator.Emit(OpCodes.Newobj, sqlParameterConstructor);
				generator.Emit(OpCodes.Stloc, parameter);

				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Ldc_I4, (int)direction);
				generator.Emit(OpCodes.Callvirt, setParameterDirectionMethod);

				int size;
				SqlDbType sqlDbType;

				GetParameterType(indexedProperty.PropertyType, out size, out sqlDbType);

				if (size > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, size);
					generator.Emit(OpCodes.Callvirt, setParameterSizeMethod);
				}
				else
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, 4000);
					generator.Emit(OpCodes.Callvirt, setParameterSizeMethod);
				}
				if (sqlDbType > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, (int)sqlDbType);
					generator.Emit(OpCodes.Call, setSqlDbTypeMethod);
				}

				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Callvirt, addParameterMethod);
				generator.Emit(OpCodes.Pop);
				#endregion

			}

			//生成初始化某个成员的IL代码
			internal static void GenerateFieldInitIL(ILGenerator generator, LocalBuilder instance, Type instanceType, FieldInfo field, ParameterDirection direction)
			{
				LocalBuilder parameter = generator.DeclareLocal(typeof(SqlParameter));
				generator.Emit(OpCodes.Ldstr, "@" + field.Name);
				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}
				generator.Emit(OpCodes.Ldfld, field);

				Type fieldType = field.FieldType;

				if (fieldType.IsValueType)
				{
					generator.Emit(OpCodes.Box, fieldType);
				}
				generator.Emit(OpCodes.Newobj, sqlParameterConstructor);
				generator.Emit(OpCodes.Stloc, parameter);

				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Ldc_I4, (int)direction);
				generator.Emit(OpCodes.Callvirt, setParameterDirectionMethod);

				int size;
				SqlDbType sqlDbType;

				GetParameterType(fieldType, out size, out sqlDbType);

				if (size > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, size);
					generator.Emit(OpCodes.Callvirt, setParameterSizeMethod);
				}
				if (sqlDbType > 0)
				{
					generator.Emit(OpCodes.Ldloc, parameter);
					generator.Emit(OpCodes.Ldc_I4, (int)sqlDbType);
					generator.Emit(OpCodes.Call, setSqlDbTypeMethod);
				}

				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldloc, parameter);
				generator.Emit(OpCodes.Callvirt, addParameterMethod);
				generator.Emit(OpCodes.Pop);
			}


			//生成根据索引回写属性的IL代码
			internal static void GenerateIndexedPropertyRetrieveIL(ILGenerator generator, LocalBuilder instance, Type instanceType, PropertyInfo indexedProperty, int index, string parameterName)
			{
				//声明变量
				LocalBuilder parameter = generator.DeclareLocal(typeof(SqlParameter));
				LocalBuilder value = generator.DeclareLocal(typeof(object));
				//获得必要的方法
				MethodInfo getInstanceItem = indexedProperty.GetGetMethod(true);
				MethodInfo setInstanceItem = indexedProperty.GetSetMethod(true);
				MethodInfo getParamByIndex = typeof(SqlParameterCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				MethodInfo getParamValue = typeof(SqlParameter).GetProperty("Value").GetGetMethod();

				//object value = src[index].Value;
				#region 首先定义一个默认值
				generator.Emit(OpCodes.Ldarg_1);
				generator.Emit(OpCodes.Ldc_I4, index);
				generator.Emit(OpCodes.Callvirt, getParamByIndex);
				generator.Emit(OpCodes.Callvirt, getParamValue);
				generator.Emit(OpCodes.Stloc, value);
				#endregion

				/*
				 * try
				 * {
				 *		instance[parameterName.Trim('@')] = value;
				 * }
				 * catch(Exception)
				 * {
				 *		try
				 *		{
				 *			instance[parameterName] = value;
				 *		}
				 *		catch(Exception)
				 *		{
				 *		}
				 * }
				 */
				#region 尝试使用去掉@字符的参数名称设定
				generator.BeginExceptionBlock();//try 1

				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}

				generator.Emit(OpCodes.Ldstr, parameterName.Trim('@'));
				if (getInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, getInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, getInstanceItem);
				}
				if (getInstanceItem.ReturnType.IsValueType)
				{
					generator.Emit(OpCodes.Box, getInstanceItem.ReturnType);
				}
				generator.Emit(OpCodes.Stloc, value);


				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}
				generator.Emit(OpCodes.Ldstr, parameterName.Trim('@'));
				generator.Emit(OpCodes.Ldloc, value);
				if (setInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, setInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, setInstanceItem);
				}

				generator.BeginCatchBlock(typeof(Exception));
				generator.Emit(OpCodes.Pop);

				#region 尝试使用完整的参数名称获取
				generator.BeginExceptionBlock();//try 2

				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}

				generator.Emit(OpCodes.Ldstr, parameterName);
				if (getInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, getInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, getInstanceItem);
				}
				if (getInstanceItem.ReturnType.IsValueType)
				{
					generator.Emit(OpCodes.Box, getInstanceItem.ReturnType);
				}
				generator.Emit(OpCodes.Stloc, value);


				if (instance == null)
				{
					if (instanceType.IsValueType)
					{
						generator.Emit(OpCodes.Ldarga, 0);
					}
					else
					{
						generator.Emit(OpCodes.Ldarg_0);
					}
				}
				else
				{
					if (instanceType.IsClass)
					{
						generator.Emit(OpCodes.Ldloc, instance);
					}
					else
					{
						generator.Emit(OpCodes.Ldloca, instance);
					}
				}
				generator.Emit(OpCodes.Ldstr, parameterName.Trim('@'));
				generator.Emit(OpCodes.Ldloc, value);
				if (setInstanceItem.DeclaringType == instanceType)
				{
					generator.Emit(OpCodes.Call, setInstanceItem);
				}
				else
				{
					generator.Emit(OpCodes.Callvirt, setInstanceItem);
				}

				generator.BeginCatchBlock(typeof(Exception));
				generator.Emit(OpCodes.Pop);

				generator.EndExceptionBlock();
				#endregion

				generator.EndExceptionBlock();
				#endregion

			}

			//获取某个类型中的this[string]或this[object]形式的属性
			internal static PropertyInfo GetIndexedPropertyIfBetter(Type instanceType)
			{
				if (Reflector.IsImplemented(instanceType, typeof(ISqlRecord)))
				{
					return null;
				}
				PropertyInfo indexedProperty = null;
				foreach (PropertyInfo property in instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					ParameterInfo[] propertyPrams = property.GetIndexParameters();
					if (propertyPrams.Length == 1)
					{
						Type paramType = propertyPrams[0].ParameterType;
						if (paramType == typeof(string) && property.CanRead)
						{
							indexedProperty = property;
							break;
						}
						else if (paramType == typeof(object) && property.CanRead)
						{
							indexedProperty = property;
						}
					}
				}
				return indexedProperty;
			}
		}

		/// <summary>
		/// 针对明确类型的Sql参数写入及回写缓存
		/// </summary>
		/// <typeparam name="T">实体类型</typeparam>
		internal class HandlerCache<T>
		{
			private delegate void SqlParameterInitHandler(T instance, SqlParameterCollection tarParam, SqlParameterCollection srcParam);

			private delegate void SqlParameterRetrieveHandler(ref T instance, SqlParameterCollection tarParam);

			private static ConcurrentDictionary<string, SqlParameterInitHandler> generationHandlers = new ConcurrentDictionary<string, SqlParameterInitHandler>();

			private static ConcurrentDictionary<string, SqlParameterRetrieveHandler> retrieveHandlers = new ConcurrentDictionary<string, SqlParameterRetrieveHandler>();

			public static void InitParameters(SqlCommandAdapter adapter, T instance, SqlParameterCollection parameters)
			{
				generationHandlers.GetOrAdd(adapter.ID, (key) =>
				{
					Type instanceType = typeof(T);
					if (instanceType != typeof(T))
					{
						PropertyInfo indexedProperty = CommandAdapterHelper.GetIndexedPropertyIfBetter(instanceType);
						if (indexedProperty == null)
						{
							if (indexedProperty.GetGetMethod(true) == null) return delegate(T o, SqlParameterCollection p, SqlParameterCollection p2) { };
							return (SqlParameterInitHandler)GenerateInitMethodForNormal(adapter.parameters, typeof(T)).CreateDelegate(typeof(SqlParameterInitHandler));
						}
						else
						{
							return (SqlParameterInitHandler)GenerateInitMethodForIndexed(adapter.parameters, indexedProperty, instanceType).CreateDelegate(typeof(SqlParameterInitHandler));
						}
					}
					else
					{
						return delegate(T instanceObj, SqlParameterCollection targetParams, SqlParameterCollection srcParams)
						{
							HandlerCache.InitParameters(adapter, instanceObj, parameters);
						};
					}
				})(instance, parameters, adapter.parameters);
			}

			public static void RetrieveParams(SqlCommandAdapter adapter, SqlCommand command, ref T instance)
			{
				retrieveHandlers.GetOrAdd(adapter.ID, (key) =>
				{
					Type instanceType = typeof(T);
					if (instanceType != typeof(object))
					{
						PropertyInfo indexedProperty = CommandAdapterHelper.GetIndexedPropertyIfBetter(instanceType);
						if (indexedProperty != null)
						{
							if (indexedProperty.GetSetMethod(true) == null) return delegate(ref T o, SqlParameterCollection p) { };
							return (SqlParameterRetrieveHandler)GenerateRetrieveMethodForIndexed(command.Parameters, typeof(T), indexedProperty).CreateDelegate(typeof(SqlParameterRetrieveHandler));
						}
						else if (instanceType.Name[0] == '<' && string.IsNullOrEmpty(instanceType.Namespace))
						{
							return delegate(ref T instanceObj, SqlParameterCollection parameters)
							{
								HandlerCache.RetrieveParams(adapter, command, instanceObj);
							};
						}
						return (SqlParameterRetrieveHandler)GenerateRetrieveMethodForNormal(command.Parameters, typeof(T)).CreateDelegate(typeof(SqlParameterRetrieveHandler));
					}
					else//如果是object类型的话，转到HandlerCache获取方法
					{
						return delegate(ref T instanceObj, SqlParameterCollection parameters)
						{
							HandlerCache.RetrieveParams(adapter, command, instanceObj);
						};
					}
				})(ref instance, command.Parameters);
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将指定实例的成员的值赋给SqlCommand的参数集合
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			private static DynamicMethod GenerateInitMethodForNormal(SqlParameterCollection parameters, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("GenerateParameters", null, new Type[] { instanceType, typeof(SqlParameterCollection), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(T instance, SqlParameterCollection target, SqlParameterCollection definition)
				 * {
				 *		SqlParameter p = new SqlParameter("@A", instance.A);
				 *		p.Direction = ParameterDirection.Input;
				 *		p.Size = 4;
				 *		p.SqlDbType = SqlDbType.Int;
				 *		target.Add(p);
				 *		
				 *		SqlParameter p2 = new SqlParameter("@B", instance.B);
				 *		p2.Direction = ParameterDirection.Input;
				 *		p.Size = 4000;
				 *		p.SqlDbType = SqlDbType.NVarchar;
				 *		target.Add(p);
				 *		
				 *		
				 *		target.Add(definition[2]);
				 *		target.Add(definition[3]);
				 * }
				 */

				SqlParameter sqlParameter;
				int index;

				#region 根据属性生成SqlParameter
				PropertyInfo[] properties = Reflector.GetProperties(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = properties.Length; i < len; i++)
				{
					PropertyInfo property = properties[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, property);;
					index = parameters.IndexOf(parameterName);
					if (property.CanRead && property.GetIndexParameters().Length == 0 && index != -1 && SqlTypeConvertor.IsConvertibleType(property.PropertyType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						sqlParameter = parameters[index];
						if ((sqlParameter.Direction & ParameterDirection.Input) == ParameterDirection.Input)
						{
							CommandAdapterHelper.GeneratePropertyInitIL(generator, null, instanceType, property, parameters[parameterName].Direction);
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion

				#region 根据成员生成SqlParameter
				FieldInfo[] fields = Reflector.GetFields(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = fields.Length; i < len; i++)
				{
					FieldInfo field = fields[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, field);;
					index = parameters.IndexOf(parameterName);
					if (index != -1 && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						sqlParameter = parameters[index];
						if ((sqlParameter.Direction & ParameterDirection.Input) == ParameterDirection.Input)
						{
							CommandAdapterHelper.GenerateFieldInitIL(generator, null, instanceType, field, parameters[parameterName].Direction);
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion

				#region 将剩余的SqlParameter写入
				MethodInfo getItem = typeof(SqlParameterCollection).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, typeof(SqlParameter), new Type[] { typeof(int) }, null).GetGetMethod();
				MethodInfo clone = typeof(ICloneable).GetMethod("Clone");
				for (int i = 0, len = parameters.Count; i < len; i++)
				{
					string parameterName = parameters[i].ParameterName.ToLower();
					if (usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						generator.Emit(OpCodes.Ldarg_1);
						generator.Emit(OpCodes.Ldarg_2);
						generator.Emit(OpCodes.Ldc_I4, i);
						generator.Emit(OpCodes.Callvirt, getItem);
						generator.Emit(OpCodes.Callvirt, clone);
						generator.Emit(OpCodes.Isinst, typeof(SqlParameter));
						generator.Emit(OpCodes.Callvirt, CommandAdapterHelper.addParameterMethod);
						generator.Emit(OpCodes.Pop);
						usedNames.Add(parameterName.ToLower());
					}
				}
				#endregion

				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将指定实例的成员的值赋给SqlCommand的参数集合
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateInitMethodForIndexed(SqlParameterCollection parameters, PropertyInfo itemProperty, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("GenerateParameters", null, new Type[] { typeof(T), typeof(SqlParameterCollection), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				LocalBuilder real = generator.DeclareLocal(instanceType);

				generator.Emit(OpCodes.Ldarg_0);
				if (instanceType.IsClass)
				{
					generator.Emit(OpCodes.Castclass, instanceType);
				}
				else
				{
					generator.Emit(OpCodes.Unbox_Any, instanceType);
				}
				generator.Emit(OpCodes.Stloc, real);


				for (int i = 0; i < parameters.Count; i++)
				{
					if ((parameters[i].Direction & ParameterDirection.Input) == ParameterDirection.Input)
					{
						CommandAdapterHelper.GenerateIndexedPropertyInitIL(generator, real, instanceType, itemProperty, i, parameters[i]);
					}
				}


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}


			/// <summary>
			/// 创建一个动态方法,这个方法将SqlCommand的参数集合中的输出参数值重新绑定到所指定的实例之中
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			private static DynamicMethod GenerateRetrieveMethodForNormal(SqlParameterCollection parameters, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("RetrieveParameters", null, new Type[] { instanceType.MakeByRefType(), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();
				List<EventHandler> handlers = new List<EventHandler>();

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(ref T instance, SqlParameterCollection parameters)
				 * {
				 *		instance.A = Convert.ToInt32(parameters[0].Value);
				 *		
				 *		instance.B = Convert.ToString(parameters[1].Value);
				 * }
				 */

				#region 根据属性设置SqlParameter输出值
				PropertyInfo[] properties = Reflector.GetProperties(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				MethodInfo getValueMethod = typeof(SqlParameter).GetProperty("Value").GetGetMethod();
				MethodInfo getItemMethod = typeof(SqlParameterCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				for (int i = 0, len = properties.Length; i < len; i++)
				{
					PropertyInfo property = properties[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, property);;
					int paramIndex = parameters.IndexOf(parameterName);
					if (property.CanWrite && property.GetIndexParameters().Length == 0 && paramIndex != -1 && SqlTypeConvertor.IsConvertibleType(property.PropertyType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(property.PropertyType);
						SqlParameter sqlParam = parameters[paramIndex];
						if (sqlParam.Direction != ParameterDirection.Input)
						{
							LocalBuilder exception = generator.DeclareLocal(typeof(Exception));
							LocalBuilder value = generator.DeclareLocal(typeof(object));
							generator.Emit(OpCodes.Ldnull);
							generator.Emit(OpCodes.Stloc, value);
							Label _try = generator.BeginExceptionBlock();
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Callvirt, getItemMethod);
							generator.Emit(OpCodes.Callvirt, getValueMethod);
							generator.Emit(OpCodes.Stloc, value);

							generator.Emit(OpCodes.Ldarg_0);
							if (instanceType.IsClass)
							{
								generator.Emit(OpCodes.Ldind_Ref);
							}
							generator.Emit(OpCodes.Ldloc, value);
							generator.Emit(OpCodes.Call, convertMethod);
							MethodInfo setMethod = property.GetSetMethod(true);
							if (setMethod.DeclaringType == instanceType)
							{
								generator.Emit(OpCodes.Call, setMethod);
							}
							else
							{
								generator.Emit(OpCodes.Callvirt, setMethod);
							}

							generator.BeginCatchBlock(typeof(Exception));

							generator.Emit(OpCodes.Stloc, exception);
							generator.Emit(OpCodes.Ldstr, Resources.SqlClient_RetrieveParameterError);
							generator.Emit(OpCodes.Ldstr, parameterName + "->" + property.Name);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Box, typeof(int));
							generator.Emit(OpCodes.Ldloc, value);
							MethodInfo stringFormat = typeof(string).GetMethod("Format", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) }, null);
							generator.Emit(OpCodes.Call, stringFormat);
							generator.Emit(OpCodes.Ldloc, exception);
							generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[] { typeof(string), typeof(Exception) }));
							generator.Emit(OpCodes.Throw);
							generator.EndExceptionBlock();
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion

				#region 根据成员设置SqlParameter输出值
				FieldInfo[] fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = fields.Length; i < len; i++)
				{
					FieldInfo field = fields[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, field);;
					int paramIndex = parameters.IndexOf(parameterName);
					if (paramIndex != -1 && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(field.FieldType);
						SqlParameter sqlParam = parameters[paramIndex];
						if (sqlParam.Direction != ParameterDirection.Input)
						{
							LocalBuilder exception = generator.DeclareLocal(typeof(Exception));
							LocalBuilder value = generator.DeclareLocal(typeof(object));
							generator.Emit(OpCodes.Ldnull);
							generator.Emit(OpCodes.Stloc, value);
							Label _try = generator.BeginExceptionBlock();
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Callvirt, getItemMethod);
							generator.Emit(OpCodes.Callvirt, getValueMethod);
							generator.Emit(OpCodes.Stloc, value);

							generator.Emit(OpCodes.Ldarg_0);
							if (instanceType.IsClass)
							{
								generator.Emit(OpCodes.Ldind_Ref);
							}
							generator.Emit(OpCodes.Ldloc, value);
							generator.Emit(OpCodes.Call, convertMethod);
							generator.Emit(OpCodes.Stfld, field);

							generator.BeginCatchBlock(typeof(Exception));

							generator.Emit(OpCodes.Stloc, exception);
							generator.Emit(OpCodes.Ldstr, Resources.SqlClient_RetrieveParameterError);
							generator.Emit(OpCodes.Ldstr, parameterName + "->" + field.Name);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Box, typeof(int));
							generator.Emit(OpCodes.Ldloc, value);
							MethodInfo stringFormat = typeof(string).GetMethod("Format", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) }, null);
							generator.Emit(OpCodes.Call, stringFormat);
							generator.Emit(OpCodes.Ldloc, exception);
							generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[] { typeof(string), typeof(Exception) }));
							generator.Emit(OpCodes.Throw);
							generator.EndExceptionBlock();
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion




				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}


			/// <summary>
			/// 创建一个动态方法,这个方法将SqlCommand的参数集合中的输出参数值重新绑定到所指定具备索引的实例之中
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <param name="indexedProperty">索引属性</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateRetrieveMethodForIndexed(SqlParameterCollection parameters, Type instanceType, PropertyInfo indexedProperty)
			{
				DynamicMethod method = new DynamicMethod("RetrieveParameters", null, new Type[] { instanceType.MakeByRefType(), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(object instance, SqlParameterCollection parameters)
				 * {
				 *		T real = (T) instance;
				 *		
				 *		real.A = Convert.ToInt32(parameters[0].Value);
				 *		
				 *		real.B = Convert.ToString(parameters[1].Value);
				 * }
				 */

				for (int i = 0; i < parameters.Count; i++)
				{
					if ((parameters[i].Direction & ParameterDirection.Output) == 0) continue;
					CommandAdapterHelper.GenerateIndexedPropertyRetrieveIL(generator, null, instanceType, indexedProperty, i, parameters[i].ParameterName);
				}


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}
		}

		/// <summary>
		/// 针对不明确类型的写入及回写方法缓存
		/// </summary>
		internal class HandlerCache
		{
			private delegate void SqlParameterInitHandler(object instance, SqlParameterCollection tarParam, SqlParameterCollection srcParam);

			private delegate void SqlParameterRetrieveHandler(object instance, SqlParameterCollection tarParam);

			private static ConcurrentDictionary<Type, ConcurrentDictionary<string, SqlParameterInitHandler>> generationHandlers = new ConcurrentDictionary<Type, ConcurrentDictionary<string, SqlParameterInitHandler>>();

			private static ConcurrentDictionary<Type, ConcurrentDictionary<string, SqlParameterRetrieveHandler>> retrieveHandlers = new ConcurrentDictionary<Type, ConcurrentDictionary<string, SqlParameterRetrieveHandler>>();

			internal static List<FieldInfo> anonymouseFields = new List<FieldInfo>();

			public static void InitParameters(SqlCommandAdapter adapter, object instance, SqlParameterCollection parameters)
			{
				if (instance == null)
				{
					SqlParameterCollection adapterParameters = adapter.Parameters;
					for (int i = adapterParameters.Count - 1; i > -1; i--)
					{
						parameters.Add((adapterParameters[i] as ICloneable).Clone() as SqlParameter);
					}
					return;
				}
				generationHandlers.GetOrAdd(instance.GetType(), (key) =>
				{
					return new ConcurrentDictionary<string, SqlParameterInitHandler>();
				}).GetOrAdd(adapter.ID, (key) =>
				{
					return GenerateInitHandler(adapter.parameters, instance.GetType());
				})(instance, parameters, adapter.parameters);
			}

			public static void RetrieveParams(SqlCommandAdapter adapter, SqlCommand command, object instance)
			{
				if (instance == null)
				{
					return;
				}
				retrieveHandlers.GetOrAdd(instance.GetType(), (key) =>
				{
					return new ConcurrentDictionary<string, SqlParameterRetrieveHandler>();
				}).GetOrAdd(adapter.ID, (key) =>
				{
					return GenerateRetrieveHandler(adapter.Parameters, instance.GetType());
				})(instance, command.Parameters);
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将指定实例的成员的值赋给SqlCommand的参数集合
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			private static SqlParameterInitHandler GenerateInitHandler(SqlParameterCollection parameters, Type instanceType)
			{
				PropertyInfo indexedProperty = CommandAdapterHelper.GetIndexedPropertyIfBetter(instanceType);
				if (indexedProperty == null)
				{
					return (SqlParameterInitHandler)GenerateInitMethodForNormal(parameters, instanceType).CreateDelegate(typeof(SqlParameterInitHandler));
				}
				else
				{
					if (indexedProperty.GetGetMethod(true) == null) return delegate(object o, SqlParameterCollection p, SqlParameterCollection p2) { };
					return (SqlParameterInitHandler)GenerateInitMethodForIndexed(parameters, indexedProperty, instanceType).CreateDelegate(typeof(SqlParameterInitHandler));
				}
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将查询结果的输出参数回写到实例
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			private static SqlParameterRetrieveHandler GenerateRetrieveHandler(SqlParameterCollection parameters, Type instanceType)
			{
				PropertyInfo indexedProperty = CommandAdapterHelper.GetIndexedPropertyIfBetter(instanceType);
				if (indexedProperty != null)
				{
					if (indexedProperty.GetSetMethod(true) == null) return delegate(object o, SqlParameterCollection p) { };
					return (SqlParameterRetrieveHandler)GenerateRetrieveMethodForIndexed(parameters, instanceType, indexedProperty).CreateDelegate(typeof(SqlParameterRetrieveHandler));
				}
				else if (instanceType.Name[0] == '<' && string.IsNullOrEmpty(instanceType.Namespace))
				{
					return (SqlParameterRetrieveHandler)GenerateRetrieveMethodForAnonymous(parameters, instanceType).CreateDelegate(typeof(SqlParameterRetrieveHandler));
				}
				else
				{
					return (SqlParameterRetrieveHandler)GenerateRetrieveMethodForNormal(parameters, instanceType).CreateDelegate(typeof(SqlParameterRetrieveHandler));
				}
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将指定实例的成员的值赋给SqlCommand的参数集合
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateInitMethodForNormal(SqlParameterCollection parameters, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("GenerateParameters", null, new Type[] { typeof(object), typeof(SqlParameterCollection), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();
				List<EventHandler> handlers = new List<EventHandler>();

				LocalBuilder real = generator.DeclareLocal(instanceType);

				generator.Emit(OpCodes.Ldarg_0);
				if (instanceType.IsClass)
				{
					generator.Emit(OpCodes.Castclass, instanceType);
				}
				else
				{
					generator.Emit(OpCodes.Unbox_Any, instanceType);
				}
				generator.Emit(OpCodes.Stloc, real);



				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(object instance, SqlParameterCollection target, SqlParameterCollection definition)
				 * {
				 *		T real = (T)instance;
				 *		SqlParameter p = new SqlParameter("@A", real.A);
				 *		p.Direction = ParameterDirection.Input;
				 *		p.Size = 4;
				 *		p.SqlDbType = SqlDbType.Int;
				 *		target.Add(p);
				 *		
				 *		SqlParameter p2 = new SqlParameter("@B", real.B);
				 *		p2.Direction = ParameterDirection.Input;
				 *		p.Size = 4000;
				 *		p.SqlDbType = SqlDbType.NVarchar;
				 *		target.Add(p);
				 *		
				 *		
				 *		target.Add(definition[2]);
				 *		target.Add(definition[3]);
				 * }
				 */

				SqlParameter sqlParameter;
				int index;

				#region 根据属性生成SqlParameter
				PropertyInfo[] properties = Reflector.GetProperties(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = properties.Length; i < len; i++)
				{
					PropertyInfo property = properties[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, property);;

					index = parameters.IndexOf(parameterName);
					if (property.CanRead && property.GetIndexParameters().Length == 0 && index != -1 && SqlTypeConvertor.IsConvertibleType(property.PropertyType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						sqlParameter = parameters[index];
						if ((sqlParameter.Direction & ParameterDirection.Input) == ParameterDirection.Input)
						{
							CommandAdapterHelper.GeneratePropertyInitIL(generator, real, instanceType, property, parameters[parameterName].Direction);
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion

				#region 根据成员生成SqlParameter
				FieldInfo[] fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = fields.Length; i < len; i++)
				{
					FieldInfo field = fields[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, field);;
					index = parameters.IndexOf(parameterName);
					if (index != -1 && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						sqlParameter = parameters[index];
						if ((sqlParameter.Direction & ParameterDirection.Input) == ParameterDirection.Input)
						{
							CommandAdapterHelper.GenerateFieldInitIL(generator, real, instanceType, field, parameters[parameterName].Direction);
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion


				#region 将剩余的SqlParameter写入
				MethodInfo getItem = typeof(SqlParameterCollection).GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, typeof(SqlParameter), new Type[] { typeof(int) }, null).GetGetMethod();
				MethodInfo clone = typeof(ICloneable).GetMethod("Clone");
				for (int i = 0, len = parameters.Count; i < len; i++)
				{
					string parameterName = parameters[i].ParameterName.ToLower();
					if (usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						generator.Emit(OpCodes.Ldarg_1);
						generator.Emit(OpCodes.Ldarg_2);
						generator.Emit(OpCodes.Ldc_I4, i);
						generator.Emit(OpCodes.Callvirt, getItem);
						generator.Emit(OpCodes.Callvirt, clone);
						generator.Emit(OpCodes.Isinst, typeof(SqlParameter));
						generator.Emit(OpCodes.Callvirt, CommandAdapterHelper.addParameterMethod);
						generator.Emit(OpCodes.Pop);
						usedNames.Add(parameterName.ToLower());
					}
				}
				#endregion

				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将指定实例的成员的值赋给SqlCommand的参数集合
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateInitMethodForIndexed(SqlParameterCollection parameters, PropertyInfo indexedProperty, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("GenerateParameters", null, new Type[] { typeof(object), typeof(SqlParameterCollection), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();
				List<EventHandler> handlers = new List<EventHandler>();

				LocalBuilder real = generator.DeclareLocal(instanceType);

				generator.Emit(OpCodes.Ldarg_0);
				if (instanceType.IsClass)
				{
					generator.Emit(OpCodes.Castclass, instanceType);
				}
				else
				{
					generator.Emit(OpCodes.Unbox_Any, instanceType);
				}
				generator.Emit(OpCodes.Stloc, real);


				for (int i = 0; i < parameters.Count; i++)
				{
					CommandAdapterHelper.GenerateIndexedPropertyInitIL(generator, real, instanceType, indexedProperty, i, parameters[i]);
				}


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将SqlCommand的参数集合中的输出参数值重新绑定到所指定的匿名类型实例之中
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateRetrieveMethodForAnonymous(SqlParameterCollection parameters, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("RetrieveParameters", null, new Type[] { typeof(object), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(object instance, SqlParameterCollection parameters)
				 * {
				 *		T real = (T) instance;
				 *		
				 *		real.A = Convert.ToInt32(parameters[0].Value);
				 *		
				 *		real.B = Convert.ToString(parameters[1].Value);
				 * }
				 */

				MethodInfo getValueMethod = typeof(SqlParameter).GetProperty("Value").GetGetMethod();
				MethodInfo getItemMethod = typeof(SqlParameterCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				MethodInfo getListItem = typeof(List<FieldInfo>).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				MethodInfo setValue = typeof(FieldInfo).GetMethod("SetValue", new Type[] { typeof(object), typeof(object) }, null);
				FieldInfo fieldList = typeof(HandlerCache).GetField("anonymouseFields", BindingFlags.NonPublic | BindingFlags.Static);



				#region 根据成员设置SqlParameter输出值
				FieldInfo[] fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = fields.Length; i < len; i++)
				{
					FieldInfo field = fields[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, field);
					int paramIndex = parameters.IndexOf(parameterName);
					if (paramIndex != -1 && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(field.FieldType);
						SqlParameter sqlParam = parameters[paramIndex];
						if (sqlParam.Direction != ParameterDirection.Input)
						{
							generator.Emit(OpCodes.Nop);
							generator.Emit(OpCodes.Ldsfld, fieldList);

							int fieldIndex;
							lock (anonymouseFields)
							{
								fieldIndex = anonymouseFields.IndexOf(field);
								if (fieldIndex == -1)
								{
									fieldIndex = anonymouseFields.Count;
									anonymouseFields.Add(field);
								}
							}
							generator.Emit(OpCodes.Ldc_I4, fieldIndex);
							generator.Emit(OpCodes.Callvirt, getListItem);

							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Callvirt, getItemMethod);
							generator.Emit(OpCodes.Callvirt, getValueMethod);
							generator.Emit(OpCodes.Call, convertMethod);
							if (field.FieldType.IsValueType)
							{
								generator.Emit(OpCodes.Box, field.FieldType);
							}
							generator.Emit(OpCodes.Callvirt, setValue);
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将SqlCommand的参数集合中的输出参数值重新绑定到所指定的非匿名的类型实例之中
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateRetrieveMethodForNormal(SqlParameterCollection parameters, Type instanceType)
			{
				DynamicMethod method = new DynamicMethod("RetrieveParameters", null, new Type[] { typeof(object), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				List<string> usedNames = new List<string>();

				LocalBuilder real = generator.DeclareLocal(instanceType);
				generator.Emit(OpCodes.Ldarg_0);
				if (instanceType.IsClass)
				{
					generator.Emit(OpCodes.Castclass, instanceType);
				}
				else
				{
					generator.Emit(OpCodes.Unbox_Any, instanceType);
				}
				generator.Emit(OpCodes.Stloc, real);

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(object instance, SqlParameterCollection parameters)
				 * {
				 *		T real = (T) instance;
				 *		
				 *		real.A = Convert.ToInt32(parameters[0].Value);
				 *		
				 *		real.B = Convert.ToString(parameters[1].Value);
				 * }
				 */

				#region 根据属性设置SqlParameter输出值
				PropertyInfo[] properties = Reflector.GetProperties(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				MethodInfo getValueMethod = typeof(SqlParameter).GetProperty("Value").GetGetMethod();
				MethodInfo getItemMethod = typeof(SqlParameterCollection).GetProperty("Item", new Type[] { typeof(int) }).GetGetMethod();
				for (int i = 0, len = properties.Length; i < len; i++)
				{
					PropertyInfo property = properties[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, property);
					int paramIndex = parameters.IndexOf(parameterName);
					if (property.CanWrite && property.GetIndexParameters().Length == 0 && paramIndex != -1 && SqlTypeConvertor.IsConvertibleType(property.PropertyType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(property.PropertyType);
						SqlParameter sqlParam = parameters[paramIndex];
						if (sqlParam.Direction != ParameterDirection.Input)
						{
							generator.BeginExceptionBlock();
							if (instanceType.IsClass)
							{
								generator.Emit(OpCodes.Ldloc, real);
							}
							else
							{
								generator.Emit(OpCodes.Ldloca, real);
							}
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Callvirt, getItemMethod);
							generator.Emit(OpCodes.Callvirt, getValueMethod);
							generator.Emit(OpCodes.Call, convertMethod);

							MethodInfo setMethod = property.GetSetMethod(true);
							if (setMethod.DeclaringType == instanceType)
							{
								generator.Emit(OpCodes.Call, setMethod);
							}
							else
							{
								generator.Emit(OpCodes.Callvirt, setMethod);
							}
							generator.BeginCatchBlock(typeof(Exception));
							generator.Emit(OpCodes.Pop);
							generator.EndExceptionBlock();
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion

				#region 根据成员设置SqlParameter输出值
				FieldInfo[] fields = instanceType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				for (int i = 0, len = fields.Length; i < len; i++)
				{
					FieldInfo field = fields[i];
					string parameterName = "@" + SqlMapper.GetMemberMap(instanceType, field);
					int paramIndex = parameters.IndexOf(parameterName);
					if (paramIndex != -1 && SqlTypeConvertor.IsConvertibleType(field.FieldType) && usedNames.IndexOf(parameterName.ToLower()) == -1)
					{
						MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(field.FieldType);
						SqlParameter sqlParam = parameters[paramIndex];
						if (sqlParam.Direction != ParameterDirection.Input)
						{
							generator.BeginExceptionBlock();
							if (instanceType.IsClass)
							{
								generator.Emit(OpCodes.Ldloc, real);
							}
							else
							{
								generator.Emit(OpCodes.Ldloca, real);
							}
							generator.Emit(OpCodes.Ldarg_1);
							generator.Emit(OpCodes.Ldc_I4, paramIndex);
							generator.Emit(OpCodes.Callvirt, getItemMethod);
							generator.Emit(OpCodes.Callvirt, getValueMethod);
							generator.Emit(OpCodes.Call, convertMethod);

							generator.Emit(OpCodes.Stfld, field);
							generator.BeginCatchBlock(typeof(Exception));
							generator.Emit(OpCodes.Pop);
							generator.EndExceptionBlock();
							usedNames.Add(parameterName.ToLower());
						}
					}
				}
				#endregion


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}

			/// <summary>
			/// 创建一个动态方法,这个方法将SqlCommand的参数集合中的输出参数值重新绑定到所指定具备索引的实例之中
			/// </summary>
			/// <param name="parameters">SqlCommand的参数集合</param>
			/// <param name="instanceType">实例类型</param>
			/// <param name="indexedProperty">索引属性</param>
			/// <returns>所创建的动态方法</returns>
			internal static DynamicMethod GenerateRetrieveMethodForIndexed(SqlParameterCollection parameters, Type instanceType, PropertyInfo indexedProperty)
			{
				DynamicMethod method = new DynamicMethod("RetrieveParameters", null, new Type[] { typeof(object), typeof(SqlParameterCollection) }, true);
				ILGenerator generator = method.GetILGenerator();

				LocalBuilder real = generator.DeclareLocal(instanceType);
				generator.Emit(OpCodes.Ldarg_0);
				if (instanceType.IsClass)
				{
					generator.Emit(OpCodes.Castclass, instanceType);
				}
				else
				{
					generator.Emit(OpCodes.Unbox_Any, instanceType);
				}
				generator.Emit(OpCodes.Stloc, real);

				/*
				 * 所生成的代码示意
				 * public void GenerateParameters(object instance, SqlParameterCollection parameters)
				 * {
				 *		T real = (T) instance;
				 *		
				 *		real.A = Convert.ToInt32(parameters[0].Value);
				 *		
				 *		real.B = Convert.ToString(parameters[1].Value);
				 * }
				 */

				for (int i = 0; i < parameters.Count; i++)
				{
					if ((parameters[i].Direction & ParameterDirection.Output) == 0) continue;
					CommandAdapterHelper.GenerateIndexedPropertyRetrieveIL(generator, real, instanceType, indexedProperty, i, parameters[i].ParameterName);
				}


				generator.Emit(OpCodes.Ret);

				//生成委托
				return method;
			}
		}
	}
}
