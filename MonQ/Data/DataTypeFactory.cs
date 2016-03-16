using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using MonQ.Properties;
using MonQ.Reflection;

namespace MonQ.Data
{
	/// <summary>
	/// 动态类型工厂
	/// 动态类型工厂可以根据所提供的接口类型或者抽象类型动态的创建一个继承该类型的新类型。
	/// </summary>
	/// <remarks>
	/// 在使用这个类时要求所提供的类型所有的成员是公开的。
	/// </remarks>
	public class DataTypeFactory
	{
		private static AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
		private static AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
		private static ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");

		internal DataTypeFactory()
		{
		}

		/// <summary>
		/// 创建一个从基类型或者接口类型派生的类型
		/// </summary>
		/// <param name="baseType">基类型或者接口类型</param>
		/// <returns></returns>
		public static Type CreateImplementedType(Type baseType)
		{
			if (!baseType.IsPublic || baseType.IsSealed)
			{
				throw new Exception(Resources.DataTypeFactory_NotSupportedType);
			}
			try
			{
				Type parent = baseType.IsInterface ? typeof(object) : baseType;
				Type[] interfaces = baseType.IsInterface ? new Type[] { baseType } : Type.EmptyTypes;

				TypeBuilder typeBuilder = moduleBuilder.DefineType(baseType.Name, TypeAttributes.Public, parent, interfaces);

				#region 定义构造方法
				ConstructorBuilder constructorBuilder;
				ILGenerator generator;
				ConstructorInfo[] constructors = parent.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (parent == typeof(object) || constructors.Length == 0)
				{
					/*
					 * public T()
					 * {
					 * }
					 */
					constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
					generator = constructorBuilder.GetILGenerator();
					generator.Emit(OpCodes.Ldarg_0);
					generator.Emit(OpCodes.Call, parent.GetConstructor(Type.EmptyTypes));
					generator.Emit(OpCodes.Ret);
				}
				else
				{
					for (int i = 0, count = constructors.Length; i < count; i++)
					{
						ConstructorInfo constructor = constructors[i];
						ParameterInfo[] parameterInfo = constructor.GetParameters();
						Type[] parameterTypes = new Type[parameterInfo.Length];
						Type[] constructorParameterTypes = new Type[parameterInfo.Length];
						for (int j = 0, countJ = parameterTypes.Length; j < countJ; j++)
						{
							parameterTypes[j] = parameterInfo[j].ParameterType;
							constructorParameterTypes[j] = parameterInfo[j].ParameterType;
						}

						/*
						 * public T(byte a, int b, long c)
						 *		: base(a, b, c)
						 * {
						 * }
						 */
						constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes);
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

				ImplementMembers(typeBuilder, baseType);

				return typeBuilder.CreateType();
			}
			catch (Exception e)
			{
				throw new NotSupportedException(string.Format(Resources.DataTypeFactory_CreateDerivedTypeError, baseType.FullName, e.Message), e);
			}
		}

		/// <summary>
		/// 使newType实现baseType所声明的所有属性，事件及方法
		/// </summary>
		/// <param name="newType"></param>
		/// <param name="baseType"></param>
		internal static void ImplementMembers(TypeBuilder newType, Type baseType)
		{
			MethodBuilder methodBuilder;

			MethodAttributes accessorAttributes = MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
			MethodAttributes methodAttributes = MethodAttributes.Virtual;
			MethodInfo method;
			ILGenerator generator;

			#region 定义属性
			PropertyInfo[] properties = baseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0, count = properties.Length; i < count; i++)
			{
				PropertyInfo property = properties[i];


				MethodInfo getMethod = property.GetGetMethod();
				MethodInfo setMethod = property.GetSetMethod();
				if ((getMethod != null && getMethod.IsAbstract) || (setMethod != null && setMethod.IsAbstract))
				{
					#region 收集属性的参数信息
					List<Type> parameterTypes = new List<Type>();
					ParameterInfo[] parameters = property.GetIndexParameters();
					for (int j = 0, countJ = parameters.Length; j < countJ; j++)
					{
						parameterTypes.Add(parameters[j].ParameterType);
					}
					#endregion

					PropertyBuilder propertyBuilder = newType.DefineProperty(property.Name, property.Attributes, property.PropertyType, parameterTypes.ToArray());
					FieldBuilder fieldBuilder = newType.DefineField("_" + property.Name + (parameters.Length > 0 ? Guid.NewGuid().ToString().Replace("-", string.Empty) : string.Empty), property.PropertyType, FieldAttributes.Private);

					#region 定义属性的get方法
					if (property.CanRead)
					{
						if (getMethod == null)
						{
							throw new NotSupportedException(string.Format(Resources.DataTypeFactory_PropertyGetAccessorError, baseType.FullName, property.Name));
						}
						else if (getMethod.IsAbstract) //如果属性的get是abstract,就定义,否则不定义
						{
							if (!getMethod.IsPublic)
							{
								throw new NotSupportedException(string.Format(Resources.DataTypeFactory_PropertyGetAccessorError, baseType.FullName, property.Name));
							}

							methodBuilder = newType.DefineMethod("get_" + property.Name, (getMethod.Attributes ^ MethodAttributes.Abstract) | accessorAttributes, property.PropertyType, parameterTypes.ToArray());
							for (int j = 0, countJ = parameters.Length; j < countJ; j++)
							{
								methodBuilder.DefineParameter(j, parameters[j].Attributes, parameters[j].Name);
							}

							/*
							 * public T PropertA
							 * {
							 *		get
							 *		{
							 *			return _PropertyA;
							 *		}
							 * }
							 */
							generator = methodBuilder.GetILGenerator();
							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldfld, fieldBuilder);
							generator.Emit(OpCodes.Ret);

							propertyBuilder.SetGetMethod(methodBuilder);
							newType.DefineMethodOverride(methodBuilder, getMethod);
						}
					}
					#endregion

					#region 定义属性的set方法
					if (property.CanWrite)
					{
						if (setMethod == null)
						{
							throw new NotSupportedException(string.Format(Resources.DataTypeFactory_PropertySetAccessorError, baseType.FullName, property.Name));
						}
						else if (setMethod.IsAbstract) //如果属性的get是abstract,就定义,否则不定义
						{
							if (!setMethod.IsPublic)
							{
								throw new NotSupportedException(string.Format(Resources.DataTypeFactory_PropertySetAccessorError, baseType.FullName, property.Name));
							}

							parameterTypes.Add(property.PropertyType);

							methodBuilder = newType.DefineMethod("set_" + property.Name, (setMethod.Attributes ^ MethodAttributes.Abstract) | accessorAttributes, null, parameterTypes.ToArray());

							for (int j = 0, countJ = parameters.Length; j < countJ; j++)
							{
								methodBuilder.DefineParameter(j, parameters[j].Attributes, parameters[j].Name);
							}
							/*
							 * public T PropertA
							 * {
							 *		set
							 *		{
							 *			_PropertyA = value;
							 *		}
							 * }
							 */
							generator = methodBuilder.GetILGenerator();
							generator.Emit(OpCodes.Ldarg_0);
							generator.Emit(OpCodes.Ldarg, parameterTypes.Count);
							generator.Emit(OpCodes.Stfld, fieldBuilder);
							generator.Emit(OpCodes.Ret);

							propertyBuilder.SetSetMethod(methodBuilder);
							newType.DefineMethodOverride(methodBuilder, setMethod);
						}
					}
					#endregion
				}
			}
			#endregion

