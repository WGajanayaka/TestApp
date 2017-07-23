using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using SchoolHealthManagement.Models;
using System.IO;
using System.Data.OleDb;
using OfficeOpenXml;
using System.Web.UI.WebControls;
using ClosedXML.Excel;
using System.Collections;
using System.Web.Helpers;
using log4net;

namespace SchoolHealthManagement.Controllers
{
    public class HomeController : Controller
    {
        private string LoggedUserName => Session["UserName"].ToString();
        
        private readonly  ILog _log;
        //
        // GET: /Home/
        private readonly string _strConnection;

        public HomeController()
        {
            _strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            _log = LogManager.GetLogger(typeof(HomeController));
        }
        public ActionResult Home()
        { 
           
            //string ParentFolder = Server.MapPath("~/Circulars");
            //string[] SubDirs = Directory.GetDirectories(ParentFolder);

            //List<string> subDirs = new List<string>();
            //for (int i = 0; i < SubDirs.Length; i++)
            //{
            //    subDirs.Add(SubDirs[i].Replace(ParentFolder, "").Replace("\\",""));
            //}

            //ViewBag.SubDirs = subDirs;


            return View();

        }

        public ActionResult Impress_Release()
        {
            ImpressReleaseModel impRel = new ImpressReleaseModel();

            ViewBag.Provinces = GetProvinces();

            List<SelectListItem> Zones = GetZones();

            Zones.Clear();

            ViewBag.Zones = Zones;

            ViewBag.Years = GetYearsInt();

            return View(impRel);
        }

        [HttpPost]
        public ActionResult Impress_Release(ImpressReleaseModel model)
        {
            if (ModelState.IsValid)
            {
                if (Save_ImpressRelease(model))
                {
                    ModelState.Clear();
                    model = new ImpressReleaseModel();

                }
            }
            else
            {

                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            ViewBag.Provinces = GetProvinces();

            List<SelectListItem> Zones = GetZones();

            Zones.Clear();

            ViewBag.Zones = Zones;

            ViewBag.Years = GetYearsInt();

            return View(model);
        }

        private ActionResult GetImpressRelease4Zone()
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                        "m_Schools.CensorsID, ImpressRequest.ZonalRequestAmount, ImpressRequest.PDEReleasedAmount, ImpressRequest.ForMonth, ImpressRequest.ReleasedDate " +
                                    "FROM " +
                                        "m_Schools INNER JOIN ImpressRequest  ON m_Schools.CensorsID = ImpressRequest.CensorsID " +
                                    "WHERE     (m_Schools.ProvinceID = (SELECT m_Schools.ProvinceID FROM m_Schools " + "'))"; // WHERE m_Schools.CensorsID = '" + CensorsID + "'))";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ImpressReleaseModel> lstRcvdFunds = new List<ImpressReleaseModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    lstRcvdFunds.Add(new ImpressReleaseModel
                    {
                        //CensorsID = mydataRow["CensorsID"].ToString().Trim(),
                        ZonalRequestAmount = Convert.ToDecimal(mydataRow["ZonalRequestAmount"]),
                        //SchoolName = Convert.ToDateTime(mydataRow["PaidDate"]).ToString("dd-MMM-yyyy"),
                        PDEReleasedAmount = Convert.ToDecimal(mydataRow["PDEReleasedAmount"]),
                        ForMonth = Convert.ToInt32(mydataRow["ForMonth"]),
                        ReleasedDate = Convert.ToDateTime(mydataRow["ReleasedDate"])

                    });
                }

