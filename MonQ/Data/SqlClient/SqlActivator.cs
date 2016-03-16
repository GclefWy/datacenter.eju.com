using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Data;
using MonQ.Properties;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using MonQ.Data.SqlClient.CommandAdapters;
using System.Collections;
using MonQ.Reflection;
using System.Data.Common;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// Sql数据催化器
	/// </summary>
	/// <remarks>
	/// SqlActivator用于将SQL语句执行器的查询结果记录集转换为指定类型的实例，概念上类似于.Net框架的Activator
	/// 在转换时即可以指定需要转换的类型，也可以不指定转换类型而使用CreateDynamic方法转换为SqlDynamicRecord实例。
	/// SqlDynamicRecord是一个用于描述查询结果的抽象类，SqlActivator根据每个SQL语句执行器查询结果的数据结构差异，为每个记录集生成一个SqlDynamicRecord的派生类，这个派生类的成员名称及数据类型与结果记录集的列一一对应。
	/// SqlActivator基于IDataReader接口进行来源数据读取操作，然而在进行操作时不会自动移动数据访问器的游标，因此在调用之前需要调用NextRecord()检测是否存在数据，同理，在访问下一个记录集时需要调用NextRecordSet()方法。之所以这么设计，是考虑到有些场景下面，可能开发人员既想使用当前的行封装一个类型为A的实例，同时又想使用当前行封装一个类型为B的实例
	/// 由于数据访问器是单向向后移动的，因此一旦移动到下一记录集或者下一行时，则不可对以前的数据进行操作，针对需要对数据遍历进行复杂操作时，建议使用IterateRecord或IterateRecordSet方法
	/// </remarks>
	public class SqlActivator : IDisposable, IEnumerable, IEnumerable<SqlRecord>
	{
		/// <summary>
		/// 与当前Sql数据催化器关联的数据访问器
		/// </summary>
		public DbDataReader Reader
		{
			get
			{
				return reader;
			}
		}

		#region 私有成员

		internal SqlReader reader;

		private string id;

		private SqlExecuter executer;

		private bool createNewType;

		#endregion

		/// <summary>
		/// 构造方法
		/// </summary>
		/// <param name="reader">数据访问器</param>
		/// <param name="executer">数据执行器</param>
		/// <param name="helper">Sql查询助手</param>
		/// <param name="id">ID</param>
		internal SqlActivator(SqlReader reader, SqlExecuter executer, string id)
		{
			if (reader == null) throw new ArgumentNullException("reader");

			this.id = id;

			this.executer = executer;

			this.reader = reader;

			createNewType = executer.commandAdapter is SqlTableAdapter || !string.IsNullOrEmpty(this.executer.InsertCommand) || !string.IsNullOrEmpty(this.executer.UpdateCommand) || !string.IsNullOrEmpty(this.executer.DeleteCommand);
		}


		/// <summary>
		/// 使数据访问器向前移动，根据数据访问器内容创建一个实例
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <returns>所创建的实例</returns>
		public EntityType CreateEntity<EntityType>()
		{
			Type instanceType = createNewType ? EntityHandlerPool<EntityType>.instanceType : EntityHandlerPool<EntityType>.originalType;
			return EntityHandlerPool<EntityType>.GetCreationHandler(id, reader.RecordSetIndex, reader, instanceType)(reader, InternalInsertHandler, InternalDeleteHandler, InternalUpdateHandler);
		}

		/// <summary>
		/// 使数据访问器向前移动，根据数据访问器内容创建一个实例，直到数据访问器到达记录集的末尾
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <returns>所创建的实例的列表</returns>
		public List<EntityType> CreateEntities<EntityType>()
		{
			Type instanceType = createNewType ? EntityHandlerPool<EntityType>.instanceType : EntityHandlerPool<EntityType>.originalType;
			List<EntityType> result = new List<EntityType>();
			EntityHandlerPool<EntityType>.InstanceCreationHandler handler = EntityHandlerPool<EntityType>.GetCreationHandler(id, reader.RecordSetIndex, reader, instanceType);
			while (reader.Read())
			{
				result.Add(handler(reader, InternalInsertHandler, InternalDeleteHandler, InternalUpdateHandler));
			}
			return result;
		}

		/// <summary>
		/// 将当前数据访问器的数据绑定到指定的对象实例
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <param name="target">目标实例</param>
		public void BindInstance<EntityType>(EntityType target)
		{
			BindInstance(ref target);
		}

		/// <summary>
		/// 将当前数据访问器的数据绑定到指定的对象实例
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <param name="target">目标实例</param>
		/// <remarks>
		/// 当EntityType为class时,此方法与BindInstance(EntityType target)皆可
		/// 当EntityType为struct时,应使用此方法以避免绑定后的数据丢失
		/// </remarks>
		public void BindInstance<EntityType>(ref EntityType target)
		{
			EntityHandlerPool<EntityType>.GetBindingHandler(id, reader.RecordSetIndex, reader)(reader, ref target);
		}

		/// <summary>
		/// 使数据访问器向前移动，逐次将当前数据访问器的内容绑定到指定的对象实例，在绑定完成后调用所传递的回调方法，直到数据访问器到达记录集的末尾
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <param name="callback">回调方法</param>
		/// <param name="target">目标实例</param>
		public void BindIterator<EntityType>(EntityType target, Action<EntityType> callback)
		{
			BindIterator(ref target, callback);
		}

		/// <summary>
		/// 使数据访问器向前移动，逐次将当前数据访问器的数据绑定到指定的对象实例，在绑定完成后调用所传递的回调方法
		/// </summary>
		/// <typeparam name="EntityType">目标数据类型</typeparam>
		/// <param name="target">目标实例</param>
		/// <param name="callback">每次绑定完成后的回调方法</param>
		/// <remarks>
		/// 当EntityType为class时,此方法与BindInstance(IDataReader reader, EntityType target)皆可
		/// 当EntityType为struct时,应使用此方法以避免绑定后的数据丢失
		/// </remarks>
		public void BindIterator<EntityType>(ref EntityType target, Action<EntityType> callback)
		{
			while (reader.Read())
			{
				EntityHandlerPool<EntityType>.GetBindingHandler(id, reader.RecordSetIndex, reader)(reader, ref target);
				callback(target);
			}
		}

		/// <summary>
		/// 从当前位置起每尝试移动数据访问游标成功一次，则调用一次回调委托一次，直到游标到达当前记录集末端为止
		/// </summary>
		/// <param name="callback">回调委托</param>
		public void IterateRecord(Action<SqlActivator> callback)
		{
			while (reader.Read())
			{
				callback(this);
			}
		}

		/// <summary>
		/// 从当前记录集位置开始调用一次回调委托，然后尝试移动到下一记录集，如若成功，则继续调用回调委托，直到下一记录集不存在为止
		/// </summary>
		/// <param name="callback">回调委托</param>
		public void IterateRecordSet(Action<SqlActivator> callback)
		{
			callback(this);
			while (reader.NextResult())
			{
				callback(this);
			}
		}

		/// <summary>
		/// 以当前记录集的信息结构创建一个类型,并且根据当前记录创建一个这个类型的实例
		/// </summary>
		/// <returns>
		/// 如果当前存在记录,返回所创建的实例,否则返回null
		/// </returns>
		public dynamic CreateDynamic()
		{
			object[] values = new object[reader.FieldCount];
			reader.GetValues(values);
			SqlRecord record = DynamicHandlerPool.CreateHandler(this.id, this.reader.RecordSetIndex, reader)();
			record.executer = this.executer;
			record.ItemArray = values;
			record.columnNames = new string[reader.FieldCount];
			for (int i = record.columnNames.Length - 1; i > -1; i--) record.columnNames[i] = reader.GetName(i);
			return record;
		}

		/// <summary>
		/// 以当前记录集的信息结构创建一个类型,并且根据当前记录集的内容创建一个这个类型的实例数组
		/// </summary>
		/// <returns>
		/// 如果当前存在记录,返回所创建的实例,否则返回null
		/// </returns>
		public List<dynamic> CreateDynamicList()
		{
			List<dynamic> result = new List<dynamic>();
			DynamicHandlerPool.DynamicCreationHandler handler = DynamicHandlerPool.CreateHandler(this.id, this.reader.RecordSetIndex, reader);
			string[] columnNames = new string[reader.FieldCount];
			for (int i = columnNames.Length - 1; i > -1; i--) columnNames[i] = reader.GetName(i);
			while (reader.Read())
			{
				object[] values = new object[reader.FieldCount];
				reader.GetValues(values);
				SqlRecord record = handler();
				record.ItemArray = values;
				record.executer = this.executer;
				record.columnNames = columnNames;
				result.Add(record);
			}
			return result;
		}

		/// <summary>
		/// 释放全部资源
		/// </summary>
		public void Close()
		{
			if (this.reader != null)
			{
				this.reader.Dispose();
				this.reader = null;
			}
		}

		/// <summary>
		/// 释放全部资源
		/// </summary>
		public void Dispose()
		{
			this.Close();
		}

		#region 内部方法

		//更新实体
		private int InternalUpdateHandler<EntityType>(EntityType instance, string updateCommand = null)
		{
			string defaultMapping = SqlMapper.GetEntityMap<EntityType>();
			updateCommand = updateCommand ?? executer.UpdateCommand ?? defaultMapping;
			bool empty = string.IsNullOrEmpty(updateCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(updateCommand);
				if (adapter is SqlTableAdapter)
				{
					updateCommand = (adapter as SqlTableAdapter).InitUpdateCommand(typeof(EntityType));
				}
				else if (updateCommand == defaultMapping)
				{
					throw new NotSupportedException(string.Format(Resources.SqlClient_NoUpdateCommand, typeof(EntityType).FullName));
				}
				return executer.helper.FromEntity(updateCommand, instance).ExecuteNonQuery();
			}
			else if (executer.commandAdapter is SqlTableAdapter)
			{
				updateCommand = (executer.commandAdapter as SqlTableAdapter).InitUpdateCommand(typeof(EntityType));
				return executer.helper.FromEntity(updateCommand, instance).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoUpdateCommand, typeof(EntityType).FullName));
		}
		//删除实体
		private int InternalDeleteHandler<EntityType>(EntityType instance, string deleteCommand = null)
		{
			string defaultMapping = SqlMapper.GetEntityMap<EntityType>();
			deleteCommand = deleteCommand ?? executer.DeleteCommand ?? defaultMapping;
			bool empty = string.IsNullOrEmpty(deleteCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(deleteCommand);
				if (adapter is SqlTableAdapter)
				{
					deleteCommand = (adapter as SqlTableAdapter).InitDeleteCommand(typeof(EntityType));
				}
				else if (deleteCommand == defaultMapping)
				{
					throw new NotSupportedException(string.Format(Resources.SqlClient_NoDeleteCommand, typeof(EntityType).FullName));
				}
				return executer.helper.FromEntity(deleteCommand, instance).ExecuteNonQuery();
			}
			else if (executer.commandAdapter is SqlTableAdapter)
			{
				deleteCommand = (executer.commandAdapter as SqlTableAdapter).InitDeleteCommand(typeof(EntityType));
				return executer.helper.FromEntity(deleteCommand, instance).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoDeleteCommand, typeof(EntityType).FullName));
		}
		//插入实体
		private int InternalInsertHandler<EntityType>(EntityType instance, string insertCommand = null)
		{
			string defaultMapping = SqlMapper.GetEntityMap<EntityType>();
			insertCommand = insertCommand ?? executer.UpdateCommand ?? defaultMapping;
			bool empty = string.IsNullOrEmpty(insertCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(insertCommand);
				if (adapter is SqlTableAdapter)
				{
					insertCommand = (adapter as SqlTableAdapter).InitInsertCommand(typeof(EntityType));
				}
				else if (insertCommand == defaultMapping)
				{
					throw new NotSupportedException(string.Format(Resources.SqlClient_NoInsertCommand, typeof(EntityType).FullName));
				}
				return executer.helper.FromEntity(insertCommand, instance).ExecuteNonQuery();
			}
			else if (executer.commandAdapter is SqlTableAdapter)
			{
				insertCommand = (executer.commandAdapter as SqlTableAdapter).InitInsertCommand(typeof(EntityType));
				return executer.helper.FromEntity(insertCommand, instance).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoInsertCommand, typeof(EntityType).FullName));
		}
		#endregion

		private static MethodInfo isDBNull = typeof(IDataRecord).GetMethod("IsDBNull");

		#region IEnumerable Members
		/// <summary>
		/// 返回一个为实现IEnumerable协议而设定的枚举器
		/// </summary>
		/// <returns>
		/// 枚举器，以此可以对此类型的实例进行foreach(object record in instance)操作
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion

		#region IEnumerable<SqlRecord> Members

		/// <summary>
		/// 返回一个为实现IEnumerable协议而设定的枚举器
		/// </summary>
		/// <returns>
		/// 枚举器，以此可以对此类型的实例进行foreach(SqlRecord record in instance)操作
		/// </returns>
		IEnumerator<SqlRecord> IEnumerable<SqlRecord>.GetEnumerator()
		{
			return new Enumerator(this);
		}

		#endregion


		/// <summary>
		/// 用于为SqlActivator生成创建或者绑定指定类型的实例所需的句柄而定义的类
		/// </summary>
		/// <typeparam name="EntityType">实例类型</typeparam>
		private static class EntityHandlerPool<EntityType>
		{
			/// <summary>
			/// 委托,用于实现创造对象实例
			/// </summary>
			/// <param name="reader">数据访问器</param>
			/// <param name="insertHandler">插入方法</param>
			/// <param name="deleteHandler">删除方法</param>
			/// <param name="updateHandler">更新方法</param>
			/// <returns>所创建的对象实例</returns>
			internal delegate EntityType InstanceCreationHandler(IDataReader reader, Func<EntityType, string, int> insertHandler, Func<EntityType, string, int> deleteHandler, Func<EntityType, string, int> updateHandler);

			/// <summary>
			/// 委托,用于实现绑定对象实例
			/// </summary>
			/// <param name="reader">数据访问器</param>
			/// <param name="target">对象实例</param>
			internal delegate void InstanceBindingHandler(IDataReader reader, ref EntityType target);

			private static ConcurrentDictionary<string, ConcurrentDictionary<int, InstanceBindingHandler>> bindingHandlers = new ConcurrentDictionary<string, ConcurrentDictionary<int, InstanceBindingHandler>>();
			private static ConcurrentDictionary<string, ConcurrentDictionary<int, InstanceCreationHandler>> creationHandlers = new ConcurrentDictionary<string, ConcurrentDictionary<int, InstanceCreationHandler>>();

			internal static Type instanceType;

			internal static Type originalType = null;

			private static string errorMessage = null;

			//标记是否实现IRecord接口
			private static bool isRecordInterface = false;

			static EntityHandlerPool()
			{
				InitInstanceType();

				if (originalType == null) originalType = instanceType;
			}

			//获取一个绑定EntityType的委托
			public static InstanceBindingHandler GetBindingHandler(string id, int index, IDataReader reader)
			{
				return bindingHandlers.GetOrAdd(id, (key) =>
				{
					return new ConcurrentDictionary<int, InstanceBindingHandler>();
				}).GetOrAdd(index, (key) =>
				{
					return GenerateBindingHandler(reader);
				});
			}

			//获取一个生成EntityType的委托
			public static InstanceCreationHandler GetCreationHandler(string id, int index, IDataReader reader, Type instanceType)
			{
				return creationHandlers.GetOrAdd(id, (key) =>
				{
					return new ConcurrentDictionary<int, InstanceCreationHandler>();
				}).GetOrAdd(index, (key) =>
				{
					return GenerateCreationHandler(reader, instanceType);
				});
			}


			#region 私有方法
			/// <summary>
			/// 准备一个专用于当前实例创建对象的句柄
			/// </summary>
			/// <param name="reader">数据访问器</param>
			private static InstanceCreationHandler GenerateCreationHandler(IDataReader reader, Type targetType)
			{
				DynamicMethod method = new DynamicMethod("CreateInstance", typeof(EntityType), new Type[] { typeof(IDataReader), typeof(Func<EntityType, string, int>), typeof(Func<EntityType, string, int>), typeof(Func<EntityType, string, int>) }, true);
				ILGenerator generator = method.GetILGenerator();
				if (targetType == null)
				{
					ConstructorInfo notSupportedException = typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) });

					if (errorMessage == null) errorMessage = string.Format(Resources.SqlClient_TypeNotSupported, typeof(EntityType).FullName);

					generator.Emit(OpCodes.Ldstr, errorMessage);
					generator.Emit(OpCodes.Newobj, notSupportedException);
					generator.Emit(OpCodes.Throw);
				}
				else
				{
					LocalBuilder instance = generator.DeclareLocal(targetType);

					List<string> usedNames = new List<string>();
					List<string> columnNames = new List<string>();
					for (int i = 0, len = reader.FieldCount; i < len; i++)
					{
						columnNames.Add(reader.GetName(i).ToLower());
					}
					MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);

					bool isClass = targetType.IsClass;
					bool instanceCreated = false;

					#region 构造实例
					ConstructorInfo[] constructors = targetType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					ConstructorInfo constructor = null;
					ParameterInfo[] constructorParameters = null;

					if (constructors.Length > 0)
					{
						#region 查找合适的构造方法
						for (int i = 0, count = constructors.Length; i < count; i++)
						{
							ConstructorInfo constructorInfo = constructors[i];
							ParameterInfo[] parameters = constructorInfo.GetParameters();
							bool match = true;
							for (int j = 0, countJ = parameters.Length; j < countJ; j++)
							{
								if (SqlTypeConvertor.GetConvertMethod(parameters[j].ParameterType) == null)
								{
									match = false;
									break;
								}
								if (columnNames.IndexOf(parameters[j].Name.ToLower()) == -1)
								{
									match = false;
									break;
								}
							}
							if (match)
							{
								if (constructor == null || parameters.Length > constructorParameters.Length)
								{
									constructorParameters = parameters;
									constructor = constructorInfo;
								}
							}
						}
						#endregion

						if (constructor == null)
						{
							constructor = constructors[0];
							constructorParameters = constructor.GetParameters();
						}
						//如果实现了IRecord接口，后面的三个参数不能初始化，而是从调用方法获取
						#region 初始化构造所需的参数
						LocalBuilder[] arguments = new LocalBuilder[constructorParameters.Length];
						for (int i = 0, count = constructorParameters.Length; i < count; i++)
						{
							ParameterInfo parameter = constructorParameters[i];
							Type parameterType = parameter.ParameterType;

							MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(parameterType);
							string fieldName = parameter.Name.ToLower();
							int index = columnNames.IndexOf(fieldName);
							LocalBuilder argument = generator.DeclareLocal(parameterType);
							if (convertMethod == null || index == -1)
							{
								if (parameterType.IsValueType)
								{
									generator.Emit(OpCodes.Ldloca, argument);
									generator.Emit(OpCodes.Initobj, parameterType);
								}
								else
								{
									generator.Emit(OpCodes.Ldnull);
									generator.Emit(OpCodes.Stloc, argument);
								}
							}
							else
							{
								Label endIf = generator.DefineLabel();
								Label elseIf = generator.DefineLabel();

								generator.Emit(OpCodes.Ldarg_0);
								generator.Emit(OpCodes.Ldc_I4, index);
								generator.Emit(OpCodes.Callvirt, isDBNull);
								generator.Emit(OpCodes.Ldc_I4, 1);
								generator.Emit(OpCodes.Ceq);
								generator.Emit(OpCodes.Brtrue, elseIf);

								generator.Emit(OpCodes.Ldarg_0);
								generator.Emit(OpCodes.Ldc_I4, index);
								generator.Emit(OpCodes.Callvirt, getValueMethod);
								generator.Emit(OpCodes.Call, convertMethod);
								generator.Emit(OpCodes.Stloc, argument);
								generator.Emit(OpCodes.Br, endIf);

								generator.MarkLabel(elseIf);

								if (parameterType.IsValueType)
								{
									generator.Emit(OpCodes.Ldloca, argument);
									generator.Emit(OpCodes.Initobj, parameterType);
								}
								else
								{
									generator.Emit(OpCodes.Ldnull);
									generator.Emit(OpCodes.Stloc, argument);
								}

								generator.MarkLabel(endIf);
							}
							arguments[i] = argument;
							usedNames.Add(fieldName);
						}
						#endregion

						#region 进行构造
						if (isClass)
						{
							for (int i = 0, count = constructorParameters.Length; i < count; i++)
							{
								generator.Emit(OpCodes.Ldloc, arguments[i]);
							}
							generator.Emit(OpCodes.Newobj, constructor);
							generator.Emit(OpCodes.Stloc, instance);
						}
						else
						{
							generator.Emit(OpCodes.Ldloca, instance);
							for (int i = 0, count = constructorParameters.Length; i < count; i++)
							{
								generator.Emit(OpCodes.Ldloc, arguments[i]);
							}
							generator.Emit(OpCodes.Call, constructor);
						}
						instanceCreated = true;
						#endregion
					}
					else if (!isClass)
					{
						generator.Emit(OpCodes.Ldloca, instance);
						generator.Emit(OpCodes.Initobj, targetType);
						instanceCreated = true;
					}
					else
					{
						throw new InvalidOperationException(string.Format(Resources.SqlClient_NoSuitableConstructor, targetType.FullName));
					}
					#endregion

					if (instanceCreated)
					{
						#region 设定属性
						PropertyInfo[] properties = Reflector.GetProperties(targetType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						for (int i = 0, count = properties.Length; i < count; i++)
						{
							PropertyInfo property = properties[i];
							string fieldName = SqlMapper.GetMemberMap(instanceType, property);
							int index = columnNames.IndexOf(fieldName.ToLower());
							if (property.CanWrite && index > -1 && usedNames.IndexOf(fieldName.ToLower()) == -1)
							{
								MethodInfo setMethod = property.GetSetMethod();
								if (setMethod.GetParameters().Length == 1)
								{
									MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(property.PropertyType);
									if (convertMethod != null)
									{
										Label endIf = generator.DefineLabel();

										generator.Emit(OpCodes.Ldarg_0);
										generator.Emit(OpCodes.Ldc_I4, index);
										generator.Emit(OpCodes.Callvirt, isDBNull);
										generator.Emit(OpCodes.Ldc_I4, 1);
										generator.Emit(OpCodes.Ceq);
										generator.Emit(OpCodes.Brtrue, endIf);

										if (isClass)
										{
											generator.Emit(OpCodes.Ldloc, instance);
										}
										else
										{
											generator.Emit(OpCodes.Ldloca, instance);
										}
										generator.Emit(OpCodes.Ldarg_0);
										generator.Emit(OpCodes.Ldc_I4, index);
										generator.Emit(OpCodes.Callvirt, getValueMethod);
										generator.Emit(OpCodes.Call, convertMethod);
										if (setMethod.DeclaringType == targetType)
										{
											generator.Emit(OpCodes.Call, setMethod);
										}
										else
										{
											generator.Emit(OpCodes.Callvirt, setMethod);
										}
										generator.MarkLabel(endIf);

										usedNames.Add(fieldName.ToLower());
									}
								}
							}
						}
						#endregion

						#region 设定成员
						FieldInfo[] fields = Reflector.GetFields(targetType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						for (int i = 0, count = fields.Length; i < count; i++)
						{
							FieldInfo field = fields[i];
							string fieldName = SqlMapper.GetMemberMap(instanceType, field);
							int index = columnNames.IndexOf(fieldName.ToLower());
							if (index > -1 && usedNames.IndexOf(fieldName.ToLower()) == -1)
							{
								MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(field.FieldType);
								if (convertMethod != null)
								{
									Label endIf = generator.DefineLabel();

									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldc_I4, index);
									generator.Emit(OpCodes.Callvirt, isDBNull);
									generator.Emit(OpCodes.Ldc_I4, 1);
									generator.Emit(OpCodes.Ceq);
									generator.Emit(OpCodes.Brtrue, endIf);

									if (isClass)
									{
										generator.Emit(OpCodes.Ldloc, instance);
									}
									else
									{
										generator.Emit(OpCodes.Ldloca, instance);
									}

									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldc_I4, index);
									generator.Emit(OpCodes.Callvirt, getValueMethod);
									generator.Emit(OpCodes.Call, convertMethod);
									generator.Emit(OpCodes.Stfld, field);

									generator.MarkLabel(endIf);


									usedNames.Add(fieldName.ToLower());
								}
							}
						}
						#endregion


						if (isRecordInterface && targetType == instanceType)
						{
							if (isClass)
							{
								generator.Emit(OpCodes.Ldloc, instance);
							}
							else
							{
								generator.Emit(OpCodes.Ldloca, instance);
							}
							generator.Emit(OpCodes.Ldarg, 1);
							generator.Emit(OpCodes.Stfld, targetType.GetField("!insertHandler"));


							if (isClass)
							{
								generator.Emit(OpCodes.Ldloc, instance);
							}
							else
							{
								generator.Emit(OpCodes.Ldloca, instance);
							}
							generator.Emit(OpCodes.Ldarg, 2);
							generator.Emit(OpCodes.Stfld, targetType.GetField("!deleteHandler"));

							if (isClass)
							{
								generator.Emit(OpCodes.Ldloc, instance);
							}
							else
							{
								generator.Emit(OpCodes.Ldloca, instance);
							}
							generator.Emit(OpCodes.Ldarg, 3);
							generator.Emit(OpCodes.Stfld, targetType.GetField("!updateHandler"));
						}
					}


					generator.Emit(OpCodes.Ldloc, instance);
				}
				generator.Emit(OpCodes.Ret);

				return (InstanceCreationHandler)method.CreateDelegate(typeof(InstanceCreationHandler));
			}

			/// <summary>
			/// 准备一个专用于当前实例绑定对象的句柄
			/// </summary>
			/// <param name="reader">数据访问器</param>
			private static InstanceBindingHandler GenerateBindingHandler(IDataReader reader)
			{
				Type instanceType = typeof(EntityType);

				ConstructorInfo argumentNullException = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) });
				ConstructorInfo notSupportedException = typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) });

				DynamicMethod method = new DynamicMethod("BindInstance", null, new Type[] { typeof(IDataReader), typeof(EntityType).MakeByRefType() }, true);
				ILGenerator generator = method.GetILGenerator();


				bool isClass = instanceType.IsClass;

				#region 类型检查
				if (instanceType.IsValueType && (SqlTypeConvertor.IsConvertibleType(instanceType)))
				{
					generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_ValueTypeNotSupported, instanceType.FullName));
					generator.Emit(OpCodes.Newobj, notSupportedException);
					generator.Emit(OpCodes.Throw);
				}
				else if (instanceType.IsArray)
				{
					generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_ArrayTypeNotSupported, instanceType.FullName));
					generator.Emit(OpCodes.Newobj, notSupportedException);
					generator.Emit(OpCodes.Throw);
				}
				else if (instanceType.IsEnum)
				{
					generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_EnumTypeNotSupported, instanceType.FullName));
					generator.Emit(OpCodes.Newobj, notSupportedException);
					generator.Emit(OpCodes.Throw);
				}
				else if (instanceType.IsCOMObject)
				{
					generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_COMObjectNotSupported, instanceType.FullName));
					generator.Emit(OpCodes.Newobj, notSupportedException);
					generator.Emit(OpCodes.Throw);
				}
				#endregion
				else
				{
					List<string> usedNames = new List<string>();
					List<string> columnNames = new List<string>();
					for (int i = 0, len = reader.FieldCount; i < len; i++)
					{
						columnNames.Add(reader.GetName(i).ToLower());
					}

					MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(int) }, null);

					#region 设定属性
					PropertyInfo[] properties = Reflector.GetProperties(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					for (int i = 0, count = properties.Length; i < count; i++)
					{
						PropertyInfo property = properties[i];
						string fieldName = SqlMapper.GetMemberMap(instanceType, property);
						int index = columnNames.IndexOf(fieldName.ToLower());
						if (property.CanWrite && index > -1 && usedNames.IndexOf(fieldName.ToLower()) == -1)
						{
							MethodInfo setMethod = property.GetSetMethod();
							if (setMethod.GetParameters().Length == 1)
							{
								MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(property.PropertyType);
								if (convertMethod != null)
								{
									Label endIf = generator.DefineLabel();

									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldc_I4, index);
									generator.Emit(OpCodes.Callvirt, isDBNull);
									generator.Emit(OpCodes.Ldc_I4, 1);
									generator.Emit(OpCodes.Ceq);
									generator.Emit(OpCodes.Brtrue, endIf);


									generator.Emit(OpCodes.Ldarg_1);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldc_I4, index);
									generator.Emit(OpCodes.Callvirt, getValueMethod);
									generator.Emit(OpCodes.Call, convertMethod);
									if (setMethod.DeclaringType == instanceType)
									{
										generator.Emit(OpCodes.Call, setMethod);
									}
									else
									{
										generator.Emit(OpCodes.Callvirt, setMethod);
									}

									generator.MarkLabel(endIf);

									usedNames.Add(fieldName.ToLower());
								}
							}
						}
					}
					#endregion

					#region 设定成员
					FieldInfo[] fields = Reflector.GetFields(instanceType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
					for (int i = 0, count = fields.Length; i < count; i++)
					{
						FieldInfo field = fields[i];
						string fieldName = SqlMapper.GetMemberMap(instanceType, field);
						int index = columnNames.IndexOf(fieldName.ToLower());
						if (index > -1 && usedNames.IndexOf(fieldName.ToLower()) == -1)
						{
							MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(field.FieldType);
							if (convertMethod != null)
							{
								Label endIf = generator.DefineLabel();

								generator.Emit(OpCodes.Ldarg_0);
								generator.Emit(OpCodes.Ldc_I4, index);
								generator.Emit(OpCodes.Callvirt, isDBNull);
								generator.Emit(OpCodes.Ldc_I4, 1);
								generator.Emit(OpCodes.Ceq);
								generator.Emit(OpCodes.Brtrue, endIf);


								generator.Emit(OpCodes.Ldarg_1);
								generator.Emit(OpCodes.Ldarg_0);
								generator.Emit(OpCodes.Ldc_I4, index);
								generator.Emit(OpCodes.Callvirt, getValueMethod);
								generator.Emit(OpCodes.Call, convertMethod);
								generator.Emit(OpCodes.Stfld, field);

								generator.MarkLabel(endIf);


								usedNames.Add(fieldName.ToLower());
							}
						}
					}
					#endregion

				}
				generator.Emit(OpCodes.Ret);

				return (InstanceBindingHandler)method.CreateDelegate(typeof(InstanceBindingHandler));
			}


			/// <summary>
			/// 准备一个合适的类型以便进行以便在需要创建对象时进行操作
			/// </summary>
			private static void InitInstanceType()
			{
				Type baseType = typeof(EntityType);
				if (!baseType.IsPublic && baseType.IsAbstract)
				{
					errorMessage = string.Format(Resources.SqlClient_TypeMustBePublic, baseType.FullName);
					return;
				}
				else if (baseType.IsArray)
				{
					errorMessage = string.Format(Resources.SqlClient_ArrayTypeNotSupported, baseType.FullName);
					return;
				}
				else if (baseType.IsEnum)
				{
					errorMessage = string.Format(Resources.SqlClient_EnumTypeNotSupported, baseType.FullName);
					return;
				}
				else if (baseType.IsSealed && (baseType.IsAbstract || baseType.IsInterface))
				{
					errorMessage = string.Format(Resources.SqlClient_SealedTypeNotSupported, baseType.FullName);
					return;
				}
				else if (baseType.IsCOMObject)
				{
					errorMessage = string.Format(Resources.SqlClient_SealedTypeNotSupported, baseType.FullName);
					return;
				}
				if (!baseType.IsArray && !baseType.IsCOMObject && !baseType.IsEnum && !baseType.IsInterface)
				{
					if (baseType.IsClass)
					{
						if (!baseType.IsAbstract)
						{
							if (baseType.IsPublic)
							{
								try
								{
									//尝试派生一个类型，如果无法派生，则使用现有类型
									instanceType = CreateInstanceType(baseType, baseType, null);
									originalType = baseType;
								}
								catch
								{
#if DEBUG
									throw;
#endif
									instanceType = baseType;
								}
							}
							else
							{
								instanceType = baseType;
							}
						}
						else if (!baseType.IsSealed)
						{
							instanceType = CreateInstanceType(baseType, baseType, null);
						}
					}
					else if (baseType.IsValueType && !baseType.IsPrimitive && !string.IsNullOrEmpty(baseType.Namespace))
					{
						instanceType = baseType;
					}
				}
				else if (baseType.IsInterface)
				{
					instanceType = CreateInstanceType(baseType, typeof(object), baseType);
				}
			}

			/// <summary>
			/// 如果T是一个抽象类或者接口,创建一个新的,从T派生的类型以便进行数据绑定
			/// </summary>
			/// <param name="baseType">基础类型</param>
			/// <param name="parentType">父类型</param>
			/// <param name="interfaceType">接口类型</param>
			/// <returns>
			/// 一个派生自parent且实现了interfaceType的动态类型
			/// </returns>
			private static Type CreateInstanceType(Type baseType, Type parentType, Type interfaceType)
			{
				if (!baseType.IsPublic)
				{
					return null;
				}
				try
				{
					string typeName = baseType.Name;
					AssemblyName assemblyName = baseType.Assembly.GetName();//new AssemblyName("SqlActivator.Assembly." + typeName);
					AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
					ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
					TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, parentType, interfaceType == null ? new Type[] { typeof(ISqlRecord) } : new Type[] { interfaceType, typeof(ISqlRecord) });


					FieldBuilder insertHandler = typeBuilder.DefineField("!insertHandler", typeof(Func<EntityType, string, int>), FieldAttributes.Public);
					FieldBuilder deleteHandler = typeBuilder.DefineField("!deleteHandler", typeof(Func<EntityType, string, int>), FieldAttributes.Public);
					FieldBuilder updateHandler = typeBuilder.DefineField("!updateHandler", typeof(Func<EntityType, string, int>), FieldAttributes.Public);

					MethodInfo invokeMethod = typeof(Func<EntityType, string, int>).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(EntityType), typeof(string) }, null);

					#region 定义构造方法
					ConstructorBuilder constructorBuilder;
					ILGenerator generator;
					ConstructorInfo[] constructors = parentType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					if (parentType == typeof(object) || constructors.Length == 0)
					{
						/*
						 * public T()
						 * {
						 * }
						 */
						constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(Func<EntityType, string, int>), typeof(Func<EntityType, string, int>), typeof(Func<EntityType, string, int>) });
						constructorBuilder.DefineParameter(0, ParameterAttributes.In, "!insertHandler");
						constructorBuilder.DefineParameter(1, ParameterAttributes.In, "!deleteHandler");
						constructorBuilder.DefineParameter(2, ParameterAttributes.In, "!updateHandler");
						generator = constructorBuilder.GetILGenerator();
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Call, parentType.GetConstructor(Type.EmptyTypes));

						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Ldarg_1);
						generator.Emit(OpCodes.Stfld, insertHandler);

						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Ldarg_2);
						generator.Emit(OpCodes.Stfld, deleteHandler);

						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Ldarg_3);
						generator.Emit(OpCodes.Stfld, updateHandler);
						generator.Emit(OpCodes.Ret);
					}
					else
					{
						for (int i = 0, count = constructors.Length; i < count; i++)
						{
							ConstructorInfo constructor = constructors[i];
							ParameterInfo[] parameterInfo = constructor.GetParameters();
							Type[] parameterTypes = new Type[parameterInfo.Length];
							for (int j = 0, countJ = parameterTypes.Length; j < countJ; j++)
							{
								parameterTypes[j] = parameterInfo[j].ParameterType;
							}

							/*
							 * public T(byte a, int b, long c)
							 *		: base(a, b, c)
							 * {
							 * }
							 */
							constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
							for (int j = 0, countJ = parameterTypes.Length; j < countJ; j++)
							{
								constructorBuilder.DefineParameter(j + 1, parameterInfo[j].Attributes, parameterInfo[j].Name);
							}

							generator = constructorBuilder.GetILGenerator();
							generator.Emit(OpCodes.Ldarg_0);
							for (int j = 0, countJ = parameterTypes.Length; j < countJ; j++)
							{
								generator.Emit(OpCodes.Ldarg, j + 1);
							}
							generator.Emit(OpCodes.Call, constructor);

							generator.Emit(OpCodes.Ret);
						}
					}
					#endregion

					Type superType = interfaceType == null ? parentType : interfaceType;

					MethodBuilder methodBuilder;
					MethodInfo method;

					#region ISqlRecord实现


					MethodInfo[] methods = typeof(ISqlRecord).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					for (int i = 0, count = methods.Length; i < count; i++)
					{
						method = methods[i];
						if (method != null)
						{
							#region 收集方法的参数信息
							ParameterInfo[] parameters = method.GetParameters();
							List<Type> parameterTypes = new List<Type>();
							for (int j = 0, countJ = parameters.Length; j < countJ; j++)
							{
								parameterTypes.Add(parameters[j].ParameterType);

							}
							#endregion

							methodBuilder = typeBuilder.DefineMethod(method.Name, method.Attributes ^ MethodAttributes.Abstract);

							/*
							 * 由于之前已经定义过与父类名称一致的泛型参数,所以我们可以大胆的使用父类的参数定义
							 * 否则你还需要重新将你自己定义的参数类型与父类的泛型参数类型一一对应,相当麻烦
							 */
							methodBuilder.SetParameters(parameterTypes.ToArray());
							methodBuilder.SetReturnType(method.ReturnType);
							for (int j = 0, countJ = parameters.Length; j < countJ; j++)
							{
								methodBuilder.DefineParameter(j, parameters[j].Attributes, parameters[j].Name);
							}
							generator = methodBuilder.GetILGenerator();

							Label exception = generator.DefineLabel();

							switch (method.Name)
							{
								case "Insert":
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, insertHandler);
									generator.Emit(OpCodes.Ldnull);
									generator.Emit(OpCodes.Ceq);
									generator.Emit(OpCodes.Brtrue, exception);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, insertHandler);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldarg_1);
									generator.Emit(OpCodes.Callvirt, invokeMethod);
									generator.Emit(OpCodes.Ret);
									generator.MarkLabel(exception);
									generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_NoInsertCommand, baseType.FullName));
									generator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
									generator.Emit(OpCodes.Throw);
									break;
								case "Update":
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, updateHandler);
									generator.Emit(OpCodes.Ldnull);
									generator.Emit(OpCodes.Ceq);
									generator.Emit(OpCodes.Brtrue, exception);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, updateHandler);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldarg_1);
									generator.Emit(OpCodes.Callvirt, invokeMethod);
									generator.Emit(OpCodes.Ret);
									generator.MarkLabel(exception);
									generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_NoUpdateCommand, baseType.FullName));
									generator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
									generator.Emit(OpCodes.Throw);
									break;
								case "Delete":
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, deleteHandler);
									generator.Emit(OpCodes.Ldnull);
									generator.Emit(OpCodes.Ceq);
									generator.Emit(OpCodes.Brtrue, exception);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldfld, deleteHandler);
									generator.Emit(OpCodes.Ldarg_0);
									generator.Emit(OpCodes.Ldarg_1);
									generator.Emit(OpCodes.Callvirt, invokeMethod);
									generator.Emit(OpCodes.Ret);
									generator.MarkLabel(exception);
									generator.Emit(OpCodes.Ldstr, string.Format(Resources.SqlClient_NoDeleteCommand, baseType.FullName));
									generator.Emit(OpCodes.Newobj, typeof(NotSupportedException).GetConstructor(new Type[] { typeof(string) }));
									generator.Emit(OpCodes.Throw);
									break;
								default:
									generator.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
									generator.Emit(OpCodes.Throw);
									break;
							}

							typeBuilder.DefineMethodOverride(methodBuilder, method);
						}
					}

					#endregion

					try
					{
						DataTypeFactory.ImplementMembers(typeBuilder, parentType);
						if (instanceType != null)
						{
							DataTypeFactory.ImplementMembers(typeBuilder, interfaceType);
						}
					}
					catch (Exception e)
					{
						errorMessage = e.Message;
						return null;
					}
					try
					{
						return typeBuilder.CreateType();
					}
					finally
					{
						isRecordInterface = true;
					}
				}
				catch (Exception e)
				{
					errorMessage = string.Format(Resources.SqlClient_CreateDerivedTypeError, baseType.FullName, e.Message);
					return null;
				}
			}
			#endregion

		}

		/// <summary>
		/// 用于为SqlActivator生成创建或者绑定SqlDynamicRecord类型的实例所需的句柄而定义的类
		/// </summary>
		private static class DynamicHandlerPool
		{
			/// <summary>
			/// 委托,用于实现创造对象实例
			/// </summary>
			/// <param name="values">访问器值集合</param>
			/// <returns>所创建的对象实例</returns>
			internal delegate SqlRecord DynamicCreationHandler();
			private static int handlersCount = 0;

			private static ConcurrentDictionary<string, ConcurrentDictionary<int, DynamicCreationHandler>> handlers = new ConcurrentDictionary<string, ConcurrentDictionary<int, DynamicCreationHandler>>();

			/// <summary>
			/// 生成一个实例创建句柄
			/// </summary>
			/// <param name="id">数据库查询助手ID</param>
			/// <param name="index">数据集ID</param>
			/// <param name="reader">数据访问器</param>
			/// <returns>
			/// 所生成的句柄
			/// </returns>
			internal static DynamicCreationHandler CreateHandler(string id, int index, IDataReader reader)
			{
				return handlers.GetOrAdd(id, (key) =>
				{
					return new ConcurrentDictionary<int, DynamicCreationHandler>();
				}).GetOrAdd(index, (key) =>
				{
					Type instanceType = null;
					ILGenerator generator;
					Type sqlRecordType = typeof(SqlRecord);
					string errorMessage = null;
					try
					{
						#region 先创建一个合适的类型
						string typeName = "SqlDyanmicRecord" + (++handlersCount);
						AssemblyName assemblyName = sqlRecordType.Assembly.GetName();//new AssemblyName("SqlActivator.Assembly." + typeName);
						AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
						ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
						TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public, sqlRecordType, Type.EmptyTypes);

						typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes), new object[] { }));

						ConstructorInfo parentConstructor = typeof(SqlRecord).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance, null, new Type[] { }, null);
						ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { });
						generator = constructor.GetILGenerator();
						generator.Emit(OpCodes.Ldarg_0);
						generator.Emit(OpCodes.Call, parentConstructor);
						generator.Emit(OpCodes.Ret);

						PropertyBuilder propertyBuilder;
						MethodBuilder methodBuilder;
						ParameterInfo[] parameters;
						FieldInfo itemArray = sqlRecordType.GetField("itemArray", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
						MethodInfo noticeChange = sqlRecordType.GetMethod("NoticeChange", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

						//#region 定义this属性重载
						PropertyInfo item = sqlRecordType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, typeof(object), new Type[] { typeof(string) }, null);



						parameters = item.GetIndexParameters();



						#region 实现各个字段对应属性
						List<string> usedNames = new List<string>();

						List<string> keywords = new List<string>(new string[] { "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "out", "override", "params", "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while", "add", "alias", "ascending", "descending", "dynamic", "from", "get", "global", "group", "into", "join", "let", "orderby", "partial", "partial", "remove", "select", "set", "value", "var", "where", "where", "yield" });
						usedNames.Add("SqlDynamicRecord");
						usedNames.AddRange(keywords);
						FieldInfo[] fields = sqlRecordType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
						for (int i = 0, count = fields.Length; i < count; i++)
						{
							usedNames.Add(fields[i].Name);
						}
						PropertyInfo[] properties = sqlRecordType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
						for (int i = 0, count = properties.Length; i < count; i++)
						{
							usedNames.Add(properties[i].Name);
						}

						MethodInfo[] methods = sqlRecordType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
						for (int i = 0, count = methods.Length; i < count; i++)
						{
							usedNames.Add(methods[i].Name);
						}
						List<string> columnNames = new List<string>();
						for (int i = 0, count = reader.FieldCount; i < count; i++)
						{
							columnNames.Add(reader.GetName(i));

						}
						for (int i = 0, count = reader.FieldCount; i < count; i++)
						{
							string propertyName = Regex.Replace(columnNames[i], @"\s", "", RegexOptions.Compiled);
							propertyName = Regex.Replace(propertyName, @"[^\w]", "_", RegexOptions.Compiled);
							if (keywords.IndexOf(propertyName) > -1)
							{
								propertyName = propertyName[0].ToString().ToUpper() + propertyName.Substring(1);
							}
							if (Regex.IsMatch(propertyName, @"^\d", RegexOptions.Compiled | RegexOptions.IgnoreCase))
							{
								propertyName = "_" + propertyName;
							}
							if (string.IsNullOrEmpty(propertyName)) propertyName = "_" + i;
							#region 生成一个唯一的名称
							if (usedNames.IndexOf(propertyName) > -1)
							{
								if (usedNames.IndexOf(propertyName[0].ToString().ToUpper() + propertyName.Substring(1)) == -1)
								{
									propertyName = propertyName[0].ToString().ToUpper() + propertyName.Substring(1);
								}
								if (usedNames.IndexOf(propertyName[0].ToString().ToLower() + propertyName.Substring(1)) == -1)
								{
									propertyName = propertyName[0].ToString().ToLower() + propertyName.Substring(1);
								}
							}
							if (usedNames.IndexOf(propertyName) > -1)
							{
								int p = 2;
								while (usedNames.IndexOf(propertyName + p) > -1 || columnNames.IndexOf(propertyName + p) > -1)
								{
									p++;
								}
								propertyName = propertyName + p;
								columnNames[i] = propertyName;
							}
							#endregion
							usedNames.Add(propertyName);



							Type propertyType = reader.GetFieldType(i);

							propertyBuilder = typeBuilder.DefineProperty(propertyName, System.Reflection.PropertyAttributes.None, propertyType, Type.EmptyTypes);

							methodBuilder = typeBuilder.DefineMethod("get" + propertyName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, propertyType, Type.EmptyTypes);
							methodBuilder.DefineParameter(0, ParameterAttributes.None, parameters[0].Name);

							generator = methodBuilder.GetILGenerator();

							LocalBuilder result = generator.DeclareLocal(propertyType);
							Label finish = generator.DefineLabel();

							generator.BeginExceptionBlock();

							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldfld, itemArray);
							generator.Emit(OpCodes.Ldc_I4, i);
							generator.Emit(OpCodes.Ldelem_Ref);

							MethodInfo convertMethod = SqlTypeConvertor.GetConvertMethod(propertyType);
							if (convertMethod != null)
							{
								generator.Emit(OpCodes.Call, convertMethod);
							}
							else
							{
								if (propertyType.IsValueType)
								{
									generator.Emit(OpCodes.Unbox_Any, propertyType);
								}
								else
								{
									generator.Emit(OpCodes.Isinst, propertyType);
								}
							}
							generator.Emit(OpCodes.Stloc, result);
							generator.Emit(OpCodes.Leave_S, finish);

							generator.BeginCatchBlock(typeof(object));
							generator.Emit(OpCodes.Pop);
							generator.EndExceptionBlock();

							if (propertyType.IsValueType)
							{
								generator.Emit(OpCodes.Ldloca, result);
								generator.Emit(OpCodes.Initobj, propertyType);
								generator.Emit(OpCodes.Leave_S, finish);
							}
							else
							{
								generator.Emit(OpCodes.Ldnull);
								generator.Emit(OpCodes.Stloc, result);
								generator.Emit(OpCodes.Leave_S, finish);
							}
							generator.MarkLabel(finish);

							generator.Emit(OpCodes.Ldloc, result);
							generator.Emit(OpCodes.Ret);

							propertyBuilder.SetGetMethod(methodBuilder);


							methodBuilder = typeBuilder.DefineMethod("get" + propertyName, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName, null, new Type[] { propertyType });
							methodBuilder.DefineParameter(0, ParameterAttributes.None, parameters[0].Name);

							generator = methodBuilder.GetILGenerator();

							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldfld, itemArray);
							generator.Emit(OpCodes.Ldc_I4, i);
							generator.Emit(OpCodes.Ldarg_1);
							if (propertyType.IsValueType)
							{
								generator.Emit(OpCodes.Box, propertyType);
							}
							generator.Emit(OpCodes.Stelem_Ref);

							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldstr, reader.GetName(i));
							generator.Emit(OpCodes.Callvirt, noticeChange);

							generator.Emit(OpCodes.Ret);

							propertyBuilder.SetSetMethod(methodBuilder);
						}
						#endregion

						instanceType = typeBuilder.CreateType();

						#endregion
					}
					catch (Exception e)
					{
						errorMessage = string.Format(Resources.SqlClient_CreateDynamicTypeError, e.Message);
					}
					#region 创建一个方法，用于生成刚才所创建的类型的实例
					DynamicMethod dynamicMethod = new DynamicMethod("CreateInstance", sqlRecordType, Type.EmptyTypes, true);
					generator = dynamicMethod.GetILGenerator();
					if (instanceType == null)
					{
						if (errorMessage == null) string.Format(Resources.SqlClient_CreateDynamicTypeError, "未知的错误");
						generator.Emit(OpCodes.Ldstr, errorMessage);
						generator.Emit(OpCodes.Newobj, typeof(Exception).GetConstructor(new Type[] { typeof(string) }));
						generator.Emit(OpCodes.Throw);
					}
					else
					{
						ConstructorInfo constructor = instanceType.GetConstructor(Type.EmptyTypes);
						generator.Emit(OpCodes.Newobj, constructor);
						generator.Emit(OpCodes.Ret);
					}
					#endregion

					return (DynamicCreationHandler)dynamicMethod.CreateDelegate(typeof(DynamicCreationHandler));
				});
			}
		}

		internal class Enumerator : IEnumerator, IEnumerator<SqlRecord>
		{
			private SqlActivator activator;
			public Enumerator(SqlActivator activator)
			{
				this.activator = activator;
			}


			#region IDisposable Members

			public void Dispose()
			{
				this.activator = null;
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public bool MoveNext()
			{
				return activator.reader.Read();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			#endregion

			#region IEnumerator<SqlRecord> Members

			public SqlRecord Current
			{
				get
				{
					if (activator.reader.Read())
					{
						return activator.CreateDynamic();
					}
					return null;
				}
			}

			#endregion
		}

	}

	/// <summary>
	/// Sql数据催化器，实现了特定的枚举类型，使得开发人员可以调用foreach语法进行处理
	/// </summary>
	/// <typeparam name="EntityType">
	/// 实体类型
	/// </typeparam>
	public class SqlActivator<EntityType> : SqlActivator, IEnumerable<EntityType>
	{
		private static Func<SqlActivator<EntityType>, IEnumerator<EntityType>> createEnumeratorHandler;

		/// <summary>
		/// 使用指定的数据访问器构造一个SqlActivator实例
		/// </summary>
		/// <param name="reader">数据访问器</param>
		/// <param name="executer">SQL执行器</param>
		internal SqlActivator(SqlReader reader, SqlExecuter executer, string id)
			: base(reader, executer, id)
		{
		}

		static SqlActivator()
		{
			if (typeof(EntityType) == typeof(SqlRecord))
			{
				createEnumeratorHandler = (activator) =>
				{
					return new SqlActivator.Enumerator(activator) as IEnumerator<EntityType>;
				};
			}
			else
			{
				createEnumeratorHandler = (activator) =>
				{
					return new SqlActivator<EntityType>.Enumerator(activator);
				};
			}
		}

		#region 公共方法
		/// <summary>
		/// 使数据访问器向前移动，根据数据访问器内容创建一个实例
		/// </summary>
		/// <returns>所创建的实例</returns>
		public EntityType CreateEntity()
		{
			return base.CreateEntity<EntityType>();
		}
		#endregion

		#region IEnumerable<EntityType> Members

		IEnumerator<EntityType> IEnumerable<EntityType>.GetEnumerator()
		{
			return createEnumeratorHandler(this);
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return createEnumeratorHandler(this);
		}

		#endregion

		private new class Enumerator : IEnumerator<EntityType>
		{
			private SqlActivator activator;
			public Enumerator(SqlActivator activator)
			{
				this.activator = activator;
			}

			#region IEnumerator<EntityType> Members

			public EntityType Current
			{
				get
				{
					if (activator.Reader.Read())
					{
						return activator.CreateEntity<EntityType>();
					}
					return default(EntityType);
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose()
			{
				this.activator = null;
			}

			#endregion

			#region IEnumerator Members

			object System.Collections.IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public bool MoveNext()
			{
				return this.activator.Reader.Read();
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			#endregion
		}
	}
}
