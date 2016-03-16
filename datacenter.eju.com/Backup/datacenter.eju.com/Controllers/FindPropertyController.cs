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
        //根据名称模糊匹配小区名
        public ActionResult GetUnitName(string typewords)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Fang.FromArray(@"select unit_id,display_project_name,address from tb_unit_main with(nolock) where state = 1 and display_project_name like '%"+typewords+"%'").ExecuteDynamicList();
            return Json(Units, JsonRequestBehavior.AllowGet);
        }

        //根据地址模糊匹配小区名
        public ActionResult GetUnitNameByAdd(string typewords)
        {
            Common.AddResponseHeader();
            var Units =
                DataHelper.Fang.FromArray(@"select unit_id,display_project_name,address,price from tb_unit_main with(nolock) where state = 1 and address like '%" + typewords + "%'").ExecuteDynamicList();
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
            int pricecase =0;
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
                case 2: score = 320;break;
                case 3: score = 320 + Math.Ceiling((price - 200000)/200000)*20;break;
                case 4: score = 400 + Math.Ceiling((price - 1000000)/200000)*10; break;
                case 5: score = 600 + Math.Ceiling((price - 5000000)/500000)*20; break;
                case 6: score = 720 + Math.Ceiling((price - 8000000)/1000000)*20; break;
                case 7: score = 770 + Math.Ceiling((price - 10000000)/2000000)*10; break;
                case 8: score = 840; break;
                case 9: score = 850; break;
                default:break;
            }

            var Result = score.ToString() + @"," + (90 + 0.7 * score).ToString();

            return Json(Result, JsonRequestBehavior.AllowGet);

        }


    }
}