			#region 定义方法
			MethodInfo[] methods = baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0, count = methods.Length; i < count; i++)
			{
				method = methods[i];
				if (method.IsAbstract && !method.IsSpecialName)
				{

					ParameterInfo[] parameters = method.GetParameters();
					if (!method.IsPublic)
					{
						StringBuilder methodName = new StringBuilder(method.ReturnType.Name + " " + method.Name);
						methodName.Append("(");
						for (int j = 0, countJ = parameters.Length; j < countJ; j++)
						{
							methodName.Append((j > 0 ? "," : string.Empty) + parameters[j].ParameterType.Name + " " + parameters[j].Name);
						}
						methodName.Append(")");
						throw new NotSupportedException(string.Format(Resources.DataTypeFactory_NonePublicMethodNotSupported, baseType.FullName, methodName));
					}

					#region 收集属性的参数信息
					/*
							 * 此处收集方法的泛型参数，请注意methodBuilder之后，如果是泛型方法，要先定义泛型参数，接着再定义参数
							 * 如果不定义泛型参数,则将导致方法与父类签名不一致，从而无法进行继承
							 */
					Type[] genericTypes = method.GetGenericArguments();
					List<string> genericParameterNames = new List<string>();
					for (int k = 0, countK = genericTypes.Length; k < countK; k++)
					{
						genericParameterNames.Add(genericTypes[k].Name);
					}

					List<Type> parameterTypes = new List<Type>();
					for (int j = 0, countJ = parameters.Length; j < countJ; j++)
					{
						parameterTypes.Add(parameters[j].ParameterType);

					}
					#endregion

					methodBuilder = newType.DefineMethod(method.Name, (method.Attributes ^ MethodAttributes.Abstract) | methodAttributes);
					if (genericParameterNames.Count > 0)
					{
						methodBuilder.DefineGenericParameters(genericParameterNames.ToArray());
					}
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
					generator.Emit(OpCodes.Newobj, typeof(NotImplementedException).GetConstructor(Type.EmptyTypes));
					generator.Emit(OpCodes.Throw);

					newType.DefineMethodOverride(methodBuilder, method);
				}
			}
			#endregion

