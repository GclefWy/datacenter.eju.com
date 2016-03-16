using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace datacenter.eju.com.Controllers
{
    public class FindPropertyController : Controller
    {
        //
        // GET: /FindProperty/

        //获取城市区域
        public ActionResult GetCityRegion(string cityname)
        {
            Common.AddResponseHeader();
            var Region = DataHelper.Esf[cityname].FromArray(@"select region_name from tb_region_main with(nolock) where state =1").ExecuteDynamicList();
            return Json(Region, JsonRequestBehavior.AllowGet);
        }

        //获取城市板块
        public ActionResult GetCityDistrict(string cityname, string regionname)
        {
            Common.AddResponseHeader();
            var District = DataHelper.Esf[cityname].FromArray(@"select district_name from tb_district_main with(nolock) where state =1 and region_name = '" + regionname + "'").ExecuteDynamicList();
            return Json(District, JsonRequestBehavior.AllowGet);
        }


        //根据名称模糊匹配小区名
        public ActionResult GetUnitName(string cityname, string typewords)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Esf[cityname].FromArray(@"select unit_id,display_project_name,address from tb_unit_main with(nolock) where state = 1 and display_project_name like '%" + typewords + "%'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据地址模糊匹配小区名
        public ActionResult GetUnitNameByAdd(string cityname, string typewords)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Esf[cityname].FromArray(@"select unit_id,display_project_name,address,price from tb_unit_main with(nolock) where state = 1 and address like '%" + typewords + "%'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据unitid获取楼栋信息
        public ActionResult GetBlockInfo(string unitid)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Fang.FromArray(@"select unit_id,block_id,block_number,block_address from tb_block_main with(nolock) where state = 1 and unit_id = '" + unitid + "'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据blockid获取房间号
        public ActionResult GetRoomInfo(string blockid)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Fang.FromArray(@"select unit_id,block_id,room_id,room_number from tb_room_main with(nolock) where state = 1 and block_id = '" + blockid + "'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据roomid获取房间详细信息
        public ActionResult GetRoomInfoDetails(string roomid)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Fang.FromArray(@"select unit_id,block_id,room_id,room_number,price,area,room_type,floor_total,direction from tb_room_main with(nolock) where state = 1 and room_id = '" + roomid + "'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据价格给出评分
        public ActionResult GetScore(double price)
        {
            Common.AddResponseHeader();
            int pricecase = 0;
            double score = 0;
            //if (price <=100000) pricecase = 1;
            //if (price >100000 && price <=1000000) pricecase = 2;
            //if (price >1000000 && price <=3000000) pricecase = 3;
            //if (price >3000000 && price <=5000000) pricecase = 4;
            //if (price >5000000 && price <=8000000) pricecase = 5;
            //if (price >8000000 && price <=10000000) pricecase = 6;
            //if (price > 10000000 && price <= 150000000) pricecase = 7;
            //if (price > 150000000) pricecase = 8;

            if (price <= 100000) pricecase = 1;
            if (price > 100000 && price <= 200000) pricecase = 2;
            if (price > 200000 && price <= 1000000) pricecase = 3;
            if (price > 1000000 && price <= 5000000) pricecase = 4;
            if (price > 5000000 && price <= 8000000) pricecase = 5;
            if (price > 8000000 && price <= 10000000) pricecase = 6;
            if (price > 10000000 && price <= 20000000) pricecase = 7;
            if (price > 20000000 && price <= 100000000) pricecase = 8;
            if (price > 100000000) pricecase = 9;

            switch (pricecase)
            {
                case 0: score = 0; break;
                case 1: score = 300; break;
                case 2: score = 320; break;
                case 3: score = 320 + Math.Ceiling((price - 200000) / 200000) * 20; break;
                case 4: score = 400 + Math.Ceiling((price - 1000000) / 200000) * 10; break;
                case 5: score = 600 + Math.Ceiling((price - 5000000) / 500000) * 20; break;
                case 6: score = 720 + Math.Ceiling((price - 8000000) / 1000000) * 20; break;
                case 7: score = 770 + Math.Ceiling((price - 10000000) / 2000000) * 10; break;
                case 8: score = 840; break;
                case 9: score = 850; break;
                default: break;
            }

            var Result = score.ToString() + @"," + (90 + 0.7 * score).ToString();

            return Json(Result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetScoreByCustomInput(string unitname, double area, double loanfirst, int loancount, int loancounttotal, int loantype, double loanrate)
        {
            Common.AddResponseHeader();
            try
            {
                int pricecase = 0;
                double score = 0;
                var sflag =
                     DataHelper.Fang.FromArray
                     (@"select count(1) from tb_unit_main with(nolock) where display_project_name = '" + unitname + "' and state =1").ExecuteDynamicList();

                int flag = Convert.ToInt16(sflag[0][0]);
                if (flag > 0)
                {
                    var up = DataHelper.Fang.FromArray(@"select price from tb_unit_main where state =1 and display_project_name = '" + unitname + "'").ExecuteDynamicList();
                    double UnitPrice = up[0][0];
                    double TotalPrice = 0;
                    if (loantype == 1)//等额本金
                    {
                        TotalPrice = UnitPrice * area * (loanfirst + (1 - loanfirst) / loancounttotal * loancount);
                    }
                    if (loantype == 2)//等额本息
                    {
                        TotalPrice = UnitPrice * area * (loanfirst + (1 - loanfirst) * (Math.Pow((1 + loanrate), loancount) - 1) / (Math.Pow((1 + loanrate), loancounttotal) - 1));
                    }
                    if (TotalPrice <= 100000) pricecase = 1;
                    if (TotalPrice > 100000 && TotalPrice <= 200000) pricecase = 2;
                    if (TotalPrice > 200000 && TotalPrice <= 1000000) pricecase = 3;
                    if (TotalPrice > 1000000 && TotalPrice <= 5000000) pricecase = 4;
                    if (TotalPrice > 5000000 && TotalPrice <= 8000000) pricecase = 5;
                    if (TotalPrice > 8000000 && TotalPrice <= 10000000) pricecase = 6;
                    if (TotalPrice > 10000000 && TotalPrice <= 20000000) pricecase = 7;
                    if (TotalPrice > 20000000 && TotalPrice <= 100000000) pricecase = 8;
                    if (TotalPrice > 100000000) pricecase = 9;

                    switch (pricecase)
                    {
                        case 0: score = 0; break;
                        case 1: score = 300; break;
                        case 2: score = 320; break;
                        case 3: score = 320 + Math.Ceiling((TotalPrice - 200000) / 200000) * 20; break;
                        case 4: score = 400 + Math.Ceiling((TotalPrice - 1000000) / 200000) * 10; break;
                        case 5: score = 600 + Math.Ceiling((TotalPrice - 5000000) / 500000) * 20; break;
                        case 6: score = 720 + Math.Ceiling((TotalPrice - 8000000) / 1000000) * 20; break;
                        case 7: score = 770 + Math.Ceiling((TotalPrice - 10000000) / 2000000) * 10; break;
                        case 8: score = 840; break;
                        case 9: score = 850; break;
                        default: break;
                    }

                    //获取近期挂牌量
                    var bc = DataHelper.Bid.FromArray(@"select count(1) from [dbo].[t_esf_sh] where ltrim(communityname)='" + unitname + "'").ExecuteDynamicList();
                    string biddingcount = Convert.ToString(bc[0][0]);

                    var Result = DataHelper.Fang.FromArray(@"select " + score.ToString() + " as Score," + Math.Ceiling(UnitPrice * area).ToString() + " as TotalPrice," + Math.Ceiling(TotalPrice).ToString() + " as CustomNowPrice," + biddingcount + "as BiddingCount").ExecuteDynamicList();
                    return Json(Result, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("未知小区", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }

        //推广接口
        public ActionResult GetScoreGame(string cityname, string regionname, string unitname, double area)
        {
            Common.AddResponseHeader();
            try
            {
                int pricecase = 0;
                double score = 0;
                var sflag =
                     DataHelper.Esf[cityname].FromArray
                     (@"select count(1) from tb_unit_main with(nolock) where display_project_name = '" + unitname + "' and state =1").ExecuteDynamicList();

                var srate =
                    DataHelper.Bid.FromArray(@"select rate from CityRate where citycode = '" + cityname + "'").ExecuteDynamicList();

                double rate = Convert.ToDouble(srate[0][0]);

                int flags = Convert.ToInt16(sflag[0][0]);
                if (flags > 0)
                {
                    var up = DataHelper.Esf[cityname].FromArray(@"select price from tb_unit_main where state =1 and display_project_name = '" + unitname + "'").ExecuteDynamicList();
                    double UnitPrice = up[0][0];
                    double TotalPrice = 0;

                    TotalPrice = UnitPrice * area;

                    if (TotalPrice <= 100000) pricecase = 1;
                    if (TotalPrice > 100000 && TotalPrice <= 200000) pricecase = 2;
                    if (TotalPrice > 200000 && TotalPrice <= 1000000) pricecase = 3;
                    if (TotalPrice > 1000000 && TotalPrice <= 5000000) pricecase = 4;
                    if (TotalPrice > 5000000 && TotalPrice <= 8000000) pricecase = 5;
                    if (TotalPrice > 8000000 && TotalPrice <= 10000000) pricecase = 6;
                    if (TotalPrice > 10000000 && TotalPrice <= 20000000) pricecase = 7;
                    if (TotalPrice > 20000000 && TotalPrice <= 100000000) pricecase = 8;
                    if (TotalPrice > 100000000) pricecase = 9;

                    switch (pricecase)
                    {
                        case 0: score = 0; break;
                        case 1: score = 300; break;
                        case 2: score = 320; break;
                        case 3: score = 320 + Math.Ceiling((TotalPrice - 200000) / 200000) * 20; break;
                        case 4: score = 400 + Math.Ceiling((TotalPrice - 1000000) / 200000) * 10; break;
                        case 5: score = 600 + Math.Ceiling((TotalPrice - 5000000) / 500000) * 20; break;
                        case 6: score = 720 + Math.Ceiling((TotalPrice - 8000000) / 1000000) * 20; break;
                        case 7: score = 770 + Math.Ceiling((TotalPrice - 10000000) / 2000000) * 10; break;
                        case 8: score = 840; break;
                        case 9: score = 850; break;
                        default: break;
                    }

                    //获取近期挂牌量
                    //var bc = DataHelper.Bid.FromArray(@"select count(1) from [dbo].[t_esf_sh] where ltrim(communityname)='" + unitname + "'").ExecuteDynamicList();
                    //string biddingcount = Convert.ToString(bc[0][0]);

                    //var Result = DataHelper.Fang.FromArray(@"select " + score.ToString() + " as Score," + Math.Ceiling(UnitPrice * area).ToString()+ " as TotalPrice," + Math.Ceiling(TotalPrice).ToString()+" as CustomNowPrice,"+ biddingcount + "as BiddingCount").ExecuteDynamicList();
                    //return Json(Result, JsonRequestBehavior.AllowGet);
                    double results = Math.Ceiling(score * ((rate - 1) / 100 + 1));
                    if (results >= 840) { results = 840; }


                    return Json(results, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rflag =
                     DataHelper.Esf[cityname].FromArray
                     (@"select count(1) from tb_region_main with(nolock) where region_name = '" + regionname + "' and state =1").ExecuteDynamicList();

                    int flagr = Convert.ToInt16(rflag[0][0]);

                    if (flagr > 0)
                    {
                        var up = DataHelper.Esf[cityname].FromArray(@"select price from tb_region_main where state =1 and region_name = '" + regionname + "'").ExecuteDynamicList();
                        double RegionPrice = up[0][0];
                        double TotalPrice = 0;

                        TotalPrice = RegionPrice * area;

                        if (TotalPrice <= 100000) pricecase = 1;
                        if (TotalPrice > 100000 && TotalPrice <= 200000) pricecase = 2;
                        if (TotalPrice > 200000 && TotalPrice <= 1000000) pricecase = 3;
                        if (TotalPrice > 1000000 && TotalPrice <= 5000000) pricecase = 4;
                        if (TotalPrice > 5000000 && TotalPrice <= 8000000) pricecase = 5;
                        if (TotalPrice > 8000000 && TotalPrice <= 10000000) pricecase = 6;
                        if (TotalPrice > 10000000 && TotalPrice <= 20000000) pricecase = 7;
                        if (TotalPrice > 20000000 && TotalPrice <= 100000000) pricecase = 8;
                        if (TotalPrice > 100000000) pricecase = 9;

                        switch (pricecase)
                        {
                            case 0: score = 0; break;
                            case 1: score = 300; break;
                            case 2: score = 320; break;
                            case 3: score = 320 + Math.Ceiling((TotalPrice - 200000) / 200000) * 20; break;
                            case 4: score = 400 + Math.Ceiling((TotalPrice - 1000000) / 200000) * 10; break;
                            case 5: score = 600 + Math.Ceiling((TotalPrice - 5000000) / 500000) * 20; break;
                            case 6: score = 720 + Math.Ceiling((TotalPrice - 8000000) / 1000000) * 20; break;
                            case 7: score = 770 + Math.Ceiling((TotalPrice - 10000000) / 2000000) * 10; break;
                            case 8: score = 840; break;
                            case 9: score = 850; break;
                            default: break;
                        }

                        //获取近期挂牌量
                        //var bc = DataHelper.Bid.FromArray(@"select count(1) from [dbo].[t_esf_sh] where ltrim(communityname)='" + unitname + "'").ExecuteDynamicList();
                        //string biddingcount = Convert.ToString(bc[0][0]);

                        //var Result = DataHelper.Fang.FromArray(@"select " + score.ToString() + " as Score," + Math.Ceiling(UnitPrice * area).ToString()+ " as TotalPrice," + Math.Ceiling(TotalPrice).ToString()+" as CustomNowPrice,"+ biddingcount + "as BiddingCount").ExecuteDynamicList();
                        //return Json(Result, JsonRequestBehavior.AllowGet);
                        double results = Math.Ceiling(score * ((rate - 1) / 100 + 1));
                        if (results >= 840) { results = 840; }


                        return Json(results, JsonRequestBehavior.AllowGet);
                    }

                    else
                    {

                        var up = DataHelper.Esf[cityname].FromArray(@"select price from tb_city_main where state =1 ").ExecuteDynamicList();
                        double CityPrice = up[0][0];
                        double TotalPrice = 0;

                        TotalPrice = CityPrice * area;

                        if (TotalPrice <= 100000) pricecase = 1;
                        if (TotalPrice > 100000 && TotalPrice <= 200000) pricecase = 2;
                        if (TotalPrice > 200000 && TotalPrice <= 1000000) pricecase = 3;
                        if (TotalPrice > 1000000 && TotalPrice <= 5000000) pricecase = 4;
                        if (TotalPrice > 5000000 && TotalPrice <= 8000000) pricecase = 5;
                        if (TotalPrice > 8000000 && TotalPrice <= 10000000) pricecase = 6;
                        if (TotalPrice > 10000000 && TotalPrice <= 20000000) pricecase = 7;
                        if (TotalPrice > 20000000 && TotalPrice <= 100000000) pricecase = 8;
                        if (TotalPrice > 100000000) pricecase = 9;

                        switch (pricecase)
                        {
                            case 0: score = 0; break;
                            case 1: score = 300; break;
                            case 2: score = 320; break;
                            case 3: score = 320 + Math.Ceiling((TotalPrice - 200000) / 200000) * 20; break;
                            case 4: score = 400 + Math.Ceiling((TotalPrice - 1000000) / 200000) * 10; break;
                            case 5: score = 600 + Math.Ceiling((TotalPrice - 5000000) / 500000) * 20; break;
                            case 6: score = 720 + Math.Ceiling((TotalPrice - 8000000) / 1000000) * 20; break;
                            case 7: score = 770 + Math.Ceiling((TotalPrice - 10000000) / 2000000) * 10; break;
                            case 8: score = 840; break;
                            case 9: score = 850; break;
                            default: break;
                        }

                        //获取近期挂牌量
                        //var bc = DataHelper.Bid.FromArray(@"select count(1) from [dbo].[t_esf_sh] where ltrim(communityname)='" + unitname + "'").ExecuteDynamicList();
                        //string biddingcount = Convert.ToString(bc[0][0]);

                        //var Result = DataHelper.Fang.FromArray(@"select " + score.ToString() + " as Score," + Math.Ceiling(UnitPrice * area).ToString()+ " as TotalPrice," + Math.Ceiling(TotalPrice).ToString()+" as CustomNowPrice,"+ biddingcount + "as BiddingCount").ExecuteDynamicList();
                        //return Json(Result, JsonRequestBehavior.AllowGet);
                        double results = Math.Ceiling(score * ((rate - 1) / 100 + 1));
                        if (results >= 840) { results = 840; }


                        return Json(results, JsonRequestBehavior.AllowGet);
                    }

                }

            }
            catch (Exception e)
            {
                return Json(e.Message, JsonRequestBehavior.AllowGet);
            }
        }
    }
}
