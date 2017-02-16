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
        public string ZonalApprovedBy;
        public DateTime? ZonalApprovedDate;
        public bool IsProvincialApproved;
        public string ProvincialApprovedBy;
        public DateTime? ProvincialApprovedDate;
        public string Status;

        public string ProvinceName { get; set; }

        public string ZoneName { get; set; }
    }
}