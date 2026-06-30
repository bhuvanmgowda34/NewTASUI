using System;
using System.Collections.Generic;

namespace NewTASUI.Models
{
    public class SODEODHistoryRowModel
    {
        public int TASDayId { get; set; }
        public string DayDate { get; set; } = "";
        public DateTime StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsSOD { get; set; }
        public bool IsEOD { get; set; }
    }

    public class SODEODStatusResponseModel
    {
        public int? CurrentDayId { get; set; }
        public DateTime ServerTime { get; set; }
        public string ExpectedEODTime { get; set; } = "23:59";
        public DateTime? CurrentSODDateTime { get; set; }
        public DateTime? CurrentEODDateTime { get; set; }
        public bool IsSODDone { get; set; }
        public bool IsEODDone { get; set; }
        public bool CanStartDay { get; set; }
        public bool CanEndDay { get; set; }
        public bool CanProceedForWorkflow { get; set; }
        public List<SODEODHistoryRowModel> History { get; set; } = new();
    }
}
