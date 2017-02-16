using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class StudentModel
    {
        [Required(ErrorMessage = "*")]
        public string AddmisionNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string CurrentGrade { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public string NameWithInitials { get; set; }

        [Required(ErrorMessage = "*")]
        public string NameInFull { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "*")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "*")]
        public string ParentName { get; set; }

        [Required(ErrorMessage = "*")]
        public string ParentAddress { get; set; }

        [Required(ErrorMessage = "*")]
        public string NIC { get; set; }

        [Required(ErrorMessage = "*")]
        public string ContactNo { get; set; }
       



    }
}