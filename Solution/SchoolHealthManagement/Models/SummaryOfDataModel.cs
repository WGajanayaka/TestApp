using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolHealthManagement.Models
{
    public class SummaryOfDataModel
    {
        public string CensusID { get; set; }

        public string Province { get; set; }

        public string Zone { get; set; }

        public string ProvinceID { get; set; }

        public string ZoneID { get; set; }

        public string School { get; set; }

        public int NoOfStudents { get; set; }

        public int NumberOfSchools { get; set; }

        public string TotalStudentCount { get; set; }

        public string Entered { get; set; }

        public decimal Per { get; set; }

        public int YearConsidered { get; set; }

        public int BMI1 { get; set; }

        public int BMI2 { get; set; }

        public int BMI3 { get; set; }

        public int SupCount { get; set; }

        public int SanitoryCount { get; set; }
        
    }
}