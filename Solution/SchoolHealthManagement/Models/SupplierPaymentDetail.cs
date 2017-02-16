using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SchoolHealthManagement.Models
{
    public class SupplierPaymentRequestDetail
    {
        public string CensorsID;
        public int PaymentRequestId;
        public int SupplierId;
        public SupplierInfoModel Supplier = new SupplierInfoModel();
        public int Amount;

        public string MAXAmount { get; set; }

        public string BranchName { get; set; }
    }
}