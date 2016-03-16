using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonQ.Data.SqlClient;
using System.Configuration;
using System.Collections.Concurrent;

namespace datacenter.eju.com
{
    /// <summary>
    /// 执行数据查询与处理的类
    /// </summary>
    public static class DataHelper
    {
        /// <summary>
        /// 连接字符串模板
        /// </summary>
        internal static string connectionString = ConfigurationManager.ConnectionStrings["Fang"].ConnectionString;
        

        /// <summary>
        /// 城市相关的SqlHelper集合
        /// </summary>
        internal static ConcurrentDictionary<string, SqlHelper> cityDataHelpers = new ConcurrentDictionary<string, SqlHelper>();

        /// <summary>
        /// 查询物业信息接口用
        /// </summary>
        public static readonly string FangConnectionString = string.Format(connectionString, "DB_HOUSE_PRICE_SHANGHAI");
        public static readonly SqlHelper Fang = SqlHelper.Create(FangConnectionString);


        public static SqlHelper GetSqlHelper(string database)
        {
            return cityDataHelpers.GetOrAdd(database, SqlHelper.Create(string.Format(connectionString, database)));
        }
    }
}