using System;

namespace RaceDay.MVC.Models
{
    public class ResultViewModel
    {
        public int ResultID { get; set; }
        public int EnrolmentID { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int FinishingPosition { get; set; }
    }
}
