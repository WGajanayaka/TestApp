using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class SanitoryDevPayments
    {
        public int PaymentNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public string SanitoryDevelopmentType { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "*")]
        public string Bank { get; set; }

        [Required(ErrorMessage = "*")]
        public string ChequeNo { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Amount { get; set; }



    }
}