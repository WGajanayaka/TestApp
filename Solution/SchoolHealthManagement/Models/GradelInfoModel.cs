using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
namespace SchoolHealthManagement.Models
{
    public class GradeInfoModel
    {

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(200, ErrorMessage = "Name cannot be longer than 200 characters.")]
        public string TeacherInCharge { get; set; }

        [Required(ErrorMessage = "*")]
        public int Male { get; set; }

        [Required(ErrorMessage = "*")]
        public int Female { get; set; }

        [Required(ErrorMessage = "*")]
        [StringLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
        public string Grade { get; set; }
        
    }

    public class School
    {
        public int SchoolID { get; set; }
        public string SchoolName { get; set; }
    }

    public class GradesInfoModel
    {
        public string SchoolID { get; set; }
        public string TeacherInCharge { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public string Grade { get; set; }
        public int Total { get; set; }
    }
   
}