namespace NewTASUI.Models
{
    public class TruckAttendanceModel
    {
        public long TruckEntryId { get; set; }
        public string? EntryDate { get; set; }
        public string? Time { get; set; }
        public string? EntryType { get; set; }
        public string? TruckNumber { get; set; }
        public string? TruckType { get; set; }
        public string? CompanyName { get; set; }
        public int? NoOfCompartments { get; set; }
        public string? TruckKey { get; set; }
        public bool IsPermanentKey { get; set; }
        public decimal? CCOEPermWeight { get; set; }
        public decimal? TankerTareWeight { get; set; }
        public decimal? TankerGrossWeight { get; set; }
        public decimal? PrintedTareWeight { get; set; }
        public decimal? PrintedGrossWeight { get; set; }
        public DateTime? CallibrationDueDate { get; set; }
        public DateTime? LicenseExpireDate { get; set; }
        public string? TTStatus { get; set; }
        public long? SequenceNumber { get; set; }
        public string? ProcessingResponse { get; set; }
        public string? LicenseExpiryInfo { get; set; }
        public long TruckId { get; set; }
    }

    public class AttendancePostRequest
    {
        public long TruckId { get; set; }
        public long TruckEntryId { get; set; }
        public string? Reason { get; set; }
    }

    public class DeleteAttendanceRequest
    {
        public long Id { get; set; }
    }

}
