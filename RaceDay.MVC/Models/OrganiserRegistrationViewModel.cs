using System;

namespace RaceDay.MVC.Models
{
    public class OrganiserRegistrationViewModel
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }

        public string EventName { get; set; } = "";

        public string ParticipantName { get; set; } = "";

        public string CategoryName { get; set; } = "";
    }
}
