using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SchoolHealthManagement.Helper
{
    public class Helper 
    {

        public static List<SelectListItem> GetMonths(string selected)
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

            var data = MyList.Find(i => i.Value == selected);
            if (data != null)
                data.Selected = true;
            return MyList;
        }

        public List<SelectListItem> GetResentYears(string selected)
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

        public List<SelectListItem> GetProvincesByUser(string userName, string selected)
        {
            // Here you are free to do whatever data access code you like
            // You can invoke direct SQL queries, stored procedures, whatever 
            string strConnection = ConfigurationManager.ConnectionStrings["UsedConnection"].ConnectionString;
            using (var conn = new SqlConnection(strConnection))
            using (var cmd = conn.CreateCommand())
            {

                conn.Open();
                cmd.CommandText = "EXEC LoadProvince '" + userName + "'";
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
    }
}