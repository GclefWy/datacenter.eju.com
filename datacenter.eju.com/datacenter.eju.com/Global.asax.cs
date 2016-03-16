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
            helper.MapRoute("api/GetCityRegion/{cityname}", "FindProperty", "GetCityRegion"); //获取城市的区域
            helper.MapRoute("api/GetCityDistrict/{cityname}/{regionname}", "FindProperty", "GetCityDistrict"); //获取城市的板块
            helper.MapRoute("api/GetUnitName/{cityname}/{typewords}", "FindProperty", "GetUnitName"); //根据小区名称模糊匹配小区名称
            helper.MapRoute("api/GetUnitNameByAdd/{cityname}/{typewords}", "FindProperty", "GetUnitNameByAdd"); //根据小区地址模糊匹配小区名称
            helper.MapRoute("api/GetBlockInfo/{unitid}", "FindProperty", "GetBlockInfo"); //根据unitid获取楼栋信息
            helper.MapRoute("api/GetRoomInfo/{blockid}", "FindProperty", "GetRoomInfo"); //根据blockid获取房间号
            helper.MapRoute("api/GetRoomInfoDetails/{roomid}", "FindProperty", "GetRoomInfoDetails"); //根据roomid获取房间详细信息
            helper.MapRoute("api/GetScore/{price}", "FindProperty", "GetScore"); //根据物业总价给出评分
            //根据用户输入返回物业分及参数
            helper.MapRoute("api/GetScoreByCustomInput/{unitname}/{area}/{loanfirst}/{loancount}/{loancounttotal}/{loantype}/{loanrate}", "FindProperty", "GetScoreByCustomInput");
            //推广接口
            helper.MapRoute("api/GetScoreGame/{cityname}/{regionname}/{unitname}/{area}", "FindProperty", "GetScoreGame");

            //交易中心根据城市、楼盘名称模糊查询楼盘信息
            helper.MapRoute("api/TCUnitInfoFuzzyQuery/{cityno}/{estatename}", "CoreCenterToTradeCenter", "GetInfoByFuzzyName");
            //交易中心根据城市、楼盘名称精确查询
            helper.MapRoute("api/TCUnitInfoQuery/{cityno}/{estatename}", "CoreCenterToTradeCenter", "GetInfoByName");
            //交易中心根据城市、楼盘ID精确查询
            helper.MapRoute("api/TCUnitInfoQueryByID/{cityno}/{estateid}", "CoreCenterToTradeCenter", "GetInfoByID");

            //统一根据城市、楼盘名称模糊查询小区
            //helper.MapRoute("api/EstateInfoQueryByFuzzyName/{cityno}/{estatename}/{key}", "GetEstateInfoByFuzzyName", "GetInfoByFuzzyName");
            helper.MapRoute("api/EstateInfoQueryByFuzzyName", "GetEstateInfoByFuzzyName", "GetInfoByFuzzyName");

            //根据城市查询动态行情
            helper.MapRoute("api/EstateInfoDynamicQuery", "GetEstateInfoByFuzzyName", "GetDynamicInfo");

            //挂牌分析
            helper.MapRoute("api/EstateInfoBiddingAnalysis", "GetEstateInfoByFuzzyName", "GetBiddingAnalysis");

            //挂牌分析小区详细
            helper.MapRoute("api/EstateInfoBiddingAnalysisEstate", "GetEstateInfoByFuzzyName", "GetBiddingAnalysisEstate");

            //挂牌分析多小区详细
            helper.MapRoute("api/EstateInfoBiddingAnalysisMultiEstate", "GetEstateInfoByFuzzyName", "GetBiddingAnalysisMultiEstate");
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