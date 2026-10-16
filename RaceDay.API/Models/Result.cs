using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Result
    {
        [Key]
        public int ResultID { get; set; }

        [Required]
        public int EnrolmentID { get; set; }

        [Required]
        public TimeSpan FinishTime { get; set; }

        [Required]
        public int FinishingPosition { get; set; }
    }
}
