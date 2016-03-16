using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections.Concurrent;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 提供对Sql记录集的基础数据的转换功能的辅助类
	/// </summary>
	public class SqlTypeConvertor
	{
		#region 需要使用到的类型转换方法
		private static ConcurrentDictionary<Type, MethodInfo> convertMethods = new ConcurrentDictionary<Type,MethodInfo>();
		#endregion

		static SqlTypeConvertor()
		{
			Type convert = typeof(Convert);
			#region 基本类型转换
			convertMethods.TryAdd(typeof(string), convert.GetMethod("ToString", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(bool), convert.GetMethod("ToBoolean", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(byte), convert.GetMethod("ToByte", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(char), convert.GetMethod("ToChar", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(DateTime), convert.GetMethod("ToDateTime", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(decimal), convert.GetMethod("ToDecimal", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(double), convert.GetMethod("ToDouble", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(short), convert.GetMethod("ToInt16", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(int), convert.GetMethod("ToInt32", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(long), convert.GetMethod("ToInt64", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(sbyte), convert.GetMethod("ToSByte", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(float), convert.GetMethod("ToSingle", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(ushort), convert.GetMethod("ToUInt16", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(uint), convert.GetMethod("ToUInt32", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(ulong), convert.GetMethod("ToUInt64", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			#endregion

			Type sqlTypeConvertor = typeof(SqlTypeConvertor);
			#region 字节与唯一ID转换
			convertMethods.TryAdd(typeof(byte[]), sqlTypeConvertor.GetMethod("ToByteArray", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Guid), sqlTypeConvertor.GetMethod("ToGUID", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			#endregion

			#region Nullable转换
			convertMethods.TryAdd(typeof(Nullable<bool>), typeof(NullableConvertor<bool>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<byte>), typeof(NullableConvertor<byte>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<char>), typeof(NullableConvertor<char>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<DateTime>), typeof(NullableConvertor<DateTime>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<decimal>), typeof(NullableConvertor<decimal>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<double>), typeof(NullableConvertor<double>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<short>), typeof(NullableConvertor<short>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<int>), typeof(NullableConvertor<int>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<long>), typeof(NullableConvertor<long>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<sbyte>), typeof(NullableConvertor<sbyte>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<float>), typeof(NullableConvertor<float>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<ushort>), typeof(NullableConvertor<ushort>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<uint>), typeof(NullableConvertor<uint>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<ulong>), typeof(NullableConvertor<ulong>).GetMethod("ConvertValue", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			convertMethods.TryAdd(typeof(Nullable<Guid>), sqlTypeConvertor.GetMethod("ToNullableGUID", BindingFlags.NonPublic | BindingFlags.Static, null, new Type[] { typeof(object) }, null));
			#endregion
		}

		//根据目标数据类型选择一个合适的转换方法
		internal static MethodInfo GetConvertMethod(Type type)
		{
			MethodInfo method;
			convertMethods.TryGetValue(type, out method);
			return method;
		}

		//检查是否是基础类型
		internal static bool IsConvertibleType(Type type)
		{
			return convertMethods.ContainsKey(type);
		}

		/// <summary>
		/// 将数据转换为字节数组
		/// </summary>
		/// <param name="o">需要转换的数据</param>
		/// <returns>
		/// 转换后的数据
		/// </returns>
		private static byte[] ToByteArray(object o)
		{
			if (o is byte[]) return (byte[])o;
			else if (o is string) return Encoding.Default.GetBytes(o as string);
			return null;
		}

		/// <summary>
		/// 将数据转换为字节数组
		/// </summary>
		/// <param name="o">需要转换的数据</param>
		/// <returns>
		/// 转换后的数据
		/// </returns>
		public static Guid ToGUID(object o)
		{
			if (o is Guid) return (Guid)o;
			else if (o is byte[]) return new Guid(o as byte[]);
            else if (o is string) return new Guid(o as string);
            else
            {
                return Guid.Empty;
            }
			//throw new InvalidCastException();
		}

		private static string ToString(object o)
		{
			if (o == null || o == DBNull.Value)
			{
				return string.Empty;
			}
			if (o is string)
			{
				return o as string;
			}
			return o.ToString();
		}

		/// <summary>
		/// 将数据转换为字节数组
		/// </summary>
		/// <param name="o">需要转换的数据</param>
		/// <returns>
		/// 转换后的数据
		/// </returns>
		private static Nullable<Guid> ToNullableGUID(object o)
		{
			if (o == null || o == DBNull.Value)
			{
				return new Nullable<Guid>();
			}
			return new Nullable<Guid>(ToGUID(o));
		}
		private class NullableConvertor<T> where T : struct
		{
			private static Nullable<T> ConvertValue(object o)
			{
				if (o == null) return new Nullable<T>();
				try
				{
					return new Nullable<T>((T)Convert.ChangeType(o, typeof(T)));
				}
				catch
				{
					return new Nullable<T>();
				}
			}
		}
	}
}
