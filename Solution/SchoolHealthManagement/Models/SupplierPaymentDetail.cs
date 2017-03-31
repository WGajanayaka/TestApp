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
        public Decimal Amount;

        public Decimal MAXAmount { get; set; }

        public string BranchName { get; set; }

        public string BranchCode { get; set; }

        public string PaymentHeaderStatus { get; set; }

        public string SupplierName { get; set; }

        public string BankAccountNo { get; set; }

        public string BankName { get; set; }

        public string BankCode { get; set; }

        public string ZoneName { get; set; }

        public string ProvinceName { get; set; }

        public int ProvinceId { get; set; } 

        public decimal SupplierPaymentReqHeaderId { get; set; }  

        public int  SupplierPaymentReqDetailId { get; set; }   
        
    }

    public class SupplierPaymentRequestDetailMoe
    {
        public decimal PaymentId { get; set; } 
        public string CensorsId  { get; set; }
        public string SupplierName { get; set; }
        public int SupplierId { get; set; } 
        public string BankAccountNo { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; } 
        public string BankCode { get; set; }
        public string BranchCode { get; set; } 
        public decimal Amount { get; set; }
        public int Year { get; set; } 
        public int ProvinceId { get; set; }  
        public string Month { get; set; }
        public string Paid { get; set; } 
        public decimal SupplierPaymentReqHeaderId { get; set; } 
        public int  SupplierPaymentReqDetailId { get; set; } 
    }

    public class SupplierDetailPayModel
    {
        public List<SupplierDetailProvinceLevel> Detail { get; set; }
        public decimal FullTotal { get; set; }

        public SupplierDetailPayModel()
        {
            Detail = new List<SupplierDetailProvinceLevel>();
            
        }
    }

    public class SupplierDetailProvinceLevel 
    {
        public string ProvinceName { get; set; }
        public decimal ProvincialTotal { get; set; }

        public List<SupplierDetailZoneLevel> SupplierDetailZoneLevel { get; set; }

        public SupplierDetailProvinceLevel()
        {

            SupplierDetailZoneLevel = new List<SupplierDetailZoneLevel>();
        }
    }

    public class SupplierDetailZoneLevel
    {
        public string ZoneName { get; set; }
        public decimal ZoneTotal { get; set; }

        public List<SupplierPaymentRequestDetailMoe> SupplierPaymentRequestDetails { get; set; }

        public SupplierDetailZoneLevel()
        {
            SupplierPaymentRequestDetails = new List<SupplierPaymentRequestDetailMoe>();

        }
    }

    public class SupplierPaymentRequestPaidDetail
    {
        public decimal PaymentNo { get; set; }  
        public DateTime PaymentDate { get; set; }  
        public string PaymentDateString { get; set; }   
        public DateTime ChequeDate { get; set; } 
        public string ChequeDateString  { get; set; } 
        public string BankName { get; set; }
        public string BankCode { get; set; } 
        public string AccountNo { get; set; }  
        public string BranchCode { get; set; }  
        public string ChequeNumber { get; set; }
        public string SupplierName  { get; set; }
        public decimal Amount { get; set; }
        public int Year { get; set; }
        public string Month { get; set; }
        public string Paid { get; set; }
        
    }
}