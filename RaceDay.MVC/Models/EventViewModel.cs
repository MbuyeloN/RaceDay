using System;

namespace RaceDay.MVC.Models
{
    public class EventViewModel
    {
        public int EventID { get; set; }

        public int OrganiserID { get; set; }

        public int EventTypeID { get; set; }

        public string EventName { get; set; } = "";

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public string Location { get; set; } = "";

        public decimal? Distance { get; set; }
    }
}
