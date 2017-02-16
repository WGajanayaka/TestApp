using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class ImpressRequestModel
    {
        [Required(ErrorMessage = "*")]
        public string CensorsID { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal ZonalRequestAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal PDEReleasedAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime ForMonth { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime ReleasedDate { get; set; }


        public string SchoolName { get; set; }


        
    }

    public class ImpressReleaseModel
    {
        [Required(ErrorMessage = "*")]
        public string ProvinceID { get; set; }

        [Required(ErrorMessage = "*")]
        public string ZoneID { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal ZonalRequestAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal PDEReleasedAmount { get; set; }

        [Required(ErrorMessage = "*")]
        public int ForMonth { get; set; }

        [Required(ErrorMessage = "*")]
        public int ForYear { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime ReleasedDate { get; set; }

        [Required(ErrorMessage = "*")]
        public string ChequeNo { get; set; }

    }
}