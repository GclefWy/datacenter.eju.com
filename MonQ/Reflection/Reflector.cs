using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Reflection.Emit;
using System.Collections.Concurrent;

namespace MonQ.Reflection
{
	/// <summary>
	/// 提供为进行反射而所需的方法
	/// </summary>
	public static class Reflector
	{
		private delegate Type CompareTypeNameHandler(Type type, string name);
		private const char CONST_DOT = '.';
		private const char CONST_LESS_THAN = '<';
		private const char CONST_COMMA = ',';
		private const char CONST_GREATER_THAN = '>';
		private const string CONST_TRIM = @"\s+";

		private static ConcurrentDictionary<Type, string> typeNames = new ConcurrentDictionary<Type, string>();
		private static ConcurrentDictionary<Type, string> qualifiedNames = new ConcurrentDictionary<Type, string>();
		private static ConcurrentDictionary<Type, string> fullNames = new ConcurrentDictionary<Type, string>();

		/// <summary>
		/// 检查源类型是否实现了目标类型
		/// </summary>
		/// <param name="srcType">源类型</param>
		/// <param name="implementionType">目标类型，可以是基类，接口，属性</param>
		/// <returns>
		/// 如果目标类型为类，则如果源类型等于目标类型或者是目标类型的派生类，返回true，否则返回false
		/// 如果目标类型为接口，则如果源类型等于目标类型或者实现了目标类型的接口，返回true
		/// </returns>
		public static bool IsImplemented(Type srcType, Type implementionType)
		{
			if (srcType == implementionType || srcType.IsSubclassOf(implementionType))
			{
				return true;
			}
			if (implementionType.IsInterface)
			{
				Type[] interfaces = srcType.GetInterfaces();
				for (int i = 0, count = interfaces.Length; i < count; i++)
				{
					Type interfaceType = interfaces[i];
					if (interfaceType == implementionType || interfaceType.IsSubclassOf(implementionType)) return true;
				}
			}
			return false;
		}

		/// <summary>
		/// 获取某个类型及其基类型的指定标志的属性
		/// </summary>
		/// <param name="type">目标类型</param>
		/// <param name="bindingAttr">绑定标志</param>
		/// <returns>
		/// 如果目标类型是类，递归获取这个类型及其基类的属性列表
		/// 如果目标类型是接口，递归获取这个接口及其实现了的接口的属性列表
		/// </returns>
		public static PropertyInfo[] GetProperties(Type type, BindingFlags bindingAttr)
		{
			List<PropertyInfo> result = new List<PropertyInfo>();

			InternalGetProperties(result, type, bindingAttr);

			return result.ToArray();
		}

		/// <summary>
		/// 获取某个类型及其基类型的指定标志的事件
		/// </summary>
		/// <param name="type">目标类型</param>
		/// <param name="bindingAttr">绑定标志</param>
		/// <returns>
		/// 如果目标类型是类，递归获取这个类型及其基类的事件列表
		/// 如果目标类型是接口，递归获取这个接口及其实现了的接口的事件列表
		/// </returns>
		public static EventInfo[] GetEvents(Type type, BindingFlags bindingAttr)
		{
			List<EventInfo> result = new List<EventInfo>();

			InternalGetEvents(result, type, bindingAttr);

			return result.ToArray();
		}

		/// <summary>
		/// 获取某个类型及其基类型的指定标志的成员
		/// </summary>
		/// <param name="type">目标类型</param>
		/// <param name="bindingAttr">绑定标志</param>
		/// <returns>
		/// 成员列表
		/// </returns>
		public static FieldInfo[] GetFields(Type type, BindingFlags bindingAttr)
		{
			List<FieldInfo> result = new List<FieldInfo>();

			InternalGetFields(result, type, bindingAttr);

			return result.ToArray();
		}

		/// <summary>
		/// 获取一个类型的简短名称
		/// </summary>
		/// <param name="type">类型</param>
		/// <returns>
		/// 返回不包含名字空间及泛型参数的类型的名称。
		/// 如果这个类型被声明于一个类的内部（即嵌套类型），所返回的名称还包含嵌套类型的简短名称
		/// </returns>
		public static string GetTypeName(Type type)
		{
			return typeNames.GetOrAdd(type, (key) =>
			{
				return GetTypeName(type, true, false, false, false);
			});
		}

