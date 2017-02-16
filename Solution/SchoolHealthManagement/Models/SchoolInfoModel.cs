using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolHealthManagement.Models
{
    public class SchoolInfoModel
    {
        [Required(ErrorMessage = "*")]
        public int ProvinceID {get; set;}

        [Required(ErrorMessage = "*")]
        public string ZoneID { get; set; }

        [Required(ErrorMessage = "*")]
        public string DevisionID { get; set; }

        //[Required(ErrorMessage = "*")]
        public string SchoolID	 {get; set;}

        [Required(ErrorMessage = "*")]
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "*")]
        public string CensorsID {get; set;}

        [Required(ErrorMessage = "*")]
        public int ExaminationNo {get; set;}

        [Required(ErrorMessage = "*")]
        public string PrincipalName { get; set; }

        [Required(ErrorMessage = "*")]
        public int AcademicStaffMale {get; set;}

        [Required(ErrorMessage = "*")]
        public int AcademicStaffFemale {get; set;}

        [Required(ErrorMessage = "*")]
        public int NonAcademicStaffMale {get; set;}

        [Required(ErrorMessage = "*")]
        public int NonAcademicStaffFemale { get; set; }

        [Required(ErrorMessage = "*")]
        public int StudentsMale {get; set;}

        [Required(ErrorMessage = "*")]
        public int StudentsFemale {get; set;}

        [Required(ErrorMessage = "*")]
        public string InchargeHealthPromotion {get; set;}

        [Required(ErrorMessage = "*")]
        public string InchargeFeedingProgram {get; set;}

        [Required(ErrorMessage = "*")]
        public string SchoolAddress {get; set;}

        [Required(ErrorMessage = "*")]
        public string TelNo {get; set;}

        [Required(ErrorMessage = "*")]
        public string Medium {get; set;}

        [Required(ErrorMessage = "*")]
        public string Sex {get; set;}

        [Required(ErrorMessage = "*")]
        public int National_Provincial { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolType {get; set;}

        [Required(ErrorMessage = "*")]
        public string GradeSpan { get; set; }

        [Required(ErrorMessage = "*")]
        public string District {get; set;}

        [Required(ErrorMessage = "*")]
        public string AGADivision {get; set;}

        [Required(ErrorMessage = "*")]
        public string NearsetPoliceStation {get; set;}

        [Required(ErrorMessage = "*")]
        public string PoliceStationContactNo {get; set;}

        [Required(ErrorMessage = "*")]
        public string HospitalName {get; set;}

        [Required(ErrorMessage = "*")]
        public string HospitalContactNo {get; set;}

        [Required(ErrorMessage = "*")]
        public string ContactPersionMOH {get; set;}

        [Required(ErrorMessage = "*")]
        public string ContactNoMOH {get; set;}

        [Required(ErrorMessage = "*")]
        public string FeedingProgramme {get; set;}

        [Required(ErrorMessage = "*")]
        public string GNDevision { get; set; }

        [Required(ErrorMessage = "*")]
        public string HealthInchargeContactNo { get; set; }
        
        [Required(ErrorMessage = "*")]
        public string FeedingFundingSource { get; set; }

        [Required(ErrorMessage = "*")]
        public string FeedingInchargeContactNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string NameOfPHI { get; set; }

        [Required(ErrorMessage = "*")]
        public string PHIContactNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string PrincipalContactNo { get; set; }

        [Required(ErrorMessage = "*")]
        public string City { get; set; }

        [Required(ErrorMessage = "*")]
        public string Bank { get; set; }

        [Required(ErrorMessage = "*")]
        public string BankAccountNo { get; set; }

        //[Required(ErrorMessage = "*")]
        public string MonitoringOfficer { get; set; }

        public int TotalMarks { get; set; }

    }

    public class Zone
    {
        public string ZoneID { get; set; }
        public string ZoneName { get; set; }
        public int ProvinceID { get; set; }

    }
}