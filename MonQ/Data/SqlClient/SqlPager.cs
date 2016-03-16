using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonQ.Data.SqlClient.ConnectionProviders;
using MonQ.Data.SqlClient.CommandAdapters;
using System.Data.SqlClient;
using System.Data.Common;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// SQL分页器
	/// </summary>
	public class SqlPager
	{
		public int PageID { get; internal set; }
		public int PageSize { get; internal set; }
		public int RowTotal { get; internal set; }
		public int PageTotal { get; internal set; }

		private SqlExecuter executer;

		internal SqlPager(SqlExecuter executer)
		{
			this.executer = executer;
		}

		/// <summary>
		/// 执行查询,并且返回一个数据访问器
		/// </summary>
		/// <returns>
		/// 数据访问器
		/// </returns>
		/// <remarks>
		/// 当数据访问器关闭后，可以通过访问此分页器的相关成员获得分页信息
		/// </remarks>
		public DbDataReader ExecuteReader(int pageID, int pageSize = 10, string orderBy = null)
		{
			this.PageID = pageID;
			this.PageSize = pageSize;
			executer.commandAdapter.InitPagination(executer, this, pageID, pageSize, orderBy);
			return executer.ExecuteDbReader();
		}

		/// <summary>
		/// 执行查询，将第一个记录集封装为指定类型的实例的数组并且返回
		/// </summary>
		/// <returns>
		/// 所生成的实例，如果没有数据，返回空白列表
		/// </returns>
		/// <remarks>
		/// 可以通过访问此分页器的相关成员获得分页信息
		/// </remarks>
		public List<EntityType> ExecuteEntityList<EntityType>(int pageID, int pageSize = 10, string orderBy = null)
		{
			this.PageID = pageID;
			this.PageSize = pageSize;
			executer.commandAdapter.InitPagination(executer, this, pageID, pageSize, orderBy);
			return executer.ExecuteEntityList<EntityType>();
		}

		/// <summary>
		/// 执行当前查询，将第一个记录集的数据并且封装成一个动态类型的List对象
		/// </summary>
		/// <returns>
		/// 所生成的实例，如果没有数据，返回空白列表
		/// </returns>
		/// <remarks>
		/// 可以通过访问此分页器的相关成员获得分页信息
		/// </remarks>
		public List<dynamic> ExecuteDynamicList(int pageID, int pageSize = 10, string orderBy = null)
		{
			this.PageID = pageID;
			this.PageSize = pageSize;
			executer.commandAdapter.InitPagination(executer, this, pageID, pageSize, orderBy);
			return executer.ExecuteDynamicList();
		}
	}
}