		/// <summary>
		/// 获取一个类型的限定名称
		/// </summary>
		/// <param name="type">类型</param>
		/// <returns>
		/// 返回不包含名字空间但是包含泛型参数的类型的名称。
		/// 如果这个类型被声明于一个类的内部（即嵌套类型），所返回的名称还包含嵌套类型的限定名称
		/// </returns>
		public static string GetQualifiedName(Type type)
		{
			return qualifiedNames.GetOrAdd(type, (key) =>
			{
				return GetTypeName(type, true, false, true, false);
			});
		}

		/// <summary>
		/// 获取一个类型的完整名称，完整名称包含名字空间及泛型参数的完整类型的名称
		/// </summary>
		/// <param name="type">类型</param>
		/// <returns>
		/// 返回包含名字空间及泛型参数的类型的名称。
		/// 如果这个类型被声明于一个类的内部（即嵌套类型），所返回的名称还包含嵌套类型的限定名称。
		/// </returns>
		/// <remarks>
		/// 与Type.FullName不同的是，这个方法返回的名称不包含应用程序集及签名信息
		/// </remarks>
		public static string GetFullName(Type type)
		{
			return fullNames.GetOrAdd(type, (key) =>
			{
				return GetTypeName(type, true, true, true, true);
			});
		}

		/// <summary>
		/// 根据所指定的名称获取对应的类型
		/// </summary>
		/// <param name="name">名称</param>
		/// <returns>
		/// 所对应的类型
		/// </returns>
		/// <remarks>
		/// 这个方法使用了反射并且遍历当前应用程序域下面的所有程序集下面的所有类进行比较，这样做不可避免的会降低性能；频繁的在业务逻辑中使用此方法是应该受到严格禁止的，
		/// 比较好的做法是在应用程序初始化时使用此方法读取你所需的类型并且进行缓存。
		/// </remarks>
		public static Type GetType(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			CompareTypeNameHandler handler = null;
			name = Regex.Replace(name, CONST_TRIM, string.Empty, RegexOptions.Compiled);
			int indexL = name.IndexOf(CONST_LESS_THAN);
			if (indexL > 0 && name[name.Length - 1] == CONST_GREATER_THAN)
			{
				handler = (type, n) =>
				{
					if (type.IsGenericType)
					{
						#region 泛型类型
						string qualifiedName = GetTypeName(type, true, false, true, false);
						if (qualifiedName == name || type.Namespace + CONST_DOT + qualifiedName == name) return type;

						qualifiedName = GetTypeName(type, true, false, false, false);
						if (qualifiedName == name || type.Namespace + CONST_DOT + qualifiedName == name)
						{
							List<Type> arguments = new List<Type>();
							List<string> argumentNames = new List<string>();
							int start = indexL + 1;
							int countLG = 0;
							for (int i = indexL + 1, len = name.Length - 1; i < len; i++)
							{
								switch (name[i])
								{
									case CONST_COMMA:
										string argument = name.Substring(start, i - start);
										if (string.IsNullOrEmpty(argument)) throw new ArgumentException("name");
										argumentNames.Add(argument);
										start = i + 1;
										break;
									case CONST_LESS_THAN:
										countLG++;
										break;
									case CONST_GREATER_THAN:
										countLG--;
										break;
								}
							}

							if (argumentNames.Count == type.GetGenericArguments().Length)
							{
								for (int i = 0, len = argumentNames.Count; i < len; i++)
								{
									Type argumentType = GetType(argumentNames[i]);
									if (argumentType == null)
									{
										throw new Exception();
									}
									arguments.Add(argumentType);
								}
								return type.MakeGenericType(arguments.ToArray());
							}
							else
							{
								throw new Exception();
							}
						}
						#endregion
					}
					return null;
				};
			}
			else if (indexL >= 0) throw new ArgumentException("name");
			else
			{
				handler = (type, n) =>
				{
					if (!type.IsGenericType)
					{
						if (type.Name == name) return type;
						string qualifiedName = GetQualifiedName(type);
						if (qualifiedName == name) return type;
						else if (type.Namespace + '.' + qualifiedName == name) return type;
					}
					return null;
				};
			}
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			for (int i = 0, len = assemblies.Length; i < len; i++)
			{
				Type[] types = assemblies[i].GetTypes();
				for (int j = 0, count = types.Length; j < count; j++)
				{
					Type type = handler(types[j], name);
					if (type != null)
					{
						return types[j];
					}
				}
			}
			return null;
		}

