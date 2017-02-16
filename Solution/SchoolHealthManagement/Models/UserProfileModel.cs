using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class UserProfileModel
    {
       
        [Required(ErrorMessage = "*")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string Designation { get; set; }

        [Required(ErrorMessage = "*")]
        public string Email { get; set; }

        [Required(ErrorMessage = "*")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "*")]
        public string Password { get; set; }

        [Required(ErrorMessage = "*")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "*")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "*")]
        public int ProvinceID { get; set; }

        //[Required(ErrorMessage = "*")]
        public string ZoneID { get; set; }

        //[Required(ErrorMessage = "*")]
        public string DevisionID { get; set; }

        //[Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }
    }
}