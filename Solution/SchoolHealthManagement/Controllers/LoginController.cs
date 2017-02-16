using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;

namespace SchoolHealthManagement.Controllers
{
    public class LoginController : Controller
    {
        //
        // GET: /Login/

        public ActionResult LogOut()
        {
            Session["UserName"] = null;
            Session.Abandon();

            return View("Login");

        }

        [HttpPost]
        public ActionResult LogOut(Models.LoginModel model, string strReturnURL)
        {
           return Login(model, strReturnURL);

        }

        public ActionResult UserProfile()
        {
            
            Models.UserProfileModel usrProf = new Models.UserProfileModel();
            HomeController homeCtrl = new HomeController();
            ViewBag.Provinces = homeCtrl.GetProvinces();
            ViewBag.Zones = homeCtrl.GetZones();
            ViewBag.Devisions = homeCtrl.GetDevisions();
            ViewBag.Roles = GetRoles(Convert.ToInt32(Session["LoginRoleID"]));
            ViewBag.Schools = homeCtrl.GetSchools();

            return View(usrProf);
           
        }

        [HttpPost]
        public ActionResult UserProfile(Models.UserProfileModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.RoleID == 3 && (model.ZoneID == null || model.ProvinceID == null))
                {
                    ModelState.AddModelError("", "Please specify Zone and Province.");
                    
                }

                if (model.RoleID == 4 && (model.ZoneID == null || model.ProvinceID == null || model.DevisionID == null))
                {
                    ModelState.AddModelError("", "Please specify Zone and Province and Devision.");
                }

                if (IsUserExisting(model.UserName))
                {
                    ModelState.AddModelError("", "Username already exists. Please try again with a different username.");
                }