		/// <summary>
		/// 获取类型名称
		/// </summary>
		/// <param name="type">类型</param>
		/// <param name="includeNamespace">是否包含名字空间</param>
		/// <param name="includeGenericArgs">是否包含泛型参数</param>
		/// <param name="includeGenericArgsNamespace">如果包含泛型参数，这些泛型参数是否包含名字空间</param>
		/// <returns>
		/// 类型名称
		/// </returns>
		public static string GetTypeName(Type type, bool includeNestName, bool includeNamespace, bool includeGenericArgs, bool includeGenericArgsNamespace)
		{
			StringBuilder sb;
			if (includeNamespace)
			{
				sb = new StringBuilder(type.Namespace);
				sb.Append(CONST_DOT);
			}
			else
			{
				sb = new StringBuilder();
			}
			if (type.IsNested && includeNestName)
			{
				Type[] nestedTypes = type.GetNestedTypes();
				if (nestedTypes.Length > 0)
				{
					sb.Append(CONST_DOT);
					sb.Append(GetTypeName(nestedTypes[0], false, includeNamespace, includeGenericArgs, includeGenericArgsNamespace));
				}
			}
			if (type.IsGenericType)
			{
				#region 泛型类型
				int index = type.Name.IndexOf('`');
				if (index > 0)
				{
					sb.Append(type.Name.Substring(0, index));
				}
				else
				{
					sb.Append(type.Name);
				}
				if (includeGenericArgs)
				{
					Type[] arguments = type.GetGenericArguments();
					sb.Append(CONST_LESS_THAN);
					Type argument = arguments[0];
					if (argument.IsGenericParameter)
					{
						sb.Append(argument.Name);
					}
					else
					{
						sb.Append(GetTypeName(argument, true, includeGenericArgsNamespace, includeGenericArgs, includeGenericArgsNamespace));
					}
					for (int i = 1, count = arguments.Length; i < count; i++)
					{
						argument = arguments[i];
						sb.Append(CONST_COMMA);
						if (argument.IsGenericParameter)
						{
							sb.Append(argument.Name);
						}
						else
						{
							sb.Append(GetTypeName(argument, true, includeGenericArgsNamespace, includeGenericArgs, includeGenericArgsNamespace));
						}
					}
					sb.Append(CONST_GREATER_THAN);
				}
				#endregion
			}
			else
			{
				sb.Append(type.Name);
			}
			return sb.ToString();
		}

		private static void InternalGetProperties(List<PropertyInfo> result, Type type, BindingFlags bindingAttr)
		{
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			for (int i = 0, count = properties.Length; i < count; i++)
			{
				PropertyInfo property = properties[i];
				if (result.IndexOf(property) == -1)
				{
					result.Add(property);
				}
			}
			if (type.IsInterface)
			{
				Type[] interfaceTypes = type.GetInterfaces();
				for (int i = 0, count = interfaceTypes.Length; i < count; i++)
				{
					InternalGetProperties(result, interfaceTypes[i], bindingAttr);
				}
			}
			else
			{
				if (type.BaseType != null && type.BaseType != typeof(object))
				{
					InternalGetProperties(result, type.BaseType, bindingAttr);
				}
			}
		}
		private static void InternalGetFields(List<FieldInfo> result, Type type, BindingFlags bindingAttr)
		{
			FieldInfo[] fields = type.GetFields(bindingAttr);
			for (int i = 0, count = fields.Length; i < count; i++)
			{
				FieldInfo field = fields[i];
				if (result.IndexOf(field) == -1)
				{
					result.Add(field);
				}
			}
			if (type.BaseType != null && type.BaseType != typeof(object))
			{
				InternalGetFields(result, type.BaseType, bindingAttr);
			}
		}
		private static void InternalGetEvents(List<EventInfo> result, Type type, BindingFlags bindingAttr)
		{
			EventInfo[] fields = type.GetEvents(bindingAttr);
			for (int i = 0, count = fields.Length; i < count; i++)
			{
				EventInfo field = fields[i];
				if (result.IndexOf(field) == -1)
				{
					result.Add(field);
				}
			}
			if (type.BaseType != null && type.BaseType != typeof(object))
			{
				InternalGetEvents(result, type.BaseType, bindingAttr);
			}
		}
	}
}
