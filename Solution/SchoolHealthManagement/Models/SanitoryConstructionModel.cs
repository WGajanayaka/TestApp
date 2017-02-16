using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class SanitoryConstructionModel
    {
        //[Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "*")]
        public string CensorsID { get; set; }

        [Required(ErrorMessage = "*")]
        public string ConstructionType { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime AgreementDate { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime AgreementStartDate { get; set; }

        [Required(ErrorMessage = "*")]
        public int Progress { get; set; }

        [Required(ErrorMessage = "*")]
        public string ConstructionID { get; set; }

        public List<ConstructionPayment> Payments { get; set; }

    }

    public class ConstructionPayment
    {
        public int PaymentNo { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "*")]
        public string BankName { get; set; }

        [Required(ErrorMessage = "*")]
        public string ChequeNo { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal PaidAmount { get; set; }
    }
}