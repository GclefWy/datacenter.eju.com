using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// 为实现实体支持对数据库记录增删改而实现的协定。
	/// </summary>
	public interface ISqlRecord
	{
		/// <summary>
		/// 将实体插入到数据库
		/// </summary>
		/// <returns>
		/// 进行插入操作后影响数据库的行数
		/// </returns>
		int Insert(string insertCommand = null);

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
		int Update(string updateCommand = null);

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
		int Delete(string deleteCommand = null);

	}
}
