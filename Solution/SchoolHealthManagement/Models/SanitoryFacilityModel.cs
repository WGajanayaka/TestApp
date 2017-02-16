using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class SanitoryFacilityModel
    {
        [Required(ErrorMessage = "*")]
        public int NoOfMaleToilets { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfMaleUrinals { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfFemaleToilets { get; set; }

        [Required(ErrorMessage = "*")]
        public int MaleCoverage { get; set; }

        [Required(ErrorMessage = "*")]
        public int FemaleCoverage { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfStaffToilets { get; set; }

        [Required(ErrorMessage = "*")]
        public int StaffCoverage { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

    }
}