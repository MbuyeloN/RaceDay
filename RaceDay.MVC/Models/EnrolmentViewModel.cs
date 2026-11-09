using System;

namespace RaceDay.MVC.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentID { get; set; }

        public int ParticipantID { get; set; }

        public int EventID { get; set; }

        public int CategoryID { get; set; }

        public DateTime EnrolmentDate { get; set; }
    }
}
