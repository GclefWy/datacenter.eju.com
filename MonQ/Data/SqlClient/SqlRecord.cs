using System;
using System.Collections.Generic;
using System.Text;
using MonQ.Properties;
using System.Dynamic;
using System.Web.Script.Serialization;
using MonQ.Data.SqlClient.CommandAdapters;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 描述了一个Sql动态记录类型的基类型。
	/// 调用SqlActivator.CreateDynamic所创建的实例都是SqlRecord类型的派生类。
	/// </summary>
	/// <remarks>
	/// SqlActivator所创建的派生类扩展了SqlRecord的属性，为说明这个问题请看下面的举例：
	///		假定SqlActivator的数据访问器当前有两列，分别是string类型的列EmployeeName及int类型的列Age
	///		则SqlActivator所创建的SqlRecord类型实际上是这样一个类型：
	///		这个类型派生自SqlRecord，但是又具备两个属性：String EmployeeName{get; set;}及 int Age{get; set; }
	///		这个派生类的代码类似于：
	///		public class SqlRecordEx : SqlRecord
	///		{
	///			public override object this[string name]
	///			{
	///				get
	///				{
	///					if (name == null) throw new ArgumentNullException("name");
	///					int hashCode = name.ToLower().GetHashCode();
	///					if (hashCode == -193700222)//-193700222为"employeename"的哈希码
	///					{
	///						return values[0];
	///					}
	///					else if (hashCode == 1699463504)//1699463504为"age"的哈希码
	///					{
	///						return values[1];
	///					}
	///					throw new Exception(String.Format("不存在所查找的字段:{0}", name));
	///				}
	///			}
	///		}
	///		
	///	
	///	这个功能在.Net 4.0下面非常有用，意味着你可以进行类似于如下方式的代码编写，以上面为例：
	///	SqlActivator activator = helper.CreateExecuter("SELECT * FROM Employee").ExecuteActivator();
	///	if (activator.NextRecord())
	///	{
	///		dynamic employee = activator.CreateDynamic();
	///		Console.WriteLine(employee.EmployeeName);
	///		Console.WriteLine(employee.Age);
	///	}
	///	
	///	派生类的扩展属性命名规则：
	///	原则上派生类的列名映射为对应的属性，请参考上面的例子，但是一些特殊名称将被修改，请看下面的Sql查询语句：
	///	SELECT 'Field' AS Field, 'A' AS Field, 'B' AS Field, 'C' AS Field, 'Field3' AS Field3, 'Field4' AS Field4, 'ToString' AS ToString, 'itemArray' AS itemArray, 'Item' AS Item, 'Hello,Jim' AS 'Hello,Jim', 'public' AS 'public', 'a b' AS 'a b', '1' AS '1', '我是中文' AS '我是中文'
	///	
	/// 从这个查询语句在创建SqlRecord时有以下冲突：
	///		1.'Field' AS Field, 'Field' AS Field, 'Field' AS Field, 'Field' AS Field的列名出现了雷同的情况，因此同样需要进行修改：
	///			a)'A' AS Field列由于在定义时是正常的，因此属性名任然是Field,'B' AS Field列的列名为Field2
	///			b)'C' AS Field列在定义时，然而三个列的列名出现了雷同的情况，按照规则应该为Field3，然而Field3已经被占用了，因此顺延到没有被占用的名称Field5
	///		2.前两列的名称分别与SqlRecord类型的成员名称雷同(ToString方法及this[int index]属性)，为避免异常，派生类会对派生的属性进行修改，在派生类中ToString列对应于属性的名称被修改为toString,Item属列的属性名为item
	///		3.itemArray与SqlRecord的保护成员名称雷同，因此被修改为RecordValues，接着发现RecordValues还是冲突，于是最后被修改为recordValues2
	///		4.'F' AS 'Hello,Jim'列包含了非a-z,0-9之间的字符，因此被修改为Hello_Jim
	///		5.'G' AS 'public'列占用了C#的关键字,同样不被允许，因此关键字的第一个字母被修改为大写,被修改为Public
	///		6.'H' AS 'a b'列包含了空格，因此在生成时里面的空白被清除，修改后的列名为ab
	///		7.'I' AS '1'列只有一个数字，因此在前面增加一个占位符"_"，被修改为_1
	///	
	/// 最后说明：
	/// A.以上2,3,4,5,6,7四项规则若应用后与1规则冲突，则继续执行1规则
	/// B.中文列名是可以接受的
	/// C.在实际开发中,可能某些规则在创建类型的时候可以创建，例如你可以创建一个包含属性为public的动态类型，但是在使用的时候可能无法以dynamic方式使用
	///		
	///	最终，我们可以类似于以下编码方式来使用:
	///	dynamic item = activator.CreateDynamic();
	///	Console.WriteLine(item.Field);
	///	Console.WriteLine(item.Field2);
	///	Console.WriteLine(item.Field5);
	///	Console.WriteLine(item.Field3);
	///	Console.WriteLine(item.Field4);
	///	Console.WriteLine(item.toString);
	///	Console.WriteLine(item.recordValues2);
	///	Console.WriteLine(item.item);
	///	Console.WriteLine(item.Hello_Jim);
	///	Console.WriteLine(item.Public);
	///	Console.WriteLine(item.ab);
	///	Console.WriteLine(item._1);
	///	Console.WriteLine(item.我是中文);
	///	
	/// 当然，上述情形在实际开发中可能并不常见，然而如果准备使用CreateDynamic，则查询时应尽量避免上述情况出现，如若一定要这么做，某些关键字可以使用大小写规避
	/// 
	/// 另：开发人员也可以定义自己的SqlRecord派生类，但是单纯的派生没什么意义。
	/// </remarks>
	[Serializable]
	public abstract class SqlRecord : DynamicObject, ISqlRecord
	{
		private const string CONST_SQL_RECORD = "SqlRecord";

		/// <summary>
		/// 根据列名获取值
		/// </summary>
		/// <param name="name">列名，不区分大小写</param>
		/// <returns>
		/// 如果列存在，返回对应值，否则抛出异常
		/// </returns>
		/// <exception cref="System.ArgumentException">列名不存在</exception>
		/// <exception cref="System.ArgumentNullException">name参数为null</exception>
		public virtual object this[string name]
		{
			get
			{
				for(int i = columnNames.Length - 1; i > -1; i--)
				{
					if (string.Compare(name, columnNames[i], true) == 0)
					{
						return itemArray[i];
					}
				}
				return null;
			}
			set
			{
				for (int i = columnNames.Length - 1; i > -1; i--)
				{
					if (string.Compare(name, columnNames[i], true) == 0)
					{
						if (itemArray[i] != value)
						{
							itemArray[i] = value;
							NoticeChange(columnNames[i]);
							return;
						}
					}
				}
			}
		}

		//保存一个SqlExecuter实例，供SqlActivator注入以便执行增删改所需的操作方法
		internal SqlExecuter executer;

		/// <summary>
		/// 行数据
		/// </summary>
		protected object[] itemArray;//最开始起名为values,但values在查询中较常见,因此进行规避

		/// <summary>
		/// 列名
		/// </summary>
		internal string[] columnNames;

		internal HashSet<string> changedColumns;

		/// <summary>
		/// 根据索引获取值
		/// </summary>
		/// <param name="index">索引</param>
		/// <returns>
		/// 如果索引正确，返回对应值，否则抛出异常
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">索引超出范围</exception>
		public object this[int index]
		{
			get
			{
				return itemArray[index];
			}
			set
			{
				if (itemArray[index] != value)
				{
					itemArray[index] = value;
					NoticeChange(columnNames[index]);
				}
			}
		}

		/// <summary>
		/// 获取当前记录的所有值
		/// </summary>
		[ScriptIgnore]
		public object[] ItemArray
		{
			get
			{
				return itemArray;
			}
			set
			{
				itemArray = value;
			}
		}

		/// <summary>
		/// 构造新的实例
		/// </summary>
		/// <param name="values">值列表</param>
		public SqlRecord()
		{
		}

		/// <summary>
		/// 根据索引获取值并且转换为指定的类型
		/// </summary>
		/// <typeparam name="T">需要转换的类型</typeparam>
		/// <param name="index">索引值,从0开始</param>
		/// <returns>
		/// 如果值存在且可以转换，返回转换后的值，否则抛出异常
		/// </returns>
		/// <exception cref="System.IndexOutOfRangeException">索引超出范围</exception>
		/// <exception cref="System.InvalidCastException">类型转换异常</exception>
		public T GetValue<T>(int index)
		{
			object o = itemArray[index];
			return o is T ? (T)o : o == null ? default(T) : (T)Convert.ChangeType(o, typeof(T));
		}

		/// <summary>
		/// 根据列名获取值并且转换为指定的类型
		/// </summary>
		/// <typeparam name="T">需要转换的类型</typeparam>
		/// <param name="name">列名</param>
		/// <returns>如果值存在且可以转换，返回转换后的值，否则抛出异常</returns>
		/// <exception cref="System.IndexOutOfRangeException">索引超出范围</exception>
		/// <exception cref="System.InvalidCastException">类型转换异常</exception>
		/// <exception cref="System.ArgumentException">列名不存在</exception>
		/// <exception cref="System.ArgumentNullException">name参数为null</exception>
		public T GetValue<T>(string name)
		{
			object o = this[name];
			return o is T ? (T)o : o == null ? default(T) : (T)Convert.ChangeType(o, typeof(T));
		}

		/// <summary>
		/// 某一列发生了变化
		/// </summary>
		/// <param name="column"></param>
		protected void NoticeChange(string column)
		{
			changedColumns = changedColumns ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (!changedColumns.Contains(column))
			{
				changedColumns.Add(column);
			}
		}

		/// <summary>
		/// 使用指定的SQL命令将实体插入到数据库
		/// </summary>
		/// <param name="insertCommand"></param>
		/// <returns></returns>
		public int Insert(string insertCommand = null)
		{
			insertCommand = insertCommand ?? this.executer.InsertCommand;
			bool empty = string.IsNullOrEmpty(insertCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(insertCommand);
				if (adapter is SqlTableAdapter)
				{
					insertCommand = (adapter as SqlTableAdapter).InitInsertCommand(this.GetType());
				}
				return this.executer.helper.FromObject(insertCommand, this).ExecuteNonQuery();
			}
			else if (this.executer.commandAdapter is SqlTableAdapter)
			{
				insertCommand = (this.executer.commandAdapter as SqlTableAdapter).InitInsertCommand(this.GetType());
				return this.executer.helper.FromObject(insertCommand, this).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoInsertCommand, CONST_SQL_RECORD));
		}

		/// <summary>
		/// 保存实体到数据库
		/// </summary>
		/// <returns>
		/// 进行保存操作后影响数据库的行数
		/// </returns>
		/// <remarks>
		/// 若返回值为0，表示保存失败，可能实体定义的数据不足以标识数据库中对应的数据；
		/// 若返回值为1，表示删除成功；
		/// 若返回值大于1，则表示实体定义可能存在问题，数据库中存在多个匹配的数据。
		/// </remarks>
		public int Update(string updateCommand = null)
		{
			updateCommand = updateCommand ?? this.executer.UpdateCommand;
			bool empty = string.IsNullOrEmpty(updateCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(updateCommand);
				if (adapter is SqlTableAdapter)
				{
					updateCommand = (adapter as SqlTableAdapter).InitUpdateCommand(this);
				}
				return this.executer.helper.FromObject(updateCommand, this).ExecuteNonQuery();
			}
			else if (this.executer.commandAdapter is SqlTableAdapter)
			{
				updateCommand = (this.executer.commandAdapter as SqlTableAdapter).InitUpdateCommand(this);
				return this.executer.helper.FromObject(updateCommand, this).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoUpdateCommand, CONST_SQL_RECORD));
		}

		/// <summary>
		/// 从数据库删除实体的数据
		/// </summary>
		/// <returns>
		/// 进行删除操作后影响数据库的行数
		/// </returns>
		/// <remarks>
		/// 若返回值为0，表示删除失败；
		/// 若返回值为1，表示删除成功；
		/// 若返回值大于1，则表示实体定义可能存在问题，数据库中存在多个匹配的数据。
		/// </remarks>
		public int Delete(string deleteCommand = null)
		{
			deleteCommand = deleteCommand ?? this.executer.DeleteCommand;
			bool empty = string.IsNullOrEmpty(deleteCommand);
			if (!empty)
			{
				SqlCommandAdapter adapter = executer.helper.InternalCreateCommandAdapter(deleteCommand);
				if (adapter is SqlTableAdapter)
				{
					deleteCommand = (adapter as SqlTableAdapter).InitDeleteCommand(this.GetType());
				}
				return this.executer.helper.FromObject(deleteCommand, this).ExecuteNonQuery();
			}
			else if (this.executer.commandAdapter is SqlTableAdapter)
			{
				deleteCommand = (this.executer.commandAdapter as SqlTableAdapter).InitDeleteCommand(this.GetType());
				return this.executer.helper.FromObject(deleteCommand, this).ExecuteNonQuery();
			}
			throw new NotSupportedException(string.Format(Resources.SqlClient_NoDeleteCommand, CONST_SQL_RECORD));
		}

		/// <summary>
		/// 获取指定名称的列的索引位置
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public int GetColOrdinary(string name)
		{
			for (int i = 0; i < this.columnNames.Length; i++)
			{
				if (string.Compare(this.columnNames[i], name, true) == 0)
				{
					return i;
				}
			}
			return -1;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			this[binder.Name] = value;
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = this[binder.Name];
			if (binder.ReturnType != null)
			{
			}
			return true;
		}
	}
}
