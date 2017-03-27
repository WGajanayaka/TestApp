using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoolHealthManagement.Models
{
    public class SupplierPaymentSummary
    {
        public string ProvinceId; 
        public string Province;
        public int Year;
        public string Month;
        public Decimal TotalAmount;
        public decimal GrantTotal { get; set; }
    }
}
