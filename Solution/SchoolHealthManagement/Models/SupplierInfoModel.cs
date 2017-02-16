using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class SupplierInfoModel
    {
        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public string SupplierName { get; set; }

        [Required(ErrorMessage = "*")]
        public string NIC { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string Phone { get; set; }
        
        public string BankName { get; set; }

        public int BankID { get; set; }

        public int BankBranchID { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string BankAccountNo { get; set; }
           
        [Required(ErrorMessage = "*")]
        public string Grade { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfMaleStudents { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfFemaleStudents { get; set; }

        public string SchoolName { get; set; }

        public string CensusID { get; set; }

        public string Zone { get; set; }

        public string Devision { get; set; }


        public string ID { get; set; }
    }
}