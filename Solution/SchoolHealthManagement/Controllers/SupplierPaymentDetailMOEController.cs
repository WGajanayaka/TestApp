using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SchoolHealthManagement.Models;

namespace SchoolHealthManagement.Controllers
{
    public class SupplierPaymentDetailMoeController : Controller 
    {
        private readonly string _strConnection;

        public SupplierPaymentDetailMoeController()
        {
            _strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString; 
        }

        public ActionResult Index()
        {
            SupplierPaymentMOE model = new SupplierPaymentMOE();

            List<SupplierPaymentMOE> paymentRequestList = getSavedSupplierPaymentMOEs();

            
            ViewBag.SupplierPaymentRequestList = paymentRequestList;
            ViewBag.Month = GetMonthsString();
            ViewBag.Year = GetResentYears(model.Year.ToString());
            ViewBag.Banks = GetBanks();
            

            ViewBag.GrantTotal = (from od in model.PaymentSummary select od.TotalAmount).Sum();

            return View(model);
        }
       
        public JsonResult GetSupplierPaymentSummery(int year, string month)
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
                cmd.CommandText = $"EXEC GET_SupplerPaymentSummery {year},'{month}'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    SupplierPaymentSummary row = new SupplierPaymentSummary();
                    string prov = Convert.ToString(dr["ProvinceID"]);
                    row.Province = lstProvinces.FirstOrDefault(p => p.Value == prov)?.Text;
                    row.ProvinceId = prov;
                    row.Year = Convert.ToInt32(dr["Year"]);
                    row.Month = Convert.ToString(dr["Month"]);
                    row.TotalAmount = Convert.ToDecimal(dr["ProvinceTotal"]);

