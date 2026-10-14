using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.Models
{
    public class Event
    {
        [Key]
        public int EventID { get; set; }

        [Required]
        public int OrganiserID { get; set; }

        [Required]
        public int EventTypeID { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; } = "";

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [MaxLength(150)]
        public string Location { get; set; } = "";

        public decimal? Distance { get; set; }
    }
}