                return Json(lstRcvdFunds);

            }
        }

        private bool Save_ImpressRelease(ImpressReleaseModel model)
        {
            try
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "INSERT INTO ImpressRelease ( PovinceID, ZoneID, ZonalRequestAmount, PDEReleasedAmount, Month, Year, ReleasedDate, CreateUser) " +
                                        "VALUES(@PovinceID, @ZoneID, @ZonalRequestAmount, @PDEReleasedAmount, @Month, @Year, @ReleasedDate, @CreateUser)";

                    cmd.Parameters.AddWithValue("@PovinceID", model.ProvinceID);
                    cmd.Parameters.AddWithValue("@ZoneID", model.ZoneID);
                    cmd.Parameters.AddWithValue("@ZonalRequestAmount", model.ZonalRequestAmount);
                    cmd.Parameters.AddWithValue("@PDEReleasedAmount", model.PDEReleasedAmount);
                    cmd.Parameters.AddWithValue("@Month", model.ForMonth);
                    cmd.Parameters.AddWithValue("@Year", model.ForYear);
                    cmd.Parameters.AddWithValue("@ReleasedDate", model.ReleasedDate);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception Err)
            {
                string ss = Err.Message;
                return false;
            }
        }

        public ActionResult MOE_Impress_Summary()
        {
            ViewBag.Provinces = GetProvinces();

            ViewBag.Years = GetYearsInt();

            List<ZoneImpressSummaryModel> newLstZISM = new List<ZoneImpressSummaryModel>();

            ViewBag.ZoneImpressInfo = newLstZISM;

            return View();
        }

        private List<SelectListItem>  GetYearsInt()
        {
            List<int> lstYears =  Enumerable.Range(2015, 50).ToList();

            List<SelectListItem> SelYears = new List<SelectListItem>();
            for (int i = 0; i < lstYears.Count; i++)
            {
                SelectListItem newItem = new SelectListItem();
                newItem.Text = lstYears[i].ToString();
                newItem.Value = lstYears[i].ToString();
                SelYears.Add(newItem);
            }

            return SelYears;
        }

        [HttpPost]
        public ActionResult MOE_Impress_Summary(ImpressSummaryModel model) //string strProvinceID)
        {
            ViewBag.Provinces = GetProvinces();
            List<int> lstYears = Enumerable.Range(2015, 50).ToList();
            List<SelectListItem> SelYears = new List<SelectListItem>();
            for (int i = 0; i < lstYears.Count; i++)
            {
                SelectListItem newItem = new SelectListItem();
                newItem.Text = lstYears[i].ToString();
                newItem.Value = lstYears[i].ToString();
                SelYears.Add(newItem);
            }
            ViewBag.Years = SelYears;

            List<ZoneImpressSummaryModel> newLstZISM = new List<ZoneImpressSummaryModel>();

            ZoneImpressSummaryModel newLst = new ZoneImpressSummaryModel();
            newLst.NoOfStudents = 78;
            newLst.ZoneName = "ttttt";
            newLstZISM.Add(newLst);

            ViewBag.ZoneImpressInfo = newLstZISM;

            //ImpressSummaryModel newMod = new ImpressSummaryModel();
            //newMod.ProvinceID = strProvinceID;

            return View();
        }

        public ActionResult CreateLine()
        {

            //Create bar chart
            var chart = new Chart(width: 600, height: 200)
            .AddSeries(chartType: "line",
                            xValue: new[] { "10 ", "50", "30 ", "70" },
                            yValues: new[] { "50", "70", "90", "110" })
                            .GetBytes("png");
            return File(chart, "image/bytes");
        }

        public ActionResult DrawChart(string ProvinceID)
        {
            //string sCar = Request.Form["ProvinceID"];

            if (Request.QueryString["ProvinceID"] != null)
            {
                string ss = Request.QueryString["ProvinceID"].ToString();
            }
           

            DataTable dtWPA = PopulateWeightVsAge();
            System.Data.DataView data = dtWPA.AsDataView();
            //System.Data.DataView dataBoys = PopulateStudentsWeightPerAge(ProvinceID, "dsg", "Dsgs", "").AsDataView();
            var myChart = new Chart(width: 500, height: 400)
                            .AddTitle("Weight Per Age")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                                        yValues: data, yFields: "ClassM3D")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassM2D")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassM1D")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassMedian")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassP1D")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassP2D")
                            .AddSeries(chartType: "Line", xValue: data, xField: "AgeInMonths",
                            yValues: data, yFields: "ClassP3D")
                //.DataBindTable(dataSource: data, xField: "AgeInMonths")
                             .AddSeries(chartType: "Point", xValue: ViewBag.Boys, xField: "AgeInMonths",
                            yValues: ViewBag.Boys, yFields: "Weight")

                            .GetBytes("png");


            return File(myChart, "image/bytes");
        }

        //public ActionResult DrawChart()
        //{
        //    var chart = new Chart(width: 300, height: 200)
        //        .AddSeries(
        //                    chartType: "bar",
        //                    xValue: new[] { "10 Records", "20 Records", "30 Records", "40 Records" },
        //                    yValues: new[] { "50", "60", "78", "80" })
        //                    .GetBytes("png");
        //    return File(chart, "image/bytes");
        //}

        public ActionResult ReportWeightvsAge()
        {
            List<SelectListItem> provList = GetProvinces();
            provList.Add(new SelectListItem { Text = "All", Value = "All" });
            provList.OrderBy(x => x.Text);
            ViewBag.Provinces = provList;

            List<SelectListItem> ZoneList = GetZones();
            ZoneList.Add(new SelectListItem { Text = "All", Value = "All" });
            ZoneList.OrderBy(x=>x.Text);
            ViewBag.Zones = ZoneList;

            List<SelectListItem> DivList = GetDevisions("");
            DivList.Add(new SelectListItem { Text = "All", Value = "All" });
            DivList.OrderBy(x => x.Text);
            ViewBag.Devisions = DivList;

            ViewBag.TotalRangesBoys = new List<ReportBMIMeasurePerAgeModel>();
            ViewBag.TotalRangesGirls = new List<ReportBMIMeasurePerAgeModel>();
            //DataTable dtWPA = PopulateWeightVsAge();
            //ViewBag.Data = dtWPA.AsDataView();
            //ViewBag.Boys = PopulateStudentsWeightPerAge("", "", "", "").AsDataView();
          
            return View();
        }

        public ActionResult ReportBMIProvincesInformation()
        {

            ViewBag.TotalBMIInfo = GetBMITotalInfoProvinces();
            return View();

        }

        public ActionResult ReportBMIZonesInformation(string strProvince)
        {

            ViewBag.TotalBMIInfo = GetBMITotalInfoZones(strProvince);
            return View();

        }

        

        [HttpPost]
        public ActionResult ReportWeightvsAge(string ProvinceID, string ZoneID, string DevisionID, string Gender)
        {
            //ViewBag.Provinces = GetProvinces();
            //ViewBag.Zones = GetZones();
            //ViewBag.Devisions = GetDevisions();

            List<SelectListItem> provList = GetProvinces();
            provList.Add(new SelectListItem { Text = "All", Value = "All" });
            provList.OrderBy(x => x.Text);
            ViewBag.Provinces = provList;

            List<SelectListItem> ZoneList = GetZones();
            ZoneList.Add(new SelectListItem { Text = "All", Value = "All" });
            ZoneList.OrderBy(x => x.Text);
            ViewBag.Zones = ZoneList;

            List<SelectListItem> DivList = GetDevisions(DevisionID);
            DivList.Add(new SelectListItem { Text = "All", Value = "All" });
            DivList.OrderBy(x => x.Text);
            ViewBag.Devisions = DivList;


            DataTable dtWPA = PopulateWeightVsAge();
            ViewBag.Data = dtWPA.AsDataView();
            //ViewBag.Boys = PopulateStudentsWeightPerAge(ProvinceID, ZoneID, DevisionID, "").AsDataView();
            ViewBag.TotalRangesBoys = GetWeightPerAgeTotalInfo(ProvinceID, ZoneID, DevisionID, "Boy", "Weight");
            ViewBag.TotalRangesGirls = GetWeightPerAgeTotalInfo(ProvinceID, ZoneID, DevisionID,  "Girl", "Weight");

            //Array test = new int[][ 12, 19, 3, 17, 6, 3, 7 ];
            //int[] test = {12, 19, 3, 17, 6, 3, 7};
            //ViewBag.ChartData = test;
            @ViewBag.PropertyName =  "1~23~28~56~123";
            return View();
        }

        private List<ReportBMIMeasurePerAgeModel> GetWeightPerAgeTotalInfo(string ProvinceID, string ZoneID, string DevisionID, string Gender, string BMIMeasure)
        {
            List<ReportBMIMeasurePerAgeModel> lstBMIMeasPAge = new List<ReportBMIMeasurePerAgeModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                string strBMIMeasure = "";
                if (BMIMeasure == "Weight")
                {
                    strBMIMeasure = "W";
                }
                else if (BMIMeasure == "Height")
                {
                    strBMIMeasure = "H";
                }

                string strGender = "";
                if (Gender == "Boy")
                {
                    strGender = "Male";
                }
                else if (Gender == "Girl")
                {
                    strGender = "Female";
                }

                string strLocationFilter = "";

                if (ProvinceID == "All")
                {
                    strLocationFilter = "";
                }
                else if (ZoneID == "All")
                {
                    strLocationFilter = (ProvinceID != "0" ? "m_Schools.ProvinceID = " + ProvinceID + "" : "");
                }
                else if (DevisionID == "All")
                {
                    strLocationFilter = (ProvinceID != "0" ? "m_Schools.ProvinceID = " + ProvinceID + "" : "") +
                                              (ZoneID != "0" ? "  AND  m_Schools.ZoneID = '" + ZoneID + "'" : "");
                }
                else
                {
                    strLocationFilter = (ProvinceID != "0" ? "m_Schools.ProvinceID = " + ProvinceID + "" : "") +
                                              (ZoneID != "0" ? "  AND  m_Schools.ZoneID = '" + ZoneID + "'" : "") + (DevisionID != "0" ? "  AND  m_Schools.DevisionID = '" + DevisionID + "'" : "");
                }

                conn.Open();
                cmd.CommandText = "SELECT " +
                                            "'Underweight' As Category, count(*) AS CountOfStudents " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_WeightHeightPerAge.[Class-2D] " +
                                            " AND Measure = 'W' " +
                                            " AND m_WeightHeightPerAge.Gender = '" + Gender + "' " +
                                            " AND BMIInformation.Gender = '" + strGender + "'" + (strLocationFilter == "" ? "" : " AND ") + strLocationFilter +
                                    " UNION " +                    
                                     "SELECT " +
                                            "'Normal' As Category, count(*) AS CountOfStudents " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight between m_WeightHeightPerAge.[Class-1D] and m_WeightHeightPerAge.[Class+1D] " +
                                            " AND Measure = 'W' " +
                                            " AND m_WeightHeightPerAge.Gender = '" + Gender + "' " +
                                            " AND BMIInformation.Gender = '" + strGender + "'" + (strLocationFilter==""?"":" AND ") + strLocationFilter +
                                    " UNION " +
                                    "SELECT " +
                                            "'Stunning' As Category, count(*) AS CountOfStudents " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Height < m_WeightHeightPerAge.[Class-2D] " +
                                            " AND Measure = 'H' " +
                                            " AND m_WeightHeightPerAge.Gender = '" + Gender + "' " +
                                            " AND BMIInformation.Gender = '" + strGender + "'" + (strLocationFilter == "" ? "" : " AND ") + strLocationFilter +
                                    " UNION " +
                                    "SELECT " +
                                            "'Wasting' As Category, count(*) AS CountOfStudents " +
                                    "FROM BMIInformation " +
                                            "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_Weight4Height.SD2neg " +
                                            " AND m_Weight4Height.Gender = '" + Gender + "' " +
                                            " AND BMIInformation.Gender = '" + strGender + "'" + (strLocationFilter == "" ? "" : " AND ") + strLocationFilter +
                                    " UNION " +
                                   "SELECT " +
                                           "'Overweight' As Category, count(*) AS CountOfStudents " +
                                   "FROM BMIInformation " +
                                           "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height " +
                                           "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                            " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                            " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                            " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                   "WHERE " +
                                           "BMIInformation.Weight > m_Weight4Height.SD2neg " +
                                           " AND m_Weight4Height.Gender = '" + Gender + "' " +
                                           " AND BMIInformation.Gender = '" + strGender + "'" + (strLocationFilter == "" ? "" : " AND ") + strLocationFilter;


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ReportBMIMeasurePerAgeModel thisRow = new ReportBMIMeasurePerAgeModel();

                    thisRow.Category = Convert.ToString(dt.Rows[i]["Category"]);
                    thisRow.Count = Convert.ToInt32(dt.Rows[i]["CountOfStudents"]);

                    lstBMIMeasPAge.Add(thisRow);
                }

                return lstBMIMeasPAge;
            }
        }

        private List<ReportBMIReportModel> GetBMITotalInfoProvinces()
        {
            List<ReportBMIReportModel> lstBMIrpt = new List<ReportBMIReportModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT A.ProvinceName, A.ProvinceID, (" + 
                                        
                    
                                    "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_WeightHeightPerAge.[Class-2D] AND m_Provinces.ProvinceID = A.ProvinceID " +
                                            " AND Measure = 'W' "  +
                                    " GROUP BY m_Provinces.ProvinceID " + ") AS Underweight " +
                                    ", ( " +
                                     "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight between m_WeightHeightPerAge.[Class-1D] and m_WeightHeightPerAge.[Class+1D] " +
                                            " AND m_Provinces.ProvinceID = A.ProvinceID AND Measure = 'W' " +
                                    " GROUP BY m_Provinces.ProvinceID " + ") AS Healthy " +
                                    ", ( " +
                                    "SELECT " +
                                            "count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Height < m_WeightHeightPerAge.[Class-2D] " +
                                            " AND m_Provinces.ProvinceID = A.ProvinceID AND Measure = 'H' " +
                                    " GROUP BY m_Provinces.ProvinceID " + ") AS Stunning " +                                            
                                    ", ( " +
                                    "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height and (BMIInformation.Gender = (CASE WHEN m_Weight4Height.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_Weight4Height.SD2neg AND m_Provinces.ProvinceID = A.ProvinceID " +
                                     " GROUP BY m_Provinces.ProvinceID " + ") AS Wasting " +        
                                    ", ( " +
                                   "SELECT " +
                                           "count(*) " +
                                   "FROM BMIInformation " +
                                           "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height and (BMIInformation.Gender = (CASE WHEN m_Weight4Height.Gender = 'Boy' THEN 'Male' ELSE 'Female' END)) " +
                                           "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                            " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                            " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                            " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                   "WHERE " +
                                           "BMIInformation.Weight > m_Weight4Height.SD2neg  AND m_Provinces.ProvinceID = A.ProvinceID " +
                                   " GROUP BY m_Provinces.ProvinceID " + ") AS Overweight " +
                                   ", ( " +
                                   "SELECT " +
                                           "count(*) " +
                                   "FROM BMIInformation " +
                                           "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                            " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                            " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                            " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                   "WHERE " +
                                            "m_Provinces.ProvinceID = A.ProvinceID " +
                                   " GROUP BY m_Provinces.ProvinceID " + ") AS CountAll " + 
                                   " FROM  m_Provinces A";


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ReportBMIReportModel thisRow = new ReportBMIReportModel();

                    thisRow.ProvinceID = Convert.ToString(dt.Rows[i]["ProvinceID"]);
                    thisRow.ProvinceName = Convert.ToString(dt.Rows[i]["ProvinceName"]);
                    thisRow.Healthy = Convert.ToInt32((dt.Rows[i]["Healthy"] == DBNull.Value ? 0 : dt.Rows[i]["Healthy"]));
                    thisRow.Stunning = Convert.ToInt32((dt.Rows[i]["Stunning"] == DBNull.Value ? 0 : dt.Rows[i]["Stunning"]));
                    thisRow.Wasting = Convert.ToInt32((dt.Rows[i]["Wasting"]==DBNull.Value?0:dt.Rows[i]["Wasting"]));
                    thisRow.Overweight = Convert.ToInt32((dt.Rows[i]["Overweight"]==DBNull.Value?0:dt.Rows[i]["Overweight"]));
                    thisRow.Underweight = Convert.ToInt32((dt.Rows[i]["Underweight"] == DBNull.Value ? 0 : dt.Rows[i]["Underweight"]));
                    thisRow.CountAll = Convert.ToInt32(dt.Rows[i]["CountAll"]);

                    lstBMIrpt.Add(thisRow);
                }

                return lstBMIrpt;
            }
        }

        private List<ReportBMIReportModel> GetBMITotalInfoZones(string strProvince)
        {
            List<ReportBMIReportModel> lstBMIrpt = new List<ReportBMIReportModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT B.ZoneName, B.ZoneID, (" +
                                    "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END)) " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_WeightHeightPerAge.[Class-2D] AND m_Provinces.ProvinceID = A.ProvinceID  AND m_Zones.ZoneID = B.ZoneID " +
                                            " AND Measure = 'W' AND m_Provinces.ProvinceID = " + strProvince +
                                    " GROUP BY m_Zones.ZoneID " + ") AS Underweight " +
                                    ", ( " +
                                     "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END)) " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight between m_WeightHeightPerAge.[Class-1D] and m_WeightHeightPerAge.[Class+1D] " +
                                            " AND m_Provinces.ProvinceID = A.ProvinceID AND Measure = 'W'  AND m_Zones.ZoneID = B.ZoneID  AND m_Provinces.ProvinceID = " + strProvince +
                                    " GROUP BY m_Zones.ZoneID " + ") AS Healthy " +
                                    ", ( " +
                                    "SELECT " +
                                            "count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_WeightHeightPerAge ON DATEDIFF(month, BMIInformation.DOB,BMIInformation.DatePerformed) = m_WeightHeightPerAge.AgeInMonths and (BMIInformation.Gender = (CASE WHEN m_WeightHeightPerAge.Gender = 'Boy' THEN 'Male' ELSE 'Female' END)) " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Height < m_WeightHeightPerAge.[Class-2D] " +
                                            " AND m_Provinces.ProvinceID = A.ProvinceID AND Measure = 'H'  AND m_Zones.ZoneID = B.ZoneID  AND m_Provinces.ProvinceID = " + strProvince +
                                    " GROUP BY m_Zones.ZoneID " + ") AS Stunning " +
                                    ", ( " +
                                    "SELECT " +
                                            " count(*) " +
                                    "FROM BMIInformation " +
                                            "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height and (BMIInformation.Gender = (CASE WHEN m_Weight4Height.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                            "BMIInformation.Weight < m_Weight4Height.SD2neg AND m_Provinces.ProvinceID = A.ProvinceID AND m_Zones.ZoneID = B.ZoneID  AND m_Provinces.ProvinceID = " + strProvince +
                                     " GROUP BY m_Zones.ZoneID " + ") AS Wasting " +
                                    ", ( " +
                                   "SELECT " +
                                           "count(*) " +
                                   "FROM BMIInformation " +
                                           "inner join m_Weight4Height ON CAST(BMIInformation.Height AS decimal(18,1)) = m_Weight4Height.Height and (BMIInformation.Gender = (CASE WHEN m_Weight4Height.Gender = 'Boy' THEN 'Male' ELSE 'Female' END))  " +
                                           "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                            " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                            " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                            " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                   "WHERE " +
                                           "BMIInformation.Weight > m_Weight4Height.SD2neg  AND m_Provinces.ProvinceID = A.ProvinceID AND m_Zones.ZoneID = B.ZoneID  AND m_Provinces.ProvinceID = " + strProvince +
                                   " GROUP BY m_Zones.ZoneID " + ") AS Overweight " +
                                   ", ( " +
                                   "SELECT " +
                                           "count(*) " +
                                   "FROM BMIInformation " +
                                           "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                            " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                            " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                            " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                   "WHERE " +
                                            "m_Provinces.ProvinceID = A.ProvinceID AND m_Zones.ZoneID = B.ZoneID  AND m_Provinces.ProvinceID = " + strProvince +
                                   " GROUP BY m_Zones.ZoneID " + ") AS CountAll " + 
                                   " FROM  m_Provinces A INNER JOIN " +
                                    " m_Zones B ON A.ProvinceID = B.ProvinceID WHERE A.ProvinceID = " + strProvince;


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ReportBMIReportModel thisRow = new ReportBMIReportModel();

                    thisRow.ZoneName = Convert.ToString(dt.Rows[i]["ZoneName"]);
                    thisRow.ZoneID = Convert.ToString(dt.Rows[i]["ZoneID"]);
                    thisRow.Healthy = Convert.ToInt32((dt.Rows[i]["Healthy"] == DBNull.Value ? 0 : dt.Rows[i]["Healthy"]));
                    thisRow.Stunning = Convert.ToInt32((dt.Rows[i]["Stunning"] == DBNull.Value ? 0 : dt.Rows[i]["Stunning"]));
                    thisRow.Wasting = Convert.ToInt32((dt.Rows[i]["Wasting"] == DBNull.Value ? 0 : dt.Rows[i]["Wasting"]));
                    thisRow.Overweight = Convert.ToInt32((dt.Rows[i]["Overweight"] == DBNull.Value ? 0 : dt.Rows[i]["Overweight"]));
                    thisRow.Underweight = Convert.ToInt32((dt.Rows[i]["Underweight"] == DBNull.Value ? 0 : dt.Rows[i]["Underweight"]));
                    thisRow.CountAll = Convert.ToInt32((dt.Rows[i]["CountAll"] == DBNull.Value ? 0 : dt.Rows[i]["CountAll"]));

                    lstBMIrpt.Add(thisRow);
                }

                return lstBMIrpt;
            }
        }

        public DataTable PopulateStudentsWeightPerAge(string ProvinceID, string ZoneID, string DevisionID, string SchoolID)
        {
            DataTable dt = new DataTable();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                            "DATEDIFF(month, DOB, GETDATE()) AS AgeInMonths, Weight " +
                                    "FROM " +
                                            "BMIInformation " +
                                            "INNER JOIN  m_Schools ON BMIInformation.SchoolID = m_Schools.SchoolID INNER JOIN  " +
                                             " m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID INNER JOIN " +
                                             " m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN " +
                                             " m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID " +
                                    "WHERE " +
                                    "Gender = 'Male' " +  (ProvinceID != "0" && ProvinceID != null && ZoneID != "0" && ZoneID != null && DevisionID != "0" && DevisionID != null ?"AND " : "") + 
                                          (ProvinceID != "0" && ProvinceID != null ? "m_Schools.ProvinceID = " + ProvinceID + "" : "") +
                                          (ZoneID != "0" && ZoneID != null ? "  AND  m_Schools.ZoneID = '" + ZoneID + "'" : "") + (DevisionID != "0" && DevisionID != null ? "  AND  m_Schools.DevisionID = '" + DevisionID + "'" : "");

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        public DataTable PopulateWeightVsAge()
        {
            DataTable dt = new DataTable();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                        "AgeInMonths, [Class-3D] as ClassM3D, [Class-2D] as ClassM2D, [Class-1D] as ClassM1D, ClassMedian, [Class+1D] as ClassP1D, [Class+2D] as ClassP2D, [Class+3D] as ClassP3D " +
                                  "FROM " +
                                        "m_WeightHeightPerAge " +
                                  "WHERE " +
                                        "Measure = 'W' AND Gender = 'Boy'";

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }

            return dt;
        }

        public ActionResult ReportSuppliers()
        {
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions("");

            ViewBag.Suppliers = new List<SupplierInfoModel>();


            return View();
        }

        [HttpPost]
        public ActionResult ReportSuppliers(string ProvinceID, string ZoneID, string DevisionID)
        {
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions(DevisionID);
            ViewBag.Suppliers = GetSupplierInfo(ProvinceID, ZoneID, DevisionID);
            return View();

        }

        private List<SupplierInfoModel> GetSupplierInfo(string strProvinceID, string ZoneID, string DevisionID)
        {
            List<SupplierInfoModel> lstSup = new List<SupplierInfoModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                         "m_Schools.CensorsID, m_Schools.SchoolName, a.SupplierName, a.Address, a.NIC, a.Phone, " +
                                         " a.BankName, a.BankAccountNo, " +
                                        "STUFF((SELECT DISTINCT ',' + Grade FROM m_SupplierInformation WHERE NIC = a.NIC FOR XML PATH ('')), 1, 1, '')  AS Grade, " +
                                        "SUM(a.NoOfMaleStudents) AS NoOfMaleStudents, SUM(a.NoOfFemaleStudents) AS NoOfFemaleStudents " +
                                  "FROM " +
                                           "m_SupplierInformation a INNER JOIN m_Schools ON a.SchoolID  = m_Schools.SchoolID  " +
                                  "WHERE " +
                                  (strProvinceID != "0" ? "m_Schools.ProvinceID = " + strProvinceID + "" : "") +
                                  (ZoneID != "0" ? "  AND  m_Schools.ZoneID = '" + ZoneID + "'" : "") + (DevisionID != "0" ? "  AND  m_Schools.DevisionID = '" + DevisionID + "'" : "") +
                                  " group by m_Schools.CensorsID, m_Schools.SchoolName, a.SupplierName, a.Address, a.NIC, a.Phone, a.BankName, a.BankAccountNo ";

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SupplierInfoModel thisRow = new SupplierInfoModel();
                    //thisRow.Province = Convert.ToString(dt.Rows[i]["ProvinceName"]);
                    thisRow.CensusID = Convert.ToString(dt.Rows[i]["CensorsID"]);
                    thisRow.SchoolName = Convert.ToString(dt.Rows[i]["SchoolName"]);
                    thisRow.SupplierName = Convert.ToString(dt.Rows[i]["SupplierName"]);
                    thisRow.Address = Convert.ToString(dt.Rows[i]["Address"]);
                    thisRow.NIC = Convert.ToString(dt.Rows[i]["NIC"]);
                    thisRow.Phone = Convert.ToString(dt.Rows[i]["Phone"]);
                    thisRow.BankName = Convert.ToString(dt.Rows[i]["BankName"]);
                    thisRow.BankAccountNo = Convert.ToString(dt.Rows[i]["BankAccountNo"]);
                    thisRow.Grade = Convert.ToString(dt.Rows[i]["Grade"]);
                    thisRow.NoOfMaleStudents = Convert.ToInt32(dt.Rows[i]["NoOfMaleStudents"]);
                    thisRow.NoOfFemaleStudents = Convert.ToInt32(dt.Rows[i]["NoOfFemaleStudents"]);
                    

                    lstSup.Add(thisRow);
                }

                return lstSup;
            }
        }

        public ActionResult ReportMonitoringOfficers()
        {
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions("");

            ViewBag.MoniteringOfficers = new List<MonitoringOfficerModel>();

            
            return View();
        }

        [HttpPost]
        public ActionResult ReportMonitoringOfficers(string ProvinceID, string ZoneID, string DevisionID)
        {
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions(DevisionID);
            ViewBag.MoniteringOfficers = GetMonOffrInfo(ProvinceID, ZoneID, DevisionID);
            return View();

        }

        private List<MonitoringOfficerModel> GetMonOffrInfo(string strProvinceID, string ZoneID, string DevisionID)
        {
            List<MonitoringOfficerModel> lstMonOff= new List<MonitoringOfficerModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT     MonitoringOfficerInformation.CensorsID, MonitoringOfficerInformation.NameOfOfficer, MonitoringOfficerInformation.Designation, MonitoringOfficerInformation.ContactNo, m_Schools.SchoolName " +
                                  "FROM " +
                                           "MonitoringOfficerInformation INNER JOIN m_Schools ON MonitoringOfficerInformation.CensorsID = m_Schools.CensorsID " +
                                  "WHERE " +
                                  (strProvinceID != "0"?"m_Schools.ProvinceID = " + strProvinceID + "" : "") +
                                  (ZoneID != "0" ? "  AND  m_Schools.ZoneID = '" + ZoneID + "'" : "") + (DevisionID != "0" ? "  AND  m_Schools.DevisionID = '" + DevisionID + "'" : "");
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MonitoringOfficerModel thisRow = new MonitoringOfficerModel();
                    //thisRow.Province = Convert.ToString(dt.Rows[i]["ProvinceName"]);
                    thisRow.CensorsID = Convert.ToString(dt.Rows[i]["CensorsID"]);
                    thisRow.SchoolName = Convert.ToString(dt.Rows[i]["SchoolName"]);
                    thisRow.NameOfOfficer = Convert.ToString(dt.Rows[i]["NameOfOfficer"]);
                    thisRow.Designation = Convert.ToString(dt.Rows[i]["Designation"]);
                    thisRow.ContactNo = Convert.ToString(dt.Rows[i]["ContactNo"]);


                    lstMonOff.Add(thisRow);
                }

                return lstMonOff;
            }
        }

        public ActionResult SDSBankInformation(string strProvinceID, string Province)
        {
            ViewBag.SDSBankInfo = GetSDSBankInformation(strProvinceID);
            ViewBag.ProvinceID = strProvinceID;
            ViewBag.ProvinceName = Province;
            return View();
        }

        private List<SDSBankInfoModel> GetSDSBankInformation(string strProvinceID)
        {
            List<SDSBankInfoModel> lstBankDet = new List<SDSBankInfoModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT * FROM SDS_Bank_List WHERE ProvinceID = " + strProvinceID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SDSBankInfoModel thisRow = new SDSBankInfoModel();
                    thisRow.Province = Convert.ToString(dt.Rows[i]["ProvinceName"]);
                    thisRow.Zone = Convert.ToString(dt.Rows[i]["ZoneName"]);
                    thisRow.SchoolName = Convert.ToString(dt.Rows[i]["SchoolName"]);
                    thisRow.Bank = Convert.ToString(dt.Rows[i]["Bank"]);
                    thisRow.BankAccountNo = Convert.ToString(dt.Rows[i]["BankAccountNo"]);
                    thisRow.CensorsID = Convert.ToString(dt.Rows[i]["CensorsID"]);


                    lstBankDet.Add(thisRow);
                }

                return lstBankDet;
            }
        }

        public ActionResult SupplierInformationZoneProvince(string strProvinceID, string strZoneID, string strZone)
        {
            ViewBag.SupplierInfo = GetSupplierInfo(strProvinceID, strZoneID);
            ViewBag.ZoneName = strZone;
            return View();
        }

        public ActionResult SupplierInformationProvince(string strProvinceID)
        {
            ViewBag.SupplierInfo = GetSupplierInfo(Convert.ToInt32(strProvinceID));
            return View();
        }

        public ActionResult PDFParentFolder(string strPDFType)
        {
            string ParentFolder = Server.MapPath("~/" + strPDFType);
            string[] SubDirs = Directory.GetDirectories(ParentFolder);

            List<string> subDirs = new List<string>();
            for (int i = 0; i < SubDirs.Length; i++)
            {
                subDirs.Add(SubDirs[i].Replace(ParentFolder, "").Replace("\\", ""));
            }

            ViewBag.SubDirs = subDirs;

            ViewBag.TitlePage = strPDFType;

            return View();

        }

        public ActionResult Gallery()
        {
            string ParentFolder = Server.MapPath("~/Gallery");
            string[] SubDirs = Directory.GetDirectories(ParentFolder);

            List<string> subDirs = new List<string>();
            for (int i = 0; i < SubDirs.Length; i++)
            {
                subDirs.Add(SubDirs[i].Replace(ParentFolder, "").Replace("\\", ""));
            }

            ViewBag.SubDirs = subDirs;

            //ViewBag.FolderIcon = Icon.ExtractAssociatedIcon(ParentFolder);

            return View();

           

        }

        public ActionResult DisplayGallery(string sFolder)
        {
            List<string> imgs = Directory.GetFiles(Path.Combine(Server.MapPath("~/Gallery"), sFolder), "*.jpg", System.IO.SearchOption.TopDirectoryOnly).Select(path => Path.GetFileName(path)).ToList();

            ViewBag.Images = imgs;

            ViewBag.Folder = sFolder;

            return View();
        }

        public ActionResult about()
        {

            return View();

        }

        public ActionResult ContactUs()
        {

            return View();

        }

        public ActionResult statistics()
        {

            return View();

        }

        public ActionResult DisplayFolderPDFs(string strMenuItem, string sFolder)
        {

            //List<string> Pdfs = Directory.GetFiles(Path.Combine(Server.MapPath("~/" + strMenuItem), sFolder), "*.pdf", System.IO.SearchOption.TopDirectoryOnly).ToList();
            List<string> Pdfs;

            if (sFolder == "All")
            {
                Pdfs = Directory.GetFiles(Path.Combine(Server.MapPath("~/" + strMenuItem), sFolder), "*.xlsx", System.IO.SearchOption.TopDirectoryOnly).ToList();

            }
            else
            {
                Pdfs = Directory.GetFiles(Path.Combine(Server.MapPath("~/" + strMenuItem), sFolder), "*.pdf", System.IO.SearchOption.TopDirectoryOnly).ToList();
            }

            List<FileModel> OutPdfs = new List<FileModel>();
            for (int i = 0; i < Pdfs.Count; i++)
            {
                FileModel newFile = new FileModel();
                newFile.FileName = Pdfs[i].Replace(Path.Combine(Server.MapPath("~/" + strMenuItem), sFolder) + "\\", "");

                FileInfo FleInf = new System.IO.FileInfo(Pdfs[i]);
                DateTime dtModTime = FleInf.LastWriteTime;
                long fleSize = FleInf.Length / 1024;   //in KB

                newFile.FileLastModified = dtModTime;
                newFile.FileSize = fleSize;

                newFile.FileDetails = "File Name    : " + FleInf.Name + "\nFile Size        : " + fleSize + " KB\nDate              : " + dtModTime.ToString("dd-MMM-yyyy");
                OutPdfs.Add(newFile);

            }
            ViewBag.FilesInFolder = OutPdfs;
            ViewBag.FolderPath = Path.Combine("~/" + strMenuItem, sFolder);
            ViewBag.TitlePage = sFolder;
            return View();
        }

        public ActionResult DownloadPDF(string strPath, string strPDF)
        {
            string filename = Path.Combine(strPath , strPDF);
            return File(filename, "application/pdf", strPDF);
        }

        public ActionResult SchoolHealthPromotionProgramme(string SchoolID)
        {
            _22CriteriasModel model = new _22CriteriasModel();

            model.SchoolID = SchoolID;

            GetSchoolHealthPromotionProgramme(model);

            return View(model);

        }

        public ActionResult ViewBMIHistory(string SchoolID)
        {

            ViewBag.BMIHistoryInfo = GetBMIHistory(SchoolID);
            return View();
        }


        public ActionResult ShowReport(string strReport)
        {

            return View();
        }

        public List<BMIHistoryInfo> GetBMIHistory(string SchoolID)
        {
            List<BMIHistoryInfo> lstBMIHisDet = new List<BMIHistoryInfo>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT  DatePerformed, TakenBy, Class, Trimester, AdmissionNo, DOB, Gender, Height, Weight, BMI " +
                                   " FROM " +
                                             "BMIInformation " +
                                    "WHERE SchoolID = '" + SchoolID + "' " +
                                    "ORDER BY Class, Trimester";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    BMIHistoryInfo thisRow = new BMIHistoryInfo();
                    thisRow.DatePerformed = Convert.ToDateTime(dt.Rows[i]["DatePerformed"]);
                    thisRow.TakenBy = Convert.ToString(dt.Rows[i]["TakenBy"]);
                    thisRow.Class = Convert.ToString(dt.Rows[i]["Class"]);
                    thisRow.Trimester = Convert.ToString(dt.Rows[i]["Trimester"]);
                    thisRow.AdmissionNo = Convert.ToString(dt.Rows[i]["AdmissionNo"]);
                    thisRow.DOB = Convert.ToDateTime(dt.Rows[i]["DOB"]);
                    thisRow.Gender = Convert.ToString(dt.Rows[i]["Gender"]);
                    thisRow.Height = Convert.ToDecimal(dt.Rows[i]["Height"]);
                    thisRow.Weight = Convert.ToDecimal(dt.Rows[i]["Weight"]);
                    thisRow.BMI = Convert.ToDecimal(dt.Rows[i]["BMI"]);

                    lstBMIHisDet.Add(thisRow);
                }

                return lstBMIHisDet;
            }
        }

        private void GetSchoolHealthPromotionProgramme(_22CriteriasModel model)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT SchoolHelthCommitee, StudentHealthSocity, FirstAidFacility, HealthMedicalTest, Reactions4IssuesOfMedicalTest, ContribOnSchHealthPrg, PublicHealthPrg, " +
                                              "SupplySanitoryFacilities, CleanlinessOfSanitoryFacilities, WaterSupply, StudentsAttendance, TeachersAttendance, ClassroomEnvironment, EnvironmentOfSchool, MinimizeNutritionProblems, " +
                                              "NutritionKnowledgeCompetence, StudentFitnessStatus, PysicalWellbeingProg, IndividualFitness, MaintananceOfCanteens, MentalEnvironment, InstructionsNGuidence " +
                                   " FROM " +
                                             "t_SHPP " +
                                    "WHERE SchoolID = '" + model.SchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    model.SchoolHelthCommitee = Convert.ToInt32(dt.Rows[0]["SchoolHelthCommitee"]);
                    model.StudentHealthSocity = Convert.ToInt32(dt.Rows[0]["StudentHealthSocity"]);
                    model.FirstAidFacility = Convert.ToInt32(dt.Rows[0]["FirstAidFacility"]);
                    model.MedicalTest = Convert.ToInt32(dt.Rows[0]["HealthMedicalTest"]);
                    model.Reactions4IssuesOfMedicalTest = Convert.ToInt32(dt.Rows[0]["Reactions4IssuesOfMedicalTest"]);
                    model.ContribOnSchHealthPrg = Convert.ToInt32(dt.Rows[0]["ContribOnSchHealthPrg"]);
                    model.PublicHealthPrg = Convert.ToInt32(dt.Rows[0]["PublicHealthPrg"]);
                    model.SupplySanitoryFacilities = Convert.ToInt32(dt.Rows[0]["SupplySanitoryFacilities"]);
                    model.CleanlinessOfSanitoryFacilities = Convert.ToInt32(dt.Rows[0]["CleanlinessOfSanitoryFacilities"]);
                    model.WaterSupply = Convert.ToInt32(dt.Rows[0]["WaterSupply"]);
                    model.StudentsAttendance = Convert.ToInt32(dt.Rows[0]["StudentsAttendance"]);
                    model.TeachersAttendance = Convert.ToInt32(dt.Rows[0]["TeachersAttendance"]);
                    model.ClassroomEnvironment = Convert.ToInt32(dt.Rows[0]["ClassroomEnvironment"]);
                    model.EnvironmentOfSchool = Convert.ToInt32(dt.Rows[0]["EnvironmentOfSchool"]);
                    model.MinimizeNutritionProblems = Convert.ToInt32(dt.Rows[0]["MinimizeNutritionProblems"]);
                    model.NutritionKnowledgeCompetence = Convert.ToInt32(dt.Rows[0]["NutritionKnowledgeCompetence"]);
                    model.StudentFitnessStatus = Convert.ToInt32(dt.Rows[0]["StudentFitnessStatus"]);
                    model.PysicalWellbeingProg = Convert.ToInt32(dt.Rows[0]["PysicalWellbeingProg"]);
                    model.IndividualFitness = Convert.ToInt32(dt.Rows[0]["IndividualFitness"]);
                    model.MaintananceOfCanteens = Convert.ToInt32(dt.Rows[0]["MaintananceOfCanteens"]);
                    model.MentalEnvironment = Convert.ToInt32(dt.Rows[0]["MentalEnvironment"]);
                    model.InstructionsNGuidence = Convert.ToInt32(dt.Rows[0]["InstructionsNGuidence"]);
                }
                
            }
        }

        [HttpPost]
        public ActionResult SchoolHealthPromotionProgramme(_22CriteriasModel model)
        {
            if (ModelState.IsValid)
            {
                if (ValidatedSHPP(model))
                    Save_SchoolHealthPromotionProgramme(model);

            }
            else
            {
                ModelState.AddModelError("Error", "Please specify required fields.");
            }
            return View(model);

        }

        private bool ValidatedSHPP(_22CriteriasModel model)
        {
            string Error = "";
            bool bError = false;
            if (model.InstructionsNGuidence > 3 || model.InstructionsNGuidence < 1)
            {
                Error = "Instructions and Guidence value should be between 1-3";
                bError = true;
            }

            if (model.EnvironmentOfSchool > 5 || model.EnvironmentOfSchool < 1)
            {
                Error += (Error == "" ? "" : ",") + "Environment Of School value should be between 1-5";
                bError = true;
            }

            if (bError)
            {
                ModelState.AddModelError("Error", Error);
                return false;
            }
            else
            {
                return true;
            }

        }

        private bool Save_SchoolHealthPromotionProgramme(_22CriteriasModel model)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO BMIInformation (SchoolID, TakenBy, DatePerformed, Class, AdmissionNo, DOB, Gender, Height, Weight, BMI, CreateDate, CreateUser) " +
                    //                    "VALUES(@SchoolID, @TakenBy, @DatePerformed, @Class, @AdmissionNo, @DOB, @Gender, @Height, @Weight, @BMI, GETDATE(),  @CreateUser)";
                    cmd.CommandText = "UpdateSHPP";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@SchoolHelthCommitee", model.SchoolHelthCommitee);
                    cmd.Parameters.AddWithValue("@StudentHealthSocity", model.StudentHealthSocity);
                    cmd.Parameters.AddWithValue("@FirstAidFacility", model.FirstAidFacility);
                    cmd.Parameters.AddWithValue("@HealthMedicalTest", model.MedicalTest);

                    cmd.Parameters.AddWithValue("@Reactions4IssuesOfMedicalTest", model.Reactions4IssuesOfMedicalTest);
                    cmd.Parameters.AddWithValue("@ContribOnSchHealthPrg", model.ContribOnSchHealthPrg);
                    cmd.Parameters.AddWithValue("@PublicHealthPrg", model.PublicHealthPrg);
                    cmd.Parameters.AddWithValue("@SupplySanitoryFacilities", model.SupplySanitoryFacilities);
                    cmd.Parameters.AddWithValue("@CleanlinessOfSanitoryFacilities", model.CleanlinessOfSanitoryFacilities);
                    cmd.Parameters.AddWithValue("@WaterSupply", model.WaterSupply);
                    cmd.Parameters.AddWithValue("@StudentsAttendance", model.StudentsAttendance);
                    cmd.Parameters.AddWithValue("@TeachersAttendance", model.TeachersAttendance);
                    cmd.Parameters.AddWithValue("@ClassroomEnvironment", model.ClassroomEnvironment);
                    cmd.Parameters.AddWithValue("@EnvironmentOfSchool", model.EnvironmentOfSchool);
                    cmd.Parameters.AddWithValue("@MinimizeNutritionProblems", model.MinimizeNutritionProblems);
                    cmd.Parameters.AddWithValue("@NutritionKnowledgeCompetence", model.NutritionKnowledgeCompetence);
                    cmd.Parameters.AddWithValue("@StudentFitnessStatus", model.StudentFitnessStatus);
                    cmd.Parameters.AddWithValue("@PysicalWellbeingProg", model.PysicalWellbeingProg);
                    cmd.Parameters.AddWithValue("@IndividualFitness", model.IndividualFitness);
                    cmd.Parameters.AddWithValue("@MaintananceOfCanteens", model.MaintananceOfCanteens);
                    cmd.Parameters.AddWithValue("@MentalEnvironment", model.MentalEnvironment);
                    cmd.Parameters.AddWithValue("@InstructionsNGuidence", model.InstructionsNGuidence);
                    cmd.Parameters.AddWithValue("@User", Convert.ToString(Session["UserName"]));
   
                    cmd.ExecuteNonQuery();
                }



                return true;

            }
            catch (Exception Err)
            {

                return false;
            }
        }

        public ActionResult SanitoryConstruction(string SchoolID)
        {
            SanitoryConstructionModel model = new SanitoryConstructionModel();
            ViewBag.SanitoryDevelopmentTypes = GetSanitoryDevelopmentTypes();
            ViewBag.CensorsID = GetCensorsID(SchoolID);
            ViewBag.SchoolName = GetSchoolInfo(SchoolID).SchoolName;
            ViewBag.Banks = GetBanks();
            ViewBag.SchoolID = SchoolID;
            ViewBag.PaymentDetails = new List<SanitoryDevPayments>();
            ViewBag.SanitoryDevelopmentType = @"\Images\Julio.jpg";
            ViewBag.SanitoryDevelopmentTypeDD = "";
            ViewBag.Constructions = GetSanitoryDevelopments(SchoolID, "");
            return View(model);
        }

        [HttpPost]
        public ActionResult SanitoryConstruction(SanitoryConstructionModel model)
        {

            Save_SanitoryConstructionType(model);

            ViewBag.SanitoryDevelopmentTypes = GetSanitoryDevelopmentTypes();
            ViewBag.CensorsID = GetCensorsID(model.SchoolID);
            ViewBag.SchoolName = GetSchoolInfo(model.SchoolID).SchoolName;
            ViewBag.Banks = GetBanks();
            ViewBag.SchoolID = model.SchoolID;
            ViewBag.PaymentDetails = GetSanitoryDevelopmentsPaymentDetails(model.SchoolID, model.ConstructionType, model.ConstructionID);
            ViewBag.Constructions = GetSanitoryDevelopments(model.SchoolID, model.ConstructionType);
            ViewBag.SanitoryDevelopmentType = @"\Images\" + model.ConstructionType + ".jpg";
            return View(model);
        }

        private bool Save_SanitoryConstructionType(SanitoryConstructionModel model)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO BMIInformation (SchoolID, TakenBy, DatePerformed, Class, AdmissionNo, DOB, Gender, Height, Weight, BMI, CreateDate, CreateUser) " +
                    //                    "VALUES(@SchoolID, @TakenBy, @DatePerformed, @Class, @AdmissionNo, @DOB, @Gender, @Height, @Weight, @BMI, GETDATE(),  @CreateUser)";
                    cmd.CommandText = "UpdateSanitoryDevelopmentInfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@ConstructionType", model.ConstructionType);
                    cmd.Parameters.AddWithValue("@AgreementDate", model.AgreementDate);
                    cmd.Parameters.AddWithValue("@AgreementStartDate", model.AgreementStartDate);
                    cmd.Parameters.AddWithValue("@Progress", model.Progress);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.Parameters.AddWithValue("@ConstructionID", model.ConstructionID);
                    

                    cmd.ExecuteNonQuery();
                }



                return true;

            }
            catch (Exception Err)
            {

                return false;
            }
        }

        public ActionResult SanitoryConstructionDD(string SchoolID, string SanitoryDevelopmentType)
        {
            SanitoryConstructionModel model = new SanitoryConstructionModel();
            ViewBag.SanitoryDevelopmentTypes = GetSanitoryDevelopmentTypes();
            ViewBag.CensorsID = GetCensorsID(SchoolID);
            ViewBag.SchoolName = GetSchoolInfo(SchoolID).SchoolName;
            ViewBag.Banks = GetBanks();
            ViewBag.SchoolID = SchoolID;
            ViewBag.SanitoryDevelopmentType = @"\Images\" + SanitoryDevelopmentType + ".jpg";
            ViewBag.SanitoryDevelopmentTypeDD = SanitoryDevelopmentType;
            ViewBag.PaymentDetails = new List<SanitoryDevPayments>(); // GetSanitoryDevelopmentsPaymentDetails(SchoolID, SanitoryDevelopmentType,"");
            List<SanitoryConstructionModel> lstSCM = GetSanitoryDevelopments(SchoolID, SanitoryDevelopmentType);
            ViewBag.Constructions = lstSCM;
            SanitoryConstructionModel modSel = lstSCM.FirstOrDefault(x => x.ConstructionType == SanitoryDevelopmentType);

            if (modSel != null)
            {
                model.AgreementDate = modSel.AgreementDate;
                model.AgreementStartDate = modSel.AgreementStartDate;
                model.Progress = modSel.Progress;
            }
            model.ConstructionType = SanitoryDevelopmentType;
            return View("SanitoryConstruction", model);
        }

        //[HttpPost]
        public ActionResult SanitoryConstructionSel(string SchoolID, string ConstructionType,
             string ConstructionID, string AgreementDate, string StartDate, int Progress)
        {
            SanitoryConstructionModel model = new SanitoryConstructionModel();
            model.SchoolID = SchoolID;
            model.ConstructionType = ConstructionType;
            model.ConstructionID = ConstructionID;
            model.AgreementDate = Convert.ToDateTime( AgreementDate);
            model.AgreementStartDate = Convert.ToDateTime(StartDate);
            model.Progress = Progress;

            ViewBag.SanitoryDevelopmentTypes = GetSanitoryDevelopmentTypes();
            ViewBag.CensorsID = GetCensorsID(model.SchoolID);
            ViewBag.SchoolName = GetSchoolInfo(model.SchoolID).SchoolName;
            ViewBag.Banks = GetBanks();
            ViewBag.SchoolID = model.SchoolID;
            ViewBag.SanitoryDevelopmentType = @"\Images\" + model.ConstructionType + ".jpg";
            ViewBag.SanitoryDevelopmentTypeDD = model.ConstructionType;
            ViewBag.PaymentDetails = GetSanitoryDevelopmentsPaymentDetails(model.SchoolID, model.ConstructionType, model.ConstructionID);
            List<SanitoryConstructionModel> lstSCM = GetSanitoryDevelopments(model.SchoolID, model.ConstructionType);
            ViewBag.Constructions = lstSCM;

            model.ConstructionType = model.ConstructionType;
            return View("SanitoryConstruction", model);
        }

        //[HttpPost]
        public ActionResult SaveSanitoryPayment(
            string SchoolID, string SanitoryDevelopmentType, string PaymentNo, DateTime PaymentDate,
            string Bank, string ChequeNo, decimal Amount, string ConstructionID)
        {
            SanitoryDevPayments model = new SanitoryDevPayments();
            model.Amount = Amount;
            model.SchoolID = SchoolID;
            model.SanitoryDevelopmentType = SanitoryDevelopmentType;
            model.PaymentNo = (PaymentNo == "" ? 0 : Convert.ToInt32(PaymentNo));
            model.PaymentDate = PaymentDate;
            model.Bank = Bank;
            model.ChequeNo = ChequeNo;

            Save_SanitoryPayment(model, ConstructionID);

            ViewBag.PaymentDetails = GetSanitoryDevelopmentsPaymentDetails(SchoolID, SanitoryDevelopmentType, ConstructionID);
            return View();
        }

        private bool Save_SanitoryPayment(SanitoryDevPayments model, string ConstructionID)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO BMIInformation (SchoolID, TakenBy, DatePerformed, Class, AdmissionNo, DOB, Gender, Height, Weight, BMI, CreateDate, CreateUser) " +
                    //                    "VALUES(@SchoolID, @TakenBy, @DatePerformed, @Class, @AdmissionNo, @DOB, @Gender, @Height, @Weight, @BMI, GETDATE(),  @CreateUser)";
                    cmd.CommandText = "UpdateSanitoryDevPaymentInfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@SanitoryDevelopmentType", model.SanitoryDevelopmentType);
                    cmd.Parameters.AddWithValue("@PaymentNo", model.PaymentNo);
                    cmd.Parameters.AddWithValue("@PaymentDate", model.PaymentDate);
                    cmd.Parameters.AddWithValue("@Bank", model.Bank);
                    cmd.Parameters.AddWithValue("@ChequeNo", model.ChequeNo);
                    cmd.Parameters.AddWithValue("@Amount", model.Amount);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.Parameters.AddWithValue("@ConstructionID", ConstructionID);

                    cmd.ExecuteNonQuery();
                }



                return true;

            }
            catch (Exception Err)
            {

                return false;
            }
        }

        [HttpPost]
        public List<SanitoryConstructionModel> GetSanitoryDevelopments(string SchoolID, string ConstructionType)
        {
            List<SanitoryConstructionModel> lstSanDevDet = new List<SanitoryConstructionModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT  ConstructionID, AgreementDate, AgreementStartDate, Progress " +
                                   " FROM " +
                                             "SanitoryDevelopments " +
                                    "WHERE SchoolID = '" + SchoolID + "' and ConstructionType = '" + ConstructionType + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SanitoryConstructionModel thisRow = new SanitoryConstructionModel();
                    thisRow.ConstructionID = Convert.ToString(dt.Rows[i]["ConstructionID"]);
                    //thisRow.ConstructionType = Convert.ToString(dt.Rows[i]["ConstructionType"]);
                    thisRow.AgreementDate = Convert.ToDateTime(dt.Rows[i]["AgreementDate"]);
                    thisRow.AgreementStartDate = Convert.ToDateTime(dt.Rows[i]["AgreementStartDate"]);
                    thisRow.Progress = Convert.ToInt32(dt.Rows[i]["Progress"]);

                    lstSanDevDet.Add(thisRow);
                }

                return lstSanDevDet;
            }
        }

        [HttpPost]
        public List<SanitoryDevPayments> GetSanitoryDevelopmentsPaymentDetails(string SchoolID, string SanitoryDevType, string ConstructionID)
        {
            List<SanitoryDevPayments> lstSanDevPaymentDet = new List<SanitoryDevPayments>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT  PaymentNo, PaymentDate, Bank, ChequeNo, Amount " +
                                   " FROM " +
                                             "SanitoryDevPayments " +
                                    "WHERE SchoolID = '" + SchoolID + "' AND SanitoryDevelopmentType = '" + SanitoryDevType + "' AND ConstructionID = " + ConstructionID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SanitoryDevPayments thisRow = new SanitoryDevPayments();
                    thisRow.PaymentNo = Convert.ToInt32(dt.Rows[i]["PaymentNo"]);
                    thisRow.PaymentDate = Convert.ToDateTime(dt.Rows[i]["PaymentDate"]);
                    thisRow.Bank = Convert.ToString(dt.Rows[i]["Bank"]);
                    thisRow.ChequeNo = Convert.ToString(dt.Rows[i]["ChequeNo"]);
                    thisRow.Amount = Convert.ToDecimal(dt.Rows[i]["Amount"]);

                    lstSanDevPaymentDet.Add(thisRow);
                }

                return lstSanDevPaymentDet;
            }
        }

        private List<SelectListItem> GetBanks()
        {
            List<SelectListItem> MyList = new List<SelectListItem>();
           

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT BankName as Bank FROM m_Banks";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MyList.Add(new SelectListItem { Text = dt.Rows[i]["Bank"].ToString(), Value = dt.Rows[i]["Bank"].ToString() });
                }

                return MyList;
            }
        }

        private List<SelectListItem> GetBanksWithIDs()
        {
            List<SelectListItem> MyList = new List<SelectListItem>();


            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT ID, BankName  FROM m_Banks ORDER BY BankName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    MyList.Add(new SelectListItem { Text = dt.Rows[i]["BankName"].ToString(), Value = dt.Rows[i]["ID"].ToString() });
                }

                return MyList;
            }
        }

        public JsonResult GetBankBranchasJson(int BankId)
        {
            List<SelectListItem> Branchas = GetBankBranchas(BankId);
            return Json(new SelectList(Branchas, "Value", "Text"));
        }

        public List<SelectListItem> GetBankBranchas(int BankId)
        {
            List<SelectListItem> Branchas = new List<SelectListItem>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT ID, BranchName  FROM m_BanksBranch WHERE BankID=" + BankId + " ORDER BY BranchName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Branchas.Add(new SelectListItem { Text = dt.Rows[i]["BranchName"].ToString(), Value = dt.Rows[i]["ID"].ToString() });
                }

                return Branchas;
            }
        }


        private List<SelectListItem> GetSanitoryDevelopmentTypes()
        {

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string TypeKey in System.Web.Configuration.WebConfigurationManager.AppSettings.AllKeys)
            {
                if (TypeKey.StartsWith("Sanitory Development Type"))
                {
                    string TypeValue = System.Web.Configuration.WebConfigurationManager.AppSettings[TypeKey];
                    MyList.Add(new SelectListItem { Text = TypeKey, Value = TypeValue });
                }
            }

            return MyList;

        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (null != filterContext.HttpContext.Session)
            {
                // Check if we have a new session
                if (filterContext.HttpContext.Session.IsNewSession)
                {
                    string cookie = filterContext.HttpContext.Request.Headers["Cookie"];
                    // Check if session has timed out
                    if ((null != cookie) && (cookie.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        //timeoutFlag = true;
                        //// Logout the user
                        //WebSecurit.Logout();
                        //redirect to login
                        filterContext.Result = RedirectToAction("Login", "Login");
                        return;
                    }
                }
            }
            // else continue with action as usual
            base.OnActionExecuting(filterContext);
        }


        public ActionResult AboutUs()
        {
            return View();
        }

        public ActionResult SummaryOfData()
        {
            SummaryOfDataModel model = new SummaryOfDataModel();
            ViewBag.SummaryOfData = GetSummaryOfData(DateTime.Now.Year);
            ViewBag.SummaryOfDataZones = GetSummaryOfDataZones(DateTime.Now.Year);
            ViewBag.SummaryOfDataProvinces = GetSummaryOfDataProvinces(DateTime.Now.Year);
            ViewBag.Years = GetYears();
            model.YearConsidered = DateTime.Now.Year;
            return View(model);
        }

        [HttpPost]
        public ActionResult SummaryOfData(SummaryOfDataModel model)
        {
            ViewBag.SummaryOfData = GetSummaryOfData(model.YearConsidered);
            ViewBag.SummaryOfDataZones = GetSummaryOfDataZones(model.YearConsidered);
            ViewBag.SummaryOfDataProvinces = GetSummaryOfDataProvinces(model.YearConsidered);
            ViewBag.Years = GetYears();
            return View(model);
        }

        public ActionResult SummaryOfDataZonesInProvince(int iYear, string ProvinceID)
        {
            SummaryOfDataModel model = new SummaryOfDataModel();
            //ViewBag.SummaryOfData = GetSummaryOfData(DateTime.Now.Year);
            ViewBag.SummaryOfDataZones = GetSummaryOfDataZones(iYear, ProvinceID);
            //ViewBag.SummaryOfDataProvinces = GetSummaryOfDataProvinces(DateTime.Now.Year);
            ViewBag.Years = GetYears();
            model.YearConsidered = iYear;
            return View(model);
        }

        public ActionResult SummaryOfDataSchoolsInZone(int iYear, string ZoneID)
        {
            SummaryOfDataModel model = new SummaryOfDataModel();
            ViewBag.SummaryOfData = GetSummaryOfDataSchoolsInZone(iYear, ZoneID);
            //ViewBag.SummaryOfDataZones = GetSummaryOfDataZones(DateTime.Now.Year);
            //ViewBag.SummaryOfDataProvinces = GetSummaryOfDataProvinces(DateTime.Now.Year);
            //ViewBag.Years = GetYears();
            //model.YearConsidered = DateTime.Now.Year;
            return View(model);
        }

        private List<SelectListItem> GetYears()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT Distinct Year FROM ZoneWiseStudentCount";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["Year"].ToString().Trim(), Value = Convert.ToString(mydataRow["Year"]) });
                }

                return MyList;
            }
        }

        public ActionResult SchoolAttendance(string SchoolID, string CensusID, string SchoolName)
        {
            //string CensusID = fCollection["CensorsID"].ToString();
            //string SchoolName = fCollection["SchoolName"].ToString();
            SchoolAttendanceModel model = new SchoolAttendanceModel();
            model.CensorsID = CensusID;
            model.SchoolID = SchoolID;
            model.SchoolName = SchoolName;
            return View(model);
        }

        private List<SummaryOfDataModel> GetSummaryOfDataZones(int iYear, string ProvinceID)
        {
            List<SummaryOfDataModel> retSumLst = new List<SummaryOfDataModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT dbo.m_Provinces.ProvinceName, B.ZoneID, B.ZoneName, A.NumberOfSchools, A.TotalStudentCount, " +
                                            " COUNT(dbo.StudentInfo.AddmisionNo) AS [Entered], COUNT(dbo.StudentInfo.AddmisionNo) * 100 / A.TotalStudentCount AS [Per], " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                            "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID  AND m_Schools.ZoneID = B.ZoneID) AS SupCount, " +
                                            "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo INNER JOIN m_Schools ON SanitoryFacilityInfo.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID =  A.ProvinceID  AND m_Schools.ZoneID = B.ZoneID) AS SanitoryCount " +
                                   " FROM " +
                                             "m_Schools INNER JOIN StudentInfo ON m_Schools.SchoolID = StudentInfo.SchoolID " +
                                             "INNER JOIN m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID " +
                                             "INNER JOIN m_Zones B ON m_Schools.ZoneID = B.ZoneID " +
                                             "INNER JOIN ZoneWiseStudentCount A ON A.ZoneID = B.ZoneID " +
                                    "WHERE A.Year = " + iYear.ToString() + " AND m_Schools.ProvinceID = '" + ProvinceID + "' " +
                                    "GROUP BY A.ProvinceID, m_Provinces.ProvinceName, B.ZoneID, B.ZoneName, m_Schools.ZoneID, A.NumberOfSchools, A.TotalStudentCount " +
                                    "ORDER BY  m_Provinces.ProvinceName, [Per] DESC, B.ZoneName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SummaryOfDataModel thisRow = new SummaryOfDataModel();
                    thisRow.Province = dt.Rows[i]["ProvinceName"].ToString();
                    thisRow.Zone = dt.Rows[i]["ZoneName"].ToString();
                    thisRow.ZoneID = dt.Rows[i]["ZoneID"].ToString();
                    thisRow.TotalStudentCount = Convert.ToInt32(dt.Rows[i]["TotalStudentCount"] == DBNull.Value ? "0" : dt.Rows[i]["TotalStudentCount"]).ToString("#,##0");
                    thisRow.NumberOfSchools = Convert.ToInt32((dt.Rows[i]["NumberOfSchools"] == DBNull.Value ? 0 : dt.Rows[i]["NumberOfSchools"]));
                    thisRow.Entered = Convert.ToInt32((dt.Rows[i]["Entered"] == DBNull.Value ? 0 : dt.Rows[i]["Entered"])).ToString("#,##0");
                    thisRow.Per = Convert.ToDecimal((dt.Rows[i]["Per"] == DBNull.Value ? 0.00 : dt.Rows[i]["Per"]));
                    thisRow.BMI1 = Convert.ToInt32((dt.Rows[i]["BMI1"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI1"]));
                    thisRow.BMI2 = Convert.ToInt32((dt.Rows[i]["BMI2"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI2"]));
                    thisRow.BMI3 = Convert.ToInt32((dt.Rows[i]["BMI3"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI3"]));
                    thisRow.SupCount = Convert.ToInt32((dt.Rows[i]["SupCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SupCount"]));
                    thisRow.SanitoryCount = Convert.ToInt32((dt.Rows[i]["SanitoryCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SanitoryCount"]));

                    //retBMILst[i] = thisRow;
                    retSumLst.Add(thisRow);
                }
            }


            return retSumLst;
        }

        private List<SummaryOfDataModel> GetSummaryOfDataZones(int iYear)
        {
            List<SummaryOfDataModel> retSumLst = new List<SummaryOfDataModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT dbo.m_Provinces.ProvinceName, B.ZoneName, A.NumberOfSchools, A.TotalStudentCount, " +
                                            " COUNT(dbo.StudentInfo.AddmisionNo) AS [Entered], COUNT(dbo.StudentInfo.AddmisionNo) * 100 / A.TotalStudentCount AS [Per], " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND m_Schools.ZoneID = B.ZoneID AND (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                            "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID  AND m_Schools.ZoneID = B.ZoneID) AS SupCount, " +
                                            "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo INNER JOIN m_Schools ON SanitoryFacilityInfo.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID =  A.ProvinceID  AND m_Schools.ZoneID = B.ZoneID) AS SanitoryCount " +
                                   " FROM " +
                                             "m_Schools INNER JOIN StudentInfo ON m_Schools.SchoolID = StudentInfo.SchoolID " +
                                             "INNER JOIN m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID " +
                                             "INNER JOIN m_Zones B ON m_Schools.ZoneID = B.ZoneID " +
                                             "INNER JOIN ZoneWiseStudentCount A ON A.ZoneID = B.ZoneID " +
                                    "WHERE A.Year = " + iYear.ToString() + " " +
                                    "GROUP BY A.ProvinceID, m_Provinces.ProvinceName, B.ZoneID, B.ZoneName, m_Schools.ZoneID, A.NumberOfSchools, A.TotalStudentCount " +
                                    "ORDER BY  m_Provinces.ProvinceName, [Per] DESC, B.ZoneName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SummaryOfDataModel thisRow = new SummaryOfDataModel();
                    thisRow.Province = dt.Rows[i]["ProvinceName"].ToString();
                    thisRow.Zone = dt.Rows[i]["ZoneName"].ToString();
                    thisRow.TotalStudentCount = Convert.ToInt32(dt.Rows[i]["TotalStudentCount"] == DBNull.Value ? "0" : dt.Rows[i]["TotalStudentCount"]).ToString("#,##0");
                    thisRow.NumberOfSchools = Convert.ToInt32((dt.Rows[i]["NumberOfSchools"] == DBNull.Value ? 0 : dt.Rows[i]["NumberOfSchools"]));
                    thisRow.Entered = Convert.ToInt32((dt.Rows[i]["Entered"] == DBNull.Value ? 0 : dt.Rows[i]["Entered"])).ToString("#,##0");
                    thisRow.Per = Convert.ToDecimal((dt.Rows[i]["Per"] == DBNull.Value ? 0.00 : dt.Rows[i]["Per"]));
                    thisRow.BMI1 = Convert.ToInt32((dt.Rows[i]["BMI1"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI1"]));
                    thisRow.BMI2 = Convert.ToInt32((dt.Rows[i]["BMI2"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI2"]));
                    thisRow.BMI3 = Convert.ToInt32((dt.Rows[i]["BMI3"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI3"]));
                    thisRow.SupCount = Convert.ToInt32((dt.Rows[i]["SupCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SupCount"]));
                    thisRow.SanitoryCount = Convert.ToInt32((dt.Rows[i]["SanitoryCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SanitoryCount"]));

                    //retBMILst[i] = thisRow;
                    retSumLst.Add(thisRow);
                }
            }


            return retSumLst;
        }

        private List<SummaryOfDataModel> GetSummaryOfDataProvinces(int iYear)
        {
            List<SummaryOfDataModel> retSumLst = new List<SummaryOfDataModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();


                /*
                cmd.CommandText = "SELECT  " +
                                            "TOP (100) PERCENT A.ProvinceID, m_Provinces.ProvinceName, SUM(A.NumberOfSchools) AS NumberOfSchools, SUM(A.TotalStudentCount) " +
                                            " AS TotalStudents, Province_DataEntery_Count.Entered, (Province_DataEntery_Count.Entered/SUM(A.TotalStudentCount))*100 AS Per, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                            "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID) AS SupCount, " +
                                            "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo INNER JOIN m_Schools ON SanitoryFacilityInfo.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID =  A.ProvinceID) AS SanitoryCount " +
                                   " FROM " +
                                            " ZoneWiseStudentCount A INNER JOIN Province_DataEntery_Count ON A.ProvinceID = dbo.Province_DataEntery_Count.ProvinceID " +
                                             "INNER JOIN m_Provinces ON A.ProvinceID = m_Provinces.ProvinceID " +
                                    "WHERE A.Year = " + iYear.ToString() + " " +
                                    "GROUP BY  A.ProvinceID, m_Provinces.ProvinceName, Province_DataEntery_Count.Entered " +
                                    "ORDER BY Per DESC, m_Provinces.ProvinceName, Province_DataEntery_Count.Entered";
                 
                 */

                cmd.CommandText = "SELECT  " +
                                            "A.ProvinceID, m_Provinces.ProvinceName, SUM(A.NumberOfSchools) AS NumberOfSchools, SUM(A.TotalStudentCount) " +
                                            " AS TotalStudents, ISNULL(dbo.Province_DataEntery_Count.Entered, 0) AS Entered, (Province_DataEntery_Count.Entered/SUM(A.TotalStudentCount))*100 AS Per, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                            "(SELECT COUNT(BMIInformation.AdmissionNo) FROM m_Schools INNER JOIN BMIInformation ON m_Schools.SchoolID = BMIInformation.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID AND (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                            "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID = A.ProvinceID) AS SupCount, " +
                                            "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo INNER JOIN m_Schools ON SanitoryFacilityInfo.SchoolID = m_Schools.SchoolID WHERE m_Schools.ProvinceID =  A.ProvinceID) AS SanitoryCount " +
                                   " FROM " +
                                            " ZoneWiseStudentCount A INNER JOIN Province_DataEntery_Count ON A.ProvinceID = dbo.Province_DataEntery_Count.ProvinceID " +
                                             "INNER JOIN m_Provinces ON A.ProvinceID = m_Provinces.ProvinceID " +
                                    "WHERE A.Year = " + iYear.ToString() + " AND (Province_DataEntery_Count.Year = " + iYear.ToString() + " ) " +
                                    "GROUP BY  A.ProvinceID, m_Provinces.ProvinceName, Province_DataEntery_Count.Entered " +
                                    "ORDER BY Per DESC, m_Provinces.ProvinceName, Province_DataEntery_Count.Entered";


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SummaryOfDataModel thisRow = new SummaryOfDataModel();
                    thisRow.Province = dt.Rows[i]["ProvinceName"].ToString();
                    thisRow.ProvinceID = dt.Rows[i]["ProvinceID"].ToString();
                    thisRow.TotalStudentCount = Convert.ToInt32(dt.Rows[i]["TotalStudents"] == DBNull.Value ? 0 : dt.Rows[i]["TotalStudents"]).ToString("#,##0");
                    thisRow.NumberOfSchools = Convert.ToInt32((dt.Rows[i]["NumberOfSchools"] == DBNull.Value ? 0 : dt.Rows[i]["NumberOfSchools"]));
                    thisRow.Entered = Convert.ToInt32((dt.Rows[i]["Entered"] == DBNull.Value ? 0 : dt.Rows[i]["Entered"])).ToString("#,##0"); 
                    thisRow.Per = Convert.ToDecimal((dt.Rows[i]["Per"] == DBNull.Value ? 0.00 : dt.Rows[i]["Per"]));
                    thisRow.BMI1 = Convert.ToInt32((dt.Rows[i]["BMI1"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI1"]));
                    thisRow.BMI2 = Convert.ToInt32((dt.Rows[i]["BMI2"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI2"]));
                    thisRow.BMI3 = Convert.ToInt32((dt.Rows[i]["BMI3"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI3"]));
                    thisRow.SupCount = Convert.ToInt32((dt.Rows[i]["SupCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SupCount"]));
                    thisRow.SanitoryCount = Convert.ToInt32((dt.Rows[i]["SanitoryCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SanitoryCount"]));

                    //retBMILst[i] = thisRow;
                    retSumLst.Add(thisRow);
                }
            }


            return retSumLst;
        }

        private List<SummaryOfDataModel> GetSummaryOfDataSchoolsInZone(int iYear, string ZoneID)
        {
            List<SummaryOfDataModel> retSumLst = new List<SummaryOfDataModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();

                cmd.CommandText = "SELECT  " +
                                       "TOP (100) PERCENT m_Provinces.ProvinceID, m_Zones.ZoneID, dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName, A.SchoolName, COUNT(dbo.StudentInfo.AddmisionNo)  AS NoOfStudents, " +
                                       "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                       "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                       "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                       "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation WHERE m_SupplierInformation.SchoolID = A.SchoolID) AS SupCount, " +
                                       "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo WHERE SanitoryFacilityInfo.SchoolID = A.SchoolID) AS SanitoryCount " +
                                  " FROM " +
                                        "m_Schools A INNER JOIN StudentInfo ON A.SchoolID = dbo.StudentInfo.SchoolID INNER JOIN " +
                                        "m_Provinces ON A.ProvinceID = dbo.m_Provinces.ProvinceID INNER JOIN " +
                                        "m_Zones ON A.ZoneID = dbo.m_Zones.ZoneID " +
                                        "INNER JOIN ZoneWiseStudentCount ON ZoneWiseStudentCount.ProvinceID = m_Provinces.ProvinceID AND ZoneWiseStudentCount.ZoneID = A.ZoneID " +
                                   "WHERE ZoneWiseStudentCount.Year = " + iYear + " AND m_Zones.ZoneID = '" + ZoneID + "' " +
                                   "GROUP BY  m_Provinces.ProvinceID, m_Zones.ZoneID, A.SchoolID, A.SchoolName, dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName " +
                                   "ORDER BY dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName";

                /*
                cmd.CommandText = "SELECT  " +
                                        "TOP (100) PERCENT m_Provinces.ProvinceID, m_Zones.ZoneID, dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName, A.SchoolName, COUNT(dbo.StudentInfo.AddmisionNo)  AS NoOfStudents, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                        "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation WHERE m_SupplierInformation.SchoolID = A.SchoolID) AS SupCount, " +
                                        "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo WHERE SanitoryFacilityInfo.SchoolID = A.SchoolID) AS SanitoryCount " +
                                   " FROM " +
                                         "m_Schools A INNER JOIN StudentInfo ON A.SchoolID = dbo.StudentInfo.SchoolID INNER JOIN " +
                                         "m_Provinces ON A.ProvinceID = dbo.m_Provinces.ProvinceID INNER JOIN " +
                                         "m_Zones ON A.ZoneID = dbo.m_Zones.ZoneID " +
                                         "INNER JOIN ZoneWiseStudentCount ON ZoneWiseStudentCount.ProvinceID = m_Provinces.ProvinceID AND ZoneWiseStudentCount.ZoneID = A.ZoneID " +
                                    "WHERE ZoneWiseStudentCount.Year = " + iYear + " AND m_Zones.ZoneID = '" + ZoneID + "' " +
                                    "GROUP BY  m_Provinces.ProvinceID, m_Zones.ZoneID, A.SchoolID, A.SchoolName, dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName " +
                                    "ORDER BY dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName";
                      */

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SummaryOfDataModel thisRow = new SummaryOfDataModel();
                    thisRow.Province = dt.Rows[i]["ProvinceName"].ToString();
                    thisRow.Zone = dt.Rows[i]["ZoneName"].ToString();
                    thisRow.School = dt.Rows[i]["SchoolName"].ToString();
                    thisRow.NoOfStudents = Convert.ToInt32((dt.Rows[i]["NoOfStudents"] == DBNull.Value ? 0 : dt.Rows[i]["NoOfStudents"]));
                    thisRow.BMI1 = Convert.ToInt32((dt.Rows[i]["BMI1"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI1"]));
                    thisRow.BMI2 = Convert.ToInt32((dt.Rows[i]["BMI2"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI2"]));
                    thisRow.BMI3 = Convert.ToInt32((dt.Rows[i]["BMI3"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI3"]));
                    thisRow.SupCount = Convert.ToInt32((dt.Rows[i]["SupCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SupCount"]));
                    thisRow.SanitoryCount = Convert.ToInt32((dt.Rows[i]["SanitoryCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SanitoryCount"]));
                    thisRow.ProvinceID = Convert.ToInt32((dt.Rows[i]["ProvinceID"] == DBNull.Value ? 0 : dt.Rows[i]["ProvinceID"])).ToString();
                    thisRow.ZoneID = dt.Rows[i]["ZoneID"].ToString();
                    //retBMILst[i] = thisRow;
                    retSumLst.Add(thisRow);
                }
            }


            return retSumLst;
        }

        private List<SummaryOfDataModel> GetSummaryOfData(int iYear)
        {
            List<SummaryOfDataModel> retSumLst = new List<SummaryOfDataModel>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                        "TOP (100) PERCENT dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName, A.SchoolName, COUNT(dbo.StudentInfo.AddmisionNo)  AS NoOfStudents, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND (BMIInformation.Trimester = " + iYear.ToString() + "1)) AS BMI1, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "2)) AS BMI2, " +
                                        "(SELECT COUNT(BMIInformation.AdmissionNo) FROM BMIInformation WHERE BMIInformation.SchoolID = A.SchoolID AND  (BMIInformation.Trimester = " + iYear.ToString() + "3)) AS BMI3, " +
                                        "(SELECT COUNT(m_SupplierInformation.SupplierName) FROM m_SupplierInformation WHERE m_SupplierInformation.SchoolID = A.SchoolID) AS SupCount, " +
                                        "(SELECT COUNT(SanitoryFacilityInfo.NoOfMaleToilets) FROM SanitoryFacilityInfo WHERE SanitoryFacilityInfo.SchoolID = A.SchoolID) AS SanitoryCount " +
                                   " FROM " +
                                         "m_Schools A INNER JOIN StudentInfo ON A.SchoolID = dbo.StudentInfo.SchoolID INNER JOIN " +
                                         "m_Provinces ON A.ProvinceID = dbo.m_Provinces.ProvinceID INNER JOIN " +
                                         "m_Zones ON A.ZoneID = dbo.m_Zones.ZoneID " +
                                         "INNER JOIN ZoneWiseStudentCount ON ZoneWiseStudentCount.ProvinceID = m_Provinces.ProvinceID AND ZoneWiseStudentCount.ZoneID = A.ZoneID " +
                                    "WHERE ZoneWiseStudentCount.Year = " + iYear + " AND (dbo.StudentInfo.Year = " + iYear + ") " +
                                    "GROUP BY  A.SchoolID, A.SchoolName, dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName " +
                                    "ORDER BY dbo.m_Provinces.ProvinceName, dbo.m_Zones.ZoneName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    SummaryOfDataModel thisRow = new SummaryOfDataModel();
                    thisRow.Province = dt.Rows[i]["ProvinceName"].ToString();
                    thisRow.Zone = dt.Rows[i]["ZoneName"].ToString();
                    thisRow.School = dt.Rows[i]["SchoolName"].ToString();
                    thisRow.NoOfStudents = Convert.ToInt32((dt.Rows[i]["NoOfStudents"] == DBNull.Value ? 0 : dt.Rows[i]["NoOfStudents"]));
                    thisRow.BMI1 = Convert.ToInt32((dt.Rows[i]["BMI1"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI1"]));
                    thisRow.BMI2 = Convert.ToInt32((dt.Rows[i]["BMI2"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI2"]));
                    thisRow.BMI3 = Convert.ToInt32((dt.Rows[i]["BMI3"] == DBNull.Value ? 0.00 : dt.Rows[i]["BMI3"]));
                    thisRow.SupCount = Convert.ToInt32((dt.Rows[i]["SupCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SupCount"]));
                    thisRow.SanitoryCount = Convert.ToInt32((dt.Rows[i]["SanitoryCount"] == DBNull.Value ? 0.00 : dt.Rows[i]["SanitoryCount"]));

                    //retBMILst[i] = thisRow;
                    retSumLst.Add(thisRow);
                }
            }


            return retSumLst;
        }

        public ActionResult Help(string HelpVideo)
        {
            ViewBag.HelpVideo = HelpVideo;
            return View();
        }

        public FileResult Download(string file)
        {
            file = string.Format(@"Files2Download\{0}", file);
            byte[] fileBytes = System.IO.File.ReadAllBytes(file);
            var response = new FileContentResult(fileBytes, "application/octet-stream");
            response.FileDownloadName = "BulkStudents.xlsx";
            return response;
        }

        public ActionResult BMIInformation(string SchoolID, string Grade)
        {
            if (CheckAccessCanBeGranted(SchoolID))
            {
                if (Grade == null)
                {
                    BMIInfoModel model = new BMIInfoModel();

                    ViewBag.Grades = GetGradesInfoList(SchoolID);
                    model.SchoolName = GetSchoolInfo(SchoolID).SchoolName;
                    ViewBag.BMIInfo = getBMIs100();
                    model.Year = DateTime.Now.Year;
                    //model.BMIDetailsOfClass = getBMIs();
                    return View(model);
                }
                else
                {
                    BMIInfoModel model = new BMIInfoModel();

                    ViewBag.Grades = GetGradesInfoList(SchoolID);
                    model.SchoolName = GetSchoolInfo(SchoolID).SchoolName;
                    ViewBag.BMIInfo = getBMIs100();
                    model.Year = DateTime.Now.Year;
                    //model.BMIDetailsOfClass = getBMIs();
                    return View(model);
                }
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        //public ActionResult BMIInformation(string SchoolID, string Grade)
        //{
        //    BMIInfoModel model = new BMIInfoModel();

        //    ViewBag.Grades = GetGradesInfoList(SchoolID);
        //    model.SchoolName = GetSchoolInfo(SchoolID).SchoolName;
        //    //ViewBag.BMIInfo = getBMIs();
        //    model.Year = DateTime.Now.Year;
        //    //model.BMIDetailsOfClass = getBMIs();
        //    return View(model);
        //}

        [HttpPost]
        public ActionResult GetExistingBMIs(string SchoolID, string Grade, int Trimester)
        {

            List<BMISDateInfo> lstBMIs = getBMIs(SchoolID, Grade, Trimester);

            return Json(lstBMIs);

        }

        public ActionResult Export2Excelsidyear(string iSchoolID, int year)
        {
            //TODO

            var StudentInfo = GetStudentsInfoDT(iSchoolID, year);
            String strCensorID = GetCensorsID(iSchoolID);


            //List<SelectListItem> lstProvinces = GetProvinces();
            //string ProvinceName = lstProvinces.Where(p => p.Value == ProvinceID).FirstOrDefault().Text;

            var grid = new GridView();
            grid.DataSource = StudentInfo;
            grid.DataBind();

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(null, "Grade Information");
                wb.Worksheets.Add(StudentInfo, "Student Information");
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;


                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= Student_Information_" + strCensorID + ".xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }

            return null;
        }

        public void Export2ExcelSDS(string ProvinceID)
        {

            var SDSBankInfo = GetSDSBankInfoTable(ProvinceID);
            List<SelectListItem> lstProvinces = GetProvinces();
            string ProvinceName = lstProvinces.Where(p => p.Value == ProvinceID).FirstOrDefault().Text;
            var grid = new GridView();
            grid.DataSource = SDSBankInfo;
            grid.DataBind();

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(SDSBankInfo, "SDSBankInfo" + ProvinceName);
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;


                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= SDSBankInfo" + ProvinceName + ".xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }

        }

        //[HttpPost]
        public void Export2Excel(string SchoolID, string Grade, string Trimester)
        {
            //We load the data
            var BMIInfo = GetBMIInfoTable(SchoolID, Grade, Trimester);
          

            var grid = new GridView();
            grid.DataSource = BMIInfo;
            grid.DataBind();

            string CensusID = GetCensorsID(SchoolID);
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(BMIInfo, "BMIInfo");
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;
                

                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= " + CensusID + "_" + Grade + "_BMIInfo.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }  
            
        }

        [HttpPost]
        public ActionResult BMIInformation(HttpPostedFileBase excelFile, BMIInfoModel model, FormCollection fCollection, string button)
        {
            try
            {

                int rowNo = 1;


                switch (button)
                {

                    case "Upload Excel":


                        if (excelFile == null || excelFile.ContentLength == 0)
                        {
                            ViewBag.Grades = GetGradesInfoList(model.SchoolID);
                            ViewBag.BMIInfo = getBMIs100();
                            return View();
                        }
                        else
                        {
                            if (excelFile.FileName.EndsWith("xls") || excelFile.FileName.EndsWith("xlsx"))
                            {

                                List<BMIInfo> lstBMI = new List<BMIInfo>();

                                using (var package = new ExcelPackage(excelFile.InputStream))
                                {
                                    // get the first worksheet in the workbook
                                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                                    for (int colInit = 1; colInit < 6; colInit++)
                                    {
                                        if (worksheet.Cells[1, colInit].Value == null)
                                        {
                                            @ViewBag.ErrorMessage = "Invalid Excel worksheet format found.";
                                            return View("../Login/Error");
                                        }
                                    }

                                    int col = 1;
                                    int ColAddmisionNo = 1;
                                    int ColDOB = 1;
                                    int ColGender = 1;
                                    int ColHeight = 1;
                                    int ColWeight = 1;
                                    int ColBMI = 1;

                                    //Check column nos according to the column name
                                    for (int colInit = 1; colInit < 6; colInit++)
                                    {

                                        if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "ADMISSIONNO")
                                        {
                                            ColAddmisionNo = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "DOB")
                                        {
                                            ColDOB = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "GENDER")
                                        {
                                            ColGender = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "HEIGHT")
                                        {
                                            ColHeight = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "WEIGHT")
                                        {
                                            ColWeight = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "BMI")
                                        {
                                            ColBMI = colInit;
                                        }

                                    }

                                    //if (worksheet.Cells[2, col].Value != null)
                                    //{
                                    //    string strCensusID = worksheet.Cells[2, ColCensusID].Value.ToString();
                                    //    if (strCensusID != GetCensorsID(model.SchoolID))
                                    //    {
                                    //        bool isVal = ModelState.IsValid;
                                    //        ModelState.AddModelError("Error", "Census ID is not matching with the logged in school");
                                    //        List<StudentModel> lstStudentsCensus = new List<StudentModel>();
                                    //        lstStudentsCensus.Add(new StudentModel());
                                    //        ViewBag.ListStudents = lstStudentsCensus;
                                    //        break;
                                    //    }
                                    //}

                                    DataTable dtAdmissionNos = getAdmissionNos(model);

                                    for (int row = 2; worksheet.Cells[row, col].Value != null; row++)
                                    {
                                        

                                        BMIInfo BmiModel = new BMIInfo();
                                        BmiModel.AdmissionNo = (worksheet.Cells[row, ColAddmisionNo].Value == null ? "" : worksheet.Cells[row, ColAddmisionNo].Value.ToString());
                                        BmiModel.DOB = (worksheet.Cells[row, ColDOB].Value == null ? DateTime.Today : Convert.ToDateTime(worksheet.Cells[row, ColDOB].Value));
                                        BmiModel.Gender = (worksheet.Cells[row, ColGender].Value == null ? "" : worksheet.Cells[row, ColGender].Value.ToString());
                                        BmiModel.Height = (worksheet.Cells[row, ColHeight].Value == null ? 0 : Convert.ToDecimal(worksheet.Cells[row, ColHeight].Value));
                                        BmiModel.Weight = (worksheet.Cells[row, ColWeight].Value == null ? 0 : Convert.ToDecimal(worksheet.Cells[row, ColWeight].Value));
                                        //BmiModel.BMI = (worksheet.Cells[row, ColBMI].Value == null ? 0 : Convert.ToDecimal(worksheet.Cells[row, ColBMI].Value));
                                        //if (BmiModel.BMI == 0)
                                        //{
                                        if (BmiModel.Height != 0)
                                        {
                                            BmiModel.BMI = Math.Round(BmiModel.Weight / (BmiModel.Height * BmiModel.Height), 2);
                                            //}

                                            if (!(dtAdmissionNos.Select("AdmissionNo = '" + BmiModel.AdmissionNo + "'").Length > 0))
                                            {
                                                @ViewBag.ErrorMessage = "Admission No - " + BmiModel.AdmissionNo + " is invalid";
                                                return View("../Login/Error");
                                            }

                                            //stuModel.ParentName = (worksheet.Cells[row, ColParentName].Value == null ? "" : worksheet.Cells[row, ColParentName].Value.ToString());
                                            //stuModel.ParentAddress = (worksheet.Cells[row, ColParentAddress].Value == null ? "" : worksheet.Cells[row, ColParentAddress].Value.ToString());
                                            //stuModel.NIC = (worksheet.Cells[row, ColNIC].Value == null ? "" : worksheet.Cells[row, ColNIC].Value.ToString());
                                            //stuModel.ContactNo = (worksheet.Cells[row, ColContactNo].Value == null ? "" : worksheet.Cells[row, ColContactNo].Value.ToString());
                                            //stuModel.SchoolID = model.SchoolID;
                                        }
                                        lstBMI.Add(BmiModel);
                                        
                                        rowNo++;

                                    }

                                    rowNo = 0;
                                } // the using 

                                ViewBag.BMIInfo = lstBMI;
                            }
                            else
                            {

                            }

                        }
                        string strSchoolIDb = model.SchoolID;
                        ViewBag.Grades = GetGradesInfoList(strSchoolIDb);
                        model.SchoolName = GetSchoolInfo(strSchoolIDb).SchoolName;
                        //return View(model);
                        break;
                    case "Save":

                        string strSchoolID = model.SchoolID;

                        if (ModelState.IsValid)
                        {
                            int totalRows = Convert.ToInt32(fCollection["CountRows"]);

                            //Validate
                            int iErrors = 0;
                            ArrayList lstErrors = new ArrayList();
                            for (int i = 1; i <= totalRows; i++)
                            {

                                string AdmisionNo = fCollection["AdmissionNoH" + i.ToString()];
                                if (AdmisionNo != null && AdmisionNo.Trim() != "")
                                {
                                    string Gender = fCollection["GenderH" + i.ToString()];
                                    DateTime DOB = Convert.ToDateTime(fCollection["DOBH" + i.ToString()]);
                                    decimal Height = Convert.ToDecimal(fCollection["HeightH" + i.ToString()]);
                                    decimal Weight = Convert.ToDecimal(fCollection["WeightH" + i.ToString()]);
                                    decimal BMI = Convert.ToDecimal(fCollection["BMIH" + i.ToString()]);

                                    if (!(BMI > 0))
                                    {
                                        iErrors++;
                                        lstErrors.Add("BMI cannot be zero - row no." + i.ToString());
                                    }

                                }
                            }

                            if (iErrors > 0)
                            {
                                @ViewBag.ErrorMessage = lstErrors.ToString();
                                return View("../Login/Error");

                            }

                            for (int i = 1; i <= totalRows; i++)
                            {

                                string AdmisionNo = fCollection["AdmissionNoH" + i.ToString()];
                                if (AdmisionNo != null && AdmisionNo.Trim() != "")
                                {
                                    string Gender = fCollection["GenderH" + i.ToString()];
                                    DateTime DOB = Convert.ToDateTime(fCollection["DOBH" + i.ToString()]);
                                    decimal Height = Convert.ToDecimal(fCollection["HeightH" + i.ToString()]);
                                    decimal Weight = Convert.ToDecimal(fCollection["WeightH" + i.ToString()]);
                                    decimal BMI = Convert.ToDecimal(fCollection["BMIH" + i.ToString()]);


                                    Save_BMI(model, AdmisionNo, Gender, DOB, Height, Weight, BMI);
                                }


                            }

                            ModelState.Clear();
                        }
                        else
                        {
                            //ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
                            @ViewBag.ErrorMessage = "Please fill all the fields which is mandatory.";
                            return View("../Login/Error");
                        }

                        ViewBag.Grades = GetGradesInfoList(strSchoolID);
                        model.SchoolName = GetSchoolInfo(strSchoolID).SchoolName;
                        ViewBag.BMIInfo = getBMIs100();


                       

                        break;

                }

                return View(model);
            }
            catch (Exception Err)
            {
                ViewBag.Grades = GetGradesInfoList(model.SchoolID);
                ViewBag.BMIInfo = getBMIs100();
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
                return View(model);
            }
        }

        private DataTable getAdmissionNos(BMIInfoModel model)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT DISTINCT StudentInfo.AddmisionNo AS AdmissionNo " +
                                   "FROM " +
                                   "StudentInfo "+
                                   " WHERE StudentInfo.SchoolID ='" + model.SchoolID + "' AND StudentInfo.CurrentGrade = '" + model.Grade + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }
        }

        private bool Save_BMI(BMIInfoModel model, string AdmisionNo, string Gender, DateTime DOB, decimal Height, decimal Weight, decimal BMI)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO BMIInformation (SchoolID, TakenBy, DatePerformed, Class, AdmissionNo, DOB, Gender, Height, Weight, BMI, CreateDate, CreateUser) " +
                    //                    "VALUES(@SchoolID, @TakenBy, @DatePerformed, @Class, @AdmissionNo, @DOB, @Gender, @Height, @Weight, @BMI, GETDATE(),  @CreateUser)";
                    cmd.CommandText = "UpdateBMIInfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@AdmissionNo", AdmisionNo);
                    cmd.Parameters.AddWithValue("@TakenBy", model.TakenBy);
                    cmd.Parameters.AddWithValue("@DatePerformed", model.PerformedDate);
                    cmd.Parameters.AddWithValue("@Class", model.Grade);
                    cmd.Parameters.AddWithValue("@DOB", DOB);
                    cmd.Parameters.AddWithValue("@Gender", Gender);
                    cmd.Parameters.AddWithValue("@Height", Height);
                    cmd.Parameters.AddWithValue("@Weight", Weight);
                    cmd.Parameters.AddWithValue("@BMI", BMI);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.Parameters.AddWithValue("@Trimester", DateTime.Now.Year.ToString() + model.Trimester.ToString());

                    cmd.ExecuteNonQuery();
                }



                return true;

            }
            catch (Exception Err)
            {

                return false;
            }
        }



        private List<BMIInfo> getBMIs1()
        {
            BMIInfo[] allBMI = new BMIInfo[1];
            return allBMI.ToList();

        }

        private List<BMIInfo> getBMIs100()
        {
            BMIInfo[] allBMI = new BMIInfo[100];
            return allBMI.ToList();

        }

        public DataTable GetSDSBankInfoTable(string strProvinceID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT ProvinceName as Province, ZoneName as Zone, CensorsID, SchoolName as School, Bank, BankAccountNo FROM SDS_Bank_List WHERE ProvinceID = " + strProvinceID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }


        }

        public DataTable GetBMIInfoTable(string SchoolID, string Grade, string Trimester)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT DISTINCT StudentInfo.AddmisionNo AS AdmissionNo, StudentInfo.DOB, StudentInfo.Gender, Height, Weight " +
                                   "FROM " +
                                   "StudentInfo LEFT OUTER JOIN BMIInformation ON StudentInfo.SchoolID = BMIInformation.SchoolID AND StudentInfo.AddmisionNo = BMIInformation.AdmissionNo AND StudentInfo.CurrentGrade = BMIInformation.Class AND Trimester = " + DateTime.Now.Year.ToString() + Trimester.ToString() +
                                   " WHERE StudentInfo.SchoolID ='" + SchoolID + "' AND StudentInfo.CurrentGrade = '" + Grade + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                return dt;
            }


        }

        [HttpPost]
        public ActionResult GetBMIInfo(string SchoolID, string Grade, string Trimester)
        {


            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT DISTINCT BMIInformation.TakenBy, BMIInformation.DatePerformed " +
                                   "FROM " +
                                   "StudentInfo LEFT OUTER JOIN BMIInformation ON StudentInfo.SchoolID = BMIInformation.SchoolID AND StudentInfo.AddmisionNo = BMIInformation.AdmissionNo AND StudentInfo.CurrentGrade = BMIInformation.Class AND Trimester = " + DateTime.Now.Year.ToString() + Trimester.ToString() +
                                   " WHERE StudentInfo.SchoolID ='" + SchoolID + "' AND StudentInfo.CurrentGrade = '" + Grade + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                object result = null;

                if (dt.Rows.Count > 0)
                    result = new { TakenBy = dt.Rows[0]["TakenBy"].ToString(), DatePerformed = dt.Rows[0]["DatePerformed"].ToString() };
                return Json(result, JsonRequestBehavior.AllowGet);


            }


        }

        private List<BMISDateInfo> getBMIs(string strSchoolID, string strGrade, int Trimester)
        {

            List<BMISDateInfo> retBMILst = new List<BMISDateInfo>();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  StudentInfo.AddmisionNo, StudentInfo.DOB, StudentInfo.Gender, BMIInformation.Height, BMIInformation.Weight, BMIInformation.BMI " +
                                   "FROM " +
                                   "StudentInfo LEFT OUTER JOIN BMIInformation ON StudentInfo.SchoolID = BMIInformation.SchoolID AND StudentInfo.AddmisionNo = BMIInformation.AdmissionNo AND StudentInfo.CurrentGrade = BMIInformation.Class AND Trimester = " + DateTime.Now.Year.ToString() + Trimester.ToString() +
                                   " WHERE StudentInfo.SchoolID ='" + strSchoolID + "' AND StudentInfo.CurrentGrade = '" + strGrade + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    BMISDateInfo thisRow = new BMISDateInfo();
                    thisRow.AdmissionNo = dt.Rows[i]["AddmisionNo"].ToString();
                    thisRow.DOB = Convert.ToDateTime(dt.Rows[i]["DOB"]).ToString("dd-MMM-yyyy");
                    thisRow.Gender = dt.Rows[i]["Gender"].ToString();
                    thisRow.Height = Convert.ToDecimal((dt.Rows[i]["Height"] == DBNull.Value ? 0 : dt.Rows[i]["Height"]));
                    thisRow.Weight = Convert.ToDecimal((dt.Rows[i]["Weight"] == DBNull.Value ? 0 : dt.Rows[i]["Weight"]));
                    thisRow.BMI = Convert.ToDecimal((dt.Rows[i]["BMI"] == DBNull.Value ? 0 : dt.Rows[i]["BMI"]));

                    //retBMILst[i] = thisRow;
                    retBMILst.Add(thisRow);
                }
            }

            //for (int i = retBMILst.Count; i < 100; i++)
            //    retBMILst.Add(new BMISDateInfo());

            ViewBag.BMIInfo = retBMILst;

            return retBMILst;

        }

        public ActionResult StudentsBulk(string SchoolID)
        {
            if (CheckAccessCanBeGranted(SchoolID))
            {
                List<StudentModel> lstStudents = new List<StudentModel>();
                lstStudents.Add(new StudentModel());
                ViewBag.ListStudents = lstStudents;
                StudentModel stuModel = new StudentModel();
                stuModel.SchoolID = SchoolID;
                return View(stuModel);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }

        }

        [HttpPost]
        public ActionResult Save(StudentModel model, FormCollection fCollection)
        {

            int totalRows = Convert.ToInt32(fCollection["CountRows"]);
            for (int i = 1; i <= totalRows; i++)
            {
                string labelValue = fCollection["AdmissionNo1"];
                //string selectValue = collection["select_" + i];

                //your insert method here
            }

            return View();
        }



        [HttpPost]
        public ActionResult StudentsBulk(HttpPostedFileBase excelFile, StudentModel model, FormCollection fCollection, string button)
        {
            int rowNo = 1;
            try
            {



                switch (button)
                {
                    case "Import":


                        if (excelFile == null || excelFile.ContentLength == 0)
                        {

                            return View();
                        }
                        else
                        {
                            if (excelFile.FileName.EndsWith("xls") || excelFile.FileName.EndsWith("xlsx"))
                            {

                                List<StudentModel> lstStudents = new List<StudentModel>();

                                using (var package = new ExcelPackage(excelFile.InputStream))
                                {
                                    // get the first worksheet in the workbook
                                    ExcelWorksheet worksheet = package.Workbook.Worksheets[2];

                                    for (int colInit = 1; colInit < 12; colInit++)
                                    {
                                        if (worksheet.Cells[1, colInit].Value == null)
                                        {
                                            @ViewBag.ErrorMessage = "Invalid Excel worksheet format found.";
                                            return View("../Login/Error");
                                        }
                                    }

                                    int col = 1;
                                    int ColAddmisionNo = 1;
                                    int ColNameWithInitials = 1;
                                    int ColNameInFull = 1;
                                    int ColDateOfBirth = 1;
                                    int ColGender = 1;
                                    int ColParentName = 1;
                                    int ColParentAddress = 1;
                                    int ColNIC = 1;
                                    int ColContactNo = 1;

                                    int ColGrade = 1;
                                    int ColCensusID = 1;

                                    //Check column nos according to the column name
                                    for (int colInit = 1; colInit < 12; colInit++)
                                    {
                                        if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "CENSUSID")
                                        {
                                            ColCensusID = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "GRADE")
                                        {
                                            ColGrade = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "ADMISSIONNO")
                                        {
                                            ColAddmisionNo = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "NAMEWITHINITIALS")
                                        {
                                            ColNameWithInitials = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "NAMEINFULL")
                                        {
                                            ColNameInFull = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "DATEOFBIRTH")
                                        {
                                            ColDateOfBirth = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "GENDER")
                                        {
                                            ColGender = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "PARENTNAME")
                                        {
                                            ColParentName = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "PARENTADDRESS")
                                        {
                                            ColParentAddress = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "NIC")
                                        {
                                            ColNIC = colInit;
                                        }
                                        else if (worksheet.Cells[1, colInit].Value.ToString().ToUpper() == "CONTACTNO")
                                        {
                                            ColContactNo = colInit;
                                        }

                                    }

                                    if (worksheet.Cells[2, col].Value != null)
                                    {
                                        string strCensusID = worksheet.Cells[2, ColCensusID].Value.ToString();
                                        if (strCensusID != GetCensorsID(model.SchoolID))
                                        {
                                            bool isVal = ModelState.IsValid;
                                            ModelState.AddModelError("Error", "Census ID is not matching with the logged in school");
                                            List<StudentModel> lstStudentsCensus = new List<StudentModel>();
                                            lstStudentsCensus.Add(new StudentModel());
                                            ViewBag.ListStudents = lstStudentsCensus;
                                            break;
                                        }
                                    }

                                    for (int row = 2; worksheet.Cells[row, col].Value != null; row++)
                                    {
                                        StudentModel stuModel = new StudentModel();
                                        stuModel.CurrentGrade = (worksheet.Cells[row, ColGrade].Value == null ? "" : worksheet.Cells[row, ColGrade].Value.ToString());
                                        stuModel.AddmisionNo = (worksheet.Cells[row, ColAddmisionNo].Value == null ? "" : worksheet.Cells[row, ColAddmisionNo].Value.ToString());
                                        stuModel.NameWithInitials = (worksheet.Cells[row, ColNameWithInitials].Value == null ? "" : worksheet.Cells[row, ColNameWithInitials].Value.ToString());
                                        stuModel.NameInFull = (worksheet.Cells[row, ColNameInFull].Value == null ? "" : worksheet.Cells[row, ColNameInFull].Value.ToString());
                                        stuModel.DOB = (worksheet.Cells[row, ColDateOfBirth].Value == null ? DateTime.Today : Convert.ToDateTime(worksheet.Cells[row, ColDateOfBirth].Value));
                                        stuModel.Gender = (worksheet.Cells[row, ColGender].Value == null ? "" : worksheet.Cells[row, ColGender].Value.ToString());
                                        stuModel.ParentName = (worksheet.Cells[row, ColParentName].Value == null ? "" : worksheet.Cells[row, ColParentName].Value.ToString());
                                        stuModel.ParentAddress = (worksheet.Cells[row, ColParentAddress].Value == null ? "" : worksheet.Cells[row, ColParentAddress].Value.ToString());
                                        stuModel.NIC = (worksheet.Cells[row, ColNIC].Value == null ? "" : worksheet.Cells[row, ColNIC].Value.ToString());
                                        stuModel.ContactNo = (worksheet.Cells[row, ColContactNo].Value == null ? "" : worksheet.Cells[row, ColContactNo].Value.ToString());
                                        stuModel.SchoolID = model.SchoolID;

                                        lstStudents.Add(stuModel);

                                        rowNo++;

                                    }

                                    rowNo = 0;
                                } // the using 

                                ViewBag.ListStudents = lstStudents;
                            }
                            else
                            {

                            }

                        }

                        break;
                    case "Save":
                        int totalRows = Convert.ToInt32(fCollection["CountRows"]);

                        if (!Validate_StudentsBulk(fCollection, model.SchoolID))
                        {
                            List<StudentModel> lstStudents = new List<StudentModel>();
                            lstStudents.Add(new StudentModel());
                            ViewBag.ListStudents = lstStudents;
                            return View();
                        }

                        string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                        using (var conn = new SqlConnection(strConnection))
                        using (var cmd = conn.CreateCommand())
                        {

                            conn.Open();
                            SqlTransaction transaction = conn.BeginTransaction("SampleTransaction");

                            cmd.Transaction = transaction;


                            rowNo = 2;
                            for (int i = 1; i <= totalRows; i++)
                            {
                                cmd.Parameters.Clear();
                                StudentModel stdModel = new StudentModel();

                                stdModel.AddmisionNo = fCollection["AdmissionNo" + i.ToString()];
                                stdModel.CurrentGrade = fCollection["CurrentGrade" + i.ToString()];
                                stdModel.NameWithInitials = fCollection["NameWithInitials" + i.ToString()];
                                stdModel.NameInFull = fCollection["NameInFull" + i.ToString()];
                                stdModel.DOB = Convert.ToDateTime(fCollection["DOB" + i.ToString()]);
                                stdModel.Gender = fCollection["Gender" + i.ToString()];
                                stdModel.ParentName = fCollection["ParentName" + i.ToString()];
                                stdModel.ParentAddress = fCollection["ParentAddress" + i.ToString()];
                                stdModel.NIC = fCollection["NIC" + i.ToString()];
                                stdModel.ContactNo = fCollection["ContactNo" + i.ToString()];
                                stdModel.SchoolID = model.SchoolID;

                               

                                rowNo++;
                                string ErrMsg = "";

                                if (stdModel.Gender.ToUpper() != "FEMALE" && stdModel.Gender.ToUpper() != "MALE")
                                {
                                    @ViewBag.ErrorMessage = "Gender should be Male or Female\nRowNo = " + rowNo.ToString();
                                    return View("../Login/Error");
                                }

                                if (!Save_StudentBulk(stdModel, cmd, out ErrMsg))
                                {
                                    transaction.Rollback();
                                    if (rowNo != 0)
                                        @ViewBag.ErrorMessage = ErrMsg + "\nRowNo = " + rowNo.ToString();
                                    return View("../Login/Error");
                                    //List<StudentModel> lstStudentsNew = new List<StudentModel>();
                                    //lstStudentsNew.Add(new StudentModel());
                                    //ViewBag.ListStudents = lstStudentsNew;

                                    //return View(model); 
                                }


                            }

                            transaction.Commit();

                            rowNo = 0;

                        }

                        List<StudentModel> lstStudentscommit = new List<StudentModel>();
                        lstStudentscommit.Add(new StudentModel());
                        ViewBag.ListStudents = lstStudentscommit;
                        break;
                }


                return View();

            }
            catch (Exception Err)
            {
                if (rowNo != 0)
                    @ViewBag.ErrorMessage = Err.Message + "\nRowNo = " + rowNo.ToString();
                return View("../Login/Error");
            }
        }

        //private bool IsGradeExisting(string SchoolID, string Grade)
        //{
        //    string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
        //    using (var conn = new SqlConnection(strConnection))
        //    using (var cmd = conn.CreateCommand())
        //    {

        //        conn.Open();
        //        cmd.CommandText = "SELECT   Grade " +
        //                           "FROM " +
        //                           "m_SchoolGradeInfo " +
        //                           "WHERE SchoolID ='" + SchoolID + "'";
        //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //        DataTable dt = new DataTable();
        //        da.Fill(dt);

        //        if (dt.Rows.Count > 0)
        //            return true;
        //        else
        //            return false;
        //    }


        //}

        private bool Validate_StudentsBulk(FormCollection fCollection, string SchoolID)
        {
            int totalRows = Convert.ToInt32(fCollection["CountRows"]);
            List<string> admissionNos = new List<string>();

            bool isValid = true;
            List<string> ErrorMsgs = new List<string>();
            string ErrMsg1 = "";
            string ErrMsg2 = "";
            string ErrMsg3 = "";

            for (int i = 1; i <= totalRows; i++)
            {
                string strAdmissionNo = fCollection["AdmissionNo" + i.ToString()];
                admissionNos.Add(strAdmissionNo);
                if (!IsGradeExisting(SchoolID, fCollection["CurrentGrade" + i.ToString()]))
                {
                    isValid = false;
                    ErrorMsgs.Add("Invalid Grade is found. Row No - " + i.ToString());
                    //ModelState.AddModelError("Error", "Invalid Grade is found. Row No - " + i.ToString());
                    //return false;
                    ErrMsg1 += ", " + i.ToString();
                }

                if (IsAdmissionNoAlreadyExisting(strAdmissionNo, SchoolID))
                {
                    isValid = false;

                    ErrorMsgs.Add("Admission No -" + strAdmissionNo  + " already existing");
                    //ModelState.AddModelError("Error", "Admission No is already existing\n hi");
                    ErrMsg2 += ", " + strAdmissionNo;
                }
            }


            var g = admissionNos.GroupBy(i => i);
            foreach (var grp in g)
            {
                if (grp.Count() > 1)
                {
                    isValid = false;
                    ErrorMsgs.Add("Duplicate admission numbers found. - " + grp.Key.ToString());
                    //ModelState.AddModelError("Error", "Duplicate admission numbers found. - " + grp.Key.ToString());
                    //return false;
                    ErrMsg3 += ", " + grp.Key.ToString();
                }
            }



            if (!isValid)
            {
                string ErrorMsg = (ErrMsg1 != "" ? "Invalid Grade is found. Row No - " + ErrMsg1 : "") +
                                    (ErrMsg2 != "" ? "Admission No -" + ErrMsg2 + " already existing" : "") +
                                   (ErrMsg3 != "" ? "Duplicate admission numbers found. - " + ErrMsg3 : "");
                ModelState.AddModelError("Error", ErrorMsg);
                return false;
            }
            else
                return true;
        }

      
        private bool IsAdmissionNoAlreadyExisting(string strAdmissionNo, string SchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID FROM  StudentInfo WHERE SchoolID = '" + SchoolID + "' AND AddmisionNo  = '" + strAdmissionNo + "' AND YEAR =  " + DateTime.Today.Year;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    return true;
                }

            }
            return false;
        }
        private DataTable Get_Data_From_File(string strFile)
        {
            try
            {
                //String sConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                //    "Data Source=" + strFile + ";" +
                //    "Extended Properties=Excel 12.0;";

                String sConnectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=strFile;Extended Properties=""Excel 12.0 Xml;HDR=YES""");

                sConnectionString = sConnectionString.Replace("strFile", strFile);

                // Create connection object by using the preceding connection string.
                OleDbConnection objConn = new OleDbConnection(sConnectionString);

                // Open connection with the database.
                objConn.Open();

                // The code to follow uses a SQL SELECT command to display the data from the worksheet.

                // Create new OleDbCommand to return data from worksheet.
                OleDbCommand objCmdSelect = new OleDbCommand("SELECT * FROM [Student Information$]", objConn);

                // Create new OleDbDataAdapter that is used to build a DataSet
                // based on the preceding SQL SELECT statement.
                OleDbDataAdapter objAdapter1 = new OleDbDataAdapter();

                // Pass the Select command to the adapter.
                objAdapter1.SelectCommand = objCmdSelect;

                // Create new DataSet to hold information from the worksheet.
                DataSet objDataset = new DataSet();

                // Fill the DataSet with the information from the worksheet.
                objAdapter1.Fill(objDataset, "Excel Data");

                return objDataset.Tables[0];

            }
            catch (Exception Err)
            {
                return new DataTable();
            }

        }

        //public ActionResult ZonalSchoolStudentSummery()
        //{
        //    ZonalSchoolStudentSummery ZonalSumm = new ZonalSchoolStudentSummery();

        //    return View(ZonalSumm);
        //}

        public ActionResult ZonalSchoolStudentSummery()
        {
            int iYear = DateTime.Today.Year;

            ZonalSchoolStudentSummery ZonalSumm = new ZonalSchoolStudentSummery();

            ViewBag.Years = LoadYears(iYear);

            return View(ZonalSumm);
        }

        public ActionResult FundsReceived()
        {
            Models.FundsReceivedModel fundsRcvd = new FundsReceivedModel();
            List<SelectListItem> MyList = new List<SelectListItem>();
            //MyList.Add(new SelectListItem() { Text = "Select Supplier", Value = "Select Supplier" });
            ViewBag.Suppliers = MyList;
            return View(fundsRcvd);
        }

        public ActionResult ImpressRequest()
        {
            ImpressRequestModel impReq = new ImpressRequestModel();

            return View(impReq);
        }

        [HttpPost]
        public ActionResult ImpressRequest(ImpressRequestModel model)
        {
            if (ModelState.IsValid)
            {
                if (Save_ImpressRequest(model))
                {
                    ModelState.Clear();
                    model = new ImpressRequestModel();

                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            return View(model);
        }

        private bool Save_ImpressRequest(ImpressRequestModel model)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "INSERT INTO FundsReceived ( CensorsID, ZonalRequestAmount, PDEReleasedAmount, ForMonth, ReleasedDate, CreateUser) " +
                                        "VALUES(@CensorsID, @ZonalRequestAmount, @PDEReleasedAmount, @ForMonth, @ReleasedDate, @CreateUser)";


                    cmd.Parameters.AddWithValue("@CensorsID", model.CensorsID);
                    cmd.Parameters.AddWithValue("@ZonalRequestAmount", model.ZonalRequestAmount);
                    cmd.Parameters.AddWithValue("@PDEReleasedAmount", model.PDEReleasedAmount);
                    cmd.Parameters.AddWithValue("@ForMonth", model.ForMonth);
                    cmd.Parameters.AddWithValue("@ReleasedDate", model.ReleasedDate);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }


                return true;
            }
            catch (Exception Err)
            {
                string ss = Err.Message;
                return false;
            }
        }

        [HttpPost]
        public ActionResult FundsReceived(Models.FundsReceivedModel model)
        {
            if (ModelState.IsValid)
            {
                if (Save_FundsReceived(model))
                {
                    ModelState.Clear();
                    model = new Models.FundsReceivedModel();

                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            return View(model);
        }

        private bool Save_FundsReceived(FundsReceivedModel model)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "INSERT INTO FundsReceived (CensorsID, ForMonth, NameOfFoodSupplier, PaidDate, ChequeNo, PaidAmount, CreateUser) " +
                                        "VALUES(@CensorsID, @ForMonth, @NameOfFoodSupplier, @PaidDate, @ChequeNo, @PaidAmount, @CreateUser)";


                    cmd.Parameters.AddWithValue("@CensorsID", model.CensorsID);
                    cmd.Parameters.AddWithValue("@ForMonth", model.ForMonth);
                    cmd.Parameters.AddWithValue("@NameOfFoodSupplier", model.NameOfFoodSupplier);
                    cmd.Parameters.AddWithValue("@PaidDate", model.PaidDate);
                    cmd.Parameters.AddWithValue("@ChequeNo", model.ChequeNo);
                    cmd.Parameters.AddWithValue("@PaidAmount", model.PaidAmount);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }


                return true;
            }
            catch (Exception Err)
            {
                string ss = Err.Message;
                return false;
            }
        }

        [HttpPost]
        public ActionResult GetStudentsInSchool(string CensorsID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT   StudentsMale, StudentsFemale " +
                                   "FROM " +
                                   "m_Schools " +
                                   "WHERE m_Schools.CensorsID ='" + CensorsID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["StudentsMale"].ToString().Trim(), Value = Convert.ToString(mydataRow["StudentsFemale"]) });
                }

                var result = new { Boys = dt.Rows[0]["StudentsMale"].ToString(), Girls = dt.Rows[0]["StudentsFemale"].ToString(), Total = Convert.ToInt32(dt.Rows[0]["StudentsMale"]) + Convert.ToInt32(dt.Rows[0]["StudentsFemale"]) };
                return Json(result, JsonRequestBehavior.AllowGet);

            }


        }


        public ActionResult GetSuppliers4School(string CensorsID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT   m_SupplierInformation.SupplierName " +
                                   "FROM " +
                                   "m_SupplierInformation INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID " +
                                   "WHERE m_Schools.CensorsID ='" + CensorsID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["SupplierName"].ToString().Trim(), Value = Convert.ToString(mydataRow["SupplierName"]) });
                }

                return Json(MyList);

            }


        }

        public ActionResult GetImpressRequests4Province(int CensorsID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                        "m_Schools.CensorsID, ImpressRequest.ZonalRequestAmount, ImpressRequest.PDEReleasedAmount, ImpressRequest.ForMonth, ImpressRequest.ReleasedDate " +
                                    "FROM " +
                                        "m_Schools INNER JOIN ImpressRequest  ON m_Schools.CensorsID = ImpressRequest.CensorsID " +
                                    "WHERE     (m_Schools.ProvinceID = (SELECT m_Schools.ProvinceID FROM m_Schools WHERE m_Schools.CensorsID = '" + CensorsID + "'))";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<ImpressRequestModel> lstRcvdFunds = new List<ImpressRequestModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    lstRcvdFunds.Add(new ImpressRequestModel
                    {
                        CensorsID = mydataRow["CensorsID"].ToString().Trim(),
                        ZonalRequestAmount = Convert.ToDecimal(mydataRow["ZonalRequestAmount"]),
                        //SchoolName = Convert.ToDateTime(mydataRow["PaidDate"]).ToString("dd-MMM-yyyy"),
                        PDEReleasedAmount = Convert.ToDecimal(mydataRow["PDEReleasedAmount"]),
                        ForMonth = Convert.ToDateTime(mydataRow["ForMonth"]),
                        ReleasedDate = Convert.ToDateTime(mydataRow["ReleasedDate"])

                    });
                }

                return Json(lstRcvdFunds);

            }
        }

        public ActionResult GetFundsReceived4School(int CensorsID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                        "m_Schools.CensorsID, FundsReceived.NameOfFoodSupplier, FundsReceived.PaidDate, FundsReceived.ChequeNo, FundsReceived.PaidAmount " +
                                    "FROM " +
                                        "m_Schools INNER JOIN FundsReceived ON m_Schools.CensorsID = FundsReceived.CensorsID " +
                                    "WHERE     (m_Schools.ProvinceID = (SELECT m_Schools.ProvinceID FROM m_Schools WHERE m_Schools.CensorsID = '" + CensorsID + "'))";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<FundsReceivedModel> lstRcvdFunds = new List<FundsReceivedModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    lstRcvdFunds.Add(new FundsReceivedModel
                    {
                        CensorsID = mydataRow["CensorsID"].ToString().Trim(),
                        NameOfFoodSupplier = Convert.ToString(mydataRow["NameOfFoodSupplier"]),
                        SchoolName = Convert.ToDateTime(mydataRow["PaidDate"]).ToString("dd-MMM-yyyy"),
                        PaidAmount = Convert.ToDecimal(mydataRow["PaidAmount"]),
                        ChequeNo = mydataRow["ChequeNo"].ToString()
                    });
                }

                return Json(lstRcvdFunds);

            }
        }

        public ActionResult MonitoringOfficerInformation()
        {
            Models.MonitoringOfficerModel MOInfo = new MonitoringOfficerModel();
            return View(MOInfo);
        }

        [HttpPost]
        public ActionResult MonitoringOfficerInformation(Models.MonitoringOfficerModel model)
        {
            if (ModelState.IsValid)
            {
                if (Save_MonitoringOfficerInfo(model))
                {
                    ModelState.Clear();
                    model = new Models.MonitoringOfficerModel();

                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }
            return View(model);
        }

        private bool Save_MonitoringOfficerInfo(MonitoringOfficerModel model)
        {
            try
            {

                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO MonitoringOfficerInformation (CensorsID, NameOfOfficer, Designation, ContactNo, CreateUser) " +
                    //                    "VALUES(@CensorsID,  @NameOfOfficer, @Designation, @ContactNo, @CreateUser)";
                    cmd.CommandText = "UpdateMonitoringOfficer";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@CensorsID", model.CensorsID);
                    cmd.Parameters.AddWithValue("@NameOfOfficer", model.NameOfOfficer);
                    cmd.Parameters.AddWithValue("@Designation", model.Designation);
                    cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }


                return true;
            }
            catch (Exception Err)
            {
                string ss = Err.Message;
                return false;
            }
        }

        [HttpPost]
        public ActionResult GetCensorsDetails(string CensorsID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT " +
                                        "m_Schools.SchoolName, m_Zones.ZoneName, m_Provinces.ProvinceName, m_Devisions.DevisionName " +
                                   "FROM " +
                                        "m_Schools INNER JOIN   m_Provinces ON m_Schools.ProvinceID = m_Provinces.ProvinceID " +
                                        "INNER JOIN m_Zones ON m_Provinces.ProvinceID = m_Zones.ProvinceID AND " +
                                        "m_Schools.ZoneID = m_Zones.ZoneID INNER JOIN m_Devisions ON " +
                                        "m_Schools.ProvinceID = m_Devisions.ProvinceID AND m_Schools.ZoneID = m_Devisions.ZoneID AND m_Schools.DevisionID = m_Devisions.DevisionID " +
                                   " WHERE " +
                                        "m_Schools.CensorsID = '" + CensorsID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                string strSchool = "";
                string strProvince = "";
                string strZone = "";
                string strDevision = "";

                if (dt.Rows.Count > 0)
                {
                    strSchool = dt.Rows[0]["SchoolName"].ToString();
                    strProvince = dt.Rows[0]["ProvinceName"].ToString();
                    strZone = dt.Rows[0]["ZoneName"].ToString();
                    strDevision = dt.Rows[0]["DevisionName"].ToString();

                }

                cmd.CommandText = "SELECT " +
                                         "NameOfOfficer, Designation, ContactNo " +
                                    "FROM " +
                                         "MonitoringOfficerInformation " +
                                    " WHERE " +
                                         "CensorsID = '" + CensorsID + "' ORDER BY YEAR DESC";

                SqlDataAdapter da2 = new SqlDataAdapter(cmd);
                DataTable dt2 = new DataTable();
                da2.Fill(dt2);

                string strNameOfOfficer = "";
                string strDesignation = "";
                string strContactNo = "";
                if (dt2.Rows.Count > 0)
                {
                    strNameOfOfficer = dt2.Rows[0]["NameOfOfficer"].ToString();
                    strDesignation = dt2.Rows[0]["Designation"].ToString();
                    strContactNo = dt2.Rows[0]["ContactNo"].ToString();
                }

                var result = new { Province = strProvince, Zone = strZone, Devision = strDevision, School = strSchool, NameOfOfficer = strNameOfOfficer, Designation = strDesignation, ContactNo = strContactNo };
                return Json(result, JsonRequestBehavior.AllowGet);

            }

        }

        public ActionResult SupplierInformation(string strSchoolID)
        {
            if (CheckAccessCanBeGranted(strSchoolID))
            {
                Models.SupplierInfoModel supInfo = new SupplierInfoModel();
                supInfo.SchoolID = strSchoolID;
                ViewBag.Grades = GetSchoolGrades(strSchoolID);
                ViewBag.SupplierInfo = GetSupplierInfo(strSchoolID);
                ViewBag.BankID = GetBanksWithIDs();
                ViewBag.BankBranchID = new List<SelectListItem>();
                return View(supInfo);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        public List<SelectListItem> GetSchoolGrades(string strSchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  Grade FROM m_SchoolGradeInfo WHERE SchoolID = '" + strSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["Grade"].ToString().Trim(), Value = Convert.ToString(mydataRow["Grade"]) });
                }

                return MyList;
            }

        }

        [HttpPost]
        public ActionResult SupplierInformation(Models.SupplierInfoModel model)
        {
            if (ModelState.IsValid)
            {
                string strSchID = model.SchoolID;
                //if (Validated_Supplier(model))
                //{
                if (Save_SupplierInfo(model))
                {
                    ModelState.Clear();
                    model = new Models.SupplierInfoModel();
                    model.SchoolID = strSchID;       
                    ViewBag.SuccessMessage = "Supplier was saved successfully.";
                }
                //}
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            ViewBag.Grades = GetSchoolGrades(model.SchoolID);
            ViewBag.SupplierInfo = GetSupplierInfo(model.SchoolID);
            ViewBag.BankID = GetBanksWithIDs();
            ViewBag.BankBranchID = GetBankBranchas(model.BankID);

            return View(model);
        }

        private List<SupplierInfoModel> GetSupplierInfo(int ProvinceID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                
                conn.Open();
                cmd.CommandText = "SELECT  m_Zones.ZoneName, m_Devisions.DevisionName, m_Schools.SchoolName, m_Schools.CensorsID, m_SupplierInformation.SupplierName, m_SupplierInformation.Address, " +
                                  "m_SupplierInformation.NIC, m_SupplierInformation.Phone, m_SupplierInformation.BankName, " +
                                  "m_SupplierInformation.BankAccountNo, m_SupplierInformation.Grade, m_SupplierInformation.NoOfMaleStudents, " +
                                  "m_SupplierInformation.NoOfFemaleStudents FROM  m_SupplierInformation " +
                                  "INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID " +
                                  " INNER JOIN m_Zones ON m_Schools.ZoneID = m_Zones.ZoneID AND m_Schools.ProvinceID = m_Zones.ProvinceID " +
                                  "INNER JOIN m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ZoneID = m_Devisions.ZoneID AND m_Schools.ProvinceID = m_Devisions.ProvinceID " +
                                  "WHERE  m_Schools.ProvinceID = " + ProvinceID.ToString();  //+ " AND m_Schools.ZoneID = '" + strZoneID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SupplierInfoModel> MyList = new List<SupplierInfoModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    SupplierInfoModel supInfo = new SupplierInfoModel();
                    supInfo.SchoolName = mydataRow["SchoolName"].ToString().Trim();
                    supInfo.CensusID = mydataRow["CensorsID"].ToString().Trim();
                    supInfo.SupplierName = mydataRow["SupplierName"].ToString().Trim();
                    supInfo.Address = mydataRow["Address"].ToString().Trim();
                    supInfo.NIC = mydataRow["NIC"].ToString().Trim();
                    supInfo.Phone = mydataRow["Phone"].ToString().Trim();
                    supInfo.Grade = mydataRow["Grade"].ToString().Trim();
                    supInfo.BankName = mydataRow["BankName"].ToString().Trim();
                    supInfo.BankAccountNo = mydataRow["BankAccountNo"].ToString().Trim();
                    supInfo.NoOfMaleStudents = Convert.ToInt32(mydataRow["NoOfMaleStudents"]);
                    supInfo.NoOfFemaleStudents = Convert.ToInt32(mydataRow["NoOfFemaleStudents"]);
                    supInfo.Zone = mydataRow["ZoneName"].ToString();
                    supInfo.Devision = mydataRow["DevisionName"].ToString();

                    MyList.Add(supInfo);
                }

                return MyList;
            }
        }

        private List<SupplierInfoModel> GetSupplierInfo(string strProvinceID, string strZoneID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  m_Zones.ZoneName, m_Devisions.DevisionName, m_Schools.SchoolName, m_Schools.CensorsID, m_SupplierInformation.SupplierName, m_SupplierInformation.Address, " +
                                  "m_SupplierInformation.NIC, m_SupplierInformation.Phone, m_SupplierInformation.BankName, " +
                                  "m_SupplierInformation.BankAccountNo, m_SupplierInformation.Grade, m_SupplierInformation.NoOfMaleStudents, " +
                                  "m_SupplierInformation.NoOfFemaleStudents FROM  m_SupplierInformation " +
                                  "INNER JOIN m_Schools ON m_SupplierInformation.SchoolID = m_Schools.SchoolID " +
                                  " INNER JOIN m_Zones ON m_Schools.ZoneID = m_Zones.ZoneID AND m_Schools.ProvinceID = m_Zones.ProvinceID " +
                                  "INNER JOIN m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID AND m_Schools.ZoneID = m_Devisions.ZoneID AND m_Schools.ProvinceID = m_Devisions.ProvinceID " +
                                  "WHERE  m_Schools.ProvinceID = " + strProvinceID + " AND m_Schools.ZoneID = '" + strZoneID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SupplierInfoModel> MyList = new List<SupplierInfoModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    SupplierInfoModel supInfo = new SupplierInfoModel();
                    supInfo.SchoolName = mydataRow["SchoolName"].ToString().Trim();
                    supInfo.CensusID = mydataRow["CensorsID"].ToString().Trim();
                    supInfo.SupplierName = mydataRow["SupplierName"].ToString().Trim();
                    supInfo.Address = mydataRow["Address"].ToString().Trim();
                    supInfo.NIC = mydataRow["NIC"].ToString().Trim();
                    supInfo.Phone = mydataRow["Phone"].ToString().Trim();
                    supInfo.Grade = mydataRow["Grade"].ToString().Trim();
                    supInfo.BankName = mydataRow["BankName"].ToString().Trim();
                    supInfo.BankAccountNo = mydataRow["BankAccountNo"].ToString().Trim();
                    supInfo.NoOfMaleStudents = Convert.ToInt32(mydataRow["NoOfMaleStudents"]);
                    supInfo.NoOfFemaleStudents = Convert.ToInt32(mydataRow["NoOfFemaleStudents"]);
                    supInfo.Zone = mydataRow["ZoneName"].ToString();
                    supInfo.Devision = mydataRow["DevisionName"].ToString();

                    MyList.Add(supInfo);
                }

                return MyList;
            }
        }

        private List<SupplierInfoModel> GetSupplierInfo(string SchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT  SupplierName, Address, NIC, Phone, BankName, BankAccountNo, Grade, NoOfMaleStudents, NoOfFemaleStudents, isNull(BankID,0) AS BankID, isNull(BankBranchID,0) AS BankBranchID FROM  m_SupplierInformation  WHERE SchoolID = '" + SchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SupplierInfoModel> MyList = new List<SupplierInfoModel>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    SupplierInfoModel supInfo = new SupplierInfoModel();

                    supInfo.SupplierName = mydataRow["SupplierName"].ToString().Trim();
                    supInfo.Address = mydataRow["Address"].ToString().Trim();
                    supInfo.NIC = mydataRow["NIC"].ToString().Trim();
                    supInfo.Phone = mydataRow["Phone"].ToString().Trim();
                    supInfo.Grade = mydataRow["Grade"].ToString().Trim();
                    supInfo.BankName = mydataRow["BankName"].ToString().Trim();
                    supInfo.BankAccountNo = mydataRow["BankAccountNo"].ToString().Trim();
                    supInfo.NoOfMaleStudents = Convert.ToInt32(mydataRow["NoOfMaleStudents"]);
                    supInfo.NoOfFemaleStudents = Convert.ToInt32(mydataRow["NoOfFemaleStudents"]);
                    supInfo.BankID = Convert.ToInt32(mydataRow["BankID"]);
                    supInfo.BankBranchID = Convert.ToInt32(mydataRow["BankBranchID"]);
                    MyList.Add(supInfo);
                }

                return MyList;
            }
        }

        private bool Validated_Supplier(SupplierInfoModel model)
        {
            try
            {


                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "SELECT " +
                                            "SupplierName " +
                                      "FROM " +
                                            "m_SupplierInformation " +
                                      "WHERE " +
                                            "SupplierName = '" + model.SupplierName + "'";

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        ModelState.AddModelError("", "Supplier is already existing.");
                        return false;
                    }
                }

                return true;
            }
            catch (Exception Err)
            {
                ModelState.AddModelError("", Err.Message);
                return false;
            }

        }

        private bool Save_SupplierInfo(SupplierInfoModel model)
        {
            try
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = "UpdateSupplierInfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@SupplierName", model.SupplierName);
                    cmd.Parameters.AddWithValue("@Address", model.Address);
                    cmd.Parameters.AddWithValue("@NIC", model.NIC);
                    cmd.Parameters.AddWithValue("@Phone", model.Phone);
                    cmd.Parameters.AddWithValue("@BankAccountNo", model.BankAccountNo);
                    cmd.Parameters.AddWithValue("@Grade", model.Grade.TrimEnd(','));
                    cmd.Parameters.AddWithValue("@NoOfMaleStudents", model.NoOfMaleStudents);
                    cmd.Parameters.AddWithValue("@NoOfFemaleStudents", model.NoOfFemaleStudents);
                    cmd.Parameters.AddWithValue("@BankID", model.BankID);
                    cmd.Parameters.AddWithValue("@BankBranchID", model.BankBranchID);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        //Wasa
        public ActionResult SupplierPayments(int? id)
        {
            List<SupplierPaymentRequest> PaymentRequestList = GetSupplierPaymentRequest();

            SupplierPaymentRequest paymentReq = PaymentRequestList.FirstOrDefault(p => p.Id == id);
            if (paymentReq==null)
            {
                paymentReq = new SupplierPaymentRequest();
                paymentReq.RequestDate = DateTime.Now.ToString("yyyy/MM/dd");
                paymentReq.Status = "Open";
            }

            paymentReq.PaymentDetails = LoadSuppliersForPayment(paymentReq.ProvinceID, paymentReq.ZoneID, paymentReq.Year, paymentReq.Month, paymentReq.Id);
            ViewBag.SupplierPaymentRequestList = PaymentRequestList;
            ViewBag.Month = GetMonths(paymentReq.Month);
            ViewBag.Year = GetResentYears(paymentReq.Year.ToString());
            ViewBag.ProvinceID = GetProvincesByUser(LoggedUserName, paymentReq.ProvinceID.ToString());
            ViewBag.ZoneID = GetZonesByUserProvice(LoggedUserName, paymentReq.ProvinceID, paymentReq.ZoneID);
            return View(paymentReq);
        }

        public ActionResult LoadSuppliersForPaymentJson(int provinceId, string zoneId, int year, string month,
            int paymentReqNo)
        {
            try
            {
                if (paymentReqNo <= 0)
                {
                    var paymentId = GetAlreadySupplierPaymentFor(provinceId, zoneId, year, month);
                    if (paymentId > 0)
                    {
                        return Json(
                            new {message = "Payment has been initiated, please follow with that.!", status = false , id = paymentId },
                            JsonRequestBehavior.AllowGet);
                    }
                }
                var data = LoadSuppliersForPayment(provinceId, zoneId, year, month, paymentReqNo);
                if (data == null)
                {
                    return Json( new { message = "unexpected error occured. Please re-try .!", status = false , Error = true },
                        JsonRequestBehavior.AllowGet);
                }
               
                return View("_SupplierDetailView", data);
            }
            catch (Exception e)
            {
                _log.Error($"Error : {e.Message}");
            }

            return Json(new { message = "unexpected error occured. Please re-try .!", status = false, Error = true },
                        JsonRequestBehavior.AllowGet);
        }

        private int GetAlreadySupplierPaymentFor(int provinceId, string zoneId, int year, string month)
        {
            ;
            using (var conn = new SqlConnection(_strConnection))
            {
                var cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandText = string.Format("SELECT PaymentReqNo FROM SupplierPaymentReq_Header WHERE " +
                                                "ProvinceID =@provinceId AND ZoneID = @zoneId AND Year = @year AND  Month = @month AND Status = @status");
                cmd.CommandType =CommandType.Text;
                cmd.Parameters.AddWithValue("@provinceId", provinceId);
                cmd.Parameters.AddWithValue("@zoneId", zoneId);
                cmd.Parameters.AddWithValue("@year", year);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@status", "New");

                var dataReader  = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dataReader);
                var data = dt.Rows;
                if (data != null && data.Count > 0)
                {
                    return Convert.ToInt32(data[0]["PaymentReqNo"]);
                }
                return 0;
            }
        }

        public List<SupplierPaymentRequestDetail> LoadSuppliersForPayment(int ProvinceID, string ZoneID, int year, string Month, int PaymentReqNo)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            try
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {
                    string UserName = "";
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    // the code that you want to measure comes here
                    
                    conn.Open();
                    cmd.CommandTimeout = 180;
                    cmd.CommandText = "EXEC LoadSuppliers " + ProvinceID + ", '" + ZoneID + "', " + year + ", '" + Month + "', " + PaymentReqNo;
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.SelectCommand.CommandTimeout = 120;
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                   _log.Info($"Data loading time for loadsuppliers : province : {ProvinceID} zoneId {ZoneID} year {year} month {Month} paymentReqNo {PaymentReqNo} : time executed - {elapsedMs}");

                    //         m_BanksBranch.BranchCode, Maximum_PayableAmount_For_Scool.MAXAmount

                    List<SupplierPaymentRequestDetail> MyList = new List<SupplierPaymentRequestDetail>();
                    foreach (DataRow mydataRow in dt.Rows)
                    {
                        SupplierPaymentRequestDetail supInfo = new SupplierPaymentRequestDetail();

                        supInfo.CensorsID = mydataRow["CensorsID"].ToString().Trim();
                        supInfo.Supplier.ID = mydataRow["ID"].ToString().Trim();
                        supInfo.PaymentHeaderStatus = Convert.ToString(mydataRow["Status"]);
                        supInfo.Supplier.SupplierName = mydataRow["SupplierName"].ToString().Trim();
                        supInfo.Supplier.BankAccountNo = mydataRow["BankAccountNo"].ToString().Trim();
                        supInfo.Supplier.BankName = mydataRow["BankName"].ToString().Trim();
                        supInfo.BranchName = mydataRow["BranchName"].ToString().Trim();
                        if (mydataRow["BankID"]!=DBNull.Value)
                            supInfo.Supplier.BankID = Convert.ToInt16(mydataRow["BankID"]);
                        if (mydataRow["BankBranchID"] != DBNull.Value)
                            supInfo.Supplier.BankBranchID = Convert.ToInt16(mydataRow["BankBranchID"]);
                        //supInfo.Supplier.BranchCode = mydataRow["BranchCode"].ToString().Trim();
                        supInfo.MAXAmount = Convert.ToDecimal(mydataRow["MAXAmount"]);
                        supInfo.Amount = Convert.ToDecimal(mydataRow["Amount"]);
                        MyList.Add(supInfo);
                    }
                   
                    return MyList;
                }
            }
            catch (Exception e)
            {
                _log.Error($"{"Error"} {e.Message}");
                return null;
            }
        }


        [HttpPost]
        public ActionResult SupplierPayments(FormCollection FormData)
        {
            string action = FormData["Action"];
            SupplierPaymentRequest modal = MakeSupplierPaymentRequest(FormData);
            if (string.Equals("Save", action, StringComparison.InvariantCultureIgnoreCase))
            {
                    modal.Status = "New";
                    modal.CreateBy = User.Identity.Name;
                    modal.CreateDate = DateTime.Now;
                    SavePaymentRequest(modal);
                
            }else if (string.Equals("Approvel", action, StringComparison.InvariantCultureIgnoreCase))
            {
                modal.Status = "Approved-Zone";
                modal.ApprovedBy = User.Identity.Name;
                modal.ApprovedDate = DateTime.Now;
                SavePaymentRequest(modal);
            }
            else
            {
                modal.Status = "Forwarded";
                modal.ProvincialApprovedBy = User.Identity.Name;
                modal.ProvincialApprovedDate = DateTime.Now;
                SavePaymentRequest(modal);
            }

            modal.PaymentDetails = LoadSuppliersForPayment(modal.ProvinceID, modal.ZoneID, modal.Year, modal.Month, modal.Id);
            ViewBag.SupplierPaymentRequestList = GetSupplierPaymentRequest();
            ViewBag.Month = GetMonths(modal.Month);
            ViewBag.Year = GetResentYears(modal.Year.ToString());
            ViewBag.ProvinceID = GetProvincesByUser(LoggedUserName, modal.ProvinceID.ToString());
            ViewBag.ZoneID = GetZonesByUserProvice(LoggedUserName, modal.ProvinceID,modal.ZoneID);
            modal.Id = 0;
            return View(modal);
        }

        private SupplierPaymentRequest MakeSupplierPaymentRequest(FormCollection FormData)
        {          
            SupplierPaymentRequest modal = new SupplierPaymentRequest();
            modal.Id = Convert.ToInt16(FormData["Id"]);
            modal.ProvinceID = Convert.ToInt16(FormData["ProvinceID"]);
            modal.ZoneID = FormData["ZoneID"];
            modal.Year = Convert.ToInt16(FormData["Year"]);
            modal.Month = FormData["Month"];
            modal.RequestDate = FormData["RequestDate"];
            modal.CreateBy = LoggedUserName;
            modal.Status = "New";
            modal.Details  = new Dictionary<int,decimal>();

            foreach(var item in FormData.AllKeys)
            {
                if(item.IndexOf('-') > 0)
                {
                    var arr = item.Split('-');
                    modal.Details.Add(Convert.ToInt16(arr[1]), Convert.ToDecimal(FormData[item]));
                }
            }
            modal.Total = (float)modal.Details.Values.Sum(c => Convert.ToDecimal(c)); 
            return modal;
        }

        private bool ValidatePaymentRequest(SupplierPaymentRequest paymentReq)
        {
            return true;
        }

        private bool SavePaymentRequest(SupplierPaymentRequest model)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "Update_SupplierPaymentReq_Header";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@PaymentReqNo", model.Id);
                cmd.Parameters.AddWithValue("@ReqDate", model.RequestDate);
                cmd.Parameters.AddWithValue("@ProvinceID", model.ProvinceID);
                cmd.Parameters.AddWithValue("@ZoneID", model.ZoneID);
                cmd.Parameters.AddWithValue("@Year", (model.Year));
                cmd.Parameters.AddWithValue("@Month", model.Month);
                cmd.Parameters.AddWithValue("@CreateUser", model.CreateBy);
                cmd.Parameters.AddWithValue("@CreateDate", model.CreateDate);
                cmd.Parameters.AddWithValue("@AprovedUser", model.ApprovedBy);
                cmd.Parameters.AddWithValue("@ApprovedDate", model.ApprovedDate);
                cmd.Parameters.AddWithValue("@ProvincialApp_User", model.ProvincialApprovedBy);
                cmd.Parameters.AddWithValue("@ProvincialApp_Date", model.ProvincialApprovedDate);
                cmd.Parameters.AddWithValue("@TotalAmount", model.Total);
                cmd.Parameters.AddWithValue("@Status", model.Status);
                cmd.Parameters.AddWithValue("@TranType", model.Id == 0 ? "NEW" : "Update");

                SqlParameter outPutVal = new SqlParameter("@New_PaymentReqNo", SqlDbType.Int);
                outPutVal.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(outPutVal);

                cmd.ExecuteNonQuery();

                if (outPutVal.Value != DBNull.Value)
                {
                    model.Id = Convert.ToInt32(outPutVal.Value);
                    SavePaymentRequestDetails(model);
                }
                
            }
            return true;
        }

        private bool SavePaymentRequestDetails(SupplierPaymentRequest model)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;

            if (model.Details != null && model.Details.Any())
            {
                using (var conn = new SqlConnection(strConnection))
                {
                    conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = string.Format("DELETE SupplierPaymentReq_Details WHERE PaymentReqNo = {0}", model.Id);
                        cmd.ExecuteNonQuery();
                    }
                    foreach (var item in model.Details)
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "Update_SupplierPaymentReq_Details";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PaymentReqNo", model.Id);
                            cmd.Parameters.AddWithValue("@SupplyerID", item.Key);
                            cmd.Parameters.AddWithValue("@Amount", item.Value);
                            cmd.Parameters.AddWithValue("@Year", model.Year);
                            cmd.Parameters.AddWithValue("@Month", model.Month);
                            cmd.Parameters.AddWithValue("@Paid",false);

                            cmd.ExecuteNonQuery();
                        }
                    }
                    return true;
                }
            }
            return false;

        }

        private List<SelectListItem> GetMonths(string selected)
        {
            List<SelectListItem> MyList = new List<SelectListItem>();
            MyList.Add(new SelectListItem { Text = "January", Value = "January" });
            MyList.Add(new SelectListItem { Text = "February", Value = "February" });
            MyList.Add(new SelectListItem { Text = "March", Value = "March" });
            MyList.Add(new SelectListItem { Text = "April", Value = "April" });
            MyList.Add(new SelectListItem { Text = "May", Value = "May" });
            MyList.Add(new SelectListItem { Text = "June", Value = "June" });
            MyList.Add(new SelectListItem { Text = "July", Value = "July" });
            MyList.Add(new SelectListItem { Text = "August", Value = "August" });
            MyList.Add(new SelectListItem { Text = "September", Value = "September" });
            MyList.Add(new SelectListItem { Text = "October", Value = "October" });
            MyList.Add(new SelectListItem { Text = "November", Value = "November" });
            MyList.Add(new SelectListItem { Text = "December", Value = "December" });

            var data = MyList.Find(i=>i.Value==selected);
            if (data != null)
                data.Selected = true;
            return MyList;
        }

        private List<SelectListItem> GetResentYears(string selected)
        {
            List<SelectListItem> MyList = new List<SelectListItem>();
            for (int i = 2016; i < 2020; i++ )
            {
                MyList.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            var data = MyList.Find(i => i.Value == selected);
            if (data != null)
                data.Selected = true;
            return MyList;
        }


        private List<SupplierPaymentRequest> GetSupplierPaymentRequest()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                string UserName = LoggedUserName;
                conn.Open();
                cmd.CommandText = "GetSupplierPaymentRequest '" + UserName + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SupplierPaymentRequest> MyList = new List<SupplierPaymentRequest>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    SupplierPaymentRequest supInfo = new SupplierPaymentRequest();

                    supInfo.Id = Convert.ToInt16(mydataRow["PaymentReqNo"]);
                    supInfo.RequestDate = mydataRow["ReqDate"].ToString().Trim();
                    supInfo.ProvinceID = Convert.ToInt16(mydataRow["ProvinceID"]);
                    supInfo.ProvinceName = mydataRow["ProvinceName"].ToString().Trim(); 
                    supInfo.ZoneID = mydataRow["ZoneID"].ToString().Trim();
                    supInfo.ZoneName = mydataRow["ZoneName"].ToString().Trim();
                    supInfo.Year = Convert.ToInt16(mydataRow["Year"]);
                    supInfo.Total = Convert.ToInt16(mydataRow["TotalAmount"]);
                    supInfo.Month = mydataRow["Month"].ToString().Trim();
                    supInfo.CreateBy = mydataRow["CreateUser"].ToString().Trim();
                    if(mydataRow["CreateDate"]!=DBNull.Value)
                        supInfo.CreateDate = Convert.ToDateTime(mydataRow["CreateDate"]);
                    //supInfo.ZonalApprovedBy = mydataRow["AprovedUser"].ToString().Trim();
                    //if (mydataRow["ApprovedDate"] != DBNull.Value)
                    //    supInfo.ZonalApprovedDate = Convert.ToDateTime(mydataRow["ApprovedDate"]);
                    //supInfo.ProvincialApprovedBy = mydataRow["ProvincialApp_User"].ToString().Trim(); ;
                    //if (mydataRow["ProvincialApp_Date"] != DBNull.Value)
                    //    supInfo.ProvincialApprovedDate = Convert.ToDateTime(mydataRow["ProvincialApp_Date"]);
                    supInfo.Status = mydataRow["Status"].ToString().Trim();
                    MyList.Add(supInfo);
                }

                return MyList;
            }
        }

        //public ActionResult SchoolInfo()
        //{
        //    List<SchoolInfoModel> lstSch = GetSchoolInfo();
        //    ViewBag.SchoolsInfo = lstSch;
        //    ViewBag.Provinces = GetProvinces();
        //    ViewBag.Zones = GetZones();
        //    ViewBag.Devisions = GetDevisions();

        //    return View();
        //}


        public ActionResult SchoolInfoFound(string iSchoolID)
        {
            if (CheckAccessCanBeGranted(iSchoolID))
            {
                SchoolInfoModel model = new SchoolInfoModel();
                List<SchoolInfoModel> lstSch = GetSchoolInfo();
                ViewBag.SchoolsInfo = lstSch;
                ViewBag.Provinces = GetProvinces();
                ViewBag.Zones = GetZones();
                ViewBag.Devisions = GetDevisions(model.DevisionID);
                model = FillSchoolInfoModel(model, iSchoolID);
                ViewBag.SchoolID = iSchoolID;
                return View("SchoolInfo", model);
            }
            else
            {
                List<SchoolInfoModel> lstSch = GetSchoolInfo();
                ViewBag.SchoolsInfo = lstSch;
                ViewBag.Provinces = GetProvinces();
                ViewBag.Zones = GetZones();
                ViewBag.Devisions = GetDevisions("");
                //ViewBag.MonitoringOfficers = GetMonitoringOfficers();
                return View("SchoolInfo");
            }
        }

        private bool CheckAccessCanBeGranted(string iSchoolID)
        {
            int iUserRoleID = Convert.ToInt32(Session["LoginRoleID"]);
            string UserName = Session["UserName"].ToString();

            bool CanBeGranted = false;
            switch (iUserRoleID)
            {
                case 1:
                    //HO User
                    CanBeGranted = true;
                    break;
                case 2:
                    //Provincial user
                    CanBeGranted = CheckAccessRights(iUserRoleID, "ProvinceID", iSchoolID, UserName);
                    break;
                case 3:
                    //Zonal user
                    CanBeGranted = CheckAccessRights(iUserRoleID, "ZoneID", iSchoolID, UserName);
                    break;
                case 4:
                    //Devisional user
                    CanBeGranted = CheckAccessRights(iUserRoleID, "DevisionID", iSchoolID, UserName);
                    break;
                case 5:
                    //School user
                    CanBeGranted = CheckAccessRights(iUserRoleID, "SchoolID", iSchoolID, UserName);
                    break;
            }

            return CanBeGranted;
        }

        private bool CheckAccessRights(int iUserRoleID, string AreaLevelNameID, string iSchoolID, string UserName)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT SchoolID " +
                                  "FROM m_Schools " +
                                  "WHERE SchoolID = '" + iSchoolID + "' AND " + AreaLevelNameID + " = (SELECT " + AreaLevelNameID +
                                  " FROM U_Users WHERE UserName = '" + UserName + "')";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        [HttpPost]
        public ActionResult SchoolInfoFound(SchoolInfoModel model)
        {

            if (Save_SchoolInfo(model))
            {
                ModelState.Clear();
                model = new SchoolInfoModel();

            }

            List<SchoolInfoModel> lstSch = GetSchoolInfo();
            ViewBag.SchoolsInfo = lstSch;
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions(model.DevisionID);

            return View("SchoolInfo", model);
        }

        private SanitoryFacilityModel FillSanitoryInfoModel(SanitoryFacilityModel model, string iSchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT NoOfMaleToilets, NoOfMaleUrinals, NoOfFemaleToilets, MaleCoverage, FemaleCoverage, NoOfStaffToilets, StaffCoverage " +
                                  "FROM SanitoryFacilityInfo " +
                                  "WHERE SchoolID = '" + iSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    model.NoOfMaleToilets = Convert.ToInt32(dr["NoOfMaleToilets"]);
                    model.NoOfMaleUrinals = Convert.ToInt32(dr["NoOfMaleUrinals"]);
                    model.NoOfFemaleToilets = Convert.ToInt32(dr["NoOfFemaleToilets"]);
                    model.MaleCoverage = Convert.ToInt32(dr["MaleCoverage"]);
                    model.NoOfStaffToilets = Convert.ToInt32(dr["NoOfStaffToilets"]);
                    model.StaffCoverage = Convert.ToInt32(dr["StaffCoverage"]);
                    model.FemaleCoverage = Convert.ToInt32(dr["FemaleCoverage"]);

                }
            }

            return model;
        }

        private WaterNHandwashModel FillWaterNHandWashModel(WaterNHandwashModel model, string iSchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT WaterSource, Availability, Quality, DrinkingWater, WasteWaterManagement, Note, NoOfTapsAvailable, SoapAvailable, Coverage " +
                                  "FROM WaterNHandwashInfo " +
                                  "WHERE SchoolID = '" + iSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    model.Availability = Convert.ToString(dr["Availability"]);
                    model.WaterSource = Convert.ToString(dr["WaterSource"]);
                    model.Quality = Convert.ToString(dr["Quality"]);
                    model.DrinkingWater = Convert.ToString(dr["DrinkingWater"]);
                    model.WasteWaterManagement = Convert.ToString(dr["WasteWaterManagement"]);
                    model.Note = Convert.ToString(dr["Note"]);
                    model.NoOfTapsAvailable = Convert.ToInt32(dr["NoOfTapsAvailable"]);
                    model.SoapAvailable = Convert.ToString(dr["SoapAvailable"]).Trim();
                    model.Coverage = Convert.ToInt32(dr["Coverage"]);

                }
            }

            return model;
        }


        private SchoolInfoModel FillSchoolInfoModel(SchoolInfoModel model, string iSchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  ProvinceID, ZoneID, DevisionID, m_Schools.SchoolID, SchoolName, m_Schools.CensorsID, ExsaminationNo, PrincipalName, InchargeHealthPromotion, InchargeFeedingProgram, " +
                                    " AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, NonAcademicStaffFemale, StudentsMale, StudentsFemale, " +
                                    " SchoolAddress, TelNo, Medium, Sex, ISNull(National_Provincial,0) AS National_Provincial, SchoolType, GradeSpan, District, " +
                                   " AGADivision, NearsetPoliceStation, PoliceStationContactNo, HospitalName, HospitalContactNo, City, PrincipalContactNo, NameOfPHI, PHIContactNo, HealthInchargeContactNo, FeedingFundingSource, FeedingInchargeContactNo, " +
                                   " ContactPersionMOH, ContactNoMOH, FeedingProgramme, GNDevision, Bank, BankAccountNo, NameOfOfficer, t_SHPP.TotalMarks FROM  m_Schools LEFT OUTER JOIN MonitoringOfficerInformation ON m_Schools.CensorsID = MonitoringOfficerInformation.CensorsID " +
                                   " LEFT OUTER JOIN t_SHPP ON m_Schools.SchoolID =  t_SHPP.SchoolID " +
                                   "WHERE m_Schools.SchoolID = '" + iSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    model.CensorsID = Convert.ToString(dr["CensorsID"]);
                    model.DevisionID = Convert.ToString(dr["DevisionID"]);
                    model.ProvinceID = Convert.ToInt32(dr["ProvinceID"]);
                    model.ZoneID = Convert.ToString(dr["ZoneID"]);
                    model.SchoolID = Convert.ToString(dr["SchoolID"]);
                    model.SchoolName = Convert.ToString(dr["SchoolName"]);
                    model.ExaminationNo = dr["ExsaminationNo"] == DBNull.Value ? 0 : Convert.ToInt32(dr["ExsaminationNo"]);
                    model.PrincipalName = Convert.ToString(dr["PrincipalName"]);
                    model.InchargeHealthPromotion = Convert.ToString(dr["InchargeHealthPromotion"]);
                    model.InchargeFeedingProgram = Convert.ToString(dr["InchargeFeedingProgram"]);

                    model.SchoolAddress = Convert.ToString(dr["SchoolAddress"]);
                    model.TelNo = Convert.ToString(dr["TelNo"]);
                    model.Medium = Convert.ToString(dr["Medium"]);
                    model.Sex = Convert.ToString(dr["Sex"]);
                    model.National_Provincial = Convert.ToInt32(dr["National_Provincial"]);
                    model.SchoolType = Convert.ToString(dr["SchoolType"]);
                    model.GradeSpan = Convert.ToString(dr["GradeSpan"]);
                    model.District = Convert.ToString(dr["District"]);
                    model.AGADivision = Convert.ToString(dr["AGADivision"]);
                    model.NearsetPoliceStation = Convert.ToString(dr["NearsetPoliceStation"]);
                    model.PoliceStationContactNo = Convert.ToString(dr["PoliceStationContactNo"]);
                    model.HospitalName = Convert.ToString(dr["HospitalName"]);
                    model.HospitalContactNo = Convert.ToString(dr["HospitalContactNo"]);
                    model.ContactPersionMOH = Convert.ToString(dr["ContactPersionMOH"]);
                    model.ContactNoMOH = Convert.ToString(dr["ContactNoMOH"]);
                    model.FeedingProgramme = Convert.ToString(dr["FeedingProgramme"]);
                    model.GNDevision = Convert.ToString(dr["GNDevision"]);

                    model.AcademicStaffMale = Convert.ToInt32(dr["AcademicStaffMale"]);
                    model.AcademicStaffFemale = Convert.ToInt32(dr["AcademicStaffFemale"]);
                    model.NonAcademicStaffMale = Convert.ToInt32(dr["NonAcademicStaffMale"]);
                    model.NonAcademicStaffFemale = Convert.ToInt32(dr["NonAcademicStaffFemale"]);
                    model.StudentsMale = Convert.ToInt32(dr["StudentsMale"]);
                    model.StudentsFemale = Convert.ToInt32(dr["StudentsFemale"]);
                    model.City = Convert.ToString(dr["City"]);
                    model.PrincipalContactNo = Convert.ToString(dr["PrincipalContactNo"]);

                    model.NameOfPHI = Convert.ToString(dr["NameOfPHI"]);
                    model.PHIContactNo = Convert.ToString(dr["PHIContactNo"]);
                    model.HealthInchargeContactNo = Convert.ToString(dr["HealthInchargeContactNo"]);
                    model.FeedingFundingSource = Convert.ToString(dr["FeedingFundingSource"]);
                    model.FeedingInchargeContactNo = Convert.ToString(dr["FeedingInchargeContactNo"]);
                    model.Bank = Convert.ToString(dr["Bank"]);
                    model.BankAccountNo = Convert.ToString(dr["BankAccountNo"]);
                    model.MonitoringOfficer = Convert.ToString(dr["NameOfOfficer"]);

                    model.TotalMarks = Convert.ToInt32(dr["TotalMarks"] == DBNull.Value ? 0 : dr["TotalMarks"]);

                }
            }

            return model;
        }

        public ActionResult SchoolInfo()
        {
            List<SchoolInfoModel> lstSch = GetSchoolInfo();
            ViewBag.SchoolsInfo = lstSch;
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions("");
            //ViewBag.MonitoringOfficers = GetMonitoringOfficers();
            return View();
        }

        private List<SelectListItem> GetMonitoringOfficers()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  NameOfOfficer FROM MonitoringOfficerInformation";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["NameOfOfficer"].ToString().Trim(), Value = Convert.ToString(mydataRow["NameOfOfficer"]) });
                }

                return MyList;
            }
        }

        [HttpPost]
        public ActionResult SchoolInfo(SchoolInfoModel model)
        {

            SetCustomMessagesSchoolInfo();

            if (ModelState.IsValid)
            {
                if (Save_SchoolInfo(model))
                {
                    ModelState.Clear();
                }

            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }
            List<SchoolInfoModel> lstSch = GetSchoolInfo();
            ViewBag.SchoolsInfo = lstSch;
            ViewBag.Provinces = GetProvinces();
            ViewBag.Zones = GetZones();
            ViewBag.Devisions = GetDevisions(model.DevisionID);
            model = new SchoolInfoModel();


            return View(model);

        }

        private void SetCustomMessagesSchoolInfo()
        {
            SchoolInfoModel schInfMod = new SchoolInfoModel();

            foreach (var property in schInfMod.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(Int32))
                {

                    ModelState myFieldState = ModelState[property.Name];
                    int value = 0;
                    if (myFieldState.Value.AttemptedValue != "")
                    {
                        if (!Int32.TryParse(myFieldState.Value.AttemptedValue, out value))
                        {
                            myFieldState.Errors.Clear();
                            myFieldState.Errors.Add("Should be numeric");
                        }
                    }

                }

            }



        }

        private void SetCustomMessagesGradeInfo()
        {
            GradeInfoModel schInfMod = new GradeInfoModel();

            foreach (var property in schInfMod.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(Int32))
                {

                    ModelState myFieldState = ModelState[property.Name];
                    int value = 0;
                    if (myFieldState.Value.AttemptedValue != "")
                    {
                        if (!Int32.TryParse(myFieldState.Value.AttemptedValue, out value))
                        {
                            myFieldState.Errors.Clear();
                            myFieldState.Errors.Add("Should be numeric");
                        }
                    }

                }

            }



        }

        private void SetCustomMessagesStudentInfo()
        {
            StudentModel schInfMod = new StudentModel();

            foreach (var property in schInfMod.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(Int32))
                {

                    ModelState myFieldState = ModelState[property.Name];
                    int value = 0;
                    if (myFieldState.Value.AttemptedValue != "")
                    {
                        if (!Int32.TryParse(myFieldState.Value.AttemptedValue, out value))
                        {
                            myFieldState.Errors.Clear();
                            myFieldState.Errors.Add("Should be numeric");
                        }
                    }

                }

            }



        }

        [HttpPost]
        public ActionResult GetSchoolID(string strCensorsID)
        {
            int ci;
            if (Int32.TryParse(strCensorsID, out ci))
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "SELECT SchoolID FROM  m_Schools WHERE CensorsID = '" + strCensorsID + "'";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    if (dt.Rows.Count > 0)
                        return Content(dt.Rows[0]["SchoolID"].ToString());
                    else
                        return Content("");
                }
            }
            else
                return Content("");

        }

        private bool Save_SchoolInfo(SchoolInfoModel model)
        {
            if (Validated_SchoolInfo(model))
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "UpdateSchool";
                    cmd.CommandType = CommandType.StoredProcedure;

                    //"INSERT INTO m_Schools ( ProvinceID, ZoneID, DevisionID, SchoolID, SchoolName, CensorsID, ExsaminationNo, PrincipalName, AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, " +
                    //                "NonAcademicStaffFemale, StudentsMale, StudentsFemale, InchargeHealthPromotion, InchargeFeedingProgram, SchoolAddress,TelNo,Medium,Sex,National_Provincial,SchoolType,GradeSpan,District, " +
                    //                "AGADivision,NearsetPoliceStation,PoliceStationContactNo,HospitalName, HospitalContactNo,ContactPersionMOH,ContactNoMOH,FeedingProgramme,GNDevision) " +
                    //                "VALUES(@ProvinceID, @ZoneID, @DevisionID, (SELECT ISNULL(MAX(SchoolID),0) + 1 FROM m_Schools),  @SchoolName, @CensorsID, @ExsaminationNo, @PrincipalName, @AcademicStaffMale, @AcademicStaffFemale, @NonAcademicStaffMale, " +
                    //                "@NonAcademicStaffFemale, @StudentsMale, @StudentsFemale, @InchargeHealthPromotion, @InchargeFeedingProgram, @SchoolAddress,@TelNo,@Medium,@Sex,@National_Provincial,@SchoolType,@GradeSpan,@District, " +
                    //                "@AGADivision,@NearsetPoliceStation,@PoliceStationContactNo,@HospitalName, @HospitalContactNo,@ContactPersionMOH,@ContactNoMOH,@FeedingProgramme,@GNDevision)";


                    cmd.Parameters.AddWithValue("@ProvinceID", model.ProvinceID);
                    cmd.Parameters.AddWithValue("@ZoneID", model.ZoneID);
                    cmd.Parameters.AddWithValue("@DevisionID", model.DevisionID);
                    cmd.Parameters.AddWithValue("@SchoolID", (model.SchoolID == null ? "" : model.SchoolID));
                    cmd.Parameters.AddWithValue("@SchoolName", model.SchoolName);
                    cmd.Parameters.AddWithValue("@CensorsID", model.CensorsID);
                    cmd.Parameters.AddWithValue("@ExsaminationNo", model.ExaminationNo);
                    cmd.Parameters.AddWithValue("@PrincipalName", model.PrincipalName);
                    cmd.Parameters.AddWithValue("@AcademicStaffMale", model.AcademicStaffMale);
                    cmd.Parameters.AddWithValue("@AcademicStaffFemale", model.AcademicStaffFemale);
                    cmd.Parameters.AddWithValue("@NonAcademicStaffMale", model.NonAcademicStaffMale);
                    cmd.Parameters.AddWithValue("@NonAcademicStaffFemale", model.NonAcademicStaffFemale);
                    cmd.Parameters.AddWithValue("@StudentsMale", model.StudentsMale);
                    cmd.Parameters.AddWithValue("@StudentsFemale", model.StudentsFemale);
                    cmd.Parameters.AddWithValue("@InchargeHealthPromotion", model.InchargeHealthPromotion);
                    cmd.Parameters.AddWithValue("@InchargeFeedingProgram", model.InchargeFeedingProgram);


                    cmd.Parameters.AddWithValue("@SchoolAddress", model.SchoolAddress);
                    cmd.Parameters.AddWithValue("@TelNo", model.TelNo);
                    cmd.Parameters.AddWithValue("@Medium", model.Medium);
                    cmd.Parameters.AddWithValue("@Sex", model.Sex);
                    cmd.Parameters.AddWithValue("@National_Provincial", model.National_Provincial);
                    cmd.Parameters.AddWithValue("@SchoolType", model.SchoolType);
                    cmd.Parameters.AddWithValue("@GradeSpan", model.GradeSpan);
                    cmd.Parameters.AddWithValue("@District", model.District);

                    cmd.Parameters.AddWithValue("@AGADivision", model.AGADivision);
                    cmd.Parameters.AddWithValue("@NearsetPoliceStation", model.NearsetPoliceStation);
                    cmd.Parameters.AddWithValue("@PoliceStationContactNo", model.PoliceStationContactNo);
                    cmd.Parameters.AddWithValue("@HospitalName", model.HospitalName);
                    cmd.Parameters.AddWithValue("@HospitalContactNo", model.HospitalContactNo);
                    cmd.Parameters.AddWithValue("@ContactPersionMOH", model.ContactPersionMOH);
                    cmd.Parameters.AddWithValue("@ContactNoMOH", model.ContactNoMOH);
                    cmd.Parameters.AddWithValue("@FeedingProgramme", model.FeedingProgramme);
                    cmd.Parameters.AddWithValue("@GNDevision", model.GNDevision);

                    cmd.Parameters.AddWithValue("@HealthInchargeContactNo", model.HealthInchargeContactNo);
                    cmd.Parameters.AddWithValue("@FeedingFundingSource", model.FeedingFundingSource);
                    cmd.Parameters.AddWithValue("@FeedingInchargeContactNo", model.FeedingInchargeContactNo);
                    cmd.Parameters.AddWithValue("@NameOfPHI", model.NameOfPHI);
                    cmd.Parameters.AddWithValue("@PHIContactNo", model.PHIContactNo);
                    cmd.Parameters.AddWithValue("@PrincipalContactNo", model.PrincipalContactNo);
                    cmd.Parameters.AddWithValue("@CreateUser", Session["UserName"]);
                    cmd.Parameters.AddWithValue("@City", model.City);

                    cmd.Parameters.AddWithValue("@Bank", model.Bank);
                    cmd.Parameters.AddWithValue("@BankAccountNo", model.BankAccountNo);
                    //cmd.Parameters.AddWithValue("@MonitoringOfficer", model.MonitoringOfficer);



                    cmd.ExecuteNonQuery();
                }



                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Validated_SchoolInfo(SchoolInfoModel model)
        {
            if (model.ProvinceID == null || model.ZoneID == null
                || model.DevisionID == null || model.AcademicStaffFemale == null || model.AcademicStaffMale == null
                || model.CensorsID == null || model.ExaminationNo == null || model.InchargeFeedingProgram == null
                || model.InchargeHealthPromotion == null || model.NonAcademicStaffMale == null || model.NonAcademicStaffFemale == null
                || model.PrincipalName == null || model.SchoolName == null || model.StudentsFemale == null
                || model.StudentsMale == null || model.FeedingProgramme == null || model.FeedingFundingSource == null || model.FeedingInchargeContactNo == null
                || model.GNDevision == null || model.HealthInchargeContactNo == null || model.HospitalContactNo == null || model.HospitalName == null
                || model.NameOfPHI == null || model.NearsetPoliceStation == null || model.PHIContactNo == null || model.PoliceStationContactNo == null
                || model.PrincipalContactNo == null || model.SchoolAddress == null || model.SchoolType == null || model.Sex == null
                || model.TelNo == null)
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }

            if (model.InchargeFeedingProgram.Trim() == "" || model.PrincipalName.Trim() == "" || model.SchoolName.Trim() == "")
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }
            return true;
        }

        public ActionResult Index()
        {

            return View();
        }

        public ActionResult WaterAndHandwashInfo(string iSchoolID)
        {
            if (CheckAccessCanBeGranted(iSchoolID))
            {
                WaterNHandwashModel WatNHWInf = new WaterNHandwashModel();

                WatNHWInf.SchoolID = iSchoolID;
                //List<SelectListItem> lstSchls = GetSchools();
                //ViewBag.Schools = lstSchls;
                ViewBag.WaterSources = GetWaterSources();
                ViewBag.Availabilities = GetAvailabilities();
                ViewBag.Qualities = GetQualities();
                ViewBag.DrinkingWaterBrands = GetDrinkingWaterBrands();
                ViewBag.WasteWaterManagementMethods = GetWasteWaterManagementMethods();

                WatNHWInf = FillWaterNHandWashModel(WatNHWInf, iSchoolID);

                return View(WatNHWInf);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        [HttpPost]
        public ActionResult WaterAndHandwashInfo(WaterNHandwashModel model)
        {
            if (ModelState.IsValid)
            {

                string SchID = model.SchoolID;
                if (Save_WaterAndHandwashInfo(model))
                {
                    //ModelState.Clear();
                    //model = new WaterNHandwashModel();
                    ViewBag.WaterSources = GetWaterSources();
                    ViewBag.Availabilities = GetAvailabilities();
                    ViewBag.Qualities = GetQualities();
                    ViewBag.DrinkingWaterBrands = GetDrinkingWaterBrands();
                    ViewBag.WasteWaterManagementMethods = GetWasteWaterManagementMethods();
                    //model.SchoolID = SchID;


                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            return View(model);
        }

        private bool Save_WaterAndHandwashInfo(WaterNHandwashModel model)
        {
            try
            {
                string strUser = Convert.ToString(Session["UserName"]);
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "UpdateWaterAndHandwashInfo";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@WaterSource", model.WaterSource);
                    cmd.Parameters.AddWithValue("@Availability", model.Availability);
                    cmd.Parameters.AddWithValue("@Quality", model.Quality);
                    cmd.Parameters.AddWithValue("@DrinkingWater", model.DrinkingWater);
                    cmd.Parameters.AddWithValue("@WasteWaterManagement", model.WasteWaterManagement);
                    cmd.Parameters.AddWithValue("@Note", model.Note);
                    cmd.Parameters.AddWithValue("@NoOfTapsAvailable", model.NoOfTapsAvailable);
                    cmd.Parameters.AddWithValue("@SoapAvailable", model.SoapAvailable);
                    cmd.Parameters.AddWithValue("@Coverage", model.Coverage);
                    cmd.Parameters.AddWithValue("@User", strUser);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception Err)
            {
                return false;
            }
        }

        public ActionResult SanitoryFacilityInfo(string iSchoolID)
        {
            if (CheckAccessCanBeGranted(iSchoolID))
            {
                SanitoryFacilityModel SanitoryInf = new SanitoryFacilityModel();

                SanitoryInf.SchoolID = iSchoolID;

                SanitoryInf = FillSanitoryInfoModel(SanitoryInf, iSchoolID);

                return View(SanitoryInf);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        [HttpPost]
        public ActionResult SanitoryFacilityInfo(SanitoryFacilityModel model)
        {
            try
            {


                if (ModelState.IsValid)
                {

                    string SchID = model.SchoolID;
                    if (Save_SanitoryFacility(model))
                    {
                        //ModelState.Clear();
                        //model = new SanitoryFacilityModel();
                        //ViewBag.Schools = GetSchools();
                        //ViewBag.Students = GetStudentsInfo();
                        //model = new SanitoryFacilityModel();
                        //model.SchoolID = SchID;
                        //model = FillSanitoryInfoModel(model, SchID);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
                }

                return View(model);
            }
            catch (Exception Err)
            {
                @ViewBag.ErrorMessage = Err.Message;
                return View("../Login/Error");
            }
        }

        [HttpPost]
        public int GetSanitoryInfoCoveragePercentage(string SchoolID, int NoOfToilets)
        {
            int iPerCoverage = 0;

            DataTable dtCoverage = Bulid_Table_4_Coverage();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  StudentsMale + StudentsFemale AS StudentsCount,  AcademicStaffMale + AcademicStaffFemale AS TeachersCount FROM  m_Schools WHERE SchoolID = " + SchoolID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    int StudentsCount = Convert.ToInt32(dt.Rows[0]["StudentsCount"]);

                    //get the right slot of students
                    //int iNoOfFemTol = Convert.ToInt32(dtCoverage.Select(StudentsCount.ToString() + ">= NoOfStudentsFrom " + " AND " + StudentsCount.ToString() + "<= NoOfStudentsTo")[0]["NoOfFemaleToilets"]);
                    int iNoOfMalTol = Convert.ToInt32(dtCoverage.Select(StudentsCount.ToString() + ">= NoOfStudentsFrom " + " AND " + StudentsCount.ToString() + "<= NoOfStudentsTo")[0]["NoOfMaleToilets"]);
                    int iNoOfUri = Convert.ToInt32(dtCoverage.Select(StudentsCount.ToString() + ">= NoOfStudentsFrom " + " AND " + StudentsCount.ToString() + "<= NoOfStudentsTo")[0]["NoOfUrinals"]);

                    iPerCoverage = ((NoOfToilets * 100) / (iNoOfMalTol + iNoOfUri));

                }


            }



            return iPerCoverage;
        }

        [HttpPost]
        public int GetSanitoryInfoCoveragePercentageFemale(string SchoolID, int NoOfToilets)
        {
            int iPerCoverage = 0;

            DataTable dtCoverage = Bulid_Table_4_Coverage();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  StudentsMale + StudentsFemale AS StudentsCount FROM  m_Schools WHERE SchoolID = " + SchoolID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    int StudentsCount = Convert.ToInt32(dt.Rows[0]["StudentsCount"]);

                    //get the right slot of students
                    int iNoOfFemTol = Convert.ToInt32(dtCoverage.Select(StudentsCount.ToString() + ">= NoOfStudentsFrom " + " AND " + StudentsCount.ToString() + "<= NoOfStudentsTo")[0]["NoOfFemaleToilets"]);

                    iPerCoverage = ((NoOfToilets * 100) / (iNoOfFemTol));

                }


            }



            return iPerCoverage;
        }

        [HttpPost]
        public int GetSanitoryInfoCoveragePercentageStaff(string SchoolID, int NoOfToilets)
        {
            int iPerCoverage = 0;

            DataTable dtCoverage = Bulid_Table_4_Coverage_Staff();

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  AcademicStaffMale + AcademicStaffFemale AS TeachersCount FROM  m_Schools WHERE SchoolID = " + SchoolID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    int TeachersCount = Convert.ToInt32(dt.Rows[0]["TeachersCount"]);

                    //get the right slot of students
                    int iNoOfStaffTol = Convert.ToInt32(dtCoverage.Select(TeachersCount.ToString() + ">= NoOfTeachersFrom " + " AND " + TeachersCount.ToString() + "<= NoOfTeachersTo")[0]["NoOfTeachersToilets"]);

                    iPerCoverage = ((NoOfToilets * 100) / (iNoOfStaffTol));

                }

            }

            return iPerCoverage;
        }

        private DataTable Bulid_Table_4_Coverage_Staff()
        {
            DataTable dtRet = new DataTable();
            dtRet.Columns.Add("NoOfTeachersFrom", typeof(System.Int32));
            dtRet.Columns.Add("NoOfTeachersTo", typeof(System.Int32));
            dtRet.Columns.Add("NoOfTeachersToilets", typeof(System.Int32));

            DataRow drNew1 = dtRet.NewRow();
            drNew1["NoOfTeachersFrom"] = 0;
            drNew1["NoOfTeachersTo"] = 10;
            drNew1["NoOfTeachersToilets"] = 2;
            dtRet.Rows.Add(drNew1);

            DataRow drNew2 = dtRet.NewRow();
            drNew2["NoOfTeachersFrom"] = 11;
            drNew2["NoOfTeachersTo"] = 40;
            drNew2["NoOfTeachersToilets"] = 4;
            dtRet.Rows.Add(drNew2);

            DataRow drNew3 = dtRet.NewRow();
            drNew3["NoOfTeachersFrom"] = 41;
            drNew3["NoOfTeachersTo"] = 50000;
            drNew3["NoOfTeachersToilets"] = 5;
            dtRet.Rows.Add(drNew3);


            return dtRet;

        }

        private DataTable Bulid_Table_4_Coverage()
        {
            DataTable dtRet = new DataTable();
            dtRet.Columns.Add("NoOfStudentsFrom", typeof(System.Int32));
            dtRet.Columns.Add("NoOfStudentsTo", typeof(System.Int32));
            dtRet.Columns.Add("NoOfFemaleToilets", typeof(System.Int32));
            dtRet.Columns.Add("NoOfMaleToilets", typeof(System.Int32));
            dtRet.Columns.Add("NoOfUrinals", typeof(System.Int32));

            DataRow drNew1 = dtRet.NewRow();
            drNew1["NoOfStudentsFrom"] = 1;
            drNew1["NoOfStudentsTo"] = 100;
            drNew1["NoOfFemaleToilets"] = 2;
            drNew1["NoOfMaleToilets"] = 1;
            drNew1["NoOfUrinals"] = 2;
            dtRet.Rows.Add(drNew1);

            DataRow drNew2 = dtRet.NewRow();
            drNew2["NoOfStudentsFrom"] = 101;
            drNew2["NoOfStudentsTo"] = 200;
            drNew2["NoOfFemaleToilets"] = 3;
            drNew2["NoOfMaleToilets"] = 1;
            drNew2["NoOfUrinals"] = 2;
            dtRet.Rows.Add(drNew2);

            DataRow drNew3 = dtRet.NewRow();
            drNew3["NoOfStudentsFrom"] = 201;
            drNew3["NoOfStudentsTo"] = 300;
            drNew3["NoOfFemaleToilets"] = 5;
            drNew3["NoOfMaleToilets"] = 2;
            drNew3["NoOfUrinals"] = 3;
            dtRet.Rows.Add(drNew3);

            DataRow drNew4 = dtRet.NewRow();
            drNew4["NoOfStudentsFrom"] = 301;
            drNew4["NoOfStudentsTo"] = 400;
            drNew4["NoOfFemaleToilets"] = 6;
            drNew4["NoOfMaleToilets"] = 2;
            drNew4["NoOfUrinals"] = 4;
            dtRet.Rows.Add(drNew4);

            DataRow drNew5 = dtRet.NewRow();
            drNew5["NoOfStudentsFrom"] = 401;
            drNew5["NoOfStudentsTo"] = 500;
            drNew5["NoOfFemaleToilets"] = 8;
            drNew5["NoOfMaleToilets"] = 3;
            drNew5["NoOfUrinals"] = 5;
            dtRet.Rows.Add(drNew5);

            DataRow drNew6 = dtRet.NewRow();
            drNew6["NoOfStudentsFrom"] = 501;
            drNew6["NoOfStudentsTo"] = 600;
            drNew6["NoOfFemaleToilets"] = 9;
            drNew6["NoOfMaleToilets"] = 3;
            drNew6["NoOfUrinals"] = 6;
            dtRet.Rows.Add(drNew6);

            DataRow drNew7 = dtRet.NewRow();
            drNew7["NoOfStudentsFrom"] = 601;
            drNew7["NoOfStudentsTo"] = 700;
            drNew7["NoOfFemaleToilets"] = 11;
            drNew7["NoOfMaleToilets"] = 4;
            drNew7["NoOfUrinals"] = 7;
            dtRet.Rows.Add(drNew7);

            DataRow drNew8 = dtRet.NewRow();
            drNew8["NoOfStudentsFrom"] = 701;
            drNew8["NoOfStudentsTo"] = 800;
            drNew8["NoOfFemaleToilets"] = 12;
            drNew8["NoOfMaleToilets"] = 4;
            drNew8["NoOfUrinals"] = 8;
            dtRet.Rows.Add(drNew8);

            DataRow drNew9 = dtRet.NewRow();
            drNew9["NoOfStudentsFrom"] = 801;
            drNew9["NoOfStudentsTo"] = 900;
            drNew9["NoOfFemaleToilets"] = 14;
            drNew9["NoOfMaleToilets"] = 5;
            drNew9["NoOfUrinals"] = 9;
            dtRet.Rows.Add(drNew9);

            DataRow drNew10 = dtRet.NewRow();
            drNew10["NoOfStudentsFrom"] = 901;
            drNew10["NoOfStudentsTo"] = 1000;
            drNew10["NoOfFemaleToilets"] = 15;
            drNew10["NoOfMaleToilets"] = 5;
            drNew10["NoOfUrinals"] = 10;
            dtRet.Rows.Add(drNew10);

            DataRow drNew11 = dtRet.NewRow();
            drNew11["NoOfStudentsFrom"] = 1001;
            drNew11["NoOfStudentsTo"] = 1200;
            drNew11["NoOfFemaleToilets"] = 16;
            drNew11["NoOfMaleToilets"] = 5;
            drNew11["NoOfUrinals"] = 11;
            dtRet.Rows.Add(drNew11);

            DataRow drNew12 = dtRet.NewRow();
            drNew12["NoOfStudentsFrom"] = 1201;
            drNew12["NoOfStudentsTo"] = 1400;
            drNew12["NoOfFemaleToilets"] = 17;
            drNew12["NoOfMaleToilets"] = 6;
            drNew12["NoOfUrinals"] = 11;
            dtRet.Rows.Add(drNew12);

            DataRow drNew13 = dtRet.NewRow();
            drNew13["NoOfStudentsFrom"] = 1401;
            drNew13["NoOfStudentsTo"] = 1600;
            drNew13["NoOfFemaleToilets"] = 18;
            drNew13["NoOfMaleToilets"] = 6;
            drNew13["NoOfUrinals"] = 12;
            dtRet.Rows.Add(drNew13);

            DataRow drNew14 = dtRet.NewRow();
            drNew14["NoOfStudentsFrom"] = 1601;
            drNew14["NoOfStudentsTo"] = 1800;
            drNew14["NoOfFemaleToilets"] = 19;
            drNew14["NoOfMaleToilets"] = 6;
            drNew14["NoOfUrinals"] = 13;
            dtRet.Rows.Add(drNew14);

            return dtRet;
        }


        //private DataTable Bulid_Table_4_Coverage_Female()
        //{
        //    DataTable dtRet = new DataTable();
        //    dtRet.Columns.Add("NoOfStudentsFrom", typeof(System.Int32));
        //    dtRet.Columns.Add("NoOfStudentsTo", typeof(System.Int32));
        //    dtRet.Columns.Add("NoOfFemaleToilets", typeof(System.Int32));
        //    dtRet.Columns.Add("NoOfMaleToilets", typeof(System.Int32));
        //    dtRet.Columns.Add("NoOfUrinals", typeof(System.Int32));

        //    DataRow drNew1 = dtRet.NewRow();
        //    drNew1["NoOfStudentsFrom"] = 1;
        //    drNew1["NoOfStudentsTo"] = 100;
        //    drNew1["NoOfFemaleToilets"] = 2;
        //    drNew1["NoOfMaleToilets"] = 1;
        //    drNew1["NoOfUrinals"] = 2;
        //    dtRet.Rows.Add(drNew1);

        //    DataRow drNew2 = dtRet.NewRow();
        //    drNew2["NoOfStudentsFrom"] = 101;
        //    drNew2["NoOfStudentsTo"] = 200;
        //    drNew2["NoOfFemaleToilets"] = 3;
        //    drNew2["NoOfMaleToilets"] = 1;
        //    drNew2["NoOfUrinals"] = 2;
        //    dtRet.Rows.Add(drNew2);

        //    DataRow drNew3 = dtRet.NewRow();
        //    drNew3["NoOfStudentsFrom"] = 201;
        //    drNew3["NoOfStudentsTo"] = 300;
        //    drNew3["NoOfFemaleToilets"] = 5;
        //    drNew3["NoOfMaleToilets"] = 2;
        //    drNew3["NoOfUrinals"] = 3;
        //    dtRet.Rows.Add(drNew3);

        //    DataRow drNew4 = dtRet.NewRow();
        //    drNew4["NoOfStudentsFrom"] = 301;
        //    drNew4["NoOfStudentsTo"] = 400;
        //    drNew4["NoOfFemaleToilets"] = 6;
        //    drNew4["NoOfMaleToilets"] = 2;
        //    drNew4["NoOfUrinals"] = 4;
        //    dtRet.Rows.Add(drNew4);

        //    DataRow drNew5 = dtRet.NewRow();
        //    drNew5["NoOfStudentsFrom"] = 401;
        //    drNew5["NoOfStudentsTo"] = 500;
        //    drNew5["NoOfFemaleToilets"] = 8;
        //    drNew5["NoOfMaleToilets"] = 3;
        //    drNew5["NoOfUrinals"] = 5;
        //    dtRet.Rows.Add(drNew5);

        //    DataRow drNew6 = dtRet.NewRow();
        //    drNew6["NoOfStudentsFrom"] = 501;
        //    drNew6["NoOfStudentsTo"] = 600;
        //    drNew6["NoOfFemaleToilets"] = 9;
        //    drNew6["NoOfMaleToilets"] = 3;
        //    drNew6["NoOfUrinals"] = 6;
        //    dtRet.Rows.Add(drNew6);

        //    DataRow drNew7 = dtRet.NewRow();
        //    drNew7["NoOfStudentsFrom"] = 601;
        //    drNew7["NoOfStudentsTo"] = 700;
        //    drNew7["NoOfFemaleToilets"] = 11;
        //    drNew7["NoOfMaleToilets"] = 4;
        //    drNew7["NoOfUrinals"] = 7;
        //    dtRet.Rows.Add(drNew7);

        //    DataRow drNew8 = dtRet.NewRow();
        //    drNew8["NoOfStudentsFrom"] = 701;
        //    drNew8["NoOfStudentsTo"] = 800;
        //    drNew8["NoOfFemaleToilets"] = 12;
        //    drNew8["NoOfMaleToilets"] = 4;
        //    drNew8["NoOfUrinals"] = 8;
        //    dtRet.Rows.Add(drNew8);

        //    DataRow drNew9 = dtRet.NewRow();
        //    drNew9["NoOfStudentsFrom"] = 801;
        //    drNew9["NoOfStudentsTo"] = 900;
        //    drNew9["NoOfFemaleToilets"] = 14;
        //    drNew9["NoOfMaleToilets"] = 5;
        //    drNew9["NoOfUrinals"] = 9;
        //    dtRet.Rows.Add(drNew9);

        //    DataRow drNew10 = dtRet.NewRow();
        //    drNew10["NoOfStudentsFrom"] = 901;
        //    drNew10["NoOfStudentsTo"] = 1000;
        //    drNew10["NoOfFemaleToilets"] = 15;
        //    drNew10["NoOfMaleToilets"] = 5;
        //    drNew10["NoOfUrinals"] = 10;
        //    dtRet.Rows.Add(drNew10);

        //    DataRow drNew11 = dtRet.NewRow();
        //    drNew11["NoOfStudentsFrom"] = 1001;
        //    drNew11["NoOfStudentsTo"] = 1200;
        //    drNew11["NoOfFemaleToilets"] = 16;
        //    drNew11["NoOfMaleToilets"] = 5;
        //    drNew11["NoOfUrinals"] = 11;
        //    dtRet.Rows.Add(drNew11);

        //    DataRow drNew12 = dtRet.NewRow();
        //    drNew12["NoOfStudentsFrom"] = 1201;
        //    drNew12["NoOfStudentsTo"] = 1400;
        //    drNew12["NoOfFemaleToilets"] = 17;
        //    drNew12["NoOfMaleToilets"] = 6;
        //    drNew12["NoOfUrinals"] = 11;
        //    dtRet.Rows.Add(drNew12);

        //    DataRow drNew13 = dtRet.NewRow();
        //    drNew13["NoOfStudentsFrom"] = 1401;
        //    drNew13["NoOfStudentsTo"] = 1600;
        //    drNew13["NoOfFemaleToilets"] = 18;
        //    drNew13["NoOfMaleToilets"] = 6;
        //    drNew13["NoOfUrinals"] = 12;
        //    dtRet.Rows.Add(drNew13);

        //    DataRow drNew14 = dtRet.NewRow();
        //    drNew14["NoOfStudentsFrom"] = 1601;
        //    drNew14["NoOfStudentsTo"] = 1800;
        //    drNew14["NoOfFemaleToilets"] = 19;
        //    drNew14["NoOfMaleToilets"] = 6;
        //    drNew14["NoOfUrinals"] = 13;
        //    dtRet.Rows.Add(drNew14);

        //    return dtRet;
        //}



        private bool Save_SanitoryFacility(SanitoryFacilityModel model)
        {
            try
            {
                string strUser = Convert.ToString(Session["UserName"]);
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "UpdateSanitoryFacility";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@NoOfMaleToilets", model.NoOfMaleToilets);
                    cmd.Parameters.AddWithValue("@NoOfMaleUrinals", model.NoOfMaleUrinals);
                    cmd.Parameters.AddWithValue("@NoOfFemaleToilets", model.NoOfFemaleToilets);
                    cmd.Parameters.AddWithValue("@MaleCoverage", model.MaleCoverage);
                    cmd.Parameters.AddWithValue("@FemaleCoverage", model.FemaleCoverage);
                    cmd.Parameters.AddWithValue("@NoOfStaffToilets", model.NoOfStaffToilets);
                    cmd.Parameters.AddWithValue("@StaffCoverage", model.StaffCoverage);
                    cmd.Parameters.AddWithValue("@User", strUser);
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception Err)
            {
                throw Err;
            }

        }

        private List<SelectListItem> GetWasteWaterManagementMethods()
        {
            // Here it'll access config and will get the relative values 
            string[] lstWaterSources = ConfigurationManager.AppSettings["WasteWaterManagementMethods"].Split('|');

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string itemWaterSource in lstWaterSources)
            {
                MyList.Add(new SelectListItem { Text = itemWaterSource, Value = itemWaterSource });
            }

            return MyList;
        }

        private List<SelectListItem> GetDrinkingWaterBrands()
        {
            // Here it'll access config and will get the relative values 
            string[] lstWaterSources = ConfigurationManager.AppSettings["DrinkingWaterBrands"].Split('|');

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string itemWaterSource in lstWaterSources)
            {
                MyList.Add(new SelectListItem { Text = itemWaterSource, Value = itemWaterSource });
            }

            return MyList;
        }

        private List<SelectListItem> GetQualities()
        {
            // Here it'll access config and will get the relative values 
            string[] lstWaterSources = ConfigurationManager.AppSettings["Qualities"].Split('|');

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string itemWaterSource in lstWaterSources)
            {
                MyList.Add(new SelectListItem { Text = itemWaterSource, Value = itemWaterSource });
            }

            return MyList;
        }

        private List<SelectListItem> GetAvailabilities()
        {
            // Here it'll access config and will get the relative values 
            string[] lstWaterSources = ConfigurationManager.AppSettings["Availabilities"].Split('|');

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string itemWaterSource in lstWaterSources)
            {
                MyList.Add(new SelectListItem { Text = itemWaterSource, Value = itemWaterSource });
            }

            return MyList;
        }

        //public ActionResult AddStudentYear(string iSchoolID, int Year)
        //{
        //    if (CheckAccessCanBeGranted(iSchoolID))
        //    {
        //        StudentModel StdInf = new StudentModel();

        //        StdInf.SchoolID = iSchoolID;
        //        List<SelectListItem> lstSchls = GetSchools();
        //        ViewBag.Schools = lstSchls;
        //        ViewBag.Students = GetStudentsInfo(iSchoolID, Year);
        //        ViewBag.SchoolGrades = GetGradesInfoList(iSchoolID);
        //        ViewBag.CensorsID = GetCensorsID(iSchoolID);
        //        ViewBag.School = lstSchls.Where(s => s.Value == iSchoolID.ToString()).First().Text;
        //        ViewBag.Years = LoadYears();
        //        return View("AddStudent",StdInf);

        //    }
        //    else
        //    {
        //        @ViewBag.ErrorMessage = "User not granted";
        //        return View("../Login/Error");
        //    }
        //}

        public ActionResult Yearend(string iSchoolID)
        {
            if (CheckAccessCanBeGranted(iSchoolID))
            {
                StudentModel StdInf = new StudentModel();

                StdInf.SchoolID = iSchoolID;
                List<SelectListItem> lstSchls = GetSchools();
                ViewBag.Schools = lstSchls;
                ViewBag.Students = GetStudentsInfo(iSchoolID);
                ViewBag.SchoolGrades = GetGradesInfoList(iSchoolID);
                ViewBag.CensorsID = GetCensorsID(iSchoolID);
                ViewBag.School = lstSchls.Where(s => s.Value == iSchoolID.ToString()).First().Text;
                ViewBag.Years = LoadYears(0);
                return View(StdInf);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        public ActionResult AddStudent(string iSchoolID, int? Year)
        {
            if (!Year.HasValue)
            {
                Year = DateTime.Now.Year;
            }

            if (CheckAccessCanBeGranted(iSchoolID))
            {
                StudentModel StdInf = new StudentModel();

                StdInf.SchoolID = iSchoolID;
                // List<SelectListItem> lstSchls = GetSchools();
                //ViewBag.Schools = lstSchls;
                ViewBag.Students = GetStudentsInfo(iSchoolID, Year.Value);
                ViewBag.SchoolGrades = GetGradesInfoList(iSchoolID);
                ViewBag.CensorsID = GetCensorsID(iSchoolID);
                ViewBag.School = GetSchoolNameByID(iSchoolID);
                ViewBag.Years = LoadYears(Year.Value);
                ViewBag.Gender = LoadGender(null);
                return View(StdInf);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }
        }

        private List<SelectListItem> LoadGender(string selected)
        {
            var list = new List<SelectListItem>{
                new SelectListItem{Text="MALE", Value="MALE"},
                new SelectListItem{Text="FEMALE", Value="FEMALE"}
                };
            var g = list.FirstOrDefault(x => x.Value == selected);
            if (g != null)
            {
                g.Selected = true;
            }

            return list;
        }

        private List<SelectListItem> LoadYears(int SelectedYear)
        {
            int EndYear = DateTime.Now.Year;  //Add any number 
            List<SelectListItem> MyList = new List<SelectListItem>();
            for (int i = 2016; i <= EndYear; i++)
            {
                if (i == SelectedYear)
                {
                    MyList.Add(new SelectListItem { Text = i.ToString().Trim(), Value = i.ToString(), Selected = true });
                }
                else
                {
                    MyList.Add(new SelectListItem { Text = i.ToString().Trim(), Value = i.ToString() });
                }
            }

            return MyList;
        }

        [HttpPost]
        public ActionResult DeleteStudent(string AdmNo, string SchID, string reason) //StudentModel model
        {
            StudentModel model = new StudentModel();
            model.AddmisionNo = AdmNo;
            model.SchoolID = SchID;
            if (Delete_Student(model, reason))
            {
                ModelState.Clear();
                model = new StudentModel();
                model.SchoolID = SchID;
            }

            return View(model);
        }


        [HttpPost]
        public ActionResult AddStudent(StudentModel model, string button, int? Year)
        {
            if (!Year.HasValue)
            {
                Year = DateTime.Now.Year;
            }
            
            string SchID = model.SchoolID;

            switch (button)
            {

                case "Save":

                SetCustomMessagesStudentInfo();

                if (ModelState.IsValid)
                {

                    
                    if (Save_Student(model))
                    {
                        ModelState.Clear();
                        //ViewBag.Schools = GetSchools();
                        //ViewBag.Students = GetStudentsInfo();
                        model = new StudentModel();
                        model.SchoolID = SchID;
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
                }

                ViewBag.CensorsID = GetCensorsID(model.SchoolID);
                //List<SelectListItem> lstSchls = GetSchools();
                //ViewBag.Schools = lstSchls;
                ViewBag.Students = GetStudentsInfo(model.SchoolID);
                ViewBag.SchoolGrades = GetGradesInfoList(model.SchoolID);
                    ViewBag.School = GetSchoolNameByID(model.SchoolID); ;
                    ViewBag.Years = LoadYears(Year.Value);
                    ViewBag.Gender = LoadGender(model.Gender);
                    break;

                case "Delete Student":
                    //if (Delete_Student(model))
                    //{
                    //    ModelState.Clear();
                    //    //ViewBag.Schools = GetSchools();
                    //    //ViewBag.Students = GetStudentsInfo();
                    //    model = new StudentModel();
                    //    model.SchoolID = SchID;
                    //}
                break;
            }

            return View(model);
        }

        private string GetSchoolNameByID(string id)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT SchoolName FROM  m_Schools WHERE SchoolID = " + id;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt.Rows[0]["SchoolName"].ToString();

            }
        }

        //[HttpPost]
        //public ActionResult AddStudent(StudentModel model, string button)
        //{
        //    string SchID = model.SchoolID;

        //    switch (button)
        //    {

        //        case "Save":

        //            SetCustomMessagesStudentInfo();

        //            if (ModelState.IsValid)
        //            {


        //                if (Save_Student(model))
        //                {
        //                    ModelState.Clear();
        //                    //ViewBag.Schools = GetSchools();
        //                    //ViewBag.Students = GetStudentsInfo();
        //                    model = new StudentModel();
        //                    model.SchoolID = SchID;
        //                }
        //            }
        //            else
        //            {
        //                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
        //            }

        //            ViewBag.CensorsID = GetCensorsID(model.SchoolID);
        //            List<SelectListItem> lstSchls = GetSchools();
        //            ViewBag.Schools = lstSchls;
        //            ViewBag.Students = GetStudentsInfo(model.SchoolID);
        //            ViewBag.SchoolGrades = GetGradesInfoList(model.SchoolID);
        //            ViewBag.School = lstSchls.Where(s => s.Value == model.SchoolID.ToString()).First().Text;
        //            break;

        //        case "Delete Student":
        //            //if (Delete_Student(model))
        //            //{
        //            //    ModelState.Clear();
        //            //    //ViewBag.Schools = GetSchools();
        //            //    //ViewBag.Students = GetStudentsInfo();
        //            //    model = new StudentModel();
        //            //    model.SchoolID = SchID;
        //            //}
        //            break;
        //    }

        //    return View(model);
        //}

        private bool Validate_Delete_StudentInfo(StudentModel model)
        {
            if (model.SchoolID == null || model.AddmisionNo == null || model.SchoolID == "" || model.AddmisionNo == "")
            {
                ModelState.AddModelError("", "Please select the student to delete.");
                return false;
            }

            return true;
        }

        private bool Delete_Student(StudentModel model, string strReason)
        {
            if (Validate_Delete_StudentInfo(model))
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "Delete_StudentInfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@AddmisionNo", model.AddmisionNo);
                    cmd.Parameters.AddWithValue("@Reason", strReason);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        [HttpPost]
        public ActionResult GetSchoolSex(int SchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT Sex FROM  m_Schools WHERE SchoolID = " + SchoolID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return Content(dt.Rows[0]["Sex"].ToString());
                else
                    return Content("");
            }
        }

        public string GetCensorsID(string SchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT    CensorsID FROM  m_Schools WHERE SchoolID = '" + SchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return dt.Rows[0]["CensorsID"].ToString();
                else
                    return "";
            }
        }

        [HttpPost]
        public ActionResult GetCensorsIDBySchoolID(int SchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT    CensorsID FROM  m_Schools WHERE SchoolID = " + SchoolID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                    return Content(dt.Rows[0]["CensorsID"].ToString());
                else
                    return Content("");
            }
        }

        [HttpPost]
        public ActionResult GetSchoolsByDevison(int ProvinceID, string ZoneID, string DevisionID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, SchoolName FROM m_Schools WHERE ProvinceID = " + ProvinceID + " AND ZoneID = '" + ZoneID + "' AND DevisionID = '" + DevisionID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["SchoolName"].ToString().Trim(), Value = Convert.ToString(mydataRow["SchoolID"]) });
                }


                return Json(MyList);
            }
        }




        [HttpPost]
        public ActionResult GetZonesByProvice(int ProvinceID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  ZoneID, ZoneName FROM m_Zones WHERE ProvinceID = " + ProvinceID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["ZoneName"].ToString().Trim(), Value = Convert.ToString(mydataRow["ZoneID"]) });
                }


                return Json(MyList);
            }
        }

        [HttpPost]
        public ActionResult GetDevisonsByZone(int ProvinceID, string ZoneID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  DevisionID, DevisionName FROM m_Devisions WHERE ProvinceID = " + ProvinceID + " AND ZoneID = '" + ZoneID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["DevisionName"].ToString().Trim(), Value = Convert.ToString(mydataRow["DevisionID"]) });
                }


                return Json(MyList);
            }
        }

        private void ShowPoupUp()
        {
            Server.Execute("~/AboutUs.cshtml");
        }

        private bool Save_StudentBulk(StudentModel model, SqlCommand cmd, out string ErrMsg)
        {
            ErrMsg = "";
            try
            {

                //string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                //using (var conn = new SqlConnection(strConnection))
                //using (var cmd = conn.CreateCommand())
                //{

                //    conn.Open();
                cmd.CommandText = "INSERT INTO StudentInfo ( SchoolID, AddmisionNo, CurrentGrade, NameWithInitials, NameInFull, DOB, Gender, ParentName, ParentAddress, NIC, ContactNo, Year, CreateUser, CreateDate) " +
                                    "VALUES(@SchoolID,  @AddmisionNo, @CurrentGrade, @NameWithInitials, @NameInFull, @DOB,  @Gender , " +
                                    "@ParentName, @ParentAddress, @NIC, @ContactNo, @Year, @CreateUser, GETDATE())";


                cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                cmd.Parameters.AddWithValue("@AddmisionNo", model.AddmisionNo);
                cmd.Parameters.AddWithValue("@CurrentGrade", model.CurrentGrade);
                cmd.Parameters.AddWithValue("@NameWithInitials", model.NameWithInitials);
                cmd.Parameters.AddWithValue("@NameInFull", model.NameInFull);
                cmd.Parameters.AddWithValue("@DOB", model.DOB);
                cmd.Parameters.AddWithValue("@Gender", model.Gender);
                cmd.Parameters.AddWithValue("@ParentName", model.ParentName);
                cmd.Parameters.AddWithValue("@ParentAddress", model.ParentAddress);
                cmd.Parameters.AddWithValue("@NIC", model.NIC);
                cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo);
                cmd.Parameters.AddWithValue("@Year", DateTime.Today.Year);
                cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                cmd.ExecuteNonQuery();
                //}



                return true;

            }
            catch (SqlException Err)
            {
                ErrMsg = Err.Message;
                ModelState.AddModelError("", "Duplicate entries found");
                return false;
            }
            catch (Exception ErrE)
            {
                ErrMsg = ErrE.Message;
                ModelState.AddModelError("", ErrE.Message);
                return false;
            }
        }

        private bool Save_Student(StudentModel model)
        {
            if (Validated_StudentInfo(model))
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO StudentInfo ( SchoolID, AddmisionNo, CurrentGrade, NameWithInitials, NameInFull, DOB, Gender, ParentName, ParentAddress, NIC, ContactNo, Year) " +
                    //                    "VALUES(@SchoolID,  @AddmisionNo, @CurrentGrade, @NameWithInitials, @NameInFull, @DOB,  @Gender , " +
                    //                    "@ParentName, @ParentAddress, @NIC, @ContactNo, @Year)";
                    cmd.CommandText = "UpdateStudentInfo";
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@AddmisionNo", model.AddmisionNo);
                    cmd.Parameters.AddWithValue("@CurrentGrade", model.CurrentGrade);
                    cmd.Parameters.AddWithValue("@NameWithInitials", model.NameWithInitials);
                    cmd.Parameters.AddWithValue("@NameInFull", model.NameInFull);
                    cmd.Parameters.AddWithValue("@DOB", model.DOB);
                    cmd.Parameters.AddWithValue("@Gender", model.Gender);
                    cmd.Parameters.AddWithValue("@ParentName", model.ParentName);
                    cmd.Parameters.AddWithValue("@ParentAddress", model.ParentAddress);
                    cmd.Parameters.AddWithValue("@NIC", model.NIC);
                    cmd.Parameters.AddWithValue("@ContactNo", model.ContactNo);
                    cmd.Parameters.AddWithValue("@Year", DateTime.Today.Year);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }



                return true;
            }
            else
            {
                return false;
            }
        }

        private bool Validated_StudentInfo(StudentModel model)
        {
            if (model.SchoolID == null || model.CurrentGrade == null || model.AddmisionNo == null
                || model.NameWithInitials == null || model.NameInFull == null || model.Gender == null
                || model.ParentName == null || model.ParentAddress == null || model.NIC == null || model.ContactNo == null)
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }

            if (model.AddmisionNo.Trim() == "" || model.NameWithInitials.Trim() == "" || model.NameInFull.Trim() == ""
                || model.Gender.Trim() == "" || model.ParentName.Trim() == "" || model.ParentAddress.Trim() == ""
                || model.NIC.Trim() == "" || model.ContactNo.Trim() == "")
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }

            //if (IsStudentExisting(model.SchoolID, model.AddmisionNo))
            //{
            //    ModelState.AddModelError("", "Student is already existing.");
            //    return false;
            //}

            return true;
        }

        public ActionResult CreateGradeInfo(string iSchoolID)
        {
            if (CheckAccessCanBeGranted(iSchoolID))
            {
                GradeInfoModel grdInf = new GradeInfoModel();

                grdInf.SchoolID = iSchoolID;

                List<SelectListItem> lstSchls = GetSchools();

                ViewBag.Schools = lstSchls;

                ViewBag.GradesInfo = GetGradesInfo(iSchoolID);

                ViewBag.CensorsID = GetCensorsID(iSchoolID);

                ViewBag.School = lstSchls.Where(s => s.Value == iSchoolID.ToString()).First().Text;

                return View(grdInf);
            }
            else
            {
                @ViewBag.ErrorMessage = "User not granted";
                return View("../Login/Error");
            }

        }

        [HttpPost]
        public ActionResult CreateGradeInfo(GradeInfoModel model)
        {
            SetCustomMessagesGradeInfo();

            if (ModelState.IsValid)
            {

                if (Save_SchoolGradeInfo(model))
                {
                    string iSchoolID = model.SchoolID;
                    ModelState.Clear();
                    model = new GradeInfoModel();
                    model.SchoolID = iSchoolID;

                }
            }
            else
            {
                ModelState.AddModelError("", "Please fill all the fields which is mandatory.");
            }

            List<SelectListItem> lstSchls = GetSchools();

            ViewBag.Schools = lstSchls;

            ViewBag.GradesInfo = GetGradesInfo(model.SchoolID);

            ViewBag.CensorsID = GetCensorsID(model.SchoolID);

            ViewBag.School = lstSchls.Where(s => s.Value == model.SchoolID.ToString()).First().Text;

            return View(model);

        }

        private DataTable GetStudentsInfoDT(string iSchoolID, int Year)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            DataTable dt;
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                // CensusID, Grade, AdmissionNo, NameWithInitials, NameInFull, DateofBirth, Gender, ParentName, ParentAddress, NIC, ContactNo

                cmd.CommandText = "SELECT m_Schools.CensorsID AS CensusID, StudentInfo.CurrentGrade AS Grade, StudentInfo.AddmisionNo AS AdmissionNo, " +
                                    "StudentInfo.NameWithInitials, StudentInfo.NameInFull, StudentInfo.DOB AS DateofBirth, StudentInfo.Gender, " +
                                    "StudentInfo.ParentName, StudentInfo.ParentAddress, StudentInfo.NIC, StudentInfo.ContactNo " +
                                    "FROM   StudentInfo INNER JOIN m_Schools ON StudentInfo.SchoolID = m_Schools.SchoolID " +
                                    "WHERE StudentInfo.SchoolID = '" + iSchoolID + "' AND StudentInfo.Status ='NEW' AND StudentInfo.Year = " + Year.ToString() + " " +
                                    "ORDER BY CurrentGrade, AddmisionNo";

                /*
                                SELECT m_Schools.CensorsID AS CensusID, StudentInfo.CurrentGrade AS Grade, StudentInfo.AddmisionNo AS AdmissionNo, 
                                StudentInfo.NameWithInitials, StudentInfo.NameInFull, StudentInfo.DOB AS DateofBirth, StudentInfo.Gender, 
                                StudentInfo.ParentName, StudentInfo.ParentAddress, StudentInfo.NIC, StudentInfo.ContactNo
                FROM   StudentInfo INNER JOIN m_Schools ON StudentInfo.SchoolID = m_Schools.SchoolID
                WHERE(StudentInfo.SchoolID = '2506017') AND(StudentInfo.Status = N'NEW') AND(StudentInfo.Year = 2016)
                ORDER BY Grade, AdmissionNo
                */


                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dt = new DataTable();
                da.Fill(dt);
            }

            return dt;

        }

        private List<StudentModel> GetStudentsInfo(string iSchoolID, int Year)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            List<StudentModel> lstStudents = new List<StudentModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, AddmisionNo, CurrentGrade, NameWithInitials, NameInFull, DOB, Gender, ParentName, " +
                                    "ParentAddress, NIC, ContactNo FROM  StudentInfo WHERE SchoolID = '" + iSchoolID + "' AND Status = 'NEW' AND Year = " + Year.ToString() + " " +
                                    "ORDER BY CurrentGrade, AddmisionNo";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    StudentModel Student = new StudentModel();
                    Student.AddmisionNo = Convert.ToString(dr["AddmisionNo"]);
                    Student.SchoolID = Convert.ToString(dr["SchoolID"]);
                    Student.NameInFull = Convert.ToString(dr["NameInFull"]);
                    Student.NameWithInitials = Convert.ToString(dr["NameWithInitials"]);
                    Student.NIC = Convert.ToString(dr["NIC"]);
                    Student.ParentName = Convert.ToString(dr["ParentName"]);
                    Student.ParentAddress = Convert.ToString(dr["ParentAddress"]);
                    Student.Gender = Convert.ToString(dr["Gender"]);
                    Student.DOB = Convert.ToDateTime(dr["DOB"]);
                    Student.ContactNo = Convert.ToString(dr["ContactNo"]);
                    Student.CurrentGrade = Convert.ToString(dr["CurrentGrade"]);

                    lstStudents.Add(Student);
                }
            }

            return lstStudents;

        }

        //yesterday i have added year as parameter i have commented that 


        private List<StudentModel> GetStudentsInfo(string iSchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            List<StudentModel> lstStudents = new List<StudentModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, AddmisionNo, CurrentGrade, NameWithInitials, NameInFull, DOB, Gender, ParentName, " +
                                    "ParentAddress, NIC, ContactNo FROM  StudentInfo WHERE SchoolID = '" + iSchoolID + "' AND Status ='NEW' AND Year = Year(getdate()) " +
                                    "ORDER BY CurrentGrade, AddmisionNo";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    StudentModel Student = new StudentModel();
                    Student.AddmisionNo = Convert.ToString(dr["AddmisionNo"]);
                    Student.SchoolID = Convert.ToString(dr["SchoolID"]);
                    Student.NameInFull = Convert.ToString(dr["NameInFull"]);
                    Student.NameWithInitials = Convert.ToString(dr["NameWithInitials"]);
                    Student.NIC = Convert.ToString(dr["NIC"]);
                    Student.ParentName = Convert.ToString(dr["ParentName"]);
                    Student.ParentAddress = Convert.ToString(dr["ParentAddress"]);
                    Student.Gender = Convert.ToString(dr["Gender"]);
                    Student.DOB = Convert.ToDateTime(dr["DOB"]);
                    Student.ContactNo = Convert.ToString(dr["ContactNo"]);
                    Student.CurrentGrade = Convert.ToString(dr["CurrentGrade"]);

                    lstStudents.Add(Student);
                }
            }

            return lstStudents;

        }

        public ActionResult SearchSchoolInfo(string strCensorsid)
        {
            List<SchoolInfoModel> lstSchools = new List<SchoolInfoModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT TOP(20)  ProvinceID, ZoneID, DevisionID, SchoolID, SchoolName, CensorsID, PrincipalName, InchargeHealthPromotion, InchargeFeedingProgram, " +
                                    " AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, NonAcademicStaffFemale, StudentsMale, StudentsFemale, " +
                                   " SchoolAddress, TelNo, Medium, Sex, ISNull(National_Provincial,0) AS National_Provincial, SchoolType, GradeSpan, District, " +
                                   " AGADivision, NearsetPoliceStation, PoliceStationContactNo, HospitalName, HospitalContactNo, " +
                                   " ContactPersionMOH, ContactNoMOH, FeedingProgramme, GNDevision,  HealthInchargeContactNo, " +
                                   "FeedingFundingSource, FeedingInchargeContactNo, NameOfPHI, PHIContactNo, PrincipalContactNo FROM  m_Schools"; // WHERE CensorsID LIKE '%" + strCensorsid + "%'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    SchoolInfoModel SchInf = new SchoolInfoModel();
                    SchInf.CensorsID = Convert.ToString(dr["CensorsID"]);
                    SchInf.DevisionID = Convert.ToString(dr["DevisionID"]);
                    SchInf.ProvinceID = Convert.ToInt32(dr["ProvinceID"]);
                    SchInf.SchoolID = Convert.ToString(dr["SchoolID"]);
                    SchInf.PrincipalName = Convert.ToString(dr["PrincipalName"]);
                    SchInf.InchargeHealthPromotion = Convert.ToString(dr["InchargeHealthPromotion"]);
                    SchInf.InchargeFeedingProgram = Convert.ToString(dr["InchargeFeedingProgram"]);
                    SchInf.SchoolName = Convert.ToString(dr["SchoolName"]);
                    SchInf.ZoneID = Convert.ToString(dr["ZoneID"]);

                    SchInf.SchoolAddress = Convert.ToString(dr["SchoolAddress"]);
                    SchInf.TelNo = Convert.ToString(dr["TelNo"]);
                    SchInf.Medium = Convert.ToString(dr["Medium"]);
                    SchInf.Sex = Convert.ToString(dr["Sex"]);
                    SchInf.National_Provincial = Convert.ToInt32(dr["National_Provincial"]);
                    SchInf.SchoolType = Convert.ToString(dr["SchoolType"]);
                    SchInf.GradeSpan = Convert.ToString(dr["GradeSpan"]);
                    SchInf.District = Convert.ToString(dr["District"]);
                    SchInf.AGADivision = Convert.ToString(dr["AGADivision"]);
                    SchInf.NearsetPoliceStation = Convert.ToString(dr["NearsetPoliceStation"]);
                    SchInf.PoliceStationContactNo = Convert.ToString(dr["PoliceStationContactNo"]);
                    SchInf.HospitalName = Convert.ToString(dr["HospitalName"]);
                    SchInf.HospitalContactNo = Convert.ToString(dr["HospitalContactNo"]);
                    SchInf.ContactPersionMOH = Convert.ToString(dr["ContactPersionMOH"]);
                    SchInf.ContactNoMOH = Convert.ToString(dr["ContactNoMOH"]);
                    SchInf.FeedingProgramme = Convert.ToString(dr["FeedingProgramme"]);
                    SchInf.GNDevision = Convert.ToString(dr["GNDevision"]);


                    SchInf.AcademicStaffMale = Convert.ToInt32(dr["AcademicStaffMale"]);
                    SchInf.AcademicStaffFemale = Convert.ToInt32(dr["AcademicStaffFemale"]);
                    SchInf.NonAcademicStaffMale = Convert.ToInt32(dr["NonAcademicStaffMale"]);
                    SchInf.NonAcademicStaffFemale = Convert.ToInt32(dr["NonAcademicStaffFemale"]);
                    SchInf.StudentsMale = Convert.ToInt32(dr["StudentsMale"]);
                    SchInf.StudentsFemale = Convert.ToInt32(dr["StudentsFemale"]);

                    SchInf.HealthInchargeContactNo = Convert.ToString(dr["HealthInchargeContactNo"]);
                    SchInf.FeedingFundingSource = Convert.ToString(dr["FeedingFundingSource"]);
                    SchInf.FeedingInchargeContactNo = Convert.ToString(dr["FeedingInchargeContactNo"]);
                    SchInf.NameOfPHI = Convert.ToString(dr["NameOfPHI"]);
                    SchInf.PHIContactNo = Convert.ToString(dr["PHIContactNo"]);
                    SchInf.PrincipalContactNo = Convert.ToString(dr["PrincipalContactNo"]);


                    lstSchools.Add(SchInf);
                }
            }

            ViewBag.SchoolsInfo = lstSchools;

            return View();
        }

        [HttpPost]
        public ActionResult SearchSchoolInfo(SchoolInfoModel model)
        //string strProvince, string strZone, string strDevision, string strCensorsid, string strSchool,
        //string strPrincipalname, string strInchargehealthpromotion, string strInchargefeedingprogram)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            // SchoolInfoModel model = new SchoolInfoModel();
            string strFilterString = "";

            if (model.ProvinceID != null && model.ProvinceID != 0)
            {
                strFilterString += " ProvinceID = " + model.ProvinceID.ToString() + " AND";
            }

            if (model.ZoneID != null)
            {
                strFilterString += " ZoneID LIKE '%" + model.ZoneID.Trim() + "%' AND";
            }

            if (model.DevisionID != null)
            {
                strFilterString += " DevisionID LIKE '%" + model.DevisionID.Trim() + "%' AND";
            }

            if (model.CensorsID != null)
            {
                strFilterString += " CensorsID LIKE '%" + model.CensorsID.Trim() + "%' AND";
            }

            if (model.SchoolName != null)
            {
                strFilterString += " SchoolName LIKE '%" + model.SchoolName.ToString() + "%' AND";
            }

            if (model.PrincipalName != null)
            {
                strFilterString += " Principalname LIKE '%" + model.PrincipalName.Trim() + "%' AND";
            }

            if (model.InchargeHealthPromotion != null)
            {
                strFilterString += " Inchargehealthpromotion LIKE '%" + model.InchargeHealthPromotion.Trim() + "%' AND";
            }

            if (model.InchargeFeedingProgram != null)
            {
                strFilterString += " Inchargefeedingprogram LIKE '%" + model.InchargeFeedingProgram.Trim() + "%' AND";
            }



            List<SchoolInfoModel> lstSchools = new List<SchoolInfoModel>();

            if (strFilterString == "")
            {
                return View();
            }
            else
            {
                strFilterString = " WHERE " + strFilterString.Substring(0, strFilterString.Length - 3);
            }

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT ProvinceID, ZoneID, DevisionID, SchoolID, SchoolName, CensorsID, PrincipalName, InchargeHealthPromotion, InchargeFeedingProgram, " +
                                    " AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, NonAcademicStaffFemale, StudentsMale, StudentsFemale, " +
                                   " SchoolAddress, TelNo, Medium, Sex, ISNull(National_Provincial,0) AS National_Provincial, SchoolType, GradeSpan, District, " +
                                   " AGADivision, NearsetPoliceStation, PoliceStationContactNo, HospitalName, HospitalContactNo, " +
                                   " ContactPersionMOH, ContactNoMOH, FeedingProgramme, GNDevision,  HealthInchargeContactNo, " +
                                   "FeedingFundingSource, FeedingInchargeContactNo, NameOfPHI, PHIContactNo, PrincipalContactNo FROM  m_Schools " + strFilterString;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    SchoolInfoModel SchInf = new SchoolInfoModel();
                    SchInf.CensorsID = Convert.ToString(dr["CensorsID"]);
                    SchInf.DevisionID = Convert.ToString(dr["DevisionID"]);
                    SchInf.ProvinceID = Convert.ToInt32(dr["ProvinceID"]);
                    SchInf.SchoolID = Convert.ToString(dr["SchoolID"]);
                    SchInf.PrincipalName = Convert.ToString(dr["PrincipalName"]);
                    SchInf.InchargeHealthPromotion = Convert.ToString(dr["InchargeHealthPromotion"]);
                    SchInf.InchargeFeedingProgram = Convert.ToString(dr["InchargeFeedingProgram"]);
                    SchInf.SchoolName = Convert.ToString(dr["SchoolName"]);
                    SchInf.ZoneID = Convert.ToString(dr["ZoneID"]);

                    SchInf.SchoolAddress = Convert.ToString(dr["SchoolAddress"]);
                    SchInf.TelNo = Convert.ToString(dr["TelNo"]);
                    SchInf.Medium = Convert.ToString(dr["Medium"]);
                    SchInf.Sex = Convert.ToString(dr["Sex"]);
                    SchInf.National_Provincial = Convert.ToInt32(dr["National_Provincial"]);
                    SchInf.SchoolType = Convert.ToString(dr["SchoolType"]);
                    SchInf.GradeSpan = Convert.ToString(dr["GradeSpan"]);
                    SchInf.District = Convert.ToString(dr["District"]);
                    SchInf.AGADivision = Convert.ToString(dr["AGADivision"]);
                    SchInf.NearsetPoliceStation = Convert.ToString(dr["NearsetPoliceStation"]);
                    SchInf.PoliceStationContactNo = Convert.ToString(dr["PoliceStationContactNo"]);
                    SchInf.HospitalName = Convert.ToString(dr["HospitalName"]);
                    SchInf.HospitalContactNo = Convert.ToString(dr["HospitalContactNo"]);
                    SchInf.ContactPersionMOH = Convert.ToString(dr["ContactPersionMOH"]);
                    SchInf.ContactNoMOH = Convert.ToString(dr["ContactNoMOH"]);
                    SchInf.FeedingProgramme = Convert.ToString(dr["FeedingProgramme"]);
                    SchInf.GNDevision = Convert.ToString(dr["GNDevision"]);


                    SchInf.AcademicStaffMale = Convert.ToInt32(dr["AcademicStaffMale"]);
                    SchInf.AcademicStaffFemale = Convert.ToInt32(dr["AcademicStaffFemale"]);
                    SchInf.NonAcademicStaffMale = Convert.ToInt32(dr["NonAcademicStaffMale"]);
                    SchInf.NonAcademicStaffFemale = Convert.ToInt32(dr["NonAcademicStaffFemale"]);
                    SchInf.StudentsMale = Convert.ToInt32(dr["StudentsMale"]);
                    SchInf.StudentsFemale = Convert.ToInt32(dr["StudentsFemale"]);

                    SchInf.HealthInchargeContactNo = Convert.ToString(dr["HealthInchargeContactNo"]);
                    SchInf.FeedingFundingSource = Convert.ToString(dr["FeedingFundingSource"]);
                    SchInf.FeedingInchargeContactNo = Convert.ToString(dr["FeedingInchargeContactNo"]);
                    SchInf.NameOfPHI = Convert.ToString(dr["NameOfPHI"]);
                    SchInf.PHIContactNo = Convert.ToString(dr["PHIContactNo"]);
                    SchInf.PrincipalContactNo = Convert.ToString(dr["PrincipalContactNo"]);


                    lstSchools.Add(SchInf);
                }
            }

            ViewBag.SchoolsInfo = lstSchools;


            return View(model);

        }

        private SchoolInfoModel GetSchoolInfo(string SchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT TOP(20)  ProvinceID, SchoolName, ZoneID, DevisionID, SchoolID, CensorsID, PrincipalName, InchargeHealthPromotion, InchargeFeedingProgram, " +
                                    " AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, NonAcademicStaffFemale, StudentsMale, StudentsFemale, " +
                                   " SchoolAddress, TelNo, Medium, Sex, ISNull(National_Provincial,0) AS National_Provincial, SchoolType, GradeSpan, District, " +
                                   " AGADivision, NearsetPoliceStation, PoliceStationContactNo, HospitalName, HospitalContactNo, " +
                                   " ContactPersionMOH, ContactNoMOH, FeedingProgramme, GNDevision,  HealthInchargeContactNo, " +
                                   "FeedingFundingSource, FeedingInchargeContactNo, NameOfPHI, PHIContactNo, PrincipalContactNo, City " +
                                " FROM  m_Schools WHERE SchoolID = '" + SchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);


                SchoolInfoModel SchInf = new SchoolInfoModel();
                SchInf.SchoolName = Convert.ToString(dt.Rows[0]["SchoolName"]);


                return SchInf;
            }

        }

        private List<SchoolInfoModel> GetSchoolInfo()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 

            List<SchoolInfoModel> lstSchools = new List<SchoolInfoModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT TOP(20)  ProvinceID, ZoneID, DevisionID, SchoolID, CensorsID, PrincipalName, InchargeHealthPromotion, InchargeFeedingProgram, " +
                                    " AcademicStaffMale, AcademicStaffFemale, NonAcademicStaffMale, NonAcademicStaffFemale, StudentsMale, StudentsFemale, " +
                                   " SchoolAddress, TelNo, Medium, Sex, ISNull(National_Provincial,0) AS National_Provincial, SchoolType, GradeSpan, District, " +
                                   " AGADivision, NearsetPoliceStation, PoliceStationContactNo, HospitalName, HospitalContactNo, " +
                                   " ContactPersionMOH, ContactNoMOH, FeedingProgramme, GNDevision,  HealthInchargeContactNo, " +
                                   "FeedingFundingSource, FeedingInchargeContactNo, NameOfPHI, PHIContactNo, PrincipalContactNo, City " +
                                " FROM  m_Schools";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    SchoolInfoModel SchInf = new SchoolInfoModel();
                    SchInf.CensorsID = Convert.ToString(dr["CensorsID"]);
                    SchInf.DevisionID = Convert.ToString(dr["DevisionID"]);
                    SchInf.ProvinceID = Convert.ToInt32(dr["ProvinceID"]);
                    SchInf.SchoolID = Convert.ToString(dr["SchoolID"]);
                    SchInf.PrincipalName = Convert.ToString(dr["PrincipalName"]);
                    SchInf.InchargeHealthPromotion = Convert.ToString(dr["InchargeHealthPromotion"]);
                    SchInf.InchargeFeedingProgram = Convert.ToString(dr["InchargeFeedingProgram"]);

                    SchInf.SchoolAddress = Convert.ToString(dr["SchoolAddress"]);
                    SchInf.TelNo = Convert.ToString(dr["TelNo"]);
                    SchInf.Medium = Convert.ToString(dr["Medium"]);
                    SchInf.Sex = Convert.ToString(dr["Sex"]);
                    SchInf.National_Provincial = Convert.ToInt32(dr["National_Provincial"]);
                    SchInf.SchoolType = Convert.ToString(dr["SchoolType"]);
                    SchInf.GradeSpan = Convert.ToString(dr["GradeSpan"]);
                    SchInf.District = Convert.ToString(dr["District"]);
                    SchInf.AGADivision = Convert.ToString(dr["AGADivision"]);
                    SchInf.NearsetPoliceStation = Convert.ToString(dr["NearsetPoliceStation"]);
                    SchInf.PoliceStationContactNo = Convert.ToString(dr["PoliceStationContactNo"]);
                    SchInf.HospitalName = Convert.ToString(dr["HospitalName"]);
                    SchInf.HospitalContactNo = Convert.ToString(dr["HospitalContactNo"]);
                    SchInf.ContactPersionMOH = Convert.ToString(dr["ContactPersionMOH"]);
                    SchInf.ContactNoMOH = Convert.ToString(dr["ContactNoMOH"]);
                    SchInf.FeedingProgramme = Convert.ToString(dr["FeedingProgramme"]);
                    SchInf.GNDevision = Convert.ToString(dr["GNDevision"]);


                    SchInf.AcademicStaffMale = Convert.ToInt32(dr["AcademicStaffMale"]);
                    SchInf.AcademicStaffFemale = Convert.ToInt32(dr["AcademicStaffFemale"]);
                    SchInf.NonAcademicStaffMale = Convert.ToInt32(dr["NonAcademicStaffMale"]);
                    SchInf.NonAcademicStaffFemale = Convert.ToInt32(dr["NonAcademicStaffFemale"]);
                    SchInf.StudentsMale = Convert.ToInt32(dr["StudentsMale"]);
                    SchInf.StudentsFemale = Convert.ToInt32(dr["StudentsFemale"]);

                    SchInf.HealthInchargeContactNo = Convert.ToString(dr["HealthInchargeContactNo"]);
                    SchInf.FeedingFundingSource = Convert.ToString(dr["FeedingFundingSource"]);
                    SchInf.FeedingInchargeContactNo = Convert.ToString(dr["FeedingInchargeContactNo"]);
                    SchInf.NameOfPHI = Convert.ToString(dr["NameOfPHI"]);
                    SchInf.PHIContactNo = Convert.ToString(dr["PHIContactNo"]);
                    SchInf.PrincipalContactNo = Convert.ToString(dr["PrincipalContactNo"]);
                    SchInf.City = Convert.ToString(dr["City"]);

                    lstSchools.Add(SchInf);
                }
            }

            return lstSchools;

        }


        private bool Save_SchoolGradeInfo(GradeInfoModel model)
        {
            if (Validated_SchoolGradeInfo(model))
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    //cmd.CommandText = "INSERT INTO m_SchoolGradeInfo (SchoolID, Grade, TeacherInCharge, Male, Female, Total, Year, CreateUser) " +
                    //                    "VALUES(@SchoolID,  @Grade, @TeacherInCharge, @Male, @Female, @Total, @Year, @CreateUser)";
                    cmd.CommandText = "UpdateGradeInfo";
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@SchoolID", model.SchoolID);
                    cmd.Parameters.AddWithValue("@Grade", model.Grade);
                    cmd.Parameters.AddWithValue("@TeacherInCharge", model.TeacherInCharge);
                    cmd.Parameters.AddWithValue("@Male", model.Male);
                    cmd.Parameters.AddWithValue("@Female", model.Female);
                    cmd.Parameters.AddWithValue("@Total", model.Male + model.Female);
                    cmd.Parameters.AddWithValue("@Year", DateTime.Today.Year);
                    cmd.Parameters.AddWithValue("@CreateUser", Convert.ToString(Session["UserName"]));
                    cmd.ExecuteNonQuery();
                }



                return true;
            }
            else
            {
                return false;
            }

        }

        private bool Validated_SchoolGradeInfo(GradeInfoModel model)
        {
            if (model.SchoolID == null || model.Grade == null || model.TeacherInCharge == null || model.Male == null || model.Female == null)
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }

            if (model.TeacherInCharge.Trim() == "")
            {
                ModelState.AddModelError("", "Please fill all the fields.");
                return false;
            }

            //if (IsGradeExisting(model.SchoolID, model.Grade))
            //{
            //    ModelState.AddModelError("", "Grade is already existing.");
            //    return false;
            //}

            return true;
        }

        private bool IsGradeExisting(string SchoolID, string Grade)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID FROM  m_SchoolGradeInfo WHERE SchoolID = '" + SchoolID + "' AND Grade = '" + Grade + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    return true;
                }

            }
            return false;
        }

        

        private bool IsStudentExisting(string SchoolID, string AdmissionNo)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID FROM  StudentInfo WHERE SchoolID = '" + SchoolID + "' AND AddmisionNo = '" + AdmissionNo + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                if (dt.Rows.Count > 0)
                {
                    return true;
                }

            }
            return false;
        }

        private List<GradesInfoModel> GetGradesInfo(string iSchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            List<GradesInfoModel> lstSchools = new List<GradesInfoModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, Grade, TeacherInCharge, Male, Female, Total FROM  m_SchoolGradeInfo WHERE SchoolID = '" + iSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    GradesInfoModel School = new GradesInfoModel();
                    School.SchoolID = Convert.ToString(dr["SchoolID"]);
                    School.Grade = Convert.ToString(dr["Grade"]);
                    School.TeacherInCharge = Convert.ToString(dr["TeacherInCharge"]);
                    School.Male = Convert.ToInt32(dr["Male"]);
                    School.Female = Convert.ToInt32(dr["Female"]);
                    School.Total = Convert.ToInt32(dr["Total"]);

                    lstSchools.Add(School);
                }
            }

            return lstSchools;

        }

        private List<SelectListItem> GetWaterSources()
        {
            // Here it'll access config and will get the relative values 
            string[] lstWaterSources = ConfigurationManager.AppSettings["WaterSources"].Split('|');

            List<SelectListItem> MyList = new List<SelectListItem>();
            foreach (string itemWaterSource in lstWaterSources)
            {
                MyList.Add(new SelectListItem { Text = itemWaterSource, Value = itemWaterSource });
            }

            return MyList;
        }

        private List<SelectListItem> GetGradesInfoList(string iSchoolID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            List<GradesInfoModel> lstSchools = new List<GradesInfoModel>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, Grade, TeacherInCharge, Male, Female, Total FROM  m_SchoolGradeInfo WHERE SchoolID = '" + iSchoolID + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                //foreach (DataRow dr in dt.Rows)
                //{
                //    GradesInfoModel School = new GradesInfoModel();
                //    School.SchoolID = Convert.ToString(dr["SchoolID"]);
                //    School.Grade = Convert.ToString(dr["Grade"]);
                //    School.TeacherInCharge = Convert.ToString(dr["TeacherInCharge"]);
                //    School.Male = Convert.ToInt32(dr["Male"]);
                //    School.Female = Convert.ToInt32(dr["Female"]);
                //    School.Total = Convert.ToInt32(dr["Total"]);

                //    lstSchools.Add(School);
                //}

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["Grade"].ToString().Trim(), Value = Convert.ToString(mydataRow["Grade"]) });
                }

                return MyList;
            }

            //return lstSchools;

        }

        // this one I rename the methode name also 
        private List<SelectListItem> GetstudentByyear(int yearid, string censorId)
        {
            StudentModel model = new StudentModel();

            // Select STATMENT HAS GIVEN IN THE DOCUMENT YOU CAN COPY AND PAST THE SAME TO hear


            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT m_Schools.CensorsID, StudentInfo.AddmisionNo, StudentInfo.CurrentGrade, " +
                    "StudentInfo.NameWithInitials, StudentInfo.NameInFull, StudentInfo.DOB, StudentInfo.Gender, StudentInfo.ParentName, " +
                    "StudentInfo.ParentAddress, StudentInfo.NIC, StudentInfo.ContactNo, StudentInfo.Year, StudentInfo.Status from StudentInfo " +
                     "inner join m_Schools on m_Schools.SchoolID = StudentInfo.SchoolID " +
                     "WHERE (m_Schools.CensorsID = '" + censorId + "') AND(StudentInfo.Year = " + yearid + ") AND (StudentInfo.Status = N'NEW')";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);



                return null;
            }
        }

        public List<SelectListItem> GetSchools()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  SchoolID, SchoolName FROM m_Schools";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["SchoolName"].ToString().Trim(), Value = Convert.ToString(mydataRow["SchoolID"]) });
                }

                return MyList;
            }



        }

        public List<SelectListItem> GetProvinces()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  ProvinceID, ProvinceName FROM m_Provinces Order by ProvinceName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["ProvinceName"].ToString().Trim(), Value = Convert.ToString(mydataRow["ProvinceID"]) });
                }

                return MyList;
            }

        }

        private List<SelectListItem> GetProvincesByUser(string userName, string selected)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "EXEC LoadProvince '" + userName+"'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["ProvinceName"].ToString().Trim(), Value = Convert.ToString(mydataRow["ProvinceID"]) });
                }
                var data = MyList.Find(i => i.Value == selected);
                if (data != null)
                    data.Selected = true;
                return MyList;
            }
        }


        public List<SelectListItem> GetZones()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  ZoneID, ZoneName FROM m_Zones Order By ZoneName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["ZoneName"].ToString().Trim(), Value = Convert.ToString(mydataRow["ZoneID"]) });
                }

                return MyList;
            }

        }

        public JsonResult GetZonesByProvinceJson(int provinceID)
        {
            
            var data =  GetZonesByUserProvice(LoggedUserName, provinceID, "");
            return Json(data);
        }

        public List<SelectListItem> GetZonesByUserProvice(string Username, int provinceID, string selected)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "EXEC LoadZone '" + Username + "', " + provinceID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["ZoneName"].ToString().Trim(), Value = Convert.ToString(mydataRow["ZoneID"]) });
                }

                var data = MyList.Find(i => i.Value == selected);
                if (data != null)
                    data.Selected = true;
                return MyList;
            }

        }

        public List<SelectListItem> GetDevisions(string selected)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  DevisionID, DevisionName FROM m_Devisions order by DevisionName";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["DevisionName"].ToString().Trim(), Value = Convert.ToString(mydataRow["DevisionID"]) });
                }

                var data  = MyList.Find(i => i.Value == selected);
                if (data != null)
                    data.Selected = true;
                return MyList;
            }

        }

        //    //public List<SchoolsInfoModel> GetSchoolsInfoAll()
        //    //{
        //    //    // Here you are free to do whatever data access code you like
        //    //    // You can invoke direct SQL queries, stored procedures, whatever 
        //    //    string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
        //    //    using (var conn = new SqlConnection(strConnection))
        //    //    using (var cmd = conn.CreateCommand())
        //    //    {

        //    //        conn.Open();
        //    //        cmd.CommandText = "SELECT     SchoolID, Grade, TeacherInCharge, Male, Female, Total FROM m_SchoolInfo";
        //    //        SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    //        DataTable dt = new DataTable();
        //    //        da.Fill(dt);

        //    //        List<SchoolsInfoModel> MyList = new List<SchoolsInfoModel>();
        //    //        foreach (DataRow mydataRow in dt.Rows)
        //    //        {
        //    //            MyList.Add(new SchoolsInfoModel()
        //    //            {
        //    //                SchoolID  = Convert.ToInt32(mydataRow["SchoolID"]),
        //    //                TeacherInCharge = mydataRow["TeacherInCharge"].ToString().Trim(),
        //    //                Male = Convert.ToInt32(mydataRow["Male"]),
        //    //                Female = Convert.ToInt32(mydataRow["Female"]),
        //    //                Grade = Convert.ToInt32(mydataRow["Grade"]),
        //    //                Total = Convert.ToInt32(mydataRow["Total"])
        //    //            });
        //    //        }

        //    //        return MyList;
        //    //    }



        //    //}

        //}


        public System.Data.DataTable getMonitoringOfficerInfo()
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "* " +
                                   " FROM " +
                                            " VW_MonitoringOfficerInfo ";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }

        }

        public ActionResult SupplierPaymentMOE()
        {
            SupplierPaymentMOE model = new SupplierPaymentMOE();
            
            //ViewBag.Provinces = GetProvinces();

            //List<SelectListItem> Zones = GetZones();

            //Zones.Clear();

            //ViewBag.Zones = Zones;

            //ViewBag.Years = GetYearsInt();

            List<SupplierPaymentMOE> PaymentRequestList = getSavedSupplierPaymentMOEs();

            //SupplierPaymentRequest paymentReq = PaymentRequestList.FirstOrDefault(p => p.Id == id);
            //if (paymentReq == null)
            //{
            //    paymentReq = new SupplierPaymentRequest();
            //}

            //paymentReq.PaymentDetails = LoadSuppliersForPayment(paymentReq.ProvinceID, paymentReq.ZoneID, paymentReq.Year);
            ViewBag.SupplierPaymentRequestList = PaymentRequestList;
            ViewBag.Month = GetMonthsString();
            ViewBag.Year = GetResentYears(model.Year.ToString());
            ViewBag.Banks = GetBanks();
            //ViewBag.ProvinceID = GetProvincesByUser("Admin");
            //ViewBag.ZoneID = GetZonesByUserProvice("Admin", paymentReq.ProvinceID);

             ViewBag.GrantTotal = (from od in model.PaymentSummary select od.TotalAmount).Sum();

            return View(model);
        }

        public JsonResult GETSupplierPaymentSummery(int Year, string Month)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            List<SupplierPaymentSummary> lstPayment = new List<SupplierPaymentSummary>();
            List<SelectListItem> lstProvinces = GetProvinces();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "EXEC GET_SupplerPaymentSummery " + Year + ",'" + Month + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    SupplierPaymentSummary row = new SupplierPaymentSummary();
                    string prov = Convert.ToString(dr["ProvinceID"]);
                    row.Province = lstProvinces.Where(p => p.Value == prov).FirstOrDefault().Text;
                    row.Year = Convert.ToInt32(dr["Year"]);
                    row.Month = Convert.ToString(dr["Month"]);
                    row.TotalAmount = Convert.ToDecimal(dr["ProvinceTotal"]);

                    lstPayment.Add(row);
                }
            }

            return Json(lstPayment);

        }

        private List<SelectListItem> GetMonthsString()
        {
            List<SelectListItem> MyList = new List<SelectListItem>();
            MyList.Add(new SelectListItem { Text = "January", Value = "January" });
            MyList.Add(new SelectListItem { Text = "February", Value = "February" });
            MyList.Add(new SelectListItem { Text = "March", Value = "March" });
            MyList.Add(new SelectListItem { Text = "April", Value = "April" });
            MyList.Add(new SelectListItem { Text = "May", Value = "May" });
            MyList.Add(new SelectListItem { Text = "June", Value = "June" });
            MyList.Add(new SelectListItem { Text = "July", Value = "July" });
            MyList.Add(new SelectListItem { Text = "August", Value = "August" });
            MyList.Add(new SelectListItem { Text = "September", Value = "September" });
            MyList.Add(new SelectListItem { Text = "October", Value = "October" });
            MyList.Add(new SelectListItem { Text = "November", Value = "November" });
            MyList.Add(new SelectListItem { Text = "December", Value = "December" });

            return MyList;
        }

        private List<SupplierPaymentMOE> getSavedSupplierPaymentMOEs()
        {
            List<SupplierPaymentMOE> lstPayment = new List<SupplierPaymentMOE>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "* " +
                                   " FROM " +
                                            " SupplierPayment_MOE_Header ";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    SupplierPaymentMOE row = new SupplierPaymentMOE();
                    row.PaymentNo = Convert.ToString(dr["PaymentID"]);
                    row.PaymentDate = Convert.ToString(dr["PaymentDate"]);
                    row.Year = Convert.ToInt32(dr["Year"]);
                    row.Month = Convert.ToString(dr["Month"]);
                    row.Amount = Convert.ToDecimal(dr["Amount"]);
                    row.ChequeNumber = Convert.ToString(dr["ChequeNumber"]);
                    row.ChequeDate = Convert.ToDateTime(dr["ChequeDate"]);

                    lstPayment.Add(row);
                }
                return lstPayment;
            }

        }

        public void Export2ExcelSupplier(string ID)
        {
            //We load the data


            string ExpType = "Province";
            DataTable SuppInfo = null;

            if (ExpType == "Province")
            {
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();

                    //cmd.CommandText = "SELECT DISTINCT StudentInfo.AddmisionNo AS AdmissionNo, StudentInfo.DOB, StudentInfo.Gender, Height, Weight " +
                    //                   "FROM " +
                    //                   "StudentInfo LEFT OUTER JOIN BMIInformation ON StudentInfo.SchoolID = BMIInformation.SchoolID AND StudentInfo.AddmisionNo = BMIInformation.AdmissionNo AND StudentInfo.CurrentGrade = BMIInformation.Class AND Trimester = " + DateTime.Now.Year.ToString() + Trimester.ToString() +
                    //                   " WHERE StudentInfo.SchoolID ='" + SchoolID + "' AND StudentInfo.CurrentGrade = '" + Grade + "'";


                    cmd.CommandText = "SELECT m_Zones.ZoneName, m_Devisions.DevisionName, m_Schools.CensorsID, m_Schools.SchoolName, " +
                                "m_SupplierInformation.SupplierName, m_SupplierInformation.Address, m_SupplierInformation.NIC, " +
                                 "m_SupplierInformation.Phone, m_SupplierInformation.BankAccountNo, m_SupplierInformation.Grade, " +
                                 "m_SupplierInformation.NoOfMaleStudents + m_SupplierInformation.NoOfFemaleStudents AS TotStudents " +
                                 "FROM m_Zones INNER JOIN " +
                                 "m_Schools ON m_Zones.ZoneID = m_Schools.ZoneID INNER JOIN " +
                                 "m_Devisions ON m_Schools.DevisionID = m_Devisions.DevisionID RIGHT OUTER JOIN " +
                                 "m_SupplierInformation ON m_Schools.SchoolID = m_SupplierInformation.SchoolID " +
                                 "WHERE(m_Schools.ProvinceID = " + Convert.ToInt64(ID) + ") AND (m_SupplierInformation.Status = 'NEW') " +
                                 "ORDER BY m_Zones.ZoneName, m_Devisions.DevisionName";


                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    SuppInfo = new DataTable();
                    da.Fill(SuppInfo);


                }
            }


            var grid = new GridView();
            grid.DataSource = SuppInfo;
            grid.DataBind();

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(SuppInfo, "SupplierInfo");
                wb.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                wb.Style.Font.Bold = true;


                Response.Clear();
                Response.Buffer = true;
                Response.Charset = "";
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename= " + ExpType + "_Supplier.xlsx");

                using (MemoryStream MyMemoryStream = new MemoryStream())
                {
                    wb.SaveAs(MyMemoryStream);
                    MyMemoryStream.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.End();
                }
            }
        }

        public List<SupplierPaymentMOE> getSupplierPaymentDetails()
        {
            List<SupplierPaymentMOE> lstPayment = new List<SupplierPaymentMOE>();
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  " +
                                            "* " +
                                   " FROM " +
                                            " SupplierPayment_MOE_Header ";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                foreach (DataRow dr in dt.Rows)
                {
                    SupplierPaymentMOE row = new SupplierPaymentMOE();
                    row.PaymentNo = Convert.ToString(dr["PaymentID"]);
                    row.PaymentDate = Convert.ToString(dr["PaymentDate"]);
                    row.Year = Convert.ToInt32(dr["Year"]);
                    row.Month = Convert.ToString(dr["Month"]);
                    row.Amount = Convert.ToDecimal(dr["Amount"]);
                    row.ChequeNumber = Convert.ToString(dr["ChequeNumber"]);
                    row.ChequeDate = Convert.ToDateTime(dr["ChequeDate"]);

                    lstPayment.Add(row);
                }
                return lstPayment;
            }

        }

        public ActionResult SupplierPaymentForwardedPdfGenerate( int proviceId , string zoneId , int year , string month ,int payReqNo)
        {
            var data = LoadSuppliersForPayment(proviceId, zoneId, year, month, payReqNo);
            return new Rotativa.PartialViewAsPdf("_SupplierPaymentPdfView", data)
            {
                FileName = $"payement_details_{proviceId}_{zoneId}_{year}_{month}.pdf"
            };
        }
    }
}
