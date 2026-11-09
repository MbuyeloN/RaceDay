using System;
using System.ComponentModel.DataAnnotations;

namespace RaceDay.MVC.Models
{
    public class ManageEventViewModel
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        [Required(ErrorMessage = "Please enter an event type ID.")]
        [Range(1, int.MaxValue)]
        public int EventTypeID { get; set; }

        [Required(ErrorMessage = "Please enter an event name.")]
        [MaxLength(100)]
        public string EventName { get; set; } = "";

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select an event date.")]
        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Please enter a location.")]
        [MaxLength(150)]
        public string Location { get; set; } = "";


        [Range(0, 99999999,
            ErrorMessage = "Please enter a valid distance.")]
        public decimal? Distance { get; set; }

    }
}
