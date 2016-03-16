using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace datacenter.eju.com.Controllers
{
    public class CoreCenterToTradeCenterController:Controller
    {
        //
        // GET: /CoreCenterToTradeCenter/

        //根据楼盘名称模糊查询信息
        public ActionResult GetInfoByFuzzyName(string cityno,string estatename)
        {
            Common.AddResponseHeader();

            try
            {
                string sql = @"select CityNo,AreaNo,AreaName,DistrictNo,DistrictName,
            EstateID,EstateName,Address,AddressFullPY,MultiName,MultiNameFullPY,Convert(varchar(4),BuildDate,120) as BuildDate
            from Estate with(nolock)
            where state =1 and
            (EstateName like '" + estatename + @"%' or NameFullPY like '" + estatename + @"%' or MultiName like '" + estatename + @"%' or MultiNameFullPY like '" + estatename + @"%') and
            CityNo = '" + cityno + @"'";

                System.Data.DataSet result = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql);

                var Info = DataTableToListModel<UnitResultInfo>.ConvertToModel(result.Tables[0]);

                return Json(new
                {
                    ErrorCode = 0x01,
                    Info
                }, JsonRequestBehavior.AllowGet);
            }

            catch(Exception ex)
            {

                return Json(new { ErrorCode = 0x00, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        //根据楼盘名称获取精确信息
        public ActionResult GetInfoByName(string cityno, string estatename)
        {
            Common.AddResponseHeader();

            try
            {
                string sql = @"select CityNo,AreaNo,AreaName,DistrictNo,DistrictName,
            EstateID,EstateName,Address,AddressFullPY,MultiName,MultiNameFullPY,Convert(varchar(4),BuildDate,120) as BuildDate
            from Estate with(nolock)
            where state =1 and
            EstateName = '" + estatename + @"' and
            CityNo = '" + cityno + @"'";

                System.Data.DataSet result = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql);

                var Info = DataTableToListModel<UnitResultInfo>.ConvertToModel(result.Tables[0]);

                return Json(new
                {
                    ErrorCode = 0x01,
                    Info
                }, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {

                return Json(new { ErrorCode = 0x00, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        //根据楼盘ID获取精确信息
        public ActionResult GetInfoByID(string cityno, string estateid)
        {
            Common.AddResponseHeader();

            try
            {
                string sql = @"select CityNo,AreaNo,AreaName,DistrictNo,DistrictName,
            EstateID,EstateName,Address,AddressFullPY,MultiName,MultiNameFullPY,Convert(varchar(4),BuildDate,120) as BuildDate
            from Estate with(nolock)
            where state =1 and
            EstateID = '" + estateid + @"' and
            CityNo = '" + cityno + @"'";

                System.Data.DataSet result = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql);

                var Info = DataTableToListModel<UnitResultInfo>.ConvertToModel(result.Tables[0]);

                return Json(new
                {
                    ErrorCode = 0x01,
                    Info
                }, JsonRequestBehavior.AllowGet);
            }


            catch (Exception ex)
            {

                return Json(new { ErrorCode = 0x00, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

    }

    //模糊查询结果
    class UnitResultInfo
    {
        public string CityNo { get; set; }
        public string AreaNo {get;set;}
        public string AreaName { get; set; }
        public string DistrictNo { get; set; }
        public string DistrictName { get; set; }

        public string EstateID { get; set; }

        public string EstateName { get; set; }
        public string Address { get; set; }
        public string AddressFullPY { get; set; }

        public string MultiName { get; set; }

        public string MultiNameFullPY { get; set; }

        public string BuildDate { get; set; }




    }
     


}