using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SchoolHealthManagement.Models
{
    public class BMIInfoModel
    {
        [Required(ErrorMessage = "*")]
        public string Grade { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        //[Required(ErrorMessage = "*")]
        public string SchoolName { get; set; }

        ////[Required(ErrorMessage = "*")]
        //public string CensusID { get; set; }

        [Required(ErrorMessage = "*")]
        public string TakenBy { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime PerformedDate { get; set; }

        [Required(ErrorMessage = "*")]
        public int Year { get; set; }

        //[Required(ErrorMessage = "*")]
        //public List<BMIInfo> BMIDetailsOfClass { get; set; }

        [Required(ErrorMessage = "*")]
        public int Trimester { get; set; }

        [AllowHtml]
        public string Csv { get; set; }
    }

    public class BMIInfo
    {
        [Required(ErrorMessage = "*")]
        public string AdmissionNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Height { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal BMI { get; set; }

    }

    public class BMISDateInfo
    {
        [Required(ErrorMessage = "*")]
        public string AdmissionNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "*")]
        public string DOB { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Height { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal BMI { get; set; }

    }

    public class BMIHistoryInfo
    {
        [Required(ErrorMessage = "*")]
        public DateTime DatePerformed { get; set; }

        [Required(ErrorMessage = "*")]
        public string TakenBy { get; set; }

        [Required(ErrorMessage = "*")]
        public string Class { get; set; }

        [Required(ErrorMessage = "*")]
        public string Trimester { get; set; }

        [Required(ErrorMessage = "*")]
        public string AdmissionNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime DOB { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Height { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal Weight { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal BMI { get; set; }

    }



}