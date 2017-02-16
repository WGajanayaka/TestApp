using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace SchoolHealthManagement.Models
{
  
    public class WaterNHandwashModel
    {
        [Required(ErrorMessage = "*")]
        public string WaterSource { get; set; }

        [Required(ErrorMessage = "*")]
        public string Availability { get; set; }

        [Required(ErrorMessage = "*")]
        public string Quality { get; set; }

        [Required(ErrorMessage = "*")]
        public string DrinkingWater { get; set; }

        [Required(ErrorMessage = "*")]
        public string WasteWaterManagement { get; set; }

        [Required(ErrorMessage = "*")]
        public string Note { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfTapsAvailable { get; set; }

        [Required(ErrorMessage = "*")]
        public string SoapAvailable { get; set; }

        [Required(ErrorMessage = "*")]
        public int Coverage { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }
    }
}