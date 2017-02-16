using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class ImpressSummaryModel
    {
        [Required(ErrorMessage = "*")]
        public int Year { get; set; }

        [Required(ErrorMessage = "*")]
        public string ProvinceID { get; set; }

        [Required(ErrorMessage = "*")]
        public DateTime EntryDate { get; set; }

        [Required(ErrorMessage = "*")]
        public decimal AmountPerStudent { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysJan { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysFeb { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysMar { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysApr { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysMay { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysJun { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysJul { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysAug { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysSep { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysOct { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysNov { get; set; }

        [Required(ErrorMessage = "*")]
        public int NoOfSchoolDaysDec { get; set; }
    }

    public class ZoneImpressSummaryModel
    {
        [Required(ErrorMessage = "*")]
        public int NoOfStudents { get; set; }

        [Required(ErrorMessage = "*")]
        public string ZoneName { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueJan { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueFeb { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueMar { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueApr { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueMay { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueJun { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueJul { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueAug { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueSep { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueOct { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueNov { get; set; }

        [Required(ErrorMessage = "*")]
        public int ValueDec { get; set; }
    }
}