                if (ModelState.IsValid)
                {

                    if (SaveUser(model))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error in Saving.");
                    }
                }
            }
            else
            {
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("", "Please check the password again and confirm.");
                }
                else
                {
                    // If we got this far, something failed, redisplay form
                    ModelState.AddModelError("", "Please fill the required fields.");
                }

            }

            HomeController homeCtrl = new HomeController();
            ViewBag.Roles = GetRoles(Convert.ToInt32(Session["LoginRoleID"]));
            ViewBag.Provinces = homeCtrl.GetProvinces();
            ViewBag.Zones = GetZonesByProvice(model.ProvinceID);
            ViewBag.Devisions = GetDevisonsByZone(model.ProvinceID, model.ZoneID);
            ViewBag.Schools = GetSchoolsByDevison(model.ProvinceID, model.ZoneID, model.DevisionID);


            return View(model);
        }

        private List<SelectListItem> GetZonesByProvice(int ProvinceID)
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

                return MyList;
            }
        }

        private List<SelectListItem> GetDevisonsByZone(int ProvinceID, string ZoneID)
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


                return MyList;
            }
        }

        private List<SelectListItem> GetSchoolsByDevison(int ProvinceID, string ZoneID, string DevisionID)
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


                return MyList;
            }
        }

        private bool IsUserExisting(string userName)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT UserName FROM U_Users WHERE UserName = @Uname";
                cmd.Parameters.AddWithValue("@Uname", userName);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }

        private bool SaveUser(Models.UserProfileModel model)
        {
            //if (Validated_StudentInfo(model))
            //{
                string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
                using (var conn = new SqlConnection(strConnection))
                using (var cmd = conn.CreateCommand())
                {

                    conn.Open();
                    cmd.CommandText = "INSERT INTO U_Users (UserName, Password, Name, Designation, Email, RoleID, ProvinceID, ZoneID, DevisionID, SchoolID, CreateUser) " +
                                        "VALUES(@UserName,  @Password, @Name, @Designation, @Email, @RoleID,  @ProvinceID, @ZoneID, @DevisionID, @SchoolID, @CreateUser) " ;


                    cmd.Parameters.AddWithValue("@UserName", model.UserName);
                    cmd.Parameters.AddWithValue("@Password", model.Password);
                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Designation", model.Designation);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@RoleID", model.RoleID);
                    cmd.Parameters.AddWithValue("@ProvinceID", model.ProvinceID);
                    cmd.Parameters.AddWithValue("@ZoneID", model.ZoneID);
                    cmd.Parameters.AddWithValue("@DevisionID", (model.DevisionID == null?"":model.DevisionID));
                    cmd.Parameters.AddWithValue("@SchoolID", (model.SchoolID == null?"":model.SchoolID));
                    cmd.Parameters.AddWithValue("@CreateUser", Session["UserName"].ToString());

                    cmd.ExecuteNonQuery();
                }



                return true;
            
            
        }

        public ActionResult Login(string returnUrl)
        {
            
            return View();
        }

        [HttpPost]
        public ActionResult GetLocationInfo()
        {
            int iRoleID = Convert.ToInt32(Session["LoginRoleID"]);
            
            //Get Logged in user's provinceID, ZoneID, DevisionID, SchoolID
            int lProvinceID = 0;
            string lZoneID = "";
            string lDevisionID = "";
            string lSchoolID = "";

            getLoggedInLocation(out lProvinceID, out lZoneID, out lDevisionID, out lSchoolID);

            var result = new { RoleID = iRoleID, ProvinceID = lProvinceID, ZoneID = lZoneID, DevisionID = lDevisionID, SchoolID = lSchoolID };
            return Json(result, JsonRequestBehavior.AllowGet);
            //return Json(iRoleID);
        }

        private void getLoggedInLocation(out int lProvinceID, out string lZoneID, out string lDevisionID, out string lSchoolID)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  ProvinceID, ZoneID, DevisionID, SchoolID FROM U_Users WHERE UserName = '" + Session["UserName"].ToString() + "'";
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                DataRow mydataRow = dt.Rows[0];
              
                lProvinceID = Convert.ToInt32((mydataRow["ProvinceID"]==DBNull.Value?0:mydataRow["ProvinceID"]));
                lZoneID = Convert.ToString((mydataRow["ZoneID"]==DBNull.Value?"":mydataRow["ZoneID"]));
                lDevisionID = Convert.ToString((mydataRow["DevisionID"] == DBNull.Value ? "": mydataRow["DevisionID"]));
                lSchoolID = Convert.ToString((mydataRow["SchoolID"] == DBNull.Value ? "" : mydataRow["SchoolID"]));
               
            }
        }
        


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Models.LoginModel model, string returnUrl)
        {
            try
            {
                int RoleID = 0;
                if (ModelState.IsValid && IsUserValid(model.UserName, model.Password, out RoleID))
                {
                    Session["UserName"] = model.UserName;
                    Session["LoginRoleID"] = RoleID;
                    return RedirectToAction("Index", "Home");
                }

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
                return View(model);
            }
            catch (Exception Err)
            {
                @ViewBag.ErrorMessage = Err.Message;
                return View("Error");
            }
          
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            Models.ChagePassWDModel model = new Models.ChagePassWDModel();
            model.UserName = Session["UserName"].ToString();

            return View(model);
        }

        [HttpPost]
        public ActionResult ChangePassword(Models.ChagePassWDModel model)
        {
            int RoleID;
            if (model.UserName.ToUpper() == Session["UserName"].ToString().ToUpper())
            {
                if (ModelState.IsValid && IsUserValid(model.UserName, model.OldPassword, out RoleID))
                {
                    if (model.ConfirmPassword != model.NewPassword)
                    {
                        ModelState.AddModelError("", "The password confirmed does not match.");
                        return View(model);
                    }

                    if (ModelState.IsValid)
                    {
                        if (Change_Password(model))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        public List<SelectListItem> GetRoles(int LoginRoleID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "SELECT  RoleID, Role FROM U_Roles WHERE RoleID > " + LoginRoleID;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                List<SelectListItem> MyList = new List<SelectListItem>();
                foreach (DataRow mydataRow in dt.Rows)
                {
                    MyList.Add(new SelectListItem { Text = mydataRow["Role"].ToString().Trim(), Value = Convert.ToString(mydataRow["RoleID"]) });
                }

                return MyList;
            }

        }

        private bool Change_Password(Models.ChagePassWDModel model)
        {
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "UPDATE U_Users SET Password = @NewPassword  WHERE UserName = @Uname AND Password = @Pwd";
                cmd.Parameters.AddWithValue("@Uname", model.UserName);
                cmd.Parameters.AddWithValue("@NewPassword", model.NewPassword);
                cmd.Parameters.AddWithValue("@Pwd", model.OldPassword);
                cmd.ExecuteNonQuery();
                return true;
            }
            
        }
        //
        // POST: /Account/LogOff

        public bool IsUserValid(string strUserName, string strPWD, out int RoleID)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT UserName, RoleID FROM U_Users WHERE UserName = @Uname AND Password = @Pwd";
                cmd.Parameters.AddWithValue("@Uname", strUserName);
                cmd.Parameters.AddWithValue("@Pwd", strPWD);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        RoleID = 0;
                        return false;
                    }
                    else
                    {
                        RoleID = Convert.ToInt32(reader["RoleID"]);
                    }
                }
            }

           
            return true;
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
