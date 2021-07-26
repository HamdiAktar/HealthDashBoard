using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleHealthDashboard.Model
{
    public class HealthData
    {
        public double Calories { get; set; }

        public double Distance { get; set; }

        public DateTime Date { get; set; }

        public int FormatedDate { get; set; }

        public int Steps { get; set; }

        public int HeartBeat { get; set; }
    }
}
