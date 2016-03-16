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
    public class DataHelper
    {
        //城市分库字段
        //public static string cityfulltype { get; set; }
        
        /// <summary>
        /// 连接字符串模板
        /// </summary>
        internal static string connectionString = ConfigurationManager.ConnectionStrings["Fang"].ConnectionString;
        internal static string connectionStringBid = ConfigurationManager.ConnectionStrings["Bidding"].ConnectionString;
        internal static string connectionStringCC = ConfigurationManager.ConnectionStrings["CoreCenter"].ConnectionString;

        /// <summary>
        /// 城市相关的SqlHelper集合
        /// </summary>
        internal static ConcurrentDictionary<string, SqlHelper> cityDataHelpers = new ConcurrentDictionary<string, SqlHelper>();

        /// <summary>
        /// 查询物业信息接口用
        /// </summary>


        public static readonly string FangConnectionString = string.Format(connectionString, "DB_HOUSE_PRICE_SHANGHAI");
        public static readonly SqlHelper Fang = SqlHelper.Create(FangConnectionString);

        public static readonly string BidConnectionString = string.Format(connectionString, "X_CRIC");
        //public static readonly string BidConnectionString = string.Format(connectionString, "BiddingData");
        public static readonly SqlHelper Bid = SqlHelper.Create(BidConnectionString);

        public static readonly string CoreCenterConnectionString = string.Format(connectionStringCC, "CoreCenter");
        public static readonly SqlHelper CoreCenter = SqlHelper.Create(CoreCenterConnectionString);



        public static SqlHelper GetSqlHelper(string database)
        {
            return cityDataHelpers.GetOrAdd(database, SqlHelper.Create(string.Format(connectionString, database)));
        }


        public static readonly EsfHelperCollection Esf = new EsfHelperCollection();



    }

    public class EsfHelperCollection
    {
        private const string CONST_DB_PREFIX = "DB_HOUSE_PRICE_";

        public SqlHelper this[string cityCode]
        {
            get
            {
                return GetEsfHelper(cityCode);
            }
        }


        internal EsfHelperCollection()
        {
        }

        public static SqlHelper GetEsfHelper(string cityCode)
        {
            if (string.IsNullOrEmpty(cityCode))
            {
                throw new ArgumentNullException("cityCode");
            }
            string database = string.Concat(CONST_DB_PREFIX, cityCode);
            return DataHelper.cityDataHelpers.GetOrAdd(database, SqlHelper.Create(string.Format(DataHelper.connectionString, database)));
        }
    }

}