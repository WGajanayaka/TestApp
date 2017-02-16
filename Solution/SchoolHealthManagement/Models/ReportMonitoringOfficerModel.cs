using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolHealthManagement.Models
{
    public class ReportMonitoringOfficerModel
    {
        public string SchoolName { get; set; }

        public string Province { get; set; }

        public string Zone { get; set; }

        public string Devision { get; set; }
    }

    public class ReportBMIInfoModel
    {
        public string SchoolName { get; set; }

        public string ProvinceID { get; set; }

        public string ZoneID { get; set; }

        public string DevisionID { get; set; }

        public string Gender { get; set; }

        public string BMIMeasure { get; set; }
    }

    public class ReportBMIMeasurePerAgeModel
    {
        public string Category { get; set; }

        public decimal Count { get; set; }


    }

    public class ReportBMIReportModel
    {
        public string ProvinceName { get; set; }

        public string ProvinceID { get; set; }

        public string ZoneName { get; set; }

        public string ZoneID { get; set; }

        public int Underweight { get; set; }

        public int Healthy { get; set; }

        public int Stunning { get; set; }

        public int Wasting { get; set; }

        public int Overweight { get; set; }

        public int CountAll { get; set; }


    }
    
}