			#region 定义事件
			EventInfo[] events = baseType.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0, count = events.Length; i < count; i++)
			{
				EventInfo evt = events[i];
				MethodInfo addMethod = evt.GetAddMethod();
				MethodInfo removeMethod = evt.GetRemoveMethod();
				if ((addMethod != null && addMethod.IsAbstract) || (removeMethod != null && removeMethod.IsAbstract))
				{
					EventBuilder eventBuilder = newType.DefineEvent(evt.Name, evt.Attributes, evt.EventHandlerType);
					if (addMethod != null && addMethod.IsAbstract)
					{
						if (!addMethod.IsPublic) throw new NotSupportedException(string.Format(Resources.DataTypeFactory_EventAddAccessorError, baseType.FullName, evt.Name));
						methodBuilder = newType.DefineMethod("add_" + evt.Name, addMethod.Attributes ^ MethodAttributes.Abstract, CallingConventions.Standard, null, new Type[] { evt.EventHandlerType });
						methodBuilder.DefineParameter(0, addMethod.GetParameters()[0].Attributes, "value");
						newType.DefineMethodOverride(methodBuilder, addMethod);
						generator = methodBuilder.GetILGenerator();
						generator.Emit(OpCodes.Ret);
						eventBuilder.SetAddOnMethod(methodBuilder);
					}
					if (removeMethod != null && removeMethod.IsAbstract)
					{
						if (!removeMethod.IsPublic) throw new NotSupportedException(string.Format(Resources.DataTypeFactory_EventRemoveAccessorError, baseType.FullName, evt.Name));
						methodBuilder = newType.DefineMethod("remove_" + evt.Name, removeMethod.Attributes ^ MethodAttributes.Abstract, CallingConventions.Standard, null, new Type[] { evt.EventHandlerType });
						methodBuilder.DefineParameter(0, addMethod.GetParameters()[0].Attributes, "value");
						newType.DefineMethodOverride(methodBuilder, removeMethod);
						generator = methodBuilder.GetILGenerator();
						generator.Emit(OpCodes.Ret);
						eventBuilder.SetRemoveOnMethod(methodBuilder);
					}
				}
			}
			#endregion

			if (baseType.BaseType != null && baseType.BaseType != typeof(object))
			{
				ImplementMembers(newType, baseType.BaseType);
			}

			if (baseType.IsInterface)
			{
				Type[] interfaceTypes = baseType.GetInterfaces();
				for (int i = 0, count = interfaceTypes.Length; i < count; i++)
				{
					Type interfaceType = interfaceTypes[i];
					if (!Reflector.IsImplemented(newType, interfaceType))
					{
						ImplementMembers(newType, interfaceType);
					}
				}
			}
		}
	}
}
