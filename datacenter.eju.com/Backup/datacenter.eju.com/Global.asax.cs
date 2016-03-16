using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace datacenter.eju.com
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "Default", // 路由名称
            //    "{controller}/{action}/{id}", // 带有参数的 URL
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional } // 参数默认值
            //);

            MvcHelper helper = MvcHelper.Create("datacenter.eju.com.Controllers");
            helper.MapRoute("api/GetUnitName/{typewords}", "FindProperty", "GetUnitName"); //根据小区名称模糊匹配小区名称
            helper.MapRoute("api/GetUnitNameByAdd/{typewords}", "FindProperty", "GetUnitNameByAdd"); //根据小区地址模糊匹配小区名称
            helper.MapRoute("api/GetBlockInfo/{unitid}", "FindProperty", "GetBlockInfo"); //根据unitid获取楼栋信息
            helper.MapRoute("api/GetRoomInfo/{blockid}", "FindProperty", "GetRoomInfo"); //根据blockid获取房间号
            helper.MapRoute("api/GetRoomInfoDetails/{roomid}", "FindProperty", "GetRoomInfoDetails"); //根据roomid获取房间详细信息
            helper.MapRoute("api/GetScore/{price}", "FindProperty", "GetScore"); //根据物业总价给出评分

        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            // 默认情况下对 Entity Framework 使用 LocalDB
            Database.DefaultConnectionFactory = new SqlConnectionFactory(@"Data Source=(localdb)\v11.0; Integrated Security=True; MultipleActiveResultSets=True");

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }
    }
}