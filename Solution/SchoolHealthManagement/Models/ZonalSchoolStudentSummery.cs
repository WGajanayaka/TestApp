using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class ZonalSchoolStudentSummery
    {
        public int Year;

        //Province
        [Required(ErrorMessage = "*")]
        public int ProvinceID { get; set; }

        [Required(ErrorMessage = "*")]
        public string ZoneID { get; set; }



    }
}
