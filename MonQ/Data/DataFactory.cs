using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.Concurrent;
using MonQ.Properties;

namespace MonQ.Data
{
	/// <summary>
	/// 数据工厂
	/// </summary>
	public static class DataFactory
	{
		internal delegate object InstanceCreationHandler();

		private static ConcurrentDictionary<Type, InstanceCreationHandler> creationHandlers = new ConcurrentDictionary<Type, InstanceCreationHandler>();

		/// <summary>
		/// 创建一个指定类型的实例
		/// </summary>
		/// <typeparam name="T">实例的类型</typeparam>
		/// <returns>
		/// 所创建的实例类型
		/// </returns>
		public static T CreateInstance<T>()
		{
			return DataFactory<T>.CreateInstance();
		}

		/// <summary>
		/// 创建一个指定类型的实例
		/// </summary>
		/// <param name="type">实例的类型</param>
		/// <returns>
		/// 所创建的实例类型
		/// </returns>
		public static object CreateInstance(Type type)
		{
			if (type == null) throw new ArgumentNullException("type");
			if (!type.IsPublic || type.IsSealed) throw new NotSupportedException(Resources.DataTypeFactory_NotSupportedType);
			return creationHandlers.GetOrAdd(type, (key) =>
			{
				DynamicMethod method = new DynamicMethod("CreateInstance", typeof(object), Type.EmptyTypes, true);
				ILGenerator generator = method.GetILGenerator();
				MethodInfo createMethod = typeof(DataFactory<object>).GetGenericTypeDefinition().MakeGenericType(type).GetMethod("CreateInstance", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
				generator.Emit(OpCodes.Call, createMethod);
				generator.Emit(OpCodes.Ret);
				return (InstanceCreationHandler)method.CreateDelegate(typeof(InstanceCreationHandler));
			})();
		}

	}

	/// <summary>
	/// 内部的数据工厂，真正实现对象实例的创建功能
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal static class DataFactory<T>
	{
		internal delegate T InstanceCreationHandler();

		private static Type instanceType;

		private static InstanceCreationHandler createInstance;

		static DataFactory()
		{
			Type baseType = typeof(T);
			if (baseType.IsAbstract || baseType.IsInterface)
			{
				instanceType = DataTypeFactory.CreateImplementedType(baseType);
			}
			else
			{
				instanceType = baseType;
			}

			DynamicMethod method = new DynamicMethod("CreateInstance", typeof(T), Type.EmptyTypes, true);
			ConstructorInfo[] constructors = instanceType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			ConstructorInfo constructor = null;
			ParameterInfo[] constructorParameters = null;

			ILGenerator generator = method.GetILGenerator();

			LocalBuilder instance = generator.DeclareLocal(instanceType);

			if (constructors.Length > 0 && instanceType.IsClass)
			{
				constructor = constructors[0];
				constructorParameters = constructor.GetParameters();
				#region 初始化构造所需的参数
				LocalBuilder[] arguments = new LocalBuilder[constructorParameters.Length];
				for (int i = 0, count = constructorParameters.Length; i < count; i++)
				{
					ParameterInfo parameter = constructorParameters[i];
					Type parameterType = parameter.ParameterType;

					LocalBuilder argument = generator.DeclareLocal(parameterType);

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
					arguments[i] = argument;
				}
				#endregion

				#region 进行构造
				if (instanceType.IsClass)
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
				#endregion
			}
			else if (!instanceType.IsClass && instanceType.IsValueType)
			{
				generator.Emit(OpCodes.Ldloca, instance);
				generator.Emit(OpCodes.Initobj, instanceType);
			}
			else
			{
				throw new NotSupportedException(string.Format(Resources.DataTypeFactory_NoSuitableConstructor, instanceType.FullName));
			}
			generator.Emit(OpCodes.Ldloc, instance);
			generator.Emit(OpCodes.Ret);
			createInstance = (InstanceCreationHandler)method.CreateDelegate(typeof(InstanceCreationHandler));
		}

		/// <summary>
		/// 创建一个指定类型的实例
		/// </summary>
		/// <returns>
		/// 所创建的实例类型
		/// </returns>
		public static T CreateInstance()
		{
			return createInstance();
		}
	}
}
