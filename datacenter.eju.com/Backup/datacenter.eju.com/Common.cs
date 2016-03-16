using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace datacenter.eju.com
{
    public static class Common
    {
        /// <summary>
        /// 添加response头,永许ajax跨域调用
        /// </summary>
        public static void AddResponseHeader()
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
        }
    }
}