                    lstPayment.Add(row);
                }
            }
            var grantTotal = lstPayment.Sum(x => x.TotalAmount);
            return Json(new { data = lstPayment, grantTotal = grantTotal }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult GetPaymentDetailMoe(string model)
        {
            try
            {
                var filters = JsonConvert.DeserializeObject<List<FilterModel>>(model);
                List<SupplierPaymentRequestDetail> lstPayment = new List<SupplierPaymentRequestDetail>();
                var zonedataDic = new Dictionary<string, SupplierDetailZoneLevel>();
                var year = filters.Select(x => x.Year).FirstOrDefault();
                var month = filters.Select(x => x.Month).FirstOrDefault();
                using (var conn = new SqlConnection(_strConnection))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    foreach (var filter in filters)
                    {
                        cmd.CommandText =
                            $"EXEC GET_SupplerPaymentDetails {filter.Year},'{filter.Month}',{filter.ProvinceId},{false}";
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        foreach (DataRow dr in dt.Rows)
                        {
                            SupplierPaymentRequestDetail row = new SupplierPaymentRequestDetail();
                            row.CensorsID = Convert.ToString(dr["CensusID"]);
                            row.SupplierId = Convert.ToInt32(dr["SupplierId"]);
                            row.SupplierName = Convert.ToString(dr["SupplierName"]);
                            row.Amount = Convert.ToDecimal(dr["Amount"]);
                            row.SupplierPaymentReqHeaderId = Convert.ToDecimal(dr["supplierPaymentReq_HeaderId"]);
                            row.BankAccountNo = Convert.ToString(dr["BankAccountNo"]);
                            row.BankName = Convert.ToString(dr["BankName"]);
                            row.BranchName = Convert.ToString(dr["BranchName"]);
                            row.BankCode = Convert.ToString(dr["BankCode"]);
                            row.BranchCode = Convert.ToString(dr["BranchCode"]);
                            row.ZoneName = Convert.ToString(dr["ZoneName"]);
                            row.ProvinceName = Convert.ToString(dr["ProvinceName"]);
                            row.ProvinceId = Convert.ToInt32(dr["ProvinceId"]);
                            row.SupplierPaymentReqDetailId = Convert.ToInt32(dr["SupplierPaymentReq_DetailId"]);

                            lstPayment.Add(row);
                        }
                    }

                    var provinceGroup = lstPayment.GroupBy(x => x.ProvinceName);
                    var returnModel = new SupplierDetailPayModel();
                    returnModel.FullTotal = lstPayment.Sum(x => x.Amount);

                    foreach (var prov in provinceGroup)
                    {
                        var provinceLevel = new SupplierDetailProvinceLevel();
                        provinceLevel.ProvincialTotal = prov.Sum(x => x.Amount);
                        provinceLevel.ProvinceName = prov.Key;
                        var zoneGroup = prov.GroupBy(x => x.ZoneName);
                        foreach (var zone in zoneGroup)
                        {
                            var zoneLevel = new SupplierDetailZoneLevel();
                            zoneLevel.ZoneTotal = zone.Sum(x => x.Amount);
                            zoneLevel.ZoneName = zone.Key;
                            var details = zone.ToList().Select(c => new SupplierPaymentRequestDetailMoe()
                            {
                                Amount = c.Amount,
                                BankName = c.BankName,
                                BankCode = c.BankCode,
                                BranchCode = c.BranchCode,
                                BranchName = c.BranchName,
                                SupplierName = c.SupplierName,
                                BankAccountNo = c.BankAccountNo,
                                CensorsId = c.CensorsID,
                                Year = year,
                                Month = month,
                                ProvinceId = c.ProvinceId,
                                SupplierId = c.SupplierId,
                                SupplierPaymentReqHeaderId = c.SupplierPaymentReqHeaderId,
                                SupplierPaymentReqDetailId = c.SupplierPaymentReqDetailId

                            }).ToList();
                            zoneLevel.SupplierPaymentRequestDetails = details;
                            provinceLevel.SupplierDetailZoneLevel.Add(zoneLevel);
                            zonedataDic.Add(zone.Key, zoneLevel);
                        }
                        returnModel.Detail.Add(provinceLevel);

                    }
                    ViewBag.TableData = zonedataDic;
                    return PartialView("_SupplierPaymentMOEGroupLevel", returnModel);
                }
            }
            catch (Exception e)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult SaveSupplyerPaymentMoe(string model, string chequeNo, string chequeDate)
        {
            try
            {
                var chequeDateConverted = Convert.ToDateTime(chequeDate);
                var savedPayments = JsonConvert.DeserializeObject<List<SupplierPaymentRequestDetailMoe>>(model);

                var grandotal = savedPayments.Sum(x => x.Amount);
                var result = Insert_SupplierPayment_MOE_Header(chequeNo, chequeDateConverted, savedPayments, grandotal);
                if (result)
                {
                    UpdateSupplierPaymentReq_Detail(savedPayments);
                }

                return Json(new { status = true, mes = "Success" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = false, mes = e.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        private bool Insert_SupplierPayment_MOE_Header(string chequeNo, DateTime chequeDateConverted, List<SupplierPaymentRequestDetailMoe> savedPayments, decimal grandotal)
        {
            try
            {
                var year = savedPayments.FirstOrDefault()?.Year;
                var month = savedPayments.FirstOrDefault()?.Month;
                var provinceId = savedPayments.FirstOrDefault()?.ProvinceId;

                using (var conn = new SqlConnection(_strConnection))
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();

                        cmd.Parameters.Clear();
                        cmd.CommandText = "Insert_SupplierPayment_MOE_Header";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Year", year);
                        cmd.Parameters.AddWithValue("@Month", month);
                        cmd.Parameters.AddWithValue("@BankCode", "");
                        cmd.Parameters.AddWithValue("@BankName", "");
                        cmd.Parameters.AddWithValue("@ChequeNumber", chequeNo);
                        cmd.Parameters.AddWithValue("@ChequeDate", chequeDateConverted);
                        cmd.Parameters.AddWithValue("@Amount", grandotal);
                        cmd.Parameters.AddWithValue("@Status", "paied");
                        cmd.Parameters.AddWithValue("@CreateUser", User.Identity.Name);
                        cmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);
                        cmd.Parameters.AddWithValue("@AprovedUser", User.Identity.Name);
                        cmd.Parameters.AddWithValue("@ApprovedDate", DateTime.Now);

                        SqlParameter outPutVal = new SqlParameter("@New_PaymentReqNo", SqlDbType.Int);
                        outPutVal.Direction = ParameterDirection.Output;
                        cmd.Parameters.Add(outPutVal);

                        cmd.ExecuteNonQuery();

                        if (outPutVal.Value != DBNull.Value)
                        {
                            var paymentId = Convert.ToDecimal(outPutVal.Value);
                            SaveSupplierPayment_MOE_Details(savedPayments, paymentId);
                        }
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                return false;

            }
        }

        private bool SaveSupplierPayment_MOE_Details(List<SupplierPaymentRequestDetailMoe> model, decimal paymentId)
        {
            try
            {
                if (model != null)
                {
                    using (var conn = new SqlConnection(_strConnection))
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            foreach (var m in model)
                            {
                                cmd.Parameters.Clear();
                                cmd.CommandText = "Insert_SupplierPayment_MOE_Details";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@PaymentID", paymentId);
                                cmd.Parameters.AddWithValue("@CensusID", m.CensorsId);
                                cmd.Parameters.AddWithValue("@SupplierId", m.SupplierId);
                                cmd.Parameters.AddWithValue("@BankCode", m.BankCode);
                                cmd.Parameters.AddWithValue("@BranchCode", m.BranchCode);
                                cmd.Parameters.AddWithValue("@AccountNo", m.BankAccountNo);
                                cmd.Parameters.AddWithValue("@Amount", m.Amount);

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }

                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }


        private List<SupplierPaymentRequestDetail> LoadSuppliersForPayment(int ProvinceID, string ZoneID, int year, string Month, int PaymentReqNo)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                string UserName = "";
                conn.Open();
                cmd.CommandText = "EXEC LoadSuppliers " + ProvinceID + ", '" + ZoneID + "', " + year + ", '" + Month + "', " + PaymentReqNo;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);



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
                    if (mydataRow["BankID"] != DBNull.Value)
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

        private void UpdateSupplierPaymentReq_Detail(List<SupplierPaymentRequestDetailMoe> model)
        {
            var supplierPaymentReqDetailIds = model.Select(x => x.SupplierPaymentReqDetailId).ToList();

            using (var conn = new SqlConnection(_strConnection))
            {
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    foreach (var id in supplierPaymentReqDetailIds)
                    {
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE SupplierPaymentReq_Details SET Paid= @paied WHERE Id = @Id";
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@paied", true);
                        cmd.Parameters.AddWithValue("@Id", id);

                        cmd.ExecuteNonQuery();
                    }

                }
            }

        }

        public ActionResult SupplierPaymentMoeTextGenerate(string model)
        {
            var filters = JsonConvert.DeserializeObject<List<FilterModel>>(model);
            List<SupplierPaymentRequestPaidDetail> lstPayment = new List<SupplierPaymentRequestPaidDetail>();

            using (var conn = new SqlConnection(_strConnection))
            using (var cmd = conn.CreateCommand())
            {
                var filter = filters.FirstOrDefault();
                    conn.Open();
                    cmd.CommandText = $"EXEC GetSupplierPaymentMOEPaidDetail {filter?.Year},'{filter?.Month}',{true}";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        SupplierPaymentRequestPaidDetail row = new SupplierPaymentRequestPaidDetail();
                        row.PaymentNo = Convert.ToDecimal(dr["PaymentNo"]);
                        row.PaymentDate = Convert.ToDateTime(dr["PaymentDate"]);
                        row.ChequeDate = Convert.ToDateTime(dr["ChequeDate"]);
                        row.Amount = Convert.ToDecimal(dr["Amount"]);
                        row.BankName = Convert.ToString(dr["BankName"]);
                        row.BankCode = Convert.ToString(dr["BankCode"]);
                        row.BranchCode = Convert.ToString(dr["BranchCode"]);
                        row.AccountNo = Convert.ToString(dr["AccountNo"]);
                        row.SupplierName = Convert.ToString(dr["SupplierName"]);
                        row.ChequeNumber = Convert.ToString(dr["ChequeNumber"]);
                        row.Month = Convert.ToString(dr["Month"]);
                        row.Year = Convert.ToInt32(dr["Year"]);


                        lstPayment.Add(row);
                    }
                
            }
            if (lstPayment.Any())
            {


                var year = lstPayment.First().Year;
                var month = lstPayment.First().Month;
                StringBuilder record = new StringBuilder();
                foreach (var re in lstPayment)
                {
                    StringBuilder payment = new StringBuilder();
                    payment.Append("0000");
                    payment.Append(re.BankCode.PadLeft(4, '0'));
                    payment.Append(re.BranchCode.PadLeft(3, '0'));
                    payment.Append(re.AccountNo.PadLeft(12, '0'));
                    payment.Append(re.SupplierName.PadLeft(23, ' '));
                    payment.Append("23"); // txn code
                    payment.Append("00"); // return code
                    payment.Append("0"); // filler 
                    payment.Append("000000"); // filler 
                    var amount = re.Amount.ToString(CultureInfo.InvariantCulture).Split('.');

                    var amountString = string.Join("", amount);

                    payment.Append(amountString.PadLeft(12, '0')); // amount 
                    payment.Append("SLR"); // filler 
                    var oBankNo = ConfigurationManager.AppSettings["OBankNo"];
                    payment.Append(oBankNo.PadLeft(4, '0')); // originating Bank  no 

                    var oBranchNo = ConfigurationManager.AppSettings["oBranchNo"];
                    payment.Append(oBranchNo.PadLeft(3, '0')); // originating Branch  no 

                    var oAccNo = ConfigurationManager.AppSettings["oAccNo"];
                    payment.Append(oAccNo.PadLeft(12, '0')); // originating Acc  no 

                    var oAccName = ConfigurationManager.AppSettings["oAccName"];
                    payment.Append(oAccName.PadLeft(12, ' ')); // originating Acc Name 

                    var particulars = ConfigurationManager.AppSettings["particulars"];
                    payment.Append(particulars.PadLeft(15, ' ')); // particulars

                    var reference = ConfigurationManager.AppSettings["reference"];
                    payment.Append(reference.PadLeft(15, ' ')); // reference

                    payment.Append(re.ChequeDate.ToString("yyMMdd").PadLeft(6, '0')); // Value Date
                    payment.Append("000000"); // filler

                    record.Append(payment);
                    record.Append(Environment.NewLine);
                }
                var byteArray = Encoding.ASCII.GetBytes(record.ToString());
                var stream = new MemoryStream(byteArray);

                return File(stream, "text/plain", $"Paid_{year}_{month}.txt");
            }
            return null;
        }

        public ActionResult SupplierPaymentMoePdfGenerate(string model)
        {
            var filters = JsonConvert.DeserializeObject<List<FilterModel>>(model);
            List<SupplierPaymentRequestDetail> lstPayment = new List<SupplierPaymentRequestDetail>();
            var year = filters.Select(x => x.Year).FirstOrDefault();
            var month = filters.Select(x => x.Month).FirstOrDefault();
            using (var conn = new SqlConnection(_strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                foreach (var filter in filters)
                {
                    cmd.CommandText = $"EXEC GET_SupplerPaymentDetails {filter.Year},'{filter.Month}',{filter.ProvinceId},{true}";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        SupplierPaymentRequestDetail row = new SupplierPaymentRequestDetail();
                        row.CensorsID = Convert.ToString(dr["CensusID"]);
                        row.SupplierId = Convert.ToInt32(dr["SupplierId"]);
                        row.SupplierName = Convert.ToString(dr["SupplierName"]);
                        row.Amount = Convert.ToDecimal(dr["Amount"]);
                        row.SupplierPaymentReqHeaderId = Convert.ToDecimal(dr["supplierPaymentReq_HeaderId"]);
                        row.BankAccountNo = Convert.ToString(dr["BankAccountNo"]);
                        row.BankName = Convert.ToString(dr["BankName"]);
                        row.BranchName = Convert.ToString(dr["BranchName"]);
                        row.BankCode = Convert.ToString(dr["BankCode"]);
                        row.ZoneName = Convert.ToString(dr["ZoneName"]);
                        row.ProvinceName = Convert.ToString(dr["ProvinceName"]);

                        lstPayment.Add(row);
                    }
                }

                var provinceGroup = lstPayment.GroupBy(x => x.ProvinceName);
                var returnModel = new SupplierDetailPayModel();
                returnModel.FullTotal = lstPayment.Sum(x => x.Amount);

                foreach (var prov in provinceGroup)
                {
                    var provinceLevel = new SupplierDetailProvinceLevel();
                    provinceLevel.ProvincialTotal = prov.Sum(x => x.Amount);
                    provinceLevel.ProvinceName = prov.Key;
                    var zoneGroup = prov.GroupBy(x => x.ZoneName);
                    foreach (var zone in zoneGroup)
                    {
                        var zoneLevel = new SupplierDetailZoneLevel();
                        zoneLevel.ZoneTotal = zone.Sum(x => x.Amount);
                        zoneLevel.ZoneName = zone.Key;
                        var details = zone.ToList().Select(c => new SupplierPaymentRequestDetailMoe()
                        {
                            Amount = c.Amount,
                            BankName = c.BankName,
                            BankCode = c.BankCode,
                            BranchName = c.BranchName,
                            SupplierName = c.SupplierName,
                            BankAccountNo = c.BankAccountNo,
                            CensorsId = c.CensorsID,
                            Year = year,
                            Month = month,
                            Paid = "Paid"

                        }).ToList();
                        zoneLevel.SupplierPaymentRequestDetails = details;
                        provinceLevel.SupplierDetailZoneLevel.Add(zoneLevel);

                    }
                    returnModel.Detail.Add(provinceLevel);

                }

                return new Rotativa.PartialViewAsPdf("_SupplierPaymentMOEPdfView", returnModel)
                {
                    FileName = $"SupplierPayment_DetailsMOE_{year}_{month}.pdf"
                };
            }
        }

        public ActionResult GetPaidPaymentDetailMoeView( string model) 
        {
            var filters = JsonConvert.DeserializeObject<List<FilterModel>>(model);
            List<SupplierPaymentRequestPaidDetail> lstPayment = new List<SupplierPaymentRequestPaidDetail>();

            using (var conn = new SqlConnection(_strConnection))
            using (var cmd = conn.CreateCommand())
            {
                    var filter = filters.FirstOrDefault();
                    conn.Open();
                
                    cmd.CommandText = $"EXEC GetSupplierPaymentMOEPaidDetail {filter?.Year},'{filter?.Month}',{false}";
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    foreach (DataRow dr in dt.Rows)
                    {
                        SupplierPaymentRequestPaidDetail row = new SupplierPaymentRequestPaidDetail();
                        row.PaymentNo = Convert.ToDecimal(dr["PaymentNo"]);
                        row.PaymentDateString = Convert.ToDateTime(dr["PaymentDate"]).ToShortDateString();
                        row.ChequeDateString = Convert.ToDateTime(dr["ChequeDate"]).ToString("yyyy MMMM dd");
                        row.Amount = Convert.ToDecimal(dr["Amount"]);
                        row.BankName = Convert.ToString(dr["BankName"]);
                        row.ChequeNumber = Convert.ToString(dr["ChequeNumber"]);
                        row.Month = Convert.ToString(dr["Month"]);
                        row.Year = Convert.ToInt32(dr["Year"]);


                        lstPayment.Add(row);
                    }
                
            }
            if (lstPayment.Any())
            {
                return PartialView("_SupplierPaymentPaidMOEView", lstPayment);
            }

            return Json(new {status = false} ,JsonRequestBehavior.AllowGet);

        }
        private List<SelectListItem> GetProvinces()
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            
            using (var conn = new SqlConnection(_strConnection))
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

        private List<SelectListItem> GetResentYears(string selected)
        {
            List<SelectListItem> MyList = new List<SelectListItem>();
            for (int i = 2016; i < 2020; i++)
            {
                MyList.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString() });
            }
            var data = MyList.Find(i => i.Value == selected);
            if (data != null)
                data.Selected = true;
            return MyList;
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
        
    }
}
