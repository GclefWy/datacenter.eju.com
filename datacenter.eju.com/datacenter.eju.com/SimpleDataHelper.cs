using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data;


namespace datacenter.eju.com
{
    public class SimpleDataHelper
    {
        /// <summary>
        /// 连接字符串模板
        /// </summary>
        internal static string connectionStringCC = ConfigurationManager.ConnectionStrings["CoreCenter"].ConnectionString;
        internal static string connectionStringHRF = ConfigurationManager.ConnectionStrings["HitRankFJ"].ConnectionString;

        /// <summary>
        /// 查询楼盘信息接口用
        /// </summary>
        /// 
        public static readonly string CoreCenterConnectionString = string.Format(connectionStringCC, "CoreCenter");

        public static readonly string APILOGConnectionString = string.Format(connectionStringCC, "DB_DC_API_LOG");

        //public static readonly string HitRankFJConnectionString = string.Format(connectionStringHRF, "DB_DC_ESTATE_BASEINFO");

        public static readonly string HitRankFJConnectionString = string.Format(connectionStringHRF, "DB_CRIC_ESTATE_INFO");

        public static System.Data.DataSet Query(string ConnString, string sql)
        {
            System.Data.DataSet result = new System.Data.DataSet();
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection())
            {
                conn.ConnectionString = ConnString;
                conn.Open();
                using (System.Data.SqlClient.SqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    using (System.Data.SqlClient.SqlDataAdapter adp = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        adp.Fill(result);
                    }
                }
                conn.Close();
            }
            return result;
        }

        public static void Excsql(string ConnString, string sql)
        {
            
            using (System.Data.SqlClient.SqlConnection conn = new System.Data.SqlClient.SqlConnection())
            {
                conn.ConnectionString = ConnString;
                conn.Open();
                using (System.Data.SqlClient.SqlCommand command = conn.CreateCommand())
                {
                    command.CommandText = sql;
                    command.ExecuteNonQuery();
                }
                conn.Close();
            }
            
        }

        /// <summary>
        /// splbulkcopy函数
        /// </summary>
        public static void SqlBCP(string ConnString, DataTable dt, string destablename)
        {
            
                    SqlBulkCopy sqlBulkCopyMobile = new SqlBulkCopy(ConnString);
                    sqlBulkCopyMobile.DestinationTableName = destablename;
                    sqlBulkCopyMobile.WriteToServer(dt);
                    sqlBulkCopyMobile.Close();
               
            
        }

        
        public static DataTable GetAPILOGDataTable (string key,string query)
        {
            DataTable dt = new DataTable();
            DataColumnCollection dc = dt.Columns;
            dc.Add("UserKey", typeof(string));
            dc.Add("QueryString", typeof(string));

            dc.Add("CreateTime", typeof(DateTime));

            DataRow dr = dt.NewRow();
            dr["UserKey"] = key;
            dr["QueryString"] = query;

            dr["CreateTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            dt.Rows.Add(dr);

            return dt;

        }



        }
}