using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class SchoolAttendanceModel
    {

        //[Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "*")]
        public string CensorsID { get; set; }

        [Required(ErrorMessage = "*")]
        public string Grade { get; set; }

        [Required(ErrorMessage = "*")]
        public int Boys { get; set; }

        [Required(ErrorMessage = "*")]
        public int Girls { get; set; }

        [Required(ErrorMessage = "*")]
        public int Total { get; set; }

        [Required(ErrorMessage = "*")]
        public int Month { get; set; }
        
    }
}