using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Enrolment
    {
        [Key]
        public int EnrolmentID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        public DateTime EnrolmentDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "";
    }
}