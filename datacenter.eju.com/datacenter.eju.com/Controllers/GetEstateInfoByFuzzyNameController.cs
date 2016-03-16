using System;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Runtime.Serialization.Json;
using System.Data;



namespace datacenter.eju.com.Controllers
{
    public class GetEstateInfoByFuzzyNameController : Controller
    {

        //根据楼盘名称模糊查询信息
        public ActionResult GetInfoByFuzzyName(string cityname, string estatename, string key)
        {
            Common.AddResponseHeader();
            try
            {
                if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                {

                    Encoding myEncoding = Encoding.GetEncoding("UTF-8");
                    string postData = "q=MultiName:" + estatename + "&fq=CityName:" + cityname + "&rows=5&wt=json";
                    string address = "http://172.28.70.31:8080/two/collection1/select?" + postData;
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(address);
                    req.Method = "GET";

                    WebResponse res = req.GetResponse();

                    Stream instream = res.GetResponseStream();
                    StreamReader sr = new StreamReader(instream, myEncoding);

                    string json = sr.ReadToEnd();

                    //string sql = "insert into API_LOG select '" + key + "','GetInfoByFuzzyName|cityname = "+cityname+"|estatename = "+estatename+"',getdate()";

                    //SimpleDataHelper.Excsql(SimpleDataHelper.APILOGConnectionString, sql);


                    //DataTable dt = new DataTable();
                    //DataColumnCollection dc = dt.Columns;
                    //dc.Add("UserKey", typeof(string));
                    //dc.Add("QueryString", typeof(string));

                    //dc.Add("CreateTime", typeof(DateTime));

                    //DataRow dr = dt.NewRow();
                    //dr["UserKey"] = key;
                    //dr["QueryString"] = "GetInfoByFuzzyName|cityname = " + cityname + "|estatename = " + estatename;

                    //dr["CreateTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    //dt.Rows.Add(dr);

                    DataTable dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetInfoByFuzzyName|cityname = " + cityname + "|estatename = " + estatename);

                    SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");



                    return Content(json);
                }
                else
                {
                    return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = 0x02, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
       

        public ActionResult GetDynamicInfo(string cityname,string regionname,string querytype, string key)
        {

            Common.AddResponseHeader();

            DataTable dt;

            try
            {

                if (regionname == "")
                {
                    if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                    {

                        switch (querytype)
                        {
                            case "1":
                                string sql1 = @"select top 10 cityname,UnitID,OfficalName from FJHitRankCity with(nolock) where cityname = '" + cityname + "' order by visitedcount desc";

                                System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.HitRankFJConnectionString, sql1);

                                var ViewEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result1.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");


                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    ViewEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            case "2":
                                string sql2 = @"select top 10 cityname,UnitID,OfficalName from GPRankCity with(nolock) where cityname = '" + cityname + "' order by visitedcount desc";

                                System.Data.DataSet result2 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql2);

                                var BiddingEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result2.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");


                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    BiddingEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            case "3":
                                string sql3 = @"select top 10 cityname,UnitID,OfficalName from CJRankCity with(nolock) where cityname = '" + cityname + "' order by visitedcount desc";

                                System.Data.DataSet result3 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql3);

                                var TradeEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result3.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    TradeEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            default:

                                return Json(new
                                {
                                    ErrorCode = 0x02,
                                    ErrMsg = "非法的类型代码"
                                }, JsonRequestBehavior.AllowGet);
                                break;


                        }
                    }
                    else
                    {
                        return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                    }
                }

                else

                {
                    #region
                    if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                    {

                        switch (querytype)
                        {
                            case "1":
                                string sql1 = @"select top 10 cityname,UnitID,OfficalName,region from FJHitRankRegion with(nolock) where cityname = '" + cityname + "' and region ='"+regionname+"' order by visitedcount desc";

                                System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.HitRankFJConnectionString, sql1);

                                var ViewEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result1.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|regionname = " + regionname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");


                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    ViewEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            case "2":
                                string sql2 = @"select top 10 cityname,UnitID,OfficalName,region from GPRankRegion with(nolock) where cityname = '" + cityname + "' and region ='" + regionname + "' order by visitedcount desc";

                                System.Data.DataSet result2 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql2);

                                var BiddingEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result2.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|regionname = " + regionname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");


                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    BiddingEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            case "3":
                                string sql3 = @"select top 10 cityname,UnitID,OfficalName,region from CJRankRegion with(nolock) where cityname = '" + cityname + "' and region ='" + regionname + "' order by visitedcount desc";

                                System.Data.DataSet result3 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql3);

                                var TradeEstate = DataTableToListModel<EstateInfoSimple>.ConvertToModel(result3.Tables[0]);

                                dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetDynamicInfo|cityname = " + cityname + "|regionname = " + regionname + "|querytype = " + querytype);

                                SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                                return Json(new
                                {
                                    ErrorCode = 0x00,
                                    TradeEstate
                                }, JsonRequestBehavior.AllowGet);



                                break;

                            default:

