using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolHealthManagement.Models
{
    public class SupplierPaymentRequest
    {
        public int Id;
        public int ProvinceID;
        public int Year;
        public string Month;
        public string RequestDate;
        public string ZoneID;
        public List<SupplierPaymentRequestDetail> PaymentDetails;
        public float Total;
        public DateTime? CreateDate;
        public string CreateBy;
        public bool IsZonalApproved;
        public string ApprovedBy;
        public DateTime? ApprovedDate;
        public bool IsProvincialApproved;
        public string ProvincialApprovedBy;
        public DateTime? ProvincialApprovedDate;
        public string Status;

        public string ProvinceName { get; set; }

        public string ZoneName { get; set; }

        public Dictionary<int, decimal> Details { get; set; }
    }

    public class SupplierPaymentMOE : SupplierPaymentRequest
    {
        public string PaymentNo = "N/A";
        public string PaymentDate = DateTime.Now.ToShortDateString();
        public List<SupplierPaymentSummary> PaymentSummary = new List<SupplierPaymentSummary>();
        public Decimal PaymentSummaryTot;
        public string BankName;
        public string ChequeNumber;
        public DateTime? ChequeDate;
        public Decimal Amount;
    }

    public class FilterModel
    {
        public int ProvinceId; 
        public int Year;
        public string Month;
    }
}