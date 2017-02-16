using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class FundsReceivedModel
    {
        [Required(ErrorMessage = "*")]
        public string CensorsID { get; set; }

        [Required(ErrorMessage = "*")]
        public string NameOfFoodSupplier { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime PaidDate { get; set; }

        [Required(ErrorMessage = "*")]
        public string ChequeNo { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal PaidAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime ForMonth { get; set; }

        public string SchoolName { get; set; }


        public string Province { get; set; }


        public string Zone { get; set; }


        public string Devision { get; set; }
    }
}