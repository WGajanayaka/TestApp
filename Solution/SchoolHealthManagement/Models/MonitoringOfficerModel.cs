using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class MonitoringOfficerModel
    {
        [Required(ErrorMessage = "*")]
        public string CensorsID { get; set; }

        [Required(ErrorMessage = "*")]
        public string NameOfOfficer { get; set; }

        [Required(ErrorMessage = "*")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "*")]
        public string ContactNo { get; set; }

        
        public string SchoolName { get; set; }

       
        public string Province { get; set; }

        
        public string Zone { get; set; }

       
        public string Devision { get; set; }
        
        
        
        

    }
}