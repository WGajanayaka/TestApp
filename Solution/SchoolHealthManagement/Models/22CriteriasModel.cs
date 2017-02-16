using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace SchoolHealthManagement.Models
{
    public class _22CriteriasModel
    {
        
        [Required(ErrorMessage = "*")]
        public int SchoolHelthCommitee { get; set; }

        [Required(ErrorMessage = "*")]
        public int StudentHealthSocity { get; set; }

        [Required(ErrorMessage = "*")]
        public int FirstAidFacility { get; set; }

        [Required(ErrorMessage = "*")]
        public string SchoolID { get; set; }

        [Required(ErrorMessage = "*")]
        public int MedicalTest { get; set; }

        [Required(ErrorMessage = "*")]
        public int Reactions4IssuesOfMedicalTest { get; set; }

        [Required(ErrorMessage = "*")]
        public int ContribOnSchHealthPrg { get; set; }

        [Required(ErrorMessage = "*")]
        public int PublicHealthPrg { get; set; }

        [Required(ErrorMessage = "*")]
        public int SupplySanitoryFacilities { get; set; }

        [Required(ErrorMessage = "*")]
        public int CleanlinessOfSanitoryFacilities { get; set; }

        [Required(ErrorMessage = "*")]
        public int WaterSupply { get; set; }

        [Required(ErrorMessage = "*")]
        public int StudentsAttendance { get; set; }

        [Required(ErrorMessage = "*")]
        public int TeachersAttendance { get; set; }

        [Required(ErrorMessage = "*")]
        public int ClassroomEnvironment { get; set; }

        [Required(ErrorMessage = "*")]
        public int EnvironmentOfSchool { get; set; }

        [Required(ErrorMessage = "*")]
        public int MinimizeNutritionProblems { get; set; }

        [Required(ErrorMessage = "*")]
        public int NutritionKnowledgeCompetence { get; set; }

        [Required(ErrorMessage = "*")]
        public int StudentFitnessStatus { get; set; }

        [Required(ErrorMessage = "*")]
        public int PysicalWellbeingProg { get; set; }

        [Required(ErrorMessage = "*")]
        public int IndividualFitness { get; set; }

        [Required(ErrorMessage = "*")]
        public int MaintananceOfCanteens { get; set; }
        
        [Required(ErrorMessage = "*")]
        public int MentalEnvironment { get; set; }

        [Required(ErrorMessage = "*")]
        public int InstructionsNGuidence { get; set; }

  
    }
}