                                return Json(new
                                {
                                    ErrorCode = 0x02,
                                    ErrMsg = "非法的类型代码"
                                }, JsonRequestBehavior.AllowGet);
                                break;


                        }
                    }
                    else
                    {
                        return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                    }


                    #endregion
                }
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = 0x03, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }


        public ActionResult GetBiddingAnalysis(string cityname,string regionname,string key)
        {
            Common.AddResponseHeader();
            DataTable dt;
            try
            {
                if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                {

                    if (regionname == "")
                    {

                        string sql1 = @"select City,AVG(avgprice) as avgprice,sum(biddingcount) as biddingcount,biddingdate from dbo.GPAnalysis with(nolock) where city='" + cityname + "'group by city,biddingdate";

                        System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql1);

                        var BiddingAnalysis = DataTableToListModel<BiddingAnalysis>.ConvertToModel(result1.Tables[0]);

                        dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetBiddingAnalysis|cityname = " + cityname + "|regionname = " + regionname );

                        SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                        return Json(new
                        {
                            ErrorCode = 0x00,
                            BiddingAnalysis
                        }, JsonRequestBehavior.AllowGet);

                    }
                    else
                    {
                        string sql1 = @"select City,district,AVG(avgprice) as avgprice,sum(biddingcount) as biddingcount,biddingdate from dbo.GPAnalysis with(nolock) where city='" + cityname + "' and district = '"+regionname+
                            "'group by city,district,biddingdate";

                        System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql1);

                        var BiddingAnalysis = DataTableToListModel<BiddingAnalysis>.ConvertToModel(result1.Tables[0]);

                        dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetBiddingAnalysis|cityname = " + cityname + "|regionname = " + regionname);

                        SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                        return Json(new
                        {
                            ErrorCode = 0x00,
                            BiddingAnalysis
                        }, JsonRequestBehavior.AllowGet);
                    }

                }

                else
                {
                    return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = 0x02, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);

            }


        }

        public ActionResult GetBiddingAnalysisEstate(string cityname, string estatename, string key)
        {
            Common.AddResponseHeader();
            DataTable dt;
            try
            {
                if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                {
                    string sql1 = @"select city,districtname,estateiD,estatename,biddingdate,biddingcount,avgprice from dbo.GPAnalysisEstate with(nolock) where city='" + cityname + "' and estatename = '" + estatename+"'";

                    System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql1);

                    var BiddingAnalysis = DataTableToListModel<EstateInfoBiddingAnalysis>.ConvertToModel(result1.Tables[0]);

                    dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetBiddingAnalysisEstate|cityname = " + cityname + "|estatename = " + estatename);

                    SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                    return Json(new
                    {
                        ErrorCode = 0x00,
                        BiddingAnalysis
                    }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = 0x02, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        public ActionResult GetBiddingAnalysisMultiEstate(string cityname,string estatename,string key)
        {
            Common.AddResponseHeader();
            DataTable dt;
            try
            {
                if (key == "C1B2CCC2-517F-418F-89AA-D4A68718B20E")
                {

                    string[] splitestate = estatename.Split(new Char[] { ','});

                    string queryestate = "";

                    for (int i = 0; i < splitestate.Length; i++)
                    {
                         queryestate += "'" + splitestate[i] + "',";
                    }

                    queryestate = queryestate.Substring(0, queryestate.Length - 1);

                    string sql1 = @"select city,districtname,estateiD,estatename,biddingdate,biddingcount,avgprice from dbo.GPAnalysisEstate with(nolock) where city='" + cityname + "' and estatename in (" + queryestate + ")";

                    System.Data.DataSet result1 = SimpleDataHelper.Query(SimpleDataHelper.CoreCenterConnectionString, sql1);

                    var BiddingAnalysis = DataTableToListModel<EstateInfoBiddingAnalysis>.ConvertToModel(result1.Tables[0]);

                    dt = SimpleDataHelper.GetAPILOGDataTable(key, "GetBiddingAnalysisMultiEstate|cityname = " + cityname + "|estatename = " + estatename);

                    SimpleDataHelper.SqlBCP(SimpleDataHelper.APILOGConnectionString, dt, "API_LOG");

                    return Json(new
                    {
                        ErrorCode = 0x00,
                        BiddingAnalysis
                    }, JsonRequestBehavior.AllowGet);
                }

                else
                {
                    return Json(new { ErrorCode = 0x01, ErrMsg = "非法的Key值" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { ErrorCode = 0x02, ErrMsg = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }


        ///<summary>
        ///JSON反序列化
        /// </summary>
        public static T JsonDeserialize<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }



    }

    class EstateInfo
    {
        public string CityNo { get; set; }
        public string AreaNo { get; set; }
        public string AreaName { get; set; }
        public string DistrictNo { get; set; }
        public string DistrictName { get; set; }

        public string EstateID { get; set; }

        public string EstateName { get; set; }
        public string Address { get; set; }

        public double CoX { get; set; }

        public double CoY { get; set; }

        public string BuildDate { get; set; }


    }


    class EstateInfoSimple
    {
        public string cityname { get; set; }

        public Guid UnitID { get; set; }


        public string OfficalName { get; set; }

        public string region { get; set; }

    }

    class BiddingAnalysis
    {
        public string city { get; set; }

        public string district { get; set; }


        public double avgprice { get; set; }

        public int biddingcount { get; set; }

        public string biddingdate { get; set; }
    }

    class EstateInfoBiddingAnalysis
    {
        public string city { get; set; }


        public string districtname { get; set; }

        public string estateid { get; set; }

        public string estatename { get; set; }

        public string biddingdate { get; set; }

        public int biddingcount { get; set; }

        public double avgprice { get; set; }

    }

}