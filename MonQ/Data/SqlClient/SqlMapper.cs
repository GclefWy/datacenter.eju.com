using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using MonQ.Reflection;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.Concurrent;
using System.IO;
using System.Web;
using System.Reflection;

namespace MonQ.Data.SqlClient
{
	public static class SqlMapper
	{
		private static ConcurrentDictionary<Type, ConcurrentDictionary<string, string>> memberMap = new ConcurrentDictionary<Type, ConcurrentDictionary<string, string>>();
		private static ConcurrentDictionary<Type, string> entityMap = new ConcurrentDictionary<Type, string>();

		internal static string GetEntityMap<T>()
		{
			Type type = typeof(T);
			if (!entityMap.ContainsKey(type))
			{
				Type t = type;
				bool succ = false;
				while (t != typeof(object) && !succ)
				{
					#region 根据DataMapping获取
					object[] attrs = t.GetCustomAttributes(typeof(DataMappingAttribute), false);
					if (attrs.Length == 1)
					{
						DataMappingAttribute attr = attrs[0] as DataMappingAttribute;
						if (!string.IsNullOrEmpty(attr.Source))
						{
							if (!Load(attr.Source))
							{
								entityMap.TryAdd(type, attr.Source);
								succ = true;
							}
						}
					}
					#endregion
					#region 根据DataContract获取
					attrs = t.GetCustomAttributes(typeof(DataContractAttribute), false);
					if (attrs.Length == 1)
					{
						DataContractAttribute attr = attrs[0] as DataContractAttribute;
						if (!string.IsNullOrEmpty(attr.Name))
						{
							entityMap.TryAdd(type, attr.Name);
							succ = true;
						}
					}
					#endregion
					t = t.BaseType;
				}
				if (!entityMap.ContainsKey(type))
				{
					entityMap.TryAdd(type, type.Name);
				}
			}
			return entityMap[type];
		}

		internal static string GetMemberMap<T>(MemberInfo member)
		{
			return GetMemberMap<T>(member);
		}
		internal static string GetMemberMap(Type type, MemberInfo member)
		{
			ConcurrentDictionary<string, string> map;
			if (memberMap.TryGetValue(type, out map))
			{
				string result;
				if (map.TryGetValue(member.Name, out result)) return result;
			}
			#region 根据DataMember获取
			object[] attrs = member.GetCustomAttributes(typeof(DataMemberAttribute), false);
			if (attrs.Length == 1)
			{
				DataMemberAttribute attr = attrs[0] as DataMemberAttribute;
				if (!string.IsNullOrEmpty(attr.Name))
				{
					return attr.Name;
				}
			}
			#endregion
			string name = member.Name;
			if (name[0] == '<') return name.Substring(1, name.IndexOf('>') - 1);
			return name;
		}

		public static bool Load(string xml)
		{
			XmlDocument dom = new XmlDocument();
			try
			{
				if (File.Exists(xml))
				{
					dom.Load(xml);
				}
				else if (File.Exists(HttpContext.Current.Server.MapPath(xml)))
				{
					dom.Load(HttpContext.Current.Server.MapPath(xml));
				}
				else if (File.Exists(Path.Combine(HttpContext.Current.Request.PhysicalApplicationPath, xml.TrimStart('~'))))
				{
					dom.Load(HttpContext.Current.Server.MapPath(xml));
				}
				else
				{
					dom.LoadXml(xml);
				}
				Load(dom);
			}
			catch
			{
				return false;
			}
			return true;
		}

		public static void Load(XmlDocument dom)
		{
			XmlNodeList entities = dom.GetElementsByTagName("Enity");
			foreach (XmlElement entity in entities)
			{
				Type type = Reflector.GetType(entity.GetAttribute("Type"));
				string data = entity.GetAttribute("Data").Trim();
				if (!string.IsNullOrEmpty(data))
				{
					if (type != null)
					{
						entityMap.AddOrUpdate(type, data, (t, d) => data);
						ConcurrentDictionary<string, string> map = memberMap.GetOrAdd(type, new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase));
						foreach (XmlNode col in entity.ChildNodes)
						{
							if (col.NodeType == XmlNodeType.Element)
							{
								map.AddOrUpdate(col.Name, col.InnerText.Trim(), (c, t) => col.InnerText);
							}
						}
					}
				}
			}
		}
